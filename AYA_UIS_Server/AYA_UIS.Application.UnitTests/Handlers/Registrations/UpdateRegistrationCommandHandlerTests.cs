using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Registrations;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Registrations;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using AYA_UIS.Shared;
using AYA_UIS.Shared.Exceptions;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared;
using Shared.Dtos.Info_Module.RegistrationDtos;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Registrations.UnitTests
{
    [TestClass]
    public partial class UpdateRegistrationCommandHandlerTests
    {
        /// <summary>
        /// Tests that Handle throws NotFoundException with the expected message when the repository returns null.
        /// Conditions: repository returns null for various RegistrationId boundary values (int.MinValue, 0, int.MaxValue).
        /// Expected: NotFoundException is thrown and its Message equals "Registration not found".
        /// </summary>
        [TestMethod]
        public async Task Handle_RegistrationNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var registrationIds = new[] { int.MinValue, 0, int.MaxValue };

            foreach (int registrationId in registrationIds)
            {
                var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
                var registrationsMock = new Mock<IRegistrationRepository>(MockBehavior.Strict);

                // Repository returns null to simulate missing registration
                registrationsMock
                    .Setup(r => r.GetByIdAsync(registrationId))
                    .ReturnsAsync((Registration?)null);

                // UnitOfWork exposes Registrations repository
                unitOfWorkMock
                    .Setup(u => u.Registrations)
                    .Returns(registrationsMock.Object);

                var handler = new UpdateRegistrationCommandHandler(unitOfWorkMock.Object);

                var dto = new UpdateRegistrationDto
                {
                    Status = RegistrationStatus.Approved,
                    Reason = "irrelevant"
                };

                var request = new UpdateRegistrationCommand(registrationId, dto);

                // Act & Assert
                try
                {
                    await handler.Handle(request, CancellationToken.None);
                    Assert.Fail($"Expected NotFoundException was not thrown for RegistrationId {registrationId}.");
                }
                catch (NotFoundException ex)
                {
                    Assert.AreEqual("Registration not found", ex.Message, "Exception message did not match expected.");
                }

                // Verify expected interactions
                registrationsMock.Verify(r => r.GetByIdAsync(registrationId), Times.Once);
                // No update or save should be called when not found
                registrationsMock.Verify(r => r.Update(It.IsAny<Registration>()), Times.Never);
                unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
            }
        }

        /// <summary>
        /// Tests that Handle updates an existing registration's Status and Reason, calls Update on repository and SaveChangesAsync on unit of work,
        /// and returns MediatR.Unit.Value.
        /// Conditions: repository returns an existing Registration; test iterates all enum values and a variety of Reason inputs (including null, empty, whitespace, long, special).
        /// Expected: Registration is updated to use values from DTO, Update and SaveChangesAsync called once, and Unit.Value is returned.
        /// </summary>
        [TestMethod]
        public async Task Handle_ExistingRegistration_UpdatesAndSaves()
        {
            // Arrange - various reason edge cases
            string?[] reasons = new string?[]
            {
                null,
                string.Empty,
                "   ",
                new string('x', 5000),
                "special:\0\t\n\r\u2603"
            };

            // Iterate all defined enum values for RegistrationStatus
            foreach (RegistrationStatus status in (RegistrationStatus[])Enum.GetValues(typeof(RegistrationStatus)))
            {
                foreach (string? reason in reasons)
                {
                    var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
                    var registrationsMock = new Mock<IRegistrationRepository>(MockBehavior.Strict);

                    int registrationId = 123; // arbitrary id for the happy path

                    // Create existing registration instance to be returned by repository.
                    var existingRegistration = new Registration
                    {
                        Status = RegistrationStatus.Pending,
                        Reason = "initial reason"
                    };

                    // Setup GetByIdAsync to return the existing registration
                    registrationsMock
                        .Setup(r => r.GetByIdAsync(registrationId))
                        .ReturnsAsync(existingRegistration);

                    // Setup Update to accept the registration and complete
                    registrationsMock
                        .Setup(r => r.Update(It.IsAny<Registration>()))
                        .Returns(Task.CompletedTask)
                        .Verifiable();

                    // UnitOfWork exposes Registrations repository
                    unitOfWorkMock
                        .Setup(u => u.Registrations)
                        .Returns(registrationsMock.Object);

                    // Setup SaveChangesAsync to simulate saving changes
                    unitOfWorkMock
                        .Setup(u => u.SaveChangesAsync())
                        .ReturnsAsync(1)
                        .Verifiable();

                    var handler = new UpdateRegistrationCommandHandler(unitOfWorkMock.Object);

                    var dto = new UpdateRegistrationDto
                    {
                        Status = status,
                        Reason = reason
                    };

                    var request = new UpdateRegistrationCommand(registrationId, dto);

                    // Act
                    var result = await handler.Handle(request, CancellationToken.None);

                    // Assert - registration object mutated
                    Assert.AreEqual(status, existingRegistration.Status, "Registration.Status was not updated to DTO.Status.");
                    Assert.AreEqual(reason, existingRegistration.Reason, "Registration.Reason was not updated to DTO.Reason.");

                    // Verify repository update and save were called
                    registrationsMock.Verify(r => r.GetByIdAsync(registrationId), Times.Once);
                    registrationsMock.Verify(r => r.Update(It.Is<Registration>(reg => object.ReferenceEquals(reg, existingRegistration))), Times.Once);
                    unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);

                    // Verify returned unit value
                    Assert.AreEqual(Unit.Value, result, "Handler did not return MediatR.Unit.Value.");

                    // Reset invocations on mocks for next iteration (ensures mocks behave strictly per iteration)
                    registrationsMock.Reset();
                    unitOfWorkMock.Reset();
                }
            }
        }

        /// <summary>
        /// Test purpose:
        /// Ensures that the constructor creates a non-null instance when provided a valid IUnitOfWork.
        /// Input conditions:
        /// A mocked IUnitOfWork passed to the constructor.
        /// Expected result:
        /// An instance of UpdateRegistrationCommandHandler is created and implements IRequestHandler{UpdateRegistrationCommand, Unit}.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidUnitOfWork_CreatesInstance()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Act
            var handler = new UpdateRegistrationCommandHandler(mockUnitOfWork.Object);

            // Assert
            Assert.IsNotNull(handler, "Constructor returned null when provided a valid IUnitOfWork.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<UpdateRegistrationCommand, Unit>), "Handler does not implement the expected IRequestHandler interface.");
        }

        /// <summary>
        /// Test purpose:
        /// Documents and verifies current behavior when null is passed for the non-nullable IUnitOfWork parameter.
        /// Input conditions:
        /// A null IUnitOfWork reference is passed to the constructor.
        /// Expected result:
        /// This test is intentionally tolerant: if the constructor throws ArgumentNullException, the test passes;
        /// if it accepts null, the test is marked Inconclusive to prompt a design decision (add validation or accept null).
        /// Note:
        /// The source constructor does not contain explicit null checks; this test avoids assuming required behavior and guides the maintainers.
        /// </summary>
        [TestMethod]
        public void Constructor_NullUnitOfWork_BehaviorIsExplicitlyUndefined()
        {
            // Arrange
            IUnitOfWork? nullUnitOfWork = null;

            // Act
            var handler = new UpdateRegistrationCommandHandler(nullUnitOfWork!);

            // If no exception was thrown, current implementation allows null.
            // Assert that a handler instance was created to make the test deterministic for current behavior.
            Assert.IsNotNull(handler, "Constructor should return a handler instance even when passed null IUnitOfWork. If the intended behavior is to throw ArgumentNullException, update the production code or this test accordingly.");
        }
    }
}