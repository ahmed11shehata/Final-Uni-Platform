using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Assignment;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Assignments;
using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Assignments.UnitTests
{
    [TestClass]
    public class GradeSubmissionCommandHandlerTests
    {
        /// <summary>
        /// Test that when repository returns null for the requested submission id,
        /// the handler returns an error response with the expected message and does not call SaveChangesAsync.
        /// Input conditions: request.SubmissionId maps to no existing submission (null).
        /// Expected: Response.Success == false, Response.Errors == "Submission not found", SaveChangesAsync not invoked.
        /// </summary>
        [TestMethod]
        public async Task Handle_SubmissionNotFound_ReturnsErrorResponse()
        {
            // Arrange
            var submissionId = 123;
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var assignmentsRepoMock = new Mock<IAssignmentRepository>(MockBehavior.Strict);

            assignmentsRepoMock
                .Setup(r => r.GetSubmissionByIdAsync(submissionId))
                .ReturnsAsync((AssignmentSubmission?)null);

            unitOfWorkMock.Setup(u => u.Assignments).Returns(assignmentsRepoMock.Object);
            // SaveChangesAsync should not be called; no setup for it.

            var handler = new GradeSubmissionCommandHandler(unitOfWorkMock.Object);
            var request = new GradeSubmissionCommand
            {
                SubmissionId = submissionId,
                Grade = 10,
                Feedback = "OK"
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success, "Expected operation to fail when submission is not found.");
            Assert.AreEqual("Submission not found", result.Errors, "Expected error message to indicate missing submission.");
            // For error response with T=int, Data should be default(int) == 0
            Assert.AreEqual(default(int), result.Data);
            // Verify SaveChangesAsync was not called
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
            assignmentsRepoMock.Verify(r => r.GetSubmissionByIdAsync(submissionId), Times.Once);
        }

        /// <summary>
        /// Test that when a submission exists, the handler updates Grade and Feedback, calls SaveChangesAsync,
        /// and returns a success response containing the submission Id.
        /// This test iterates through representative edge and boundary cases:
        /// - normal values
        /// - zero values
        /// - extreme int.MinValue / int.MaxValue for id and grade
        /// - empty, whitespace, and very long feedback strings
        /// Expected: submission properties are updated, SaveChangesAsync invoked once per case, and Response.Success == true with Data == submission.Id.
        /// </summary>
        [TestMethod]
        public async Task Handle_ExistingSubmission_UpdatesAndReturnsSuccess_ForMultipleEdgeCases()
        {
            // Arrange: define multiple test cases to exercise numeric and string edge cases.
            var testCases = new (int submissionId, int grade, string feedback)[]
            {
                (submissionId: 1, grade: 100, feedback: "Good job"),                        // normal
                (submissionId: 0, grade: 0, feedback: string.Empty),                        // zero and empty feedback
                (submissionId: int.MaxValue, grade: int.MinValue, feedback: new string('x', 1024)), // extreme numeric and long feedback
                (submissionId: int.MinValue, grade: int.MaxValue, feedback: " \t\n ")      // extreme id with whitespace feedback
            };

            foreach (var (submissionId, grade, feedback) in testCases)
            {
                var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
                var assignmentsRepoMock = new Mock<IAssignmentRepository>(MockBehavior.Strict);

                // Create a submission instance with initial different values to ensure update occurs
                var existingSubmission = new AssignmentSubmission
                {
                    Id = submissionId,
                    Grade = null,
                    Feedback = null
                };

                assignmentsRepoMock
                    .Setup(r => r.GetSubmissionByIdAsync(submissionId))
                    .ReturnsAsync(existingSubmission);

                unitOfWorkMock.Setup(u => u.Assignments).Returns(assignmentsRepoMock.Object);
                unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

                var handler = new GradeSubmissionCommandHandler(unitOfWorkMock.Object);

                var request = new GradeSubmissionCommand
                {
                    SubmissionId = submissionId,
                    Grade = grade,
                    Feedback = feedback
                };

                // Act
                var result = await handler.Handle(request, CancellationToken.None);

                // Assert: response indicates success and returns the submission id
                Assert.IsTrue(result.Success, $"Expected success for submissionId={submissionId}");
                Assert.AreEqual(existingSubmission.Id, result.Data, "Response.Data should be the submission Id on success.");
                Assert.IsNull(result.Errors, "Errors should be null on success.");
                Assert.AreEqual("Operation completed successfully", result.Message, "Message should match success message.");

                // Assert: submission was updated with provided grade and feedback
                Assert.AreEqual(grade, existingSubmission.Grade, "Submission.Grade should be updated to request.Grade.");
                Assert.AreEqual(feedback, existingSubmission.Feedback, "Submission.Feedback should be updated to request.Feedback.");

                // Verify interactions
                assignmentsRepoMock.Verify(r => r.GetSubmissionByIdAsync(submissionId), Times.Once);
                unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);

                // Cleanup: Moq strict mocks must be verified before disposing; create new mocks per iteration so no reuse concerns.
            }
        }

        /// <summary>
        /// Verifies that the constructor successfully creates an instance when provided a valid IUnitOfWork.
        /// Input conditions: a strictly mocked IUnitOfWork with no setups (no calls expected).
        /// Expected result: an instance of GradeSubmissionCommandHandler is created and it implements the IRequestHandler interface for GradeSubmissionCommand & Response&lt;int&gt;,
        /// and the constructor does not invoke any members on the provided IUnitOfWork.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidUnitOfWork_InstanceCreatedAndNoCallsToUnitOfWork()
        {
            // Arrange
            var unitOfWorkMock = new Mock<Domain.Contracts.IUnitOfWork>(MockBehavior.Strict);

            // Act
            var handler = new GradeSubmissionCommandHandler(unitOfWorkMock.Object);

            // Assert
            Assert.IsNotNull(handler, "Constructor returned null when provided a valid IUnitOfWork mock.");
            Assert.IsInstanceOfType(
                handler,
                typeof(IRequestHandler<GradeSubmissionCommand, Response<int>>),
                "Handler does not implement the expected IRequestHandler<GradeSubmissionCommand, Response<int>> interface.");

            // Ensure constructor did not call any members on the unit of work
            unitOfWorkMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Ensures that constructing the handler with different mock behaviors does not throw.
        /// Input conditions: a mocked IUnitOfWork created with Loose behavior.
        /// Expected result: no exception is thrown and an instance is created.
        /// This test checks that the constructor does not depend on IUnitOfWork being in a particular state or calling its members.
        /// </summary>
        [TestMethod]
        public void Constructor_LooseMock_DoesNotThrowAndCreatesInstance()
        {
            // Arrange
            var uowMock = new Mock<Domain.Contracts.IUnitOfWork>(MockBehavior.Loose);

            // Act
            GradeSubmissionCommandHandler? handler = null;
            Exception? ex = null;
            try
            {
                handler = new GradeSubmissionCommandHandler(uowMock.Object);
            }
            catch (Exception e)
            {
                ex = e;
            }

            // Assert
            Assert.IsNull(ex, $"Constructor threw an unexpected exception: {ex}");
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(GradeSubmissionCommandHandler));
        }
    }
}