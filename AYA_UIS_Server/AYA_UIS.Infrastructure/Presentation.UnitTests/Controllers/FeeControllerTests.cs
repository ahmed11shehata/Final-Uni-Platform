using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Fees;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Fees;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Presentation;
using Presentation.Controllers;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.FeeDtos;

namespace Presentation.Controllers.UnitTests
{
    [TestClass]
    public partial class FeeControllerTests
    {
        /// <summary>
        /// Tests that CreateFee returns an OkObjectResult containing the integer returned by IMediator.Send.
        /// Input: a non-null CreateFeeDto and IMediator configured to return 42.
        /// Expected: OkObjectResult with Value equal to 42 and mediator Send called exactly once.
        /// </summary>
        [TestMethod]
        public async Task CreateFee_ValidDto_ReturnsOkWithResult()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var dto = new CreateFeeDto();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateFeeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(42);
            var controller = new FeeController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.CreateFee(dto);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(42, okResult?.Value);
            mediatorMock.Verify(m => m.Send(It.IsAny<CreateFeeCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests CreateFee with a range of integer return values from the mediator to cover boundary and typical values.
        /// Input: a non-null CreateFeeDto and mediator returning values: int.MinValue, -1, 0, 1, int.MaxValue.
        /// Expected: For each value, controller returns OkObjectResult containing that exact integer.
        /// </summary>
        [TestMethod]
        public async Task CreateFee_MediatorReturnsVariousIntegers_ReturnsOkForAll()
        {
            // Arrange - test multiple return values in a loop to avoid redundant test methods
            int[] testValues = new[] { int.MinValue, -1, 0, 1, int.MaxValue };
            foreach (int expected in testValues)
            {
                var mediatorMock = new Mock<IMediator>();
                var dto = new CreateFeeDto();
                mediatorMock
                    .Setup(m => m.Send(It.IsAny<CreateFeeCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expected);
                var controller = new FeeController(mediatorMock.Object);

                // Act
                IActionResult actionResult = await controller.CreateFee(dto);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
                var okResult = actionResult as OkObjectResult;
                Assert.IsNotNull(okResult);
                Assert.AreEqual(expected, okResult?.Value);
                mediatorMock.Verify(m => m.Send(It.IsAny<CreateFeeCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        /// <summary>
        /// Tests that CreateFee propagates exceptions thrown by IMediator.Send.
        /// Input: a non-null CreateFeeDto and IMediator configured to throw InvalidOperationException.
        /// Expected: The same InvalidOperationException is thrown by the controller method.
        /// </summary>
        [TestMethod]
        public async Task CreateFee_MediatorThrows_PropagatesException()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var dto = new CreateFeeDto();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateFeeCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("mediator failure"));
            var controller = new FeeController(mediatorMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await controller.CreateFee(dto));
            mediatorMock.Verify(m => m.Send(It.IsAny<CreateFeeCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetFeesByDepartment forwards the query to IMediator and returns Ok(result)
        /// for a variety of integer inputs including boundary values.
        /// Input conditions: multiple (departmentId, studyYearId) pairs including int.MinValue, -1, 0, 1, int.MaxValue.
        /// Expected result: IActionResult is OkObjectResult and its Value is the same object returned by IMediator.Send.
        /// </summary>
        [TestMethod]
        public async Task GetFeesByDepartment_VariousNumericInputs_ReturnsOkWithMediatorResult()
        {
            // Arrange
            var testCases = new List<(int departmentId, int studyYearId)>
            {
                (int.MinValue, int.MinValue),
                (-1, -1),
                (0, 0),
                (1, 1),
                (int.MaxValue, int.MaxValue)
            };

            foreach (var (departmentId, studyYearId) in testCases)
            {
                var expected = new List<FeeDto> { new FeeDto() };
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
                mediatorMock
                    .Setup(m => m.Send(It.IsAny<GetFeesOfDepartmentForStudyYearQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expected)
                    .Verifiable();

                var controller = new FeeController(mediatorMock.Object);

                // Act
                IActionResult actionResult = await controller.GetFeesByDepartment(departmentId, studyYearId);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult for valid mediator response.");
                var ok = actionResult as OkObjectResult;
                Assert.AreSame(expected, ok?.Value, "Controller should return the exact list instance returned by IMediator.");
                mediatorMock.Verify(m => m.Send(It.IsAny<GetFeesOfDepartmentForStudyYearQuery>(), It.IsAny<CancellationToken>()), Times.Once);

                // Cleanup verification for next iteration
                mediatorMock.VerifyNoOtherCalls();
            }
        }

        /// <summary>
        /// Tests that GetFeesByDepartment returns OkObjectResult with a null Value when IMediator returns null.
        /// Input conditions: mediator.Send returns null.
        /// Expected result: OkObjectResult whose Value is null (controller does not throw).
        /// </summary>
        [TestMethod]
        public async Task GetFeesByDepartment_MediatorReturnsNull_ReturnsOkWithNullValue()
        {
            // Arrange
            int departmentId = 5;
            int studyYearId = 2022;
            List<FeeDto>? expected = null;
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetFeesOfDepartmentForStudyYearQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new FeeController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.GetFeesByDepartment(departmentId, studyYearId);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult even when mediator returns null.");
            var ok = actionResult as OkObjectResult;
            Assert.IsNull(ok?.Value, "Expected null payload when mediator returns null.");
            mediatorMock.Verify(m => m.Send(It.IsAny<GetFeesOfDepartmentForStudyYearQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            mediatorMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Tests that GetFeesByDepartment returns OkObjectResult with an empty list when IMediator returns an empty list.
        /// Input conditions: mediator.Send returns an empty List&lt;FeeDto&gt;.
        /// Expected result: OkObjectResult whose Value is the same empty list instance returned by IMediator.
        /// </summary>
        [TestMethod]
        public async Task GetFeesByDepartment_MediatorReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            int departmentId = 10;
            int studyYearId = 1;
            var expected = new List<FeeDto>();
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetFeesOfDepartmentForStudyYearQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new FeeController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.GetFeesByDepartment(departmentId, studyYearId);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult for empty list response.");
            var ok = actionResult as OkObjectResult;
            Assert.AreSame(expected, ok?.Value, "Expected the controller to return the exact empty list instance from IMediator.");
            mediatorMock.Verify(m => m.Send(It.IsAny<GetFeesOfDepartmentForStudyYearQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            mediatorMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Tests that GetFeesByStudyYear constructs the GetFeesOfStudyYearQuery with the provided studyYearId,
        /// forwards it to IMediator.Send and returns an OkObjectResult containing exactly the list returned by the mediator.
        /// This test exercises multiple studyYearId boundary and representative values (int.MinValue, -1, 0, 1, int.MaxValue).
        /// Expected: OkObjectResult and the returned Value is the same instance as provided by IMediator.
        /// </summary>
        [TestMethod]
        public async Task GetFeesByStudyYear_StudyYearIdValues_ReturnsOkWithMediatorResult()
        {
            // Arrange
            int[] testStudyYearIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int studyYearId in testStudyYearIds)
            {
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

                // Provide a distinct list instance per iteration to validate returned object identity
                var expectedList = new List<FeeDto>();

                mediatorMock
                    .Setup(m => m.Send(
                        It.Is<GetFeesOfStudyYearQuery>(q => q != null && q.StudyYearId == studyYearId),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedList)
                    .Verifiable();

                var controller = new FeeController(mediatorMock.Object);

                // Act
                IActionResult actionResult = await controller.GetFeesByStudyYear(studyYearId);

                // Assert
                Assert.IsNotNull(actionResult, "ActionResult should not be null.");
                var okResult = actionResult as OkObjectResult;
                Assert.IsNotNull(okResult, "Expected OkObjectResult when mediator returns a list.");
                Assert.AreSame(expectedList, okResult.Value, "Returned object must be the exact instance provided by IMediator.");

                mediatorMock.Verify(m => m.Send(
                    It.Is<GetFeesOfStudyYearQuery>(q => q != null && q.StudyYearId == studyYearId),
                    It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        /// <summary>
        /// Tests the behavior when IMediator.Send returns null for the GetFeesOfStudyYearQuery.
        /// Input: an arbitrary studyYearId (0).
        /// Expected: OkObjectResult is returned and its Value is null (no exception thrown).
        /// </summary>
        [TestMethod]
        public async Task GetFeesByStudyYear_MediatorReturnsNull_ReturnsOkWithNullValue()
        {
            // Arrange
            int studyYearId = 0;
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            mediatorMock
                .Setup(m => m.Send(
                    It.Is<GetFeesOfStudyYearQuery>(q => q != null && q.StudyYearId == studyYearId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<FeeDto>?)null)
                .Verifiable();

            var controller = new FeeController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.GetFeesByStudyYear(studyYearId);

            // Assert
            Assert.IsNotNull(actionResult, "ActionResult should not be null even if mediator returns null.");
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult, "Expected OkObjectResult when mediator returns null.");
            Assert.IsNull(okResult.Value, "OkObjectResult.Value should be null when mediator returned null.");

            mediatorMock.Verify(m => m.Send(
                It.Is<GetFeesOfStudyYearQuery>(q => q != null && q.StudyYearId == studyYearId),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that UpdateFee constructs an UpdateFeeCommand with the provided id and feeDto,
        /// sends it through IMediator exactly once, and returns a NoContentResult.
        /// Tests multiple representative id boundary values (int.MinValue, negative, zero, positive, int.MaxValue)
        /// and a few representative UpdateFeeDto payloads (null Description, whitespace Description, extreme Amounts).
        /// Expected: mediator.Send is invoked with an UpdateFeeCommand containing the same id and the same feeDto instance,
        /// and the controller returns NoContentResult.
        /// </summary>
        [TestMethod]
        public async Task UpdateFee_ValidInputs_CallsMediatorAndReturnsNoContent()
        {
            // Arrange: parameter sets to iterate (covers numeric boundaries for id and amount edge-cases for DTO)
            int[] ids = new[] { int.MinValue, -1, 0, 1, int.MaxValue };
            UpdateFeeDto[] dtos = new[]
            {
                new UpdateFeeDto { Description = null, Amount = 0m },                                   // description null, zero amount
                new UpdateFeeDto { Description = "   ", Amount = 100.50m },                              // whitespace description, normal amount
                new UpdateFeeDto { Description = new string('x', 1024), Amount = decimal.MaxValue },     // very long description, max amount
                new UpdateFeeDto { Description = "special:\u0000\u001F\n\t", Amount = decimal.MinValue } // control chars, min amount
            };

            foreach (var id in ids)
            {
                foreach (var feeDto in dtos)
                {
                    // Arrange: fresh mock & controller per iteration to keep verifications independent
                    var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
                    mediatorMock
                        .Setup(m => m.Send(It.IsAny<IRequest<Unit>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Unit.Value)
                        .Verifiable();

                    var controller = new FeeController(mediatorMock.Object);

                    // Act
                    IActionResult result = await controller.UpdateFee(id, feeDto);

                    // Assert
                    Assert.IsNotNull(result, "Result should not be null.");
                    Assert.IsInstanceOfType(result, typeof(NoContentResult), "Expected NoContentResult.");

                    mediatorMock.Verify(m =>
                        m.Send(It.Is<IRequest<Unit>>(r =>
                            r is UpdateFeeCommand uc && uc.Id == id && object.ReferenceEquals(uc.FeeDto, feeDto)
                        ), It.IsAny<CancellationToken>()), Times.Once);

                    mediatorMock.Verify();
                }
            }
        }

        /// <summary>
        /// Verifies that if IMediator.Send throws an exception, the controller does not swallow it and the exception propagates.
        /// Condition: mediator configured to throw InvalidOperationException when Send is called.
        /// Expected: the call to UpdateFee results in the same InvalidOperationException being thrown.
        /// </summary>
        [TestMethod]
        public async Task UpdateFee_MediatorThrows_ExceptionPropagates()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            var expectedEx = new InvalidOperationException("mediator-failure");
            mediatorMock
                .Setup(m => m.Send(It.IsAny<IRequest<Unit>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedEx)
                .Verifiable();

            var controller = new FeeController(mediatorMock.Object);

            var dto = new UpdateFeeDto { Description = "test", Amount = 1.23m };
            int id = 42;

            // Act & Assert
            var thrown = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await controller.UpdateFee(id, dto));

            Assert.AreSame(expectedEx, thrown, "The exception thrown by the mediator should propagate unchanged.");

            mediatorMock.Verify(m =>
                m.Send(It.Is<IRequest<Unit>>(r =>
                    r is UpdateFeeCommand uc && uc.Id == id && object.ReferenceEquals(uc.FeeDto, dto)
                ), It.IsAny<CancellationToken>()), Times.Once);

            mediatorMock.Verify();
        }

        /// <summary>
        /// Verifies that DeleteFee calls IMediator.Send with a DeleteFeeCommand whose Id matches the provided id
        /// and that the controller returns NoContentResult on successful mediator completion.
        /// Tests multiple representative integer values including boundary values.
        /// </summary>
        [TestMethod]
        public async Task DeleteFee_ValidIds_CallsMediatorAndReturnsNoContent()
        {
            // Arrange
            int[] testIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int id in testIds)
            {
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
                mediatorMock
                    .Setup(m => m.Send(It.Is<DeleteFeeCommand>(c => c != null && c.Id == id), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Unit.Value)
                    .Verifiable();

                var controller = new FeeController(mediatorMock.Object);

                // Act
                IActionResult result = await controller.DeleteFee(id);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NoContentResult), $"Expected NoContentResult for id={id}");
                mediatorMock.Verify(m => m.Send(It.Is<DeleteFeeCommand>(c => c != null && c.Id == id), It.IsAny<CancellationToken>()), Times.Once, $"Mediator.Send should be called once for id={id}");
            }
        }

        /// <summary>
        /// Ensures that if IMediator.Send throws an exception, the exception propagates from DeleteFee.
        /// Input: arbitrary id (42). Expected: the same exception is thrown by the controller action.
        /// </summary>
        [TestMethod]
        public async Task DeleteFee_MediatorThrows_ExceptionPropagates()
        {
            // Arrange
            int id = 42;
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            var expectedEx = new InvalidOperationException("mediator failure");
            mediatorMock
                .Setup(m => m.Send(It.Is<DeleteFeeCommand>(c => c != null && c.Id == id), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedEx);

            var controller = new FeeController(mediatorMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await controller.DeleteFee(id));
            Assert.AreEqual(expectedEx.Message, ex.Message);
            mediatorMock.Verify(m => m.Send(It.Is<DeleteFeeCommand>(c => c != null && c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}