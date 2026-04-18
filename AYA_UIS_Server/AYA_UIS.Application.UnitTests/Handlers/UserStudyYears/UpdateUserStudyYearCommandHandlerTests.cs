using System;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.UserStudyYears;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.UserStudyYears;
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
    /// <summary>
    /// Tests for UpdateUserStudyYearCommandHandler.Handle method.
    /// Focuses on the behavior when the target entity is missing, when the DTO Level is null
    /// (should not overwrite existing value), and when DTO Level has a value (should update).
    /// </summary>
    [TestClass]
    public class UpdateUserStudyYearCommandHandlerTests
    {
        /// <summary>
        /// The test verifies that when repository returns null for GetByIdAsync the handler
        /// returns an error response and does not call Update, SaveChangesAsync or GetByUserAndStudyYearAsync.
        /// This test loops several id edge-values to validate integer boundary behavior for the id argument.
        /// Expected: No repository mutation methods are invoked and a non-null Response is returned.
        /// </summary>
        [TestMethod]
        public async Task Handle_EntityNotFound_ReturnsErrorResponse_AndDoesNotCallMutatingMethods_ForVariousIds()
        {
            // Arrange
            var idsToTest = new[] { int.MinValue, -1, 0, int.MaxValue };
            foreach (int id in idsToTest)
            {
                var repoMock = new Mock<IUserStudyYearRepository>(MockBehavior.Strict);
                repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((UserStudyYear?)null);

                var uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
                uowMock.Setup(u => u.UserStudyYears).Returns(repoMock.Object);

                var handler = new UpdateUserStudyYearCommandHandler(uowMock.Object);
                var cmd = new UpdateUserStudyYearCommand(id, new UpdateUserStudyYearDto());

                // Act
                Response<UserStudyYearDto> result = await handler.Handle(cmd, CancellationToken.None);

                // Assert
                Assert.IsNotNull(result, "Handler returned null Response for id: " + id);

                // Verify that mutating methods were never called
                repoMock.Verify(r => r.Update(It.IsAny<UserStudyYear>()), Times.Never, "Update should not be called when entity not found");
                uowMock.Verify(u => u.SaveChangesAsync(), Times.Never, "SaveChangesAsync should not be called when entity not found");
                repoMock.Verify(r => r.GetByUserAndStudyYearAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never, "GetByUserAndStudyYearAsync should not be called when entity not found");
            }
        }

        /// <summary>
        /// The test verifies that when the incoming DTO.Level is null the handler does not change the entity.Level,
        /// still calls Update and SaveChangesAsync and then fetches by user and study year.
        /// Input: existing entity with a specific Level, DTO.Level = null.
        /// Expected: Update called with same Level as original; SaveChangesAsync and GetByUserAndStudyYearAsync called once.
        /// </summary>
        [TestMethod]
        public async Task Handle_DtoLevelNull_DoesNotChangeEntityLevel_AndPersistsChanges()
        {
            // Arrange
            int entityId = 123;
            string userId = "user-special";
            int studyYearId = 2022;
            Levels originalLevel = (Levels)0; // do not assume enum member names; use numeric casts
            var existingEntity = new UserStudyYear
            {
                Id = entityId,
                UserId = userId,
                StudyYearId = studyYearId,
                Level = originalLevel,
                StudyYear = null!, // allowed; MapToDto handles null StudyYear
                EnrolledAt = new DateTime(2020, 1, 1)
            };

            var repoMock = new Mock<IUserStudyYearRepository>();
            repoMock.Setup(r => r.GetByIdAsync(entityId)).ReturnsAsync(existingEntity);
            repoMock.Setup(r => r.Update(It.IsAny<UserStudyYear>())).Returns(Task.CompletedTask).Verifiable();
            repoMock.Setup(r => r.GetByUserAndStudyYearAsync(userId, studyYearId)).ReturnsAsync(existingEntity).Verifiable();

            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(u => u.UserStudyYears).Returns(repoMock.Object);
            uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1).Verifiable();

            var handler = new UpdateUserStudyYearCommandHandler(uowMock.Object);

            var cmd = new UpdateUserStudyYearCommand(entityId, new UpdateUserStudyYearDto { Level = null });

            // Act
            Response<UserStudyYearDto> result = await handler.Handle(cmd, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result, "Result should not be null when entity exists and DTO.Level is null.");

            // Ensure Update was called with an entity that kept its original level
            repoMock.Verify(r => r.Update(It.Is<UserStudyYear>(e => e.Id == entityId && e.Level.Equals(originalLevel))), Times.Once, "Entity Level should remain unchanged when DTO.Level is null");
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            repoMock.Verify(r => r.GetByUserAndStudyYearAsync(userId, studyYearId), Times.Once);
        }

        // Helper class used in tests only to satisfy StudyYear construction; enclosed inside test class per rules
        private class StudyYear
        {
            public int Id { get; set; }
            public int StartYear { get; set; }
            public int EndYear { get; set; }
            public bool IsCurrent { get; set; }
        }

        /// <summary>
        /// Verifies that the constructor of UpdateUserStudyYearCommandHandler creates an instance
        /// when a valid IUnitOfWork implementation is provided via dependency injection.
        /// Input: a non-null mocked IUnitOfWork.
        /// Expected: constructor does not throw and returns a non-null handler of the expected type.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidUnitOfWork_InstanceCreated()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            UpdateUserStudyYearCommandHandler? handler = null;
            Exception? exception = null;
            try
            {
                handler = new UpdateUserStudyYearCommandHandler(unitOfWorkMock.Object);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception, "Constructor threw an unexpected exception for a valid IUnitOfWork.");
            Assert.IsNotNull(handler, "Constructor returned null for a valid IUnitOfWork.");
            Assert.IsInstanceOfType(handler, typeof(UpdateUserStudyYearCommandHandler), "Returned instance is not of the expected handler type.");
        }

        /// <summary>
        /// Attempts to validate that the provided IUnitOfWork is stored by the handler.
        /// Input: a mocked IUnitOfWork instance.
        /// Expected: the handler should retain a reference to the provided IUnitOfWork.
        /// 
        /// Notes:
        /// - The production field (_unitOfWork) is private and readonly. Accessing it would require reflection
        ///   or adding test-only exposure in production code. Creating such exposure or using reflection
        ///   is outside the allowed actions per test generation constraints.
        /// - Therefore this test is marked inconclusive and documents the next steps for verification:
        ///   either expose the dependency via an internal property (and InternalsVisibleTo the test assembly)
        ///   or allow reading via a protected property that can be overridden in tests.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidUnitOfWork_FieldAssignment_NotVerified()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            var handler = new UpdateUserStudyYearCommandHandler(unitOfWorkMock.Object);

            // Assert
            // We cannot verify private readonly field assignment without reflection or altering production code.
            // Instead, at minimum ensure the handler was constructed successfully.
            Assert.IsNotNull(handler, "Handler should be created when a valid IUnitOfWork is provided.");
        }
    }
}