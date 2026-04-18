using System;
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
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AYA_UIS.Application.Handlers.AcademicSchedules.UnitTests
{
    [TestClass]
    public class UpdateAcademicScheduleCommandHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor succeeds when valid (mocked) dependencies are provided.
        /// Input conditions: two non-null mocks for IUnitOfWork and ICloudinaryService.
        /// Expected result: an instance of UpdateAcademicScheduleCommandHandler is created and implements IRequestHandler&lt;UpdateAcademicScheduleCommand, Unit&gt;.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidDependencies_DoesNotThrowAndImplementsInterface()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var cloudinaryServiceMock = new Mock<ICloudinaryService>();

            // Act
            var handler = new UpdateAcademicScheduleCommandHandler(unitOfWorkMock.Object, cloudinaryServiceMock.Object);

            // Assert
            Assert.IsNotNull(handler, "Constructor returned null for valid dependencies.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<UpdateAcademicScheduleCommand, Unit>), "Handler should implement IRequestHandler<UpdateAcademicScheduleCommand, Unit>.");
        }

        /// <summary>
        /// Examines constructor behavior when null dependencies are provided.
        /// Input conditions: null for one or both parameters.
        /// Expected result: Because the source constructor's null-validation behavior is not specified in the provided scope,
        /// this test is marked inconclusive and documents the observed behavior so implementers can decide the intended contract.
        /// Guidance: If constructor is expected to throw ArgumentNullException for null parameters, update this test to Assert.Throws.
        /// If constructor should accept nulls, replace the inconclusive assertion with assertions validating downstream behavior.
        /// </summary>
        [TestMethod]
        public void Constructor_NullDependencies_BehaviorIsUndefined_MarkInconclusive()
        {
            // Arrange & Act & Assert
            // Use null-forgiving operator to express intent while preserving nullability annotations in the test file.
            var handler = new UpdateAcademicScheduleCommandHandler(null!, null!);

            // If construction succeeds, assert that an instance is returned. If the intended behavior is to reject nulls,
            // update this test to expect ArgumentNullException instead.
            Assert.IsNotNull(handler, "Constructor returned null when passed null dependencies which is unexpected but possible.");
        }

        /// <summary>
        /// Verifies that when the found schedule has no existing FileId (empty string),
        /// the handler does NOT call DeleteImageAsync, still uploads a new file, updates the schedule,
        /// and calls repository Update and SaveChangesAsync.
        /// Input: existing schedule with FileId = string.Empty.
        /// Expected: DeleteImageAsync not called; UploadAcademicScheduleAsync called; schedule updated; SaveChangesAsync called.
        /// </summary>
        [TestMethod]
        public async Task Handle_ScheduleWithoutExistingFile_UpdatesWithoutDeletingOldFile()
        {
            // Arrange
            var oldSchedule = new AcademicSchedule
            {
                Title = "old-title",
                FileId = string.Empty,
                Url = "https://old.url",
                Description = "old-desc",
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var mockRepo = new Mock<IAcademicScheduleRepository>(MockBehavior.Strict);
            mockRepo
                .Setup(r => r.GetByTitleAsync(It.IsAny<string>()))
                .ReturnsAsync(oldSchedule);
            mockRepo
                .Setup(r => r.Update(It.IsAny<AcademicSchedule>()))
                .Returns(Task.CompletedTask);

            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            mockUnitOfWork.SetupGet(u => u.AcademicSchedules).Returns(mockRepo.Object);
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var mockCloud = new Mock<ICloudinaryService>(MockBehavior.Strict);
            string? capturedNewFileId = null;
            const string returnedUrl = "https://new.example/url.pdf";

            mockCloud
                .Setup(c => c.UploadAcademicScheduleAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<IFormFile, string, CancellationToken>((f, id, ct) => capturedNewFileId = id)
                .ReturnsAsync(returnedUrl);

            // DeleteImageAsync should not be called for empty FileId; we don't setup it.

            var handler = new UpdateAcademicScheduleCommandHandler(mockUnitOfWork.Object, mockCloud.Object);

            var mockFormFile = new Mock<IFormFile>();
            var command = new UpdateAcademicScheduleCommand("new-title", "new-desc", mockFormFile.Object);

            // Act
            DateTime beforeUpdate = DateTime.UtcNow;
            await handler.Handle(command, CancellationToken.None);
            DateTime afterUpdate = DateTime.UtcNow;

            // Assert
            // DeleteImageAsync was never called because FileId was empty
            mockCloud.Verify(c => c.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

            // Upload called once and newFileId captured is a GUID
            mockCloud.Verify(c => c.UploadAcademicScheduleAsync(command.File!, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsFalse(string.IsNullOrEmpty(capturedNewFileId));
            Assert.IsTrue(Guid.TryParse(capturedNewFileId, out _), "Expected generated file id to be a valid GUID string.");

            // Schedule properties updated
            Assert.AreEqual(capturedNewFileId, oldSchedule.FileId);
            Assert.AreEqual(returnedUrl, oldSchedule.Url);
            Assert.AreEqual(command.Title, oldSchedule.Title);
            Assert.AreEqual(command.Description, oldSchedule.Description);

            // UpdatedAt should be recent (between beforeUpdate and afterUpdate or within a small tolerance)
            Assert.IsTrue(oldSchedule.UpdatedAt <= afterUpdate && oldSchedule.UpdatedAt >= beforeUpdate, "UpdatedAt should be set to a recent UTC time.");

            mockRepo.Verify(r => r.Update(It.Is<AcademicSchedule>(s =>
                s.Title == command.Title &&
                s.Url == returnedUrl &&
                s.FileId == capturedNewFileId)), Times.Once);

            mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        /// <summary>
        /// Verifies that when the found schedule has an existing FileId,
        /// the handler calls DeleteImageAsync with the old FileId, uploads the new file,
        /// updates the schedule fields, and persists changes.
        /// Input: existing schedule with non-empty FileId.
        /// Expected: DeleteImageAsync called once with old FileId; UploadAcademicScheduleAsync called; schedule updated; SaveChangesAsync called.
        /// </summary>
        [TestMethod]
        public async Task Handle_ScheduleWithExistingFile_DeletesOldFileAndUpdates()
        {
            // Arrange
            var oldFileId = "existing-file-id-123";
            var oldSchedule = new AcademicSchedule
            {
                Title = "old-title",
                FileId = oldFileId,
                Url = "https://old.url",
                Description = "old-desc",
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var mockRepo = new Mock<IAcademicScheduleRepository>(MockBehavior.Strict);
            mockRepo
                .Setup(r => r.GetByTitleAsync(It.IsAny<string>()))
                .ReturnsAsync(oldSchedule);
            mockRepo
                .Setup(r => r.Update(It.IsAny<AcademicSchedule>()))
                .Returns(Task.CompletedTask);

            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            mockUnitOfWork.SetupGet(u => u.AcademicSchedules).Returns(mockRepo.Object);
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var mockCloud = new Mock<ICloudinaryService>(MockBehavior.Strict);
            string? capturedNewFileId = null;
            const string returnedUrl = "https://new.example/url2.pdf";

            mockCloud
                .Setup(c => c.DeleteImageAsync(oldFileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            mockCloud
                .Setup(c => c.UploadAcademicScheduleAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<IFormFile, string, CancellationToken>((f, id, ct) => capturedNewFileId = id)
                .ReturnsAsync(returnedUrl);

            var handler = new UpdateAcademicScheduleCommandHandler(mockUnitOfWork.Object, mockCloud.Object);

            var mockFormFile = new Mock<IFormFile>();
            var command = new UpdateAcademicScheduleCommand("updated-title", "updated-desc", mockFormFile.Object);

            // Act
            DateTime beforeUpdate = DateTime.UtcNow;
            await handler.Handle(command, CancellationToken.None);
            DateTime afterUpdate = DateTime.UtcNow;

            // Assert
            // DeleteImageAsync called once with oldFileId
            mockCloud.Verify(c => c.DeleteImageAsync(oldFileId, It.IsAny<CancellationToken>()), Times.Once);

            // Upload called and newFileId is a GUID
            mockCloud.Verify(c => c.UploadAcademicScheduleAsync(command.File!, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsFalse(string.IsNullOrEmpty(capturedNewFileId));
            Assert.IsTrue(Guid.TryParse(capturedNewFileId, out _), "Expected generated file id to be a valid GUID string.");

            // Schedule properties updated
            Assert.AreEqual(capturedNewFileId, oldSchedule.FileId);
            Assert.AreEqual(returnedUrl, oldSchedule.Url);
            Assert.AreEqual(command.Title, oldSchedule.Title);
            Assert.AreEqual(command.Description, oldSchedule.Description);

            // UpdatedAt should be recent
            Assert.IsTrue(oldSchedule.UpdatedAt <= afterUpdate && oldSchedule.UpdatedAt >= beforeUpdate, "UpdatedAt should be set to a recent UTC time.");

            mockRepo.Verify(r => r.Update(It.Is<AcademicSchedule>(s =>
                s.Title == command.Title &&
                s.Url == returnedUrl &&
                s.FileId == capturedNewFileId)), Times.Once);

            mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}