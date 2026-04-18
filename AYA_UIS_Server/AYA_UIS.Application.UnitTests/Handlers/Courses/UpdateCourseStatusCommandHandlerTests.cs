using System;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Courses;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AYA_UIS.Application.UnitTests
{
    [TestClass]
    public class UpdateCourseStatusCommandHandlerTests
    {
        /// <summary>
        /// Verifies that when a course exists for a variety of CourseId numeric edge values
        /// and for each defined CourseStatus, the handler updates the entity's Status,
        /// calls SaveChangesAsync once, and returns MediatR.Unit.
        /// Inputs tested: int.MinValue, negative, zero, positive, int.MaxValue and all enum values.
        /// Expected: course.Status equals requested Status and SaveChangesAsync is invoked exactly once.
        /// </summary>
        [TestMethod]
        public async Task Handle_CourseExists_UpdatesStatusAndSaves()
        {
            // Arrange - test a range of numeric ids and all enum values
            int[] idsToTest = new[] { int.MinValue, -1, 0, 1, int.MaxValue };
            var statuses = (AYA_UIS.Core.Domain.Enums.CourseStatus[])Enum.GetValues(typeof(AYA_UIS.Core.Domain.Enums.CourseStatus));

            foreach (int id in idsToTest)
            {
                foreach (var targetStatus in statuses)
                {
                    // Arrange
                    var courseRepoMock = new Mock<ICourseRepository>(MockBehavior.Strict);
                    var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

                    // Create a course with an initial status different from the target to detect change
                    var initialStatus = targetStatus == AYA_UIS.Core.Domain.Enums.CourseStatus.Opened
                        ? AYA_UIS.Core.Domain.Enums.CourseStatus.Closed
                        : AYA_UIS.Core.Domain.Enums.CourseStatus.Opened;

                    var course = new Course
                    {
                        Status = initialStatus
                    };

                    courseRepoMock
                        .Setup(r => r.GetByIdAsync(id))
                        .ReturnsAsync(course)
                        .Verifiable();

                    unitOfWorkMock
                        .SetupGet(u => u.Courses)
                        .Returns(courseRepoMock.Object);

                    unitOfWorkMock
                        .Setup(u => u.SaveChangesAsync())
                        .ReturnsAsync(1)
                        .Verifiable();

                    var handler = new UpdateCourseStatusCommandHandler(unitOfWorkMock.Object);

                    var request = new UpdateCourseStatusCommand(id, targetStatus);

                    // Act
                    var result = await handler.Handle(request, CancellationToken.None);

                    // Assert
                    Assert.AreEqual(Unit.Value, result, "Handler should return Unit.Value for successful update.");
                    Assert.AreEqual(targetStatus, course.Status, "Course.Status should be updated to the requested status.");

                    courseRepoMock.Verify(r => r.GetByIdAsync(id), Times.Once, "GetByIdAsync should be called exactly once for the tested id.");
                    unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once, "SaveChangesAsync should be called exactly once for successful update.");

                    // Cleanup verifications for next iteration
                    courseRepoMock.VerifyAll();
                    unitOfWorkMock.VerifyAll();
                }
            }
        }

        /// <summary>
        /// Ensures that providing a valid IUnitOfWork instance to the constructor
        /// does not throw and that the constructor does not call any members on the dependency.
        /// This verifies the constructor performs only assignment (no side-effects).
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidUnitOfWork_DoesNotThrowAndDoesNotCallUnitOfWork()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            UpdateCourseStatusCommandHandler? handler = null;
            Exception? caught = null;
            try
            {
                handler = new UpdateCourseStatusCommandHandler(unitOfWorkMock.Object);
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            // Assert
            // Constructor should not throw for a valid IUnitOfWork instance.
            Assert.IsNull(caught, $"Constructor threw an unexpected exception: {caught?.Message}");
            Assert.IsNotNull(handler, "Handler instance should be created.");

            // Verify that constructor did not call any members on the injected unit of work.
            // This asserts the constructor performs only assignment and no side effects.
            unitOfWorkMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Ensures that multiple distinct IUnitOfWork instances can be supplied to separate handler instances
        /// and that constructing the handlers does not cause cross-interaction or calls on the provided dependencies.
        /// </summary>
        [TestMethod]
        public void Constructor_WithDifferentUnitOfWorkInstances_CreatesDistinctHandlerInstancesWithoutCalls()
        {
            // Arrange
            var firstMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var secondMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            var firstHandler = new UpdateCourseStatusCommandHandler(firstMock.Object);
            var secondHandler = new UpdateCourseStatusCommandHandler(secondMock.Object);

            // Assert
            Assert.IsNotNull(firstHandler, "First handler should be created.");
            Assert.IsNotNull(secondHandler, "Second handler should be created.");
            Assert.AreNotSame(firstHandler, secondHandler, "Handlers created with different dependencies should be distinct instances.");

            // Ensure neither constructor invoked any members on their respective unit of work mocks.
            firstMock.VerifyNoOtherCalls();
            secondMock.VerifyNoOtherCalls();
        }

        // NOTE:
        // The constructor's parameter type IUnitOfWork is non-nullable in the production code.
        // The test suite intentionally does not pass null to the constructor to respect nullability annotations.
        // If the production constructor is later updated to validate and throw for null inputs,
        // an additional test asserting ArgumentNullException should be added at that time.
    }
}