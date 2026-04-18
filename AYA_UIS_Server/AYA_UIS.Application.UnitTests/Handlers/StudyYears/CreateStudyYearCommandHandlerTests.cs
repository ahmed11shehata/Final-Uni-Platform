using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.StudyYears;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.StudyYears;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module.StudyYearDtos;

namespace AYA_UIS.Application.Handlers.StudyYears.UnitTests
{
    [TestClass]
    public class CreateStudyYearCommandHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor of CreateStudyYearCommandHandler accepts a valid IUnitOfWork
        /// and does not invoke any operations on it during construction.
        /// Input conditions:
        ///   - A strict Mock&lt;IUnitOfWork&gt; (no setups) is provided to detect any unexpected calls.
        /// Expected result:
        ///   - No exception is thrown and an instance of CreateStudyYearCommandHandler is created.
        ///   - No calls are made on the provided IUnitOfWork during construction.
        /// </summary>
        [TestMethod]
        public void CreateStudyYearCommandHandler_Ctor_WithValidUnitOfWork_DoesNotThrowAndCreatesInstance()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            CreateStudyYearCommandHandler? handler = null;
            Exception? ctorException = null;

            try
            {
                handler = new CreateStudyYearCommandHandler(unitOfWorkMock.Object);
            }
            catch (Exception ex)
            {
                ctorException = ex;
            }

            // Assert
            Assert.IsNull(ctorException, $"Constructor threw an unexpected exception: {ctorException?.Message}");
            Assert.IsNotNull(handler, "Handler instance should be created when a valid IUnitOfWork is provided.");
        }

        /// <summary>
        /// Ensures that constructing CreateStudyYearCommandHandler with different MockBehavior does not cause constructor to invoke IUnitOfWork.
        /// Input conditions:
        ///   - A loose Mock&lt;IUnitOfWork&gt; is provided.
        /// Expected result:
        ///   - No exception is thrown and an instance is created.
        ///   - This test complements the strict-behavior test to ensure constructor does not depend on side effects.
        /// </summary>
        [TestMethod]
        public void CreateStudyYearCommandHandler_Ctor_WithLooseMock_DoesNotThrow()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Loose);

            // Act & Assert
            // If constructor had side-effects calling members, Strict behavior test would fail.
            // Here we only assert that loose mock also results in a successfully constructed instance.
            CreateStudyYearCommandHandler handler = null!;
            try
            {
                handler = new CreateStudyYearCommandHandler(unitOfWorkMock.Object);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Constructor threw an unexpected exception with loose mock: {ex.Message}");
            }

            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Verifies that Handle maps CreateStudyYearDto to StudyYear, calls AddAsync and SaveChangesAsync,
        /// and returns the Id set by the repository. Tests multiple year combinations including edge integers
        /// and an inverted range (StartYear &gt; EndYear) to ensure mapping and persistence occur regardless of domain validation.
        /// Inputs: several StartYear/EndYear integer pairs.
        /// Expected: repository AddAsync is invoked with a StudyYear having matching StartYear/EndYear, SaveChangesAsync is called,
        /// and the handler returns the Id assigned during AddAsync.
        /// </summary>
        [TestMethod]
        public async Task Handle_VariousYearCombinations_PersistsAndReturnsAssignedId()
        {
            // Arrange / Act / Assert for multiple cases
            var testCases = new (int StartYear, int EndYear, int AssignedId)[]
            {
                (0, 0, 1), // boundary zero values
                (int.MinValue, int.MaxValue, 2), // extreme values
                (2024, 2025, 3), // normal valid-looking range
                (2026, 2025, 4) // inverted range: StartYear > EndYear (no validation in handler)
            };

            foreach (var (start, end, assignedId) in testCases)
            {
                // Arrange
                var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
                var mockStudyYearRepo = new Mock<IStudyYearRepository>(MockBehavior.Strict);

                // Capture the StudyYear passed to AddAsync to assert mapping
                StudyYear? captured = null;
                mockStudyYearRepo
                    .Setup(r => r.AddAsync(It.IsAny<StudyYear>()))
                    .Returns<StudyYear>(sy =>
                    {
                        // Simulate repository assigning an Id to the entity (e.g., DB identity)
                        sy.Id = assignedId;
                        captured = sy;
                        return Task.CompletedTask;
                    });

                mockUnitOfWork.Setup(u => u.StudyYears).Returns(mockStudyYearRepo.Object);
                mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

                var handler = new CreateStudyYearCommandHandler(mockUnitOfWork.Object);
                var dto = new CreateStudyYearDto { StartYear = start, EndYear = end };
                var command = new CreateStudyYearCommand(dto);

                // Act
                var result = await handler.Handle(command, CancellationToken.None);

                // Assert
                Assert.AreEqual(assignedId, result, "Handler should return the Id assigned by repository.");
                Assert.IsNotNull(captured, "AddAsync should have been called with a StudyYear instance.");
                Assert.AreEqual(start, captured!.StartYear, "StartYear should be mapped from DTO to entity.");
                Assert.AreEqual(end, captured.EndYear, "EndYear should be mapped from DTO to entity.");

                mockStudyYearRepo.Verify(r => r.AddAsync(It.IsAny<StudyYear>()), Times.Once);
                mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);

                // Cleanup strict mocks to avoid cross-iteration issues
                mockStudyYearRepo.VerifyNoOtherCalls();
                mockUnitOfWork.Verify(u => u.StudyYears, Times.AtLeastOnce);
                mockUnitOfWork.VerifyNoOtherCalls();
            }
        }

    }
}