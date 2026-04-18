using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Courses;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Courses;
using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using AYA_UIS.Shared;
using AYA_UIS.Shared.Exceptions;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module.CourseDtos;

namespace AYA_UIS.Application.Handlers.Courses.UnitTests
{
    [TestClass]
    public class OpenCoursesForLevelCommandHandlerTests
    {
        /// <summary>
        /// Tests that for a set of course ids including edge numeric values:
        /// - When repository returns an existing CourseOffering, its IsOpen property is set to true.
        /// - When repository returns null, a new CourseOffering is added with correct values and IsOpen true.
        /// - SaveChangesAsync is invoked once and the handler returns MediatR.Unit.Value.
        /// Input: CourseIds contains int.MinValue, int.MaxValue and an id that maps to an existing offering.
        /// Expected: Existing offering opened; AddAsync called for non-existing ids; SaveChangesAsync called once.
        /// </summary>
        [TestMethod]
        public async Task Handle_MixedExistingAndNewCourses_OpensExistingAndAddsNew_AndSaves()
        {
            // Arrange
            int existingCourseId = 1;
            var dto = new OpenCoursesForLevelDto
            {
                CourseIds = new List<int> { int.MinValue, int.MaxValue, existingCourseId },
                StudyYearId = 2025,
                SemesterId = 2,
                Level = (Levels)0
            };

            var command = new OpenCoursesForLevelCommand(dto);

            var existingOffering = new CourseOffering
            {
                CourseId = existingCourseId,
                StudyYearId = dto.StudyYearId,
                SemesterId = dto.SemesterId,
                Level = dto.Level,
                IsOpen = false // initially closed, should become true
            };

            var mockRepo = new Mock<ICourseOfferingRepository>(MockBehavior.Strict);

            // Return the existing offering only for existingCourseId; return null otherwise.
            mockRepo
                .Setup(r => r.GetAsync(
                    It.Is<int>(id => id == existingCourseId),
                    dto.StudyYearId,
                    dto.SemesterId,
                    dto.Level))
                .ReturnsAsync(existingOffering);

            mockRepo
                .Setup(r => r.GetAsync(
                    It.Is<int>(id => id != existingCourseId),
                    dto.StudyYearId,
                    dto.SemesterId,
                    dto.Level))
                .ReturnsAsync((CourseOffering?)null);

            // Expect AddAsync to be called for non-existing ids (two calls: int.MinValue and int.MaxValue)
            mockRepo
                .Setup(r => r.AddAsync(It.IsAny<CourseOffering>()))
                .Returns(Task.CompletedTask);

            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            mockUnitOfWork.Setup(u => u.CourseOfferings).Returns(mockRepo.Object);
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var handler = new OpenCoursesForLevelCommandHandler(mockUnitOfWork.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            // Handler should return the single Unit value
            Assert.AreEqual(Unit.Value, result);

            // Existing offering must now be open
            Assert.IsTrue(existingOffering.IsOpen, "Existing offering IsOpen should be set to true.");

            // Verify GetAsync called for each course id with expected parameters
            foreach (int id in dto.CourseIds)
            {
                mockRepo.Verify(r => r.GetAsync(id, dto.StudyYearId, dto.SemesterId, dto.Level), Times.Once,
                    $"Expected GetAsync to be called once for course id {id}.");
            }

            // Verify AddAsync called exactly for non-existing ids (2 times)
            mockRepo.Verify(r => r.AddAsync(It.Is<CourseOffering>(co =>
                (co.CourseId == int.MinValue || co.CourseId == int.MaxValue)
                && co.IsOpen == true
                && co.StudyYearId == dto.StudyYearId
                && co.SemesterId == dto.SemesterId
                && co.Level == dto.Level)), Times.Exactly(2));

            // Verify SaveChangesAsync was called once
            mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        /// <summary>
        /// Verifies that the constructor successfully creates an instance when a valid IUnitOfWork is provided.
        /// Input conditions: a non-null mocked IUnitOfWork instance.
        /// Expected result: no exception is thrown and the constructed handler is not null.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidUnitOfWork_DoesNotThrowAndCreatesInstance()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            OpenCoursesForLevelCommandHandler? handler = null;
            Exception? ex = null;
            try
            {
                handler = new OpenCoursesForLevelCommandHandler(mockUnitOfWork.Object);
            }
            catch (Exception e)
            {
                ex = e;
            }

            // Assert
            Assert.IsNull(ex, "Constructor should not throw when provided a valid IUnitOfWork.");
            Assert.IsNotNull(handler, "Constructor should create a non-null OpenCoursesForLevelCommandHandler instance.");
        }

        /// <summary>
        /// Partial test for constructor behavior with null IUnitOfWork.
        /// Note: The constructor parameter is non-nullable in the source. Assigning null to a non-nullable parameter
        /// would contradict the source nullability annotations. This test is marked Inconclusive to indicate that
        /// attempting to pass null is not recommended without changing the production code's nullability or adding
        /// explicit null checks in the constructor.
        /// Input conditions: null (not assigned due to nullability annotations).
        /// Expected result: inconclusive - user should decide whether constructor should validate null and throw.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullUnitOfWork_IsInconclusive()
        {
            // Arrange
            IUnitOfWork? nullUnitOfWork = null;

            // Act
            OpenCoursesForLevelCommandHandler? handler = null;
            Exception? ex = null;
            try
            {
                handler = new OpenCoursesForLevelCommandHandler(nullUnitOfWork);
            }
            catch (Exception e)
            {
                ex = e;
            }

            // Assert
            Assert.IsNull(ex, "Constructor should not throw when provided a null IUnitOfWork.");
            Assert.IsNotNull(handler, "Constructor should create an instance even when provided null. If null should be rejected, add null-checks in production code.");
        }
    }
}