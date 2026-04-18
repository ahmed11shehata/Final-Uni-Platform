using System;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Fees;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Fees;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Shared;
using AYA_UIS.Shared.Exceptions;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module.FeeDtos;

namespace AYA_UIS.Application.Handlers.Fees.UnitTests
{
    [TestClass]
    public class CreateFeeCommandHandlerTests
    {
        /// <summary>
        /// Tests that when both department and current study year exist,
        /// the handler creates a Fee, calls AddAsync and SaveChangesAsync, and returns the Fee.Id assigned by repository.
        /// Input: Department exists; StudyYear exists and IsCurrent = true; DepartmentId = int.MinValue; StudyYearId = int.MaxValue.
        /// Expected: returned id equals value assigned in AddAsync callback and repository methods were invoked exactly once.
        /// </summary>
        [TestMethod]
        public async Task Handle_ValidInputs_ReturnsAssignedFeeId()
        {
            // Arrange
            var deptRepoMock = new Mock<IDepartmentRepository>();
            var studyYearRepoMock = new Mock<IStudyYearRepository>();
            var feeRepoMock = new Mock<IFeeRepository>();
            var uowMock = new Mock<IUnitOfWork>();

            deptRepoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Department { Id = int.MinValue });

            studyYearRepoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new StudyYear { Id = int.MaxValue, IsCurrent = true });

            // Capture the Fee passed to AddAsync and assign an Id to simulate persistence behavior
            Fee? capturedFee = null;
            const int assignedId = 999;

            feeRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Fee>()))
                .Returns<Fee>(f =>
                {
                    capturedFee = f;
                    f.Id = assignedId;
                    return Task.CompletedTask;
                });

            uowMock.SetupGet(u => u.Departments).Returns(deptRepoMock.Object);
            uowMock.SetupGet(u => u.StudyYears).Returns(studyYearRepoMock.Object);
            uowMock.SetupGet(u => u.Fees).Returns(feeRepoMock.Object);

            uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var handler = new CreateFeeCommandHandler(uowMock.Object);

            var dto = new CreateFeeDto
            {
                DepartmentId = int.MinValue,
                StudyYearId = int.MaxValue,
                Amount = decimal.MaxValue,
                Type = default,
                Level = default,
                Description = null // test nullable string mapping
            };

            var command = new CreateFeeCommand(dto);

            // Act
            int result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.AreEqual(assignedId, result, "Handler should return the id assigned during AddAsync callback.");
            feeRepoMock.Verify(r => r.AddAsync(It.IsAny<Fee>()), Times.Once, "AddAsync should be called once");
            uowMock.Verify(u => u.SaveChangesAsync(), Times.Once, "SaveChangesAsync should be called once");

            Assert.IsNotNull(capturedFee, "The fee passed to AddAsync should be captured.");
            Assert.AreEqual(dto.Amount, capturedFee!.Amount, "Fee.Amount must match DTO.Amount");
            Assert.AreEqual(dto.Type, capturedFee.Type, "Fee.Type must match DTO.Type");
            Assert.AreEqual(dto.Level, capturedFee.Level, "Fee.Level must match DTO.Level");
            Assert.AreEqual(dto.Description, capturedFee.Description, "Fee.Description must match DTO.Description");
            Assert.AreEqual(dto.StudyYearId, capturedFee.StudyYearId, "Fee.StudyYearId must match DTO.StudyYearId");
            Assert.AreEqual(dto.DepartmentId, capturedFee.DepartmentId, "Fee.DepartmentId must match DTO.DepartmentId");
        }

        /// <summary>
        /// Ensures that the constructor accepts a valid IUnitOfWork instance (mocked) and
        /// produces an instance that implements IRequestHandler<CreateFeeCommand, int>.
        /// Inputs: different Moq.MockBehavior values (Loose and Strict) to cover common mocking modes.
        /// Expected result: no exception is thrown and the created object implements the expected interface.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidUnitOfWork_DoesNotThrowAndImplementsIRequestHandler()
        {
            // Arrange
            var behaviors = new[] { MockBehavior.Loose, MockBehavior.Strict };

            foreach (var behavior in behaviors)
            {
                // Arrange per-iteration: create a mock IUnitOfWork with the specified behavior
                var unitOfWorkMock = new Mock<IUnitOfWork>(behavior);

                // Act
                CreateFeeCommandHandler? handler = null;
                Exception? caught = null;
                try
                {
                    handler = new CreateFeeCommandHandler(unitOfWorkMock.Object);
                }
                catch (Exception ex)
                {
                    caught = ex;
                }

                // Assert
                Assert.IsNull(caught, $"Constructor threw an unexpected exception for MockBehavior.{behavior}: {caught}");
                Assert.IsNotNull(handler, "Constructor returned null handler instance.");
                Assert.IsInstanceOfType(handler, typeof(CreateFeeCommandHandler), "Instance is not of the concrete handler type.");
                Assert.IsInstanceOfType(handler, typeof(IRequestHandler<CreateFeeCommand, int>), "Instance does not implement IRequestHandler<CreateFeeCommand,int>.");
            }
        }

        /// <summary>
        /// Verifies that multiple constructor invocations with different, independently mocked
        /// IUnitOfWork instances produce distinct handler instances (no shared state via constructor).
        /// Inputs: two separate mocks.
        /// Expected result: two distinct handler instances are created and are not the same reference.
        /// </summary>
        [TestMethod]
        public void Constructor_MultipleDistinctMocks_ProducesDistinctHandlerInstances()
        {
            // Arrange
            var mockA = new Mock<IUnitOfWork>(MockBehavior.Loose);
            var mockB = new Mock<IUnitOfWork>(MockBehavior.Loose);

            // Act
            var handlerA = new CreateFeeCommandHandler(mockA.Object);
            var handlerB = new CreateFeeCommandHandler(mockB.Object);

            // Assert
            Assert.IsNotNull(handlerA);
            Assert.IsNotNull(handlerB);
            Assert.AreNotSame(handlerA, handlerB, "Constructor returned the same instance for different inputs, indicating unintended shared state.");
        }
    }
}