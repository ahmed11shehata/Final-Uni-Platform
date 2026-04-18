using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

using Abstraction;
using Abstraction.Contracts;
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.CreateAssignment;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Assignments;
using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Assignments.UnitTests
{
    [TestClass]
    public class SubmitAssignmentCommandHandlerTests
    {
        /// <summary>
        /// Verifies that when the repository does not find an assignment the handler returns an error response.
        /// Input conditions: various AssignmentId edge values (int.MinValue, 0, int.MaxValue) and repository returning null.
        /// Expected: Response.Success is false and Errors equals "Assignment not found".
        /// </summary>
        [TestMethod]
        public async Task Handle_AssignmentNotFound_ReturnsErrorResponse_ForEdgeAssignmentIds()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockAssignments = new Mock<IAssignmentRepository>();
            mockAssignments
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Assignment?)null);

            mockUnitOfWork.Setup(u => u.Assignments).Returns(mockAssignments.Object);

            var mockFileService = new Mock<ILocalFileService>();

            var handler = new SubmitAssignmentCommandHandler(mockUnitOfWork.Object, mockFileService.Object);

            int[] testIds = new[] { int.MinValue, 0, int.MaxValue };

            foreach (var id in testIds)
            {
                var request = new SubmitAssignmentCommand
                {
                    AssignmentId = id,
                    Academic_Code = "student1",
                    File = null
                };

                // Act
                var response = await handler.Handle(request, CancellationToken.None);

                // Assert
                Assert.IsFalse(response.Success, "Expected failure when assignment is not found.");
                Assert.AreEqual("Assignment not found", response.Errors, "Expected specific error message for missing assignment.");
            }

            mockAssignments.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Exactly(testIds.Length));
            mockUnitOfWork.Verify(u => u.Assignments, Times.AtLeastOnce);
        }

        /// <summary>
        /// Verifies that when the assignment deadline is earlier than current UTC time the handler returns a deadline error.
        /// Input conditions: repository returns an assignment with Deadline in the past.
        /// Expected: Response.Success is false and Errors equals "Deadline passed".
        /// </summary>
        [TestMethod]
        public async Task Handle_DeadlinePassed_ReturnsErrorResponse()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockAssignments = new Mock<IAssignmentRepository>();

            var pastAssignment = new Assignment
            {
                Id = 1,
                Deadline = DateTime.UtcNow.AddSeconds(-5) // past
            };

            mockAssignments
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(pastAssignment);

            mockUnitOfWork.Setup(u => u.Assignments).Returns(mockAssignments.Object);

            var mockFileService = new Mock<ILocalFileService>();

            var handler = new SubmitAssignmentCommandHandler(mockUnitOfWork.Object, mockFileService.Object);

            var request = new SubmitAssignmentCommand
            {
                AssignmentId = 1,
                Academic_Code = "student2",
                File = null
            };

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Deadline passed", response.Errors);
            mockAssignments.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        /// <summary>
        /// Verifies that if a submission already exists for the student and assignment, the handler returns an error.
        /// Input conditions: repository returns assignment with future deadline and SubmissionExists returns true.
        /// Expected: Response.Success is false and Errors equals "Already submitted".
        /// </summary>
        [TestMethod]
        public async Task Handle_AlreadySubmitted_ReturnsErrorResponse()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockAssignments = new Mock<IAssignmentRepository>();

            var futureAssignment = new Assignment
            {
                Id = 2,
                Deadline = DateTime.UtcNow.AddHours(1) // future
            };

            mockAssignments.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                           .ReturnsAsync(futureAssignment);

            mockAssignments.Setup(r => r.SubmissionExists(It.IsAny<int>(), It.IsAny<string>()))
                           .ReturnsAsync(true);

            mockUnitOfWork.Setup(u => u.Assignments).Returns(mockAssignments.Object);

            var mockFileService = new Mock<ILocalFileService>();

            var handler = new SubmitAssignmentCommandHandler(mockUnitOfWork.Object, mockFileService.Object);

            var request = new SubmitAssignmentCommand
            {
                AssignmentId = 2,
                Academic_Code = "student3",
                File = null
            };

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsFalse(response.Success);
            Assert.AreEqual("Already submitted", response.Errors);
            mockAssignments.Verify(r => r.SubmissionExists(2, "student3"), Times.Once);
        }

        /// <summary>
        /// Verifies successful submission flow:
        /// - assignment exists and deadline not passed
        /// - no existing submission
        /// - file upload returns a URL
        /// - submission is added and SaveChangesAsync is called
        /// Expected: Response.Success is true and Data equals the Id assigned to the submission.
        /// </summary>
        [TestMethod]
        public async Task Handle_ValidSubmission_AddsSubmissionAndReturnsSuccess()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockAssignments = new Mock<IAssignmentRepository>();

            var futureAssignment = new Assignment
            {
                Id = 3,
                Deadline = DateTime.UtcNow.AddMinutes(10)
            };

            mockAssignments.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                           .ReturnsAsync(futureAssignment);

            mockAssignments.Setup(r => r.SubmissionExists(It.IsAny<int>(), It.IsAny<string>()))
                           .ReturnsAsync(false);

            // Capture the submission passed to AddSubmissionAsync and set an Id to simulate DB generated Id
            AssignmentSubmission? capturedSubmission = null;
            mockAssignments.Setup(r => r.AddSubmissionAsync(It.IsAny<AssignmentSubmission>()))
                           .Callback<AssignmentSubmission>(s =>
                           {
                               capturedSubmission = s;
                               // simulate DB generated id
                               s.Id = 42;
                           })
                           .Returns(Task.CompletedTask);

            mockUnitOfWork.Setup(u => u.Assignments).Returns(mockAssignments.Object);
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var mockFileService = new Mock<ILocalFileService>();
            mockFileService.Setup(f => f.UploadSubmissionFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync("https://files.example/submission.pdf");

            var handler = new SubmitAssignmentCommandHandler(mockUnitOfWork.Object, mockFileService.Object);

            var request = new SubmitAssignmentCommand
            {
                AssignmentId = 3,
                Academic_Code = "student4",
                File = null
            };

            // Act
            var response = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsTrue(response.Success, "Expected success response for valid submission flow.");
            Assert.IsNotNull(response.Data, "Expected Data to contain the submission Id.");
            Assert.AreEqual(42, response.Data, "Expected returned Data to equal the Id set during AddSubmissionAsync callback.");

            // Validate that the uploaded file URL was assigned to the created submission and student/assignment ids match
            Assert.IsNotNull(capturedSubmission, "Expected AddSubmissionAsync to be invoked with a submission.");
            Assert.AreEqual(3, capturedSubmission!.AssignmentId);
            Assert.AreEqual("student4", capturedSubmission.StudentId);
            Assert.AreEqual("https://files.example/submission.pdf", capturedSubmission.FileUrl);
            Assert.IsTrue(capturedSubmission.SubmittedAt <= DateTime.UtcNow && capturedSubmission.SubmittedAt > DateTime.UtcNow.AddMinutes(-1));

            mockFileService.Verify(f => f.UploadSubmissionFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), 3, It.IsAny<CancellationToken>()), Times.Once);
            mockAssignments.Verify(r => r.AddSubmissionAsync(It.IsAny<AssignmentSubmission>()), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        /// <summary>
        /// Verifies that constructing SubmitAssignmentCommandHandler with valid non-null dependencies
        /// produces a non-null instance and that the instance implements
        /// IRequestHandler&lt;SubmitAssignmentCommand, Response&lt;int&gt;&gt;.
        /// Input conditions: both IUnitOfWork and ILocalFileService are provided as mocks (non-null).
        /// Expected result: no exception thrown, instance is not null and implements the MediatR handler interface.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidDependencies_ReturnsHandlerImplementingIRequestHandler()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var fileServiceMock = new Mock<ILocalFileService>(MockBehavior.Strict);

            // Act
            var handler = new SubmitAssignmentCommandHandler(unitOfWorkMock.Object, fileServiceMock.Object);

            // Assert
            Assert.IsNotNull(handler, "Constructor should return a non-null instance when provided valid dependencies.");
            Assert.IsInstanceOfType(
                handler,
                typeof(IRequestHandler<SubmitAssignmentCommand, Response<int>>),
                "Instance should implement IRequestHandler<SubmitAssignmentCommand, Response<int>>.");
        }

        /// <summary>
        /// Verifies that multiple constructions with different valid dependency instances succeed independently.
        /// Input conditions: two distinct sets of mocked dependencies.
        /// Expected result: both constructions succeed without exception and produce distinct handler instances.
        /// This guards against any accidental static/shared state in the constructor.
        /// </summary>
        [TestMethod]
        public void Constructor_MultipleValidConstructions_ProduceDistinctInstancesWithoutThrowing()
        {
            // Arrange
            var unitOfWorkMock1 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var fileServiceMock1 = new Mock<ILocalFileService>(MockBehavior.Strict);

            var unitOfWorkMock2 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var fileServiceMock2 = new Mock<ILocalFileService>(MockBehavior.Strict);

            // Act
            var handler1 = new SubmitAssignmentCommandHandler(unitOfWorkMock1.Object, fileServiceMock1.Object);
            var handler2 = new SubmitAssignmentCommandHandler(unitOfWorkMock2.Object, fileServiceMock2.Object);

            // Assert
            Assert.IsNotNull(handler1, "First construction should succeed and return a non-null instance.");
            Assert.IsNotNull(handler2, "Second construction should succeed and return a non-null instance.");
            Assert.AreNotSame(handler1, handler2, "Separate constructions should produce distinct handler instances.");
        }
    }
}