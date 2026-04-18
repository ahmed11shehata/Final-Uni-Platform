using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.UserStudyYears;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.UserStudyYears;
using AYA_UIS.Core.Domain;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.UserStudyYearDtos;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.UserStudyYears.UnitTests
{
    [TestClass]
    public class CreateUserStudyYearCommandHandlerTests
    {
        /// <summary>
        /// Test that when the study year repository returns null for various StudyYearId extremes,
        /// the handler returns an error response indicating the study year was not found.
        /// Inputs tested: int.MinValue, 0, int.MaxValue as StudyYearId.
        /// Expected: Response.Success == false and Errors == "Study year not found."
        /// </summary>
        [TestMethod]
        public async Task Handle_StudyYearNotFound_ReturnsError_ForVariousStudyYearIds()
        {
            // Arrange
            var testedIds = new[] { int.MinValue, 0, int.MaxValue };

            foreach (var id in testedIds)
            {
                var mockUow = new Mock<IUnitOfWork>(MockBehavior.Strict);
                var mockStudyYearRepo = new Mock<IStudyYearRepository>(MockBehavior.Strict);
                var mockUserStudyYearRepo = new Mock<IUserStudyYearRepository>(MockBehavior.Strict);

                // StudyYears.GetByIdAsync returns null to simulate not found
                mockStudyYearRepo
                    .Setup(r => r.GetByIdAsync(id))
                    .ReturnsAsync((StudyYear?)null)
                    .Verifiable();

                mockUow.Setup(u => u.StudyYears).Returns(mockStudyYearRepo.Object);
                mockUow.Setup(u => u.UserStudyYears).Returns(mockUserStudyYearRepo.Object);

                var handler = new CreateUserStudyYearCommandHandler(mockUow.Object);

                var dto = new CreateUserStudyYearDto
                {
                    UserId = "user-" + id,
                    StudyYearId = id,
                    Level = Levels.First_Year
                };
                var command = new CreateUserStudyYearCommand(dto);

                // Act
                var response = await handler.Handle(command, CancellationToken.None);

                // Assert
                Assert.IsNotNull(response);
                Assert.IsFalse(response.Success);
                Assert.AreEqual("Study year not found.", response.Errors);

                mockStudyYearRepo.Verify(r => r.GetByIdAsync(id), Times.Once);
                // No add or save should be called; ensure no unexpected calls
                mockUserStudyYearRepo.VerifyNoOtherCalls();
                mockStudyYearRepo.VerifyNoOtherCalls();
                mockUow.Verify(u => u.StudyYears, Times.AtLeastOnce);
                mockUow.Verify(u => u.UserStudyYears, Times.Never);
            }
        }

        /// <summary>
        /// Test that when the user is already enrolled (existing record returned),
        /// the handler returns an error response indicating duplicate enrollment and does not add or save changes.
        /// Input: valid StudyYear exists and existing UserStudyYear returned from repository.
        /// Expected: Response.Success == false and Errors == "User is already enrolled in this study year."
        /// </summary>
        [TestMethod]
        public async Task Handle_UserAlreadyEnrolled_ReturnsError_AndDoesNotAddOrSave()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mockStudyYearRepo = new Mock<IStudyYearRepository>(MockBehavior.Strict);
            var mockUserStudyYearRepo = new Mock<IUserStudyYearRepository>(MockBehavior.Strict);

            var dto = new CreateUserStudyYearDto
            {
                UserId = "existing-user",
                StudyYearId = 10,
                Level = Levels.Second_Year
            };

            var existingUserStudy = new UserStudyYear
            {
                Id = 1,
                UserId = dto.UserId,
                StudyYearId = dto.StudyYearId,
                Level = dto.Level,
                EnrolledAt = DateTime.UtcNow
            };

            // StudyYear exists
            mockStudyYearRepo
                .Setup(r => r.GetByIdAsync(dto.StudyYearId))
                .ReturnsAsync(new StudyYear { Id = dto.StudyYearId })
                .Verifiable();

            // Existing enrollment returned
            mockUserStudyYearRepo
                .Setup(r => r.GetByUserAndStudyYearAsync(dto.UserId, dto.StudyYearId))
                .ReturnsAsync(existingUserStudy)
                .Verifiable();

            mockUow.Setup(u => u.StudyYears).Returns(mockStudyYearRepo.Object);
            mockUow.Setup(u => u.UserStudyYears).Returns(mockUserStudyYearRepo.Object);

            var handler = new CreateUserStudyYearCommandHandler(mockUow.Object);
            var command = new CreateUserStudyYearCommand(dto);

            // Act
            var response = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.AreEqual("User is already enrolled in this study year.", response.Errors);

            mockStudyYearRepo.Verify(r => r.GetByIdAsync(dto.StudyYearId), Times.Once);
            mockUserStudyYearRepo.Verify(r => r.GetByUserAndStudyYearAsync(dto.UserId, dto.StudyYearId), Times.Once);

            // Ensure AddAsync and SaveChangesAsync were NOT called
            mockUserStudyYearRepo.Verify(r => r.AddAsync(It.IsAny<UserStudyYear>()), Times.Never);
            mockUow.Verify(u => u.SaveChangesAsync(), Times.Never);

            mockStudyYearRepo.VerifyNoOtherCalls();
            mockUserStudyYearRepo.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Test that a valid request results in adding a new UserStudyYear entity, saving changes,
        /// re-fetching the saved entity and returning a successful response with mapped DTO.
        /// Input: StudyYear exists, no existing enrollment initially, repository returns saved entity on re-fetch.
        /// Expected: Response.Success == true and returned DTO matches saved entity values.
        /// </summary>
        [TestMethod]
        public async Task Handle_ValidRequest_AddsEntityAndReturnsSuccess()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mockStudyYearRepo = new Mock<IStudyYearRepository>(MockBehavior.Strict);
            var mockUserStudyYearRepo = new Mock<IUserStudyYearRepository>(MockBehavior.Strict);

            var dto = new CreateUserStudyYearDto
            {
                UserId = "new-user",
                StudyYearId = 5,
                Level = Levels.Third_Year
            };

            var studyYear = new StudyYear
            {
                Id = dto.StudyYearId,
                StartYear = 2020,
                EndYear = 2021,
                IsCurrent = true
            };

            var savedEntity = new UserStudyYear
            {
                Id = 123,
                UserId = dto.UserId,
                StudyYearId = dto.StudyYearId,
                StudyYear = studyYear,
                Level = dto.Level,
                EnrolledAt = DateTime.UtcNow
            };

            // Setup StudyYears.GetByIdAsync to return the study year (validation passes)
            mockStudyYearRepo
                .Setup(r => r.GetByIdAsync(dto.StudyYearId))
                .ReturnsAsync(studyYear)
                .Verifiable();

            // Setup UserStudyYears.GetByUserAndStudyYearAsync:
            // - First call (check existing) returns null
            // - Second call (re-fetch after save) returns savedEntity
            mockUserStudyYearRepo
                .SetupSequence(r => r.GetByUserAndStudyYearAsync(dto.UserId, dto.StudyYearId))
                .ReturnsAsync((UserStudyYear?)null)
                .ReturnsAsync(savedEntity);

            // Expect AddAsync to be called with an entity matching key properties
            mockUserStudyYearRepo
                .Setup(r => r.AddAsync(It.Is<UserStudyYear>(e =>
                    e.UserId == dto.UserId &&
                    e.StudyYearId == dto.StudyYearId &&
                    e.Level == dto.Level)))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // SaveChangesAsync returns 1 (rows affected)
            mockUow
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1)
                .Verifiable();

            mockUow.Setup(u => u.StudyYears).Returns(mockStudyYearRepo.Object);
            mockUow.Setup(u => u.UserStudyYears).Returns(mockUserStudyYearRepo.Object);

            var handler = new CreateUserStudyYearCommandHandler(mockUow.Object);
            var command = new CreateUserStudyYearCommand(dto);

            // Act
            var response = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.IsNotNull(response.Data);

            var data = response.Data!;
            Assert.AreEqual(savedEntity.Id, data.Id);
            Assert.AreEqual(savedEntity.UserId, data.UserId);
            Assert.AreEqual(savedEntity.StudyYearId, data.StudyYearId);
            Assert.AreEqual(studyYear.StartYear, data.StartYear);
            Assert.AreEqual(studyYear.EndYear, data.EndYear);
            Assert.AreEqual(savedEntity.Level, data.Level);
            Assert.IsTrue(data.IsCurrent);
            Assert.AreEqual(savedEntity.EnrolledAt, data.EnrolledAt);

            mockStudyYearRepo.Verify(r => r.GetByIdAsync(dto.StudyYearId), Times.Once);
            mockUserStudyYearRepo.Verify(r => r.GetByUserAndStudyYearAsync(dto.UserId, dto.StudyYearId), Times.Exactly(2));
            mockUserStudyYearRepo.Verify(r => r.AddAsync(It.IsAny<UserStudyYear>()), Times.Once);
            mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);

            mockStudyYearRepo.VerifyNoOtherCalls();
            mockUserStudyYearRepo.VerifyNoOtherCalls();
            mockUow.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Verifies that the constructor successfully creates an instance when provided a non-null IUnitOfWork.
        /// Input: a mocked IUnitOfWork (non-null).
        /// Expected: instance is created, not null, and implements the expected IRequestHandler interface.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidUnitOfWork_InstanceCreatedAndImplementsInterface()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            var sut = new CreateUserStudyYearCommandHandler(mockUnitOfWork.Object);

            // Assert
            Assert.IsNotNull(sut, "Constructor returned null for a valid IUnitOfWork.");
            Assert.IsInstanceOfType(
                sut,
                typeof(IRequestHandler<CreateUserStudyYearCommand, Response<UserStudyYearDto>>),
                "Constructed object does not implement the expected IRequestHandler interface.");
        }
    }
}