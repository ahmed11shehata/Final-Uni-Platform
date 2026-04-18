using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using AYA_UIS.Application;
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.AcademicSchedules;
using AYA_UIS.Application.Contracts;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.AcademicSchedules;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Shared;
using AYA_UIS.Shared.Exceptions;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AYA_UIS.Application.Handlers.AcademicSchedules.UnitTests
{
    [TestClass]
    public class DeleteAcademicScheduleByTitleCommandHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor creates an instance when valid, non-null dependencies are provided.
        /// Input conditions: valid mocks for IUnitOfWork and ICloudinaryService.
        /// Expected result: an instance of DeleteAcademicScheduleByTitleCommandHandler is created and implements the expected IRequestHandler interface.
        /// </summary>
        [TestMethod]
        public void DeleteAcademicScheduleByTitleCommandHandler_Constructor_WithValidDependencies_CreatesInstance()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var cloudinaryMock = new Mock<ICloudinaryService>(MockBehavior.Strict);

            // Act
            var handler = new DeleteAcademicScheduleByTitleCommandHandler(unitOfWorkMock.Object, cloudinaryMock.Object);

            // Assert
            Assert.IsNotNull(handler, "Constructor returned null for valid dependencies.");
            Assert.IsInstanceOfType(handler, typeof(DeleteAcademicScheduleByTitleCommandHandler), "Instance is not of the expected concrete type.");
            Assert.IsTrue(handler is IRequestHandler<DeleteAcademicScheduleByTitleCommand, bool>, "Instance does not implement the expected IRequestHandler interface.");
        }

        /// <summary>
        /// Examines constructor behavior when null dependencies are provided.
        /// Input conditions: null for IUnitOfWork and/or ICloudinaryService (both nullable local variables).
        /// Expected result: This test marks inconclusive if the constructor allows null (no explicit null validation present).
        /// If an ArgumentNullException is thrown by the constructor, the test will assert that exception.
        /// Note: The production constructor does not currently perform null checks; adjust constructor or this test according to intended design.
        /// </summary>
        [TestMethod]
        public void DeleteAcademicScheduleByTitleCommandHandler_Constructor_NullParameters_Inconclusive()
        {
            // Arrange
            IUnitOfWork? nullUnitOfWork = null;
            ICloudinaryService? nullCloudinaryService = null;

            try
            {
                // Act
                // Passing nulls to the constructor to observe behavior. The constructor signature does not declare parameters as nullable,
                // but runtime will accept null. Per project design, either constructor should validate and throw, or callers must ensure non-null.
                var handler = new DeleteAcademicScheduleByTitleCommandHandler(nullUnitOfWork!, nullCloudinaryService!);

                // Assert: current production behavior constructs the handler even with null dependencies.
                Assert.IsNotNull(handler, "Constructor returned null handler when passed null dependencies.");
            }
            catch (ArgumentNullException ex)
            {
                // Assert that if the constructor performs null validation, it throws ArgumentNullException.
                Assert.IsInstanceOfType(ex, typeof(ArgumentNullException));
            }
            catch (Exception ex)
            {
                // If a different exception type is thrown, fail with explanatory message to surface unexpected behavior.
                Assert.Fail($"Constructor threw unexpected exception type: {ex.GetType()}. Message: {ex.Message}");
            }
        }

        /// <summary>
        /// Test that when a schedule exists and its FileId is null or empty, the cloudinary service is not called,
        /// the schedule repository Delete is invoked and SaveChangesAsync is called. Expected return value: true.
        /// Input conditions: repository returns an AcademicSchedule with FileId = string.Empty.
        /// </summary>
        [TestMethod]
        public async Task Handle_ScheduleWithNoFileId_DeletesScheduleWithoutCallingCloudinary_ReturnsTrue()
        {
            // Arrange
            var scheduleTitle = "Schedule_NoFile";
            var schedule = new AcademicSchedule
            {
                Title = scheduleTitle,
                FileId = string.Empty
            };

            var repoMock = new Mock<IAcademicScheduleRepository>(MockBehavior.Strict);
            repoMock.Setup(r => r.GetByTitleAsync(It.Is<string>(s => s == scheduleTitle)))
                    .ReturnsAsync(schedule)
                    .Verifiable();

            repoMock.Setup(r => r.Delete(It.Is<AcademicSchedule>(a => ReferenceEquals(a, schedule))))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            unitOfWorkMock.SetupGet(u => u.AcademicSchedules).Returns(repoMock.Object);
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1).Verifiable();

            var cloudinaryMock = new Mock<ICloudinaryService>(MockBehavior.Strict);
            // No setup for DeleteImageAsync; strict mock will fail if called.

            var handler = new DeleteAcademicScheduleByTitleCommandHandler(unitOfWorkMock.Object, cloudinaryMock.Object);
            var request = new DeleteAcademicScheduleByTitleCommand(scheduleTitle);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsTrue(result);
            repoMock.Verify(r => r.GetByTitleAsync(scheduleTitle), Times.Once);
            repoMock.Verify(r => r.Delete(It.Is<AcademicSchedule>(a => ReferenceEquals(a, schedule))), Times.Once);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            cloudinaryMock.Verify(c => c.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Test that when a schedule exists and its FileId is whitespace-only (string.IsNullOrEmpty returns false),
        /// the cloudinary DeleteImageAsync is called with the FileId and the schedule is deleted and saved.
        /// Input conditions: repository returns an AcademicSchedule with FileId = whitespace.
        /// Expected result: DeleteImageAsync invoked, Delete invoked, SaveChangesAsync invoked, and handler returns true.
        /// </summary>
        [TestMethod]
        public async Task Handle_ScheduleWithWhitespaceFileId_CallsCloudinaryAndDeletes_ReturnsTrue()
        {
            // Arrange
            var scheduleTitle = "Schedule_WhitespaceFileId";
            var whitespaceFileId = "   ";
            var schedule = new AcademicSchedule
            {
                Title = scheduleTitle,
                FileId = whitespaceFileId
            };

            var repoMock = new Mock<IAcademicScheduleRepository>(MockBehavior.Strict);
            repoMock.Setup(r => r.GetByTitleAsync(It.Is<string>(s => s == scheduleTitle)))
                    .ReturnsAsync(schedule)
                    .Verifiable();

            repoMock.Setup(r => r.Delete(It.Is<AcademicSchedule>(a => ReferenceEquals(a, schedule))))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            unitOfWorkMock.SetupGet(u => u.AcademicSchedules).Returns(repoMock.Object);
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1).Verifiable();

            var cloudinaryMock = new Mock<ICloudinaryService>(MockBehavior.Strict);
            cloudinaryMock.Setup(c => c.DeleteImageAsync(It.Is<string>(id => id == whitespaceFileId), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(true)
                          .Verifiable();

            var handler = new DeleteAcademicScheduleByTitleCommandHandler(unitOfWorkMock.Object, cloudinaryMock.Object);
            var request = new DeleteAcademicScheduleByTitleCommand(scheduleTitle);
            var ct = new CancellationTokenSource().Token;

            // Act
            var result = await handler.Handle(request, ct);

            // Assert
            Assert.IsTrue(result);
            cloudinaryMock.Verify(c => c.DeleteImageAsync(whitespaceFileId, ct), Times.Once);
            repoMock.Verify(r => r.Delete(It.Is<AcademicSchedule>(a => ReferenceEquals(a, schedule))), Times.Once);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

    }
}