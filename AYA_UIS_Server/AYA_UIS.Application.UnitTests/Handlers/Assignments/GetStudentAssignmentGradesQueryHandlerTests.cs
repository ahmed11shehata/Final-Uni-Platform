using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Assignments;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Assignments;
using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.AssignmentDto;

namespace AYA_UIS.Application.Handlers.Assignments.UnitTests
{
    [TestClass]
    public class GetStudentAssignmentGradesQueryHandlerTests
    {
        /// <summary>
        /// Verifies that when the repository returns no assignments, Handle returns an empty collection.
        /// Input: repository returns empty assignments list for given CourseId.
        /// Expected: returned enumerable is non-null and has zero elements.
        /// </summary>
        [TestMethod]
        public async Task Handle_NoAssignments_ReturnsEmptyEnumerable()
        {
            // Arrange
            var mockRepo = new Mock<IAssignmentRepository>(MockBehavior.Strict);
            mockRepo.Setup(r => r.GetAssignmentsByCourseIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(new List<Assignment>());

            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            mockUnitOfWork.Setup(u => u.Assignments).Returns(mockRepo.Object);

            var handler = new GetStudentAssignmentGradesQueryHandler(mockUnitOfWork.Object);
            var request = new GetStudentAssignmentGradesQuery { CourseId = 123, StudentId = "student-1" };

            // Act
            IEnumerable<StudentAssignmentGradeDto> result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        /// <summary>
        /// Verifies mapping behavior for assignments with:
        /// - A matching submission (multiple submissions for same student: first should be picked)
        /// - No matching submission (DTO fields from submission should be null)
        /// Also exercises numeric and DateTime boundary values for Assignment fields.
        /// Input: two assignments; one has two submissions both from the target student; the other has submissions only from other students.
        /// Expected: first assignment DTO maps the first matching submission values; second assignment DTO has null SubmittedAt/Grade/Feedback/FileUrl.
        /// </summary>
        [TestMethod]
        public async Task Handle_AssignmentsWithAndWithoutMatchingSubmission_MapsCorrectly()
        {
            // Arrange
            const string targetStudentId = "s1";
            var assignment1 = new Assignment
            {
                Id = int.MaxValue,
                Title = "Extremes Assignment",
                Points = int.MaxValue,
                Deadline = DateTime.MaxValue
            };

            var assignment2 = new Assignment
            {
                Id = int.MinValue + 1, // avoid potential framework special Ids but still a boundary-like value
                Title = "NoSubmissionAssignment",
                Points = 0,
                Deadline = DateTime.MinValue
            };

            var assignments = new List<Assignment> { assignment1, assignment2 };

            var firstMatching = new AssignmentSubmission
            {
                Id = 100,
                AssignmentId = assignment1.Id,
                StudentId = targetStudentId,
                SubmittedAt = new DateTime(2021, 1, 1),
                Grade = 95,
                Feedback = "Excellent",
                FileUrl = "http://file1"
            };

            var secondMatching = new AssignmentSubmission
            {
                Id = 101,
                AssignmentId = assignment1.Id,
                StudentId = targetStudentId,
                SubmittedAt = new DateTime(2022, 1, 1),
                Grade = 50,
                Feedback = "Late",
                FileUrl = "http://file2"
            };

            var otherStudentSubmission = new AssignmentSubmission
            {
                Id = 200,
                AssignmentId = assignment2.Id,
                StudentId = "other",
                SubmittedAt = new DateTime(2020, 6, 1),
                Grade = null,
                Feedback = null,
                FileUrl = string.Empty
            };

            var mockRepo = new Mock<IAssignmentRepository>(MockBehavior.Strict);
            mockRepo.Setup(r => r.GetAssignmentsByCourseIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(assignments);
            // Setup GetSubmissions for assignment1 to return two matching submissions (first should be chosen)
            mockRepo.Setup(r => r.GetSubmissions(It.Is<int>(id => id == assignment1.Id)))
                    .ReturnsAsync(new List<AssignmentSubmission> { firstMatching, secondMatching });
            // Setup GetSubmissions for assignment2 to return only other student's submission
            mockRepo.Setup(r => r.GetSubmissions(It.Is<int>(id => id == assignment2.Id)))
                    .ReturnsAsync(new List<AssignmentSubmission> { otherStudentSubmission });

            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            mockUnitOfWork.Setup(u => u.Assignments).Returns(mockRepo.Object);

            var handler = new GetStudentAssignmentGradesQueryHandler(mockUnitOfWork.Object);
            var request = new GetStudentAssignmentGradesQuery { CourseId = 999, StudentId = targetStudentId };

            // Act
            var results = (await handler.Handle(request, CancellationToken.None)).ToList();

            // Assert - two assignments processed
            Assert.AreEqual(2, results.Count);

            // Verify mapping for assignment1 -> should pick firstMatching
            var dto1 = results.SingleOrDefault(r => r.AssignmentId == assignment1.Id);
            Assert.IsNotNull(dto1);
            Assert.AreEqual(assignment1.Title, dto1!.AssignmentTitle);
            Assert.AreEqual(assignment1.Points, dto1.Points);
            Assert.AreEqual(assignment1.Deadline, dto1.Deadline);
            Assert.AreEqual(firstMatching.SubmittedAt, dto1.SubmittedAt);
            Assert.AreEqual(firstMatching.Grade, dto1.Grade);
            Assert.AreEqual(firstMatching.Feedback, dto1.Feedback);
            Assert.AreEqual(firstMatching.FileUrl, dto1.FileUrl);

            // Verify mapping for assignment2 -> no matching submission => submission-related fields null
            var dto2 = results.SingleOrDefault(r => r.AssignmentId == assignment2.Id);
            Assert.IsNotNull(dto2);
            Assert.AreEqual(assignment2.Title, dto2!.AssignmentTitle);
            Assert.AreEqual(assignment2.Points, dto2.Points);
            Assert.AreEqual(assignment2.Deadline, dto2.Deadline);
            Assert.IsNull(dto2.SubmittedAt);
            Assert.IsNull(dto2.Grade);
            Assert.IsNull(dto2.Feedback);
            Assert.IsNull(dto2.FileUrl);
        }

        /// <summary>
        /// Verifies behavior when GetSubmissions returns null for an assignment.
        /// Input: repository returns one assignment and null for its submissions.
        /// Expected: Handler throws an ArgumentNullException because LINQ FirstOrDefault is invoked on a null sequence.
        /// Note: This test documents current behavior and expects an exception.
        /// </summary>
        [TestMethod]
        public async Task Handle_GetSubmissionsReturnsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var assignment = new Assignment
            {
                Id = 42,
                Title = "AssignmentWithNullSubs",
                Points = 10,
                Deadline = DateTime.UtcNow
            };

            var mockRepo = new Mock<IAssignmentRepository>(MockBehavior.Strict);
            mockRepo.Setup(r => r.GetAssignmentsByCourseIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(new List<Assignment> { assignment });
            // Intentionally return null to exercise null handling (or lack thereof) in handler
            mockRepo.Setup(r => r.GetSubmissions(It.IsAny<int>()))
                    .ReturnsAsync((IEnumerable<AssignmentSubmission>?)null);

            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            mockUnitOfWork.Setup(u => u.Assignments).Returns(mockRepo.Object);

            var handler = new GetStudentAssignmentGradesQueryHandler(mockUnitOfWork.Object);
            var request = new GetStudentAssignmentGradesQuery { CourseId = 1, StudentId = "any" };

            // Act & Assert
            try
            {
                await handler.Handle(request, CancellationToken.None);
                Assert.Fail("Expected ArgumentNullException was not thrown when GetSubmissions returned null.");
            }
            catch (ArgumentNullException)
            {
                // Expected - current implementation calls LINQ FirstOrDefault on a null sequence which throws ArgumentNullException
            }
        }

        /// <summary>
        /// Verifies that the constructor creates an instance when provided with a valid IUnitOfWork.
        /// Input conditions: a non-null mocked IUnitOfWork (both loose and strict MockBehavior).
        /// Expected result: construction succeeds (no exception), instance is not null and implements the expected IRequestHandler interface.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidUnitOfWork_DoesNotThrowAndImplementsInterface()
        {
            // Arrange
            var behaviors = new[] { MockBehavior.Loose, MockBehavior.Strict };

            foreach (var behavior in behaviors)
            {
                var mockUow = new Mock<IUnitOfWork>(behavior);

                // Act
                GetStudentAssignmentGradesQueryHandler? handler = null;
                try
                {
                    handler = new GetStudentAssignmentGradesQueryHandler(mockUow.Object);
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Constructor threw an unexpected exception for MockBehavior '{behavior}': {ex}");
                }

                // Assert
                Assert.IsNotNull(handler, "Handler instance should not be null for a valid IUnitOfWork.");
                Assert.IsInstanceOfType(
                    handler,
                    typeof(IRequestHandler<GetStudentAssignmentGradesQuery, IEnumerable<StudentAssignmentGradeDto>>),
                    "Handler should implement IRequestHandler<GetStudentAssignmentGradesQuery, IEnumerable<StudentAssignmentGradeDto>>.");
            }
        }

        /// <summary>
        /// Ensures multiple, distinct mocked IUnitOfWork instances can be supplied across constructions.
        /// Input conditions: two different mocks of IUnitOfWork with no setups.
        /// Expected result: each construction produces a separate non-null handler instance without throwing.
        /// </summary>
        [TestMethod]
        public void Constructor_DifferentUnitOfWorkInstances_CreateDistinctHandlerInstances()
        {
            // Arrange
            var mock1 = new Mock<IUnitOfWork>(MockBehavior.Loose);
            var mock2 = new Mock<IUnitOfWork>(MockBehavior.Loose);

            // Act
            GetStudentAssignmentGradesQueryHandler handler1 = new GetStudentAssignmentGradesQueryHandler(mock1.Object);
            GetStudentAssignmentGradesQueryHandler handler2 = new GetStudentAssignmentGradesQueryHandler(mock2.Object);

            // Assert
            Assert.IsNotNull(handler1, "First handler instance should not be null.");
            Assert.IsNotNull(handler2, "Second handler instance should not be null.");
            Assert.AreNotSame(handler1, handler2, "Separate constructor calls should produce distinct handler instances.");
            Assert.IsInstanceOfType(handler1, typeof(IRequestHandler<GetStudentAssignmentGradesQuery, IEnumerable<StudentAssignmentGradeDto>>));
            Assert.IsInstanceOfType(handler2, typeof(IRequestHandler<GetStudentAssignmentGradesQuery, IEnumerable<StudentAssignmentGradeDto>>));
        }
    }
}