using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Fees;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Fees;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AYA_UIS.Application.Handlers.Fees.UnitTests
{
    [TestClass]
    public class DeleteFeeCommandHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor creates an instance when provided a valid IUnitOfWork.
        /// Input: a mocked IUnitOfWork instance.
        /// Expected: an instance of DeleteFeeCommandHandler is returned and it implements IRequestHandler&lt;DeleteFeeCommand, Unit&gt;; no exception is thrown.
        /// </summary>
        [TestMethod]
        public void DeleteFeeCommandHandler_Constructor_WithValidUnitOfWork_DoesNotThrowAndImplementsInterface()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            DeleteFeeCommandHandler? handler = null;
            Exception? caught = null;
            try
            {
                handler = new DeleteFeeCommandHandler(unitOfWorkMock.Object);
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            // Assert
            Assert.IsNull(caught, "Constructor should not throw when provided a valid IUnitOfWork mock.");
            Assert.IsNotNull(handler, "Handler instance should not be null after construction.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<DeleteFeeCommand, Unit>), "Handler should implement IRequestHandler<DeleteFeeCommand, Unit>.");
        }

        /// <summary>
        /// Ensures multiple constructions with different IUnitOfWork mocks produce independent, non-null instances.
        /// Input: two different mocked IUnitOfWork instances.
        /// Expected: two distinct DeleteFeeCommandHandler instances are created without exceptions.
        /// </summary>
        [TestMethod]
        public void DeleteFeeCommandHandler_Constructor_MultipleMocks_CreateDistinctInstances()
        {
            // Arrange
            var mockA = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mockB = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            var handlerA = new DeleteFeeCommandHandler(mockA.Object);
            var handlerB = new DeleteFeeCommandHandler(mockB.Object);

            // Assert
            Assert.IsNotNull(handlerA, "First handler instance should not be null.");
            Assert.IsNotNull(handlerB, "Second handler instance should not be null.");
            Assert.AreNotSame(handlerA, handlerB, "Two handler instances constructed with different IUnitOfWork mocks should be distinct objects.");
        }

        /// <summary>
        /// Verifies that when the repository returns null (fee not found) for various id boundary values,
        /// the handler returns Unit.Value and does not call Delete or SaveChangesAsync.
        /// Tested id inputs: int.MinValue, -1, 0, int.MaxValue.
        /// Expected: No deletion or save occurs and handler returns Unit.Value.
        /// </summary>
        [TestMethod]
        public async Task Handle_FeeNotFound_ReturnsUnitAndDoesNotCallDeleteOrSaveChanges()
        {
            // Arrange
            int[] testIds = new[] { int.MinValue, -1, 0, int.MaxValue };

            foreach (int testId in testIds)
            {
                var mockUnitOfWork = new Mock<IUnitOfWork>();
                var mockFeeRepo = new Mock<IFeeRepository>();

                // For the specific test id, repository returns null (not found).
                mockFeeRepo.Setup(r => r.GetByIdAsync(testId)).ReturnsAsync((Fee?)null);
                mockUnitOfWork.Setup(u => u.Fees).Returns(mockFeeRepo.Object);

                var handler = new DeleteFeeCommandHandler(mockUnitOfWork.Object);
                var command = new DeleteFeeCommand(testId);

                // Act
                var result = await handler.Handle(command, CancellationToken.None);

                // Assert
                Assert.AreEqual(Unit.Value, result);
                mockFeeRepo.Verify(r => r.Delete(It.IsAny<Fee>()), Times.Never, $"Delete should not be called for id={testId}");
                mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never, $"SaveChangesAsync should not be called for id={testId}");
            }
        }

        /// <summary>
        /// Verifies that when the repository returns an existing Fee for several representative id values,
        /// the handler calls Delete with the same Fee instance, calls SaveChangesAsync once, and returns Unit.Value.
        /// Tested id inputs: 0, 1, int.MaxValue.
        /// Expected: Delete invoked once with the retrieved Fee and SaveChangesAsync invoked once per call.
        /// </summary>
        [TestMethod]
        public async Task Handle_FeeExists_DeletesFeeAndSavesChanges_ReturnsUnit()
        {
            // Arrange
            int[] testIds = new[] { 0, 1, int.MaxValue };

            foreach (int testId in testIds)
            {
                var mockUnitOfWork = new Mock<IUnitOfWork>();
                var mockFeeRepo = new Mock<IFeeRepository>();

                // Create a Fee instance to be returned by repository for this id.
                var fee = new Fee
                {
                    Amount = 123.45m,
                    Description = $"desc-{testId}"
                    // Other properties are not required for this handler's logic.
                };

                mockFeeRepo.Setup(r => r.GetByIdAsync(testId)).ReturnsAsync(fee);
                mockFeeRepo.Setup(r => r.Delete(fee)).Returns(Task.CompletedTask);
                mockUnitOfWork.Setup(u => u.Fees).Returns(mockFeeRepo.Object);
                mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

                var handler = new DeleteFeeCommandHandler(mockUnitOfWork.Object);
                var command = new DeleteFeeCommand(testId);

                // Act
                var result = await handler.Handle(command, CancellationToken.None);

                // Assert
                Assert.AreEqual(Unit.Value, result);
                mockFeeRepo.Verify(r => r.Delete(fee), Times.Once, $"Delete should be called once for id={testId}");
                mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once, $"SaveChangesAsync should be called once for id={testId}");
            }
        }
    }
}