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
    public partial class GetAssignmentsByCourseQueryHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor accepts a non-null IUnitOfWork and constructs an instance.
        /// Input conditions: a mocked IUnitOfWork instance (non-null).
        /// Expected result: no exception is thrown, returned handler is non-null and implements the expected IRequestHandler interface.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNonNullUnitOfWork_CreatesHandlerInstance()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Act
            var handler = new GetAssignmentsByCourseQueryHandler(mockUnitOfWork.Object);

            // Assert
            Assert.IsNotNull(handler, "Handler instance should not be null after construction with a valid IUnitOfWork.");
            // Ensure it implements the expected MediatR handler interface for type safety
            Assert.IsTrue(handler is IRequestHandler<GetAssignmentsByCourseQuery, IEnumerable<AssignmentDto>>,
                "Handler should implement IRequestHandler<GetAssignmentsByCourseQuery, IEnumerable<AssignmentDto>>.");
        }

        /// <summary>
        /// Ensures that constructing handlers with different IUnitOfWork instances produces distinct handler objects.
        /// Input conditions: two different mocked IUnitOfWork instances.
        /// Expected result: two separate handler instances are created (reference inequality) and both are not null.
        /// This helps detect accidental use of shared/static state in the constructor.
        /// </summary>
        [TestMethod]
        public void Constructor_WithDifferentUnitOfWorkInstances_CreatesDistinctHandlerInstances()
        {
            // Arrange
            var mockUnitOfWorkA = new Mock<IUnitOfWork>();
            var mockUnitOfWorkB = new Mock<IUnitOfWork>();

            // Act
            var handlerA = new GetAssignmentsByCourseQueryHandler(mockUnitOfWorkA.Object);
            var handlerB = new GetAssignmentsByCourseQueryHandler(mockUnitOfWorkB.Object);

            // Assert
            Assert.IsNotNull(handlerA, "First handler instance should not be null.");
            Assert.IsNotNull(handlerB, "Second handler instance should not be null.");
            Assert.AreNotSame(handlerA, handlerB, "Handler instances created with different IUnitOfWork instances should be distinct objects.");
        }

        /// <summary>
        /// Verifies that when the repository returns a populated assignment collection, Handle maps all fields correctly.
        /// Input: single Assignment with Course and CreatedBy populated.
        /// Expected: returned AssignmentDto sequence contains a mapped element with identical Id, Title, Description, Points, Deadline, FileUrl, CourseId,
        ///           CourseName from Course.Name and InstructorName from CreatedBy.DisplayName.
        /// </summary>
        [TestMethod]
        public async Task Handle_ValidAssignment_MapsPropertiesCorrectly()
        {
            // Arrange
            var repoMock = new Mock<IAssignmentRepository>();
            var uowMock = new Mock<IUnitOfWork>();
            uowMock.SetupGet(u => u.Assignments).Returns(repoMock.Object);

            var assignment = new Assignment
            {
                Id = 42,
                Title = "Unit Test Title",
                Description = "Desc",
                Points = 100,
                Deadline = new DateTime(2030, 1, 1),
                FileUrl = "/files/1",
                CourseId = 7,
                Course = new Course { Name = "Advanced Testing" },
                CreatedBy = new User { DisplayName = "Dr. Tester", UserName = "tester" }
            };

            repoMock.Setup(r => r.GetAssignmentsByCourseIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(new[] { assignment });

            var handler = new GetAssignmentsByCourseQueryHandler(uowMock.Object);
            var request = new GetAssignmentsByCourseQuery { CourseId = 7 };

            // Act
            var result = (await handler.Handle(request, CancellationToken.None)).ToList();

            // Assert
            Assert.AreEqual(1, result.Count, "Expected exactly one mapped DTO.");
            var dto = result[0];
            Assert.AreEqual(assignment.Id, dto.Id);
            Assert.AreEqual(assignment.Title, dto.Title);
            Assert.AreEqual(assignment.Description, dto.Description);
            Assert.AreEqual(assignment.Points, dto.Points);
            Assert.AreEqual(assignment.Deadline, dto.Deadline);
            Assert.AreEqual(assignment.FileUrl, dto.FileUrl);
            Assert.AreEqual(assignment.CourseId, dto.CourseId);
            Assert.AreEqual(assignment.Course?.Name ?? string.Empty, dto.CourseName);
            Assert.AreEqual(assignment.CreatedBy?.DisplayName ?? assignment.CreatedBy?.UserName ?? string.Empty, dto.InstructorName);

            // Verify repository was queried with the provided CourseId
            repoMock.Verify(r => r.GetAssignmentsByCourseIdAsync(7), Times.Once);
        }

        /// <summary>
        /// Tests edge behavior for nullable Course and CreatedBy and for empty DisplayName.
        /// Input: Assignment with Course = null and CreatedBy.DisplayName = empty string but UserName non-empty.
        /// Expected: CourseName becomes empty string; InstructorName equals DisplayName (empty string) because null-coalescing does not treat empty as null.
        /// </summary>
        [TestMethod]
        public async Task Handle_NullCourseAndEmptyDisplayName_ProducesEmptyCourseNameAndUsesEmptyDisplayName()
        {
            // Arrange
            var repoMock = new Mock<IAssignmentRepository>();
            var uowMock = new Mock<IUnitOfWork>();
            uowMock.SetupGet(u => u.Assignments).Returns(repoMock.Object);

            var assignment = new Assignment
            {
                Id = 3,
                Title = "Edge",
                Description = "Edge Desc",
                Points = 0,
                Deadline = DateTime.MinValue,
                FileUrl = string.Empty,
                CourseId = -1,
                Course = null, // should map to empty CourseName
                CreatedBy = new User
                {
                    DisplayName = string.Empty, // empty string is selected by the ?? operator (not fallback)
                    UserName = "user_fallback"
                }
            };

            repoMock.Setup(r => r.GetAssignmentsByCourseIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(new[] { assignment });

            var handler = new GetAssignmentsByCourseQueryHandler(uowMock.Object);
            var request = new GetAssignmentsByCourseQuery { CourseId = int.MinValue };

            // Act
            var dtos = (await handler.Handle(request, CancellationToken.None)).ToList();

            // Assert
            Assert.AreEqual(1, dtos.Count);
            var dto = dtos[0];
            Assert.AreEqual(string.Empty, dto.CourseName, "CourseName should be string.Empty when Course is null.");
            // Because DisplayName is an empty string (non-null), it should be used as InstructorName
            Assert.AreEqual(string.Empty, dto.InstructorName, "InstructorName should equal DisplayName even if it's empty (null-coalescing checks null only).");

            // verify repository called with the extreme CourseId
            repoMock.Verify(r => r.GetAssignmentsByCourseIdAsync(int.MinValue), Times.Once);
        }

        /// <summary>
        /// Verifies behavior across numeric boundary CourseId values when repository returns an empty collection.
        /// Input: three CourseIds: int.MinValue, 0, int.MaxValue and repository returns empty list for any.
        /// Expected: handler returns an empty sequence and repository is called with each CourseId.
        /// </summary>
        [TestMethod]
        public async Task Handle_VariousCourseIdBoundaries_WithNoAssignments_ReturnsEmptyAndInvokesRepository()
        {
            // Arrange
            var repoMock = new Mock<IAssignmentRepository>();
            var uowMock = new Mock<IUnitOfWork>();
            uowMock.SetupGet(u => u.Assignments).Returns(repoMock.Object);

            repoMock.Setup(r => r.GetAssignmentsByCourseIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(Array.Empty<Assignment>());

            var handler = new GetAssignmentsByCourseQueryHandler(uowMock.Object);

            var courseIds = new[] { int.MinValue, 0, int.MaxValue };

            // Act & Assert
            foreach (var id in courseIds)
            {
                var request = new GetAssignmentsByCourseQuery { CourseId = id };
                var result = await handler.Handle(request, CancellationToken.None);
                Assert.IsNotNull(result, "Handler should not return null when repository returns an empty collection.");
                Assert.IsFalse(result.Any(), $"Expected no assignments for CourseId {id}.");
                repoMock.Verify(r => r.GetAssignmentsByCourseIdAsync(id), Times.Once, $"Repository should be called once for CourseId {id}.");
            }
        }

    }
}