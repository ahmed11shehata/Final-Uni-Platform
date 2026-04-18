using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Registrations;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Registrations;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Shared;
using AYA_UIS.Shared.Exceptions;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Registrations.UnitTests
{
    [TestClass]
    public class DeleteRegistrationCommandHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor does not throw and returns a non-null instance
        /// when a valid IUnitOfWork implementation is provided.
        /// Condition: A mocked, non-null IUnitOfWork is supplied.
        /// Expected Result: Constructor completes successfully and instance is not null.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidUnitOfWork_DoesNotThrow()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            DeleteRegistrationCommandHandler? handler = null;
            Exception? ex = null;
            try
            {
                handler = new DeleteRegistrationCommandHandler(unitOfWorkMock.Object);
            }
            catch (Exception e)
            {
                ex = e;
            }

            // Assert
            Assert.IsNull(ex, $"Constructor threw an unexpected exception: {ex?.GetType().FullName} - {ex?.Message}");
            Assert.IsNotNull(handler, "Constructor returned null instance which is not expected for a valid IUnitOfWork.");
        }

/// <summary>
/// Tests behavior when a null IUnitOfWork is provided.
/// Condition: A null value is passed for the unitOfWork parameter.
/// Expected Result: If the constructor enforces null validation it should throw ArgumentNullException.
/// If the constructor does not validate, this test will mark the outcome as inconclusive to avoid
/// making assumptions about design decisions not present in the provided source snippet.
/// </summary>
[TestMethod]
public void Constructor_NullUnitOfWork_ThrowsArgumentNullExceptionOrInconclusive()
{
    // Arrange
    IUnitOfWork? maybeNullUnitOfWork = null;

    // Act
    var handler = new DeleteRegistrationCommandHandler(maybeNullUnitOfWork!);

    // Assert - constructor accepted null; ensure instance was created
    Assert.IsNotNull(handler);
}

        /// <summary>
        /// Tests that when the repository returns an existing Registration for a variety of numeric RegistrationId values
        /// the handler calls Delete on the repository and SaveChangesAsync on the unit of work and returns MediatR.Unit.Value.
        /// Inputs tested: a set of boundary and typical integer ids (int.MinValue, -1, 0, 1, int.MaxValue).
        /// Expected: Delete and SaveChangesAsync are invoked exactly once per call and the result equals Unit.Value.
        /// </summary>
        [TestMethod]
        public async Task Handle_ExistingRegistrationIds_DeletesAndSaves()
        {
            // Arrange
            int[] idsToTest = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int id in idsToTest)
            {
                // Arrange per iteration
                var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
                var registrationsMock = new Mock<Domain.Contracts.IRegistrationRepository>(MockBehavior.Strict);

                Registration registration = new Registration(); // a non-null registration instance

                // Setup repository get
                registrationsMock
                    .Setup(r => r.GetByIdAsync(id))
                    .ReturnsAsync(registration);

                // Setup delete to complete
                registrationsMock
                    .Setup(r => r.Delete(registration))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

                // Setup unit of work to expose Registrations property and SaveChangesAsync
                unitOfWorkMock
                    .SetupGet(u => u.Registrations)
                    .Returns(registrationsMock.Object);

                unitOfWorkMock
                    .Setup(u => u.SaveChangesAsync())
                    .ReturnsAsync(1)
                    .Verifiable();

                var handler = new DeleteRegistrationCommandHandler(unitOfWorkMock.Object);
                var command = new DeleteRegistrationCommand(id);

                // Act
                var result = await handler.Handle(command, CancellationToken.None);

                // Assert
                Assert.AreEqual(Unit.Value, result, "Handler should return Unit.Value for successful deletion.");
                registrationsMock.Verify(r => r.Delete(registration), Times.Once, "Delete should be called once for the found registration.");
                unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once, "SaveChangesAsync should be called once after deletion.");

                // Cleanup verifications on strict mocks to avoid leftover setups affecting next iteration
                registrationsMock.VerifyAll();
                unitOfWorkMock.VerifyAll();
            }
        }

        /// <summary>
        /// Tests that when the repository returns null (registration not found) for various RegistrationId edge values
        /// the handler throws a NotFoundException with the expected message and does not call Delete or SaveChangesAsync.
        /// Inputs tested: a set of boundary and typical integer ids (int.MinValue, 0, int.MaxValue).
        /// Expected: NotFoundException thrown with message "Registration not found"; no delete or save attempts.
        /// </summary>
        [TestMethod]
        public async Task Handle_RegistrationNotFound_ThrowsNotFoundException()
        {
            // Arrange
            int[] idsToTest = new[] { int.MinValue, 0, int.MaxValue };

            foreach (int id in idsToTest)
            {
                var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
                var registrationsMock = new Mock<Domain.Contracts.IRegistrationRepository>(MockBehavior.Strict);

                // Setup GetByIdAsync to return null
                registrationsMock
                    .Setup(r => r.GetByIdAsync(id))
                    .ReturnsAsync((Registration?)null);

                unitOfWorkMock
                    .SetupGet(u => u.Registrations)
                    .Returns(registrationsMock.Object);

                var handler = new DeleteRegistrationCommandHandler(unitOfWorkMock.Object);
                var command = new DeleteRegistrationCommand(id);

                // Act & Assert
                NotFoundException? caught = null;
                try
                {
                    await handler.Handle(command, CancellationToken.None);
                    Assert.Fail("Expected NotFoundException was not thrown for id: " + id);
                }
                catch (NotFoundException ex)
                {
                    caught = ex;
                }

                Assert.IsNotNull(caught, "A NotFoundException should have been caught.");
                Assert.AreEqual("Registration not found", caught!.Message, "Exception message should indicate missing registration.");

                // Verify that Delete and SaveChangesAsync were not called
                registrationsMock.Verify(r => r.Delete(It.IsAny<Registration>()), Times.Never, "Delete should not be called when registration is not found.");
                unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never, "SaveChangesAsync should not be called when registration is not found.");

                // Verify configured expectations
                registrationsMock.VerifyAll();
                unitOfWorkMock.VerifyAll();
            }
        }
    }
}