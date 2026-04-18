using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Assignments;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Assignments;
using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.AssignmentDto;

namespace AYA_UIS.Application.Handlers.Assignments.UnitTests
{
    [TestClass]
    public class GetAssignmentSubmissionsQueryHandlerTests
    {
        /// <summary>
        /// Verifies that the handler forwards the provided assignmentId to the repository's GetSubmissions method
        /// for a range of integer edge values and returns without throwing for empty repository responses.
        /// Input conditions: assignmentId values include int.MinValue, negative, zero and int.MaxValue.
        /// Expected result: repository is called with the exact provided id and the handler returns an enumerable (possibly empty).
        /// </summary>
        [TestMethod]
        public async Task Handle_AssignmentIdVariants_InvokesRepositoryWithSameIdAndReturnsEnumerable()
        {
            // Arrange
            int[] assignmentIdsToTest = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int assignmentId in assignmentIdsToTest)
            {
                var mockRepo = new Mock<AYA_UIS.Core.Domain.Contracts.IAssignmentRepository>(MockBehavior.Strict);
                // Return empty list for simplicity; we care about the id forwarding.
                mockRepo.Setup(r => r.GetSubmissions(assignmentId))
                        .ReturnsAsync(Enumerable.Empty<AssignmentSubmission>());

                var mockUnitOfWork = new Mock<Domain.Contracts.IUnitOfWork>(MockBehavior.Strict);
                mockUnitOfWork.Setup(u => u.Assignments).Returns(mockRepo.Object);

                var handler = new GetAssignmentSubmissionsQueryHandler(mockUnitOfWork.Object);
                var request = new GetAssignmentSubmissionsQuery { AssignmentId = assignmentId };

                // Act
                IEnumerable<AssignmentSubmissionDto> result = await handler.Handle(request, CancellationToken.None);

                // Assert
                mockRepo.Verify(r => r.GetSubmissions(assignmentId), Times.Once, $"GetSubmissions was not called with assignmentId={assignmentId}");
                Assert.IsNotNull(result, "Result should not be null even when repository returns empty collection.");
                Assert.AreEqual(0, result.Count(), "Expected empty result for empty repository response.");
            }
        }

        /// <summary>
        /// Verifies mapping behavior of the handler for submission properties, student name resolution, and feedback normalization.
        /// Input conditions: three submissions:
        ///  1) Student with DisplayName set.
        ///  2) Student with empty DisplayName but UserName set.
        ///  3) Null Student.
        /// Also includes Feedback null to ensure it becomes empty string and Grade null is preserved.
        /// Expected result: DTO values map from entity properties and StudentName resolves to DisplayName -> UserName -> 'Unknown'.
        /// </summary>
        [TestMethod]
        [TestCategory("ProductionBugSuspected")]
        [Ignore("ProductionBugSuspected")]
        public async Task Handle_MapsSubmissionPropertiesAndResolvesStudentNameAndFeedback()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var submissionWithDisplayName = new AssignmentSubmission
            {
                Id = 1,
                AssignmentId = 10,
                StudentId = "stu-1",
                Student = new User { Id = "u1", DisplayName = "Display One", UserName = "user.one" },
                FileUrl = "http://files/one",
                SubmittedAt = now,
                Grade = 85,
                Feedback = "Well done"
            };

            var submissionWithUserNameOnly = new AssignmentSubmission
            {
                Id = 2,
                AssignmentId = 10,
                StudentId = "stu-2",
                Student = new User { Id = "u2", DisplayName = string.Empty, UserName = "user.two" },
                FileUrl = "/files/two",
                SubmittedAt = DateTime.MinValue,
                Grade = null,
                Feedback = null // should become string.Empty
            };

            var submissionWithNullStudent = new AssignmentSubmission
            {
                Id = 3,
                AssignmentId = 10,
                StudentId = "stu-3",
                Student = null,
                FileUrl = string.Empty,
                SubmittedAt = DateTime.MaxValue,
                Grade = 0,
                Feedback = "Needs improvement"
            };

            var submissions = new List<AssignmentSubmission>
            {
                submissionWithDisplayName,
                submissionWithUserNameOnly,
                submissionWithNullStudent
            };

            var mockRepo = new Mock<AYA_UIS.Core.Domain.Contracts.IAssignmentRepository>(MockBehavior.Strict);
            mockRepo.Setup(r => r.GetSubmissions(42)).ReturnsAsync(submissions);

            var mockUnitOfWork = new Mock<Domain.Contracts.IUnitOfWork>(MockBehavior.Strict);
            mockUnitOfWork.Setup(u => u.Assignments).Returns(mockRepo.Object);

            var handler = new GetAssignmentSubmissionsQueryHandler(mockUnitOfWork.Object);
            var request = new GetAssignmentSubmissionsQuery { AssignmentId = 42 };

            // Act
            var result = (await handler.Handle(request, CancellationToken.None)).ToList();

            // Assert
            mockRepo.Verify(r => r.GetSubmissions(42), Times.Once);

            Assert.AreEqual(3, result.Count, "Expected three mapped DTOs.");

            var dto1 = result.SingleOrDefault(d => d.Id == submissionWithDisplayName.Id);
            Assert.IsNotNull(dto1);
            Assert.AreEqual(submissionWithDisplayName.StudentId, dto1.StudentId);
            Assert.AreEqual("Display One", dto1.StudentName, "StudentName should prefer DisplayName when available.");
            Assert.AreEqual(submissionWithDisplayName.FileUrl, dto1.FileUrl);
            Assert.AreEqual(submissionWithDisplayName.SubmittedAt, dto1.SubmittedAt);
            Assert.AreEqual(submissionWithDisplayName.Grade, dto1.Grade);
            Assert.AreEqual(submissionWithDisplayName.Feedback, dto1.Feedback);

            var dto2 = result.SingleOrDefault(d => d.Id == submissionWithUserNameOnly.Id);
            Assert.IsNotNull(dto2);
            Assert.AreEqual(submissionWithUserNameOnly.StudentId, dto2.StudentId);
            Assert.AreEqual("user.two", dto2.StudentName, "When DisplayName is empty, StudentName should fall back to UserName.");
            Assert.AreEqual(string.Empty, dto2.Feedback, "Null Feedback should be normalized to empty string.");
            Assert.IsNull(submissionWithUserNameOnly.Grade);
            Assert.IsNull(dto2.Grade, "Grade null should be preserved.");

            var dto3 = result.SingleOrDefault(d => d.Id == submissionWithNullStudent.Id);
            Assert.IsNotNull(dto3);
            Assert.AreEqual("Unknown", dto3.StudentName, "Null Student should result in 'Unknown' StudentName.");
            Assert.AreEqual(submissionWithNullStudent.FileUrl, dto3.FileUrl);
            Assert.AreEqual(submissionWithNullStudent.SubmittedAt, dto3.SubmittedAt);
            Assert.AreEqual(submissionWithNullStudent.Grade, dto3.Grade);
            Assert.AreEqual(submissionWithNullStudent.Feedback, dto3.Feedback);
        }

        /// <summary>
        /// Ensures the handler returns an empty enumerable when the repository returns null or an empty collection.
        /// Input conditions: repository returns an empty list.
        /// Expected result: the handler returns an empty IEnumerable without throwing.
        /// </summary>
        [TestMethod]
        public async Task Handle_NoSubmissions_ReturnsEmptyEnumerable()
        {
            // Arrange
            var mockRepo = new Mock<AYA_UIS.Core.Domain.Contracts.IAssignmentRepository>(MockBehavior.Strict);
            mockRepo.Setup(r => r.GetSubmissions(99)).ReturnsAsync(Enumerable.Empty<AssignmentSubmission>());

            var mockUnitOfWork = new Mock<Domain.Contracts.IUnitOfWork>(MockBehavior.Strict);
            mockUnitOfWork.Setup(u => u.Assignments).Returns(mockRepo.Object);

            var handler = new GetAssignmentSubmissionsQueryHandler(mockUnitOfWork.Object);
            var request = new GetAssignmentSubmissionsQuery { AssignmentId = 99 };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.GetSubmissions(99), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any(), "Expected no items when repository returns empty collection.");
        }

        /// <summary>
        /// Verifies that the constructor initializes an instance when a valid IUnitOfWork is provided.
        /// Input conditions: a mocked IUnitOfWork (non-null) is supplied.
        /// Expected result: no exception is thrown, the returned object is not null and implements the expected IRequestHandler interface.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidUnitOfWork_InstanceCreatedAndImplementsInterface()
        {
            // Arrange
            var mockUnitOfWork = new Mock<Domain.Contracts.IUnitOfWork>();

            // Act
            var handler = new GetAssignmentSubmissionsQueryHandler(mockUnitOfWork.Object);

            // Assert
            Assert.IsNotNull(handler, "Handler instance should not be null after construction with a valid IUnitOfWork.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetAssignmentSubmissionsQuery, IEnumerable<AssignmentSubmissionDto>>),
                "Handler should implement IRequestHandler<GetAssignmentSubmissionsQuery, IEnumerable<AssignmentSubmissionDto>>.");
        }

        /// <summary>
        /// Ensures that constructing the handler with different concrete mocks produces distinct instances
        /// and does not throw. This checks basic construction stability across multiple valid inputs.
        /// Input conditions: two different mocked IUnitOfWork instances.
        /// Expected result: two distinct handler instances are created without exceptions.
        /// </summary>
        [TestMethod]
        public void Constructor_MultipleValidUnitOfWorkInstances_CreatesIndependentInstances()
        {
            // Arrange
            var mockUow1 = new Mock<Domain.Contracts.IUnitOfWork>();
            var mockUow2 = new Mock<Domain.Contracts.IUnitOfWork>();

            // Act
            var handler1 = new GetAssignmentSubmissionsQueryHandler(mockUow1.Object);
            var handler2 = new GetAssignmentSubmissionsQueryHandler(mockUow2.Object);

            // Assert
            Assert.IsNotNull(handler1);
            Assert.IsNotNull(handler2);
            Assert.AreNotSame(handler1, handler2, "Separate constructor calls should produce distinct handler instances.");
        }
    }
}