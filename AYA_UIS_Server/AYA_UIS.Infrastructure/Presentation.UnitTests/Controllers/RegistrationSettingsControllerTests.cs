using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.RegistrationSettings;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.RegistrationSettings;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Presentation;
using Presentation.Controllers;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.RegistrationSettingsDtos;

namespace Presentation.Controllers.UnitTests
{
    [TestClass]
    public partial class RegistrationSettingsControllerTests
    {
        /// <summary>
        /// Verifies that when mediator returns a non-null RegistrationStatusDto the controller
        /// returns OkObjectResult containing the same instance and that mediator.Send is invoked exactly once
        /// with a GetRegistrationStatusQuery.
        /// Input conditions:
        /// - IMediator.Send returns a non-null RegistrationStatusDto.
        /// Expected result:
        /// - IActionResult is OkObjectResult whose Value equals the returned dto.
        /// </summary>
        [TestMethod]
        public async Task GetStatus_MediatorReturnsDto_ReturnsOkWithDto()
        {
            // Arrange
            var dto = new RegistrationStatusDto
            {
                // Properties are unknown; rely on reference equality check.
            };

            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            mockMediator
                .Setup(m => m.Send(It.IsAny<GetRegistrationStatusQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto)
                .Verifiable();

            var controller = new RegistrationSettingsController(mockMediator.Object);

            // Act
            IActionResult actionResult = await controller.GetStatus();

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult for successful mediator response.");
            var okResult = (OkObjectResult)actionResult;
            Assert.AreSame(dto, okResult.Value, "Returned value should be the exact dto instance from mediator.");
            mockMediator.Verify(m => m.Send(It.IsAny<GetRegistrationStatusQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that when mediator returns null the controller still returns OkObjectResult with null value.
        /// Input conditions:
        /// - IMediator.Send returns null.
        /// Expected result:
        /// - IActionResult is OkObjectResult whose Value is null.
        /// </summary>
        [TestMethod]
        public async Task GetStatus_MediatorReturnsNull_ReturnsOkWithNull()
        {
            // Arrange
            RegistrationStatusDto? dto = null;

            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            mockMediator
                .Setup(m => m.Send(It.IsAny<GetRegistrationStatusQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto)
                .Verifiable();

            var controller = new RegistrationSettingsController(mockMediator.Object);

            // Act
            IActionResult actionResult = await controller.GetStatus();

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult even when mediator returns null.");
            var okResult = (OkObjectResult)actionResult;
            Assert.IsNull(okResult.Value, "Returned OkObjectResult.Value should be null when mediator returns null.");
            mockMediator.Verify(m => m.Send(It.IsAny<GetRegistrationStatusQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that if mediator.Send throws an exception the controller does not catch it and it propagates.
        /// Input conditions:
        /// - IMediator.Send throws InvalidOperationException.
        /// Expected result:
        /// - The call to GetStatus throws the same exception.
        /// </summary>
        [TestMethod]
        public async Task GetStatus_MediatorThrows_ExceptionIsPropagated()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            mockMediator
                .Setup(m => m.Send(It.IsAny<GetRegistrationStatusQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("mediator failure"))
                .Verifiable();

            var controller = new RegistrationSettingsController(mockMediator.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await controller.GetStatus());

            mockMediator.Verify(m => m.Send(It.IsAny<GetRegistrationStatusQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Arrange: Controller with invalid ModelState (one model error).
        /// Act: Call Open with any DTO.
        /// Assert: Expect BadRequestObjectResult whose 'errors' contains the model error messages.
        /// </summary>
        [TestMethod]
        public async Task Open_ModelStateInvalid_ReturnsBadRequest_WithModelErrors()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            var controller = new RegistrationSettingsController(mediatorMock.Object);

            // Make ModelState invalid
            controller.ModelState.AddModelError("field", "model error message");

            var dto = new OpenRegistrationDto
            {
                Semester = "S1",
                Deadline = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var actionResult = await controller.Open(dto);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)actionResult;
            Assert.IsNotNull(bad.Value);

            // anonymous type { errors = IEnumerable<string> }
            dynamic value = bad.Value;
            IEnumerable<string>? errors = value.errors as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.IsTrue(errors!.Contains("model error message"));
        }

        /// <summary>
        /// Arrange: Controller with valid ModelState but null body DTO.
        /// Act: Call Open with null.
        /// Assert: Expect BadRequestObjectResult with errors = \"Request body is required.\".
        /// </summary>
        [TestMethod]
        public async Task Open_NullDto_ReturnsBadRequest_RequestBodyRequired()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            var controller = new RegistrationSettingsController(mediatorMock.Object);

            // Act
            var actionResult = await controller.Open(dto: null!);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)actionResult;
            Assert.IsNotNull(bad.Value);

            dynamic value = bad.Value;
            // anonymous type { errors = string }
            string? error = value.errors as string;
            Assert.IsNotNull(error);
            Assert.AreEqual("Request body is required.", error);
        }

        /// <summary>
        /// Arrange: Controller with valid ModelState and DTOs having empty or whitespace Semester values.
        /// Act: Call Open for each invalid semester string.
        /// Assert: Expect BadRequestObjectResult with errors = \"Semester is required.\".
        /// </summary>
        [TestMethod]
        public async Task Open_EmptyOrWhitespaceSemester_ReturnsBadRequest_SemesterRequired()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            var controller = new RegistrationSettingsController(mediatorMock.Object);

            var invalidSemesters = new[] { string.Empty, "   ", "\t\n" };

            foreach (var sem in invalidSemesters)
            {
                var dto = new OpenRegistrationDto
                {
                    Semester = sem,
                    Deadline = DateTime.UtcNow.AddDays(2)
                };

                // Act
                var actionResult = await controller.Open(dto);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult), $"Failed for semester value: [{sem}]");
                var bad = (BadRequestObjectResult)actionResult;
                Assert.IsNotNull(bad.Value);

                dynamic value = bad.Value;
                string? error = value.errors as string;
                Assert.IsNotNull(error);
                Assert.AreEqual("Semester is required.", error);
            }
        }

        /// <summary>
        /// Arrange: Controller with valid ModelState and DTO having default Deadline (DateTime.MinValue).
        /// Act: Call Open.
        /// Assert: Expect BadRequestObjectResult with errors = \"Deadline is required.\".
        /// </summary>
        [TestMethod]
        public async Task Open_DeadlineDefault_ReturnsBadRequest_DeadlineRequired()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            var controller = new RegistrationSettingsController(mediatorMock.Object);

            var dto = new OpenRegistrationDto
            {
                Semester = "Spring",
                Deadline = default // DateTime.MinValue
            };

            // Act
            var actionResult = await controller.Open(dto);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)actionResult;
            Assert.IsNotNull(bad.Value);

            dynamic value = bad.Value;
            string? error = value.errors as string;
            Assert.IsNotNull(error);
            Assert.AreEqual("Deadline is required.", error);
        }

        /// <summary>
        /// Arrange: Controller with valid ModelState and DTO having a Deadline in the past.
        /// Act: Call Open.
        /// Assert: Expect BadRequestObjectResult with errors = \"Deadline must be in the future.\".
        /// </summary>
        [TestMethod]
        public async Task Open_DeadlineInPastOrNow_ReturnsBadRequest_DeadlineMustBeInFuture()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            var controller = new RegistrationSettingsController(mediatorMock.Object);

            var dto = new OpenRegistrationDto
            {
                Semester = "Fall",
                Deadline = DateTime.UtcNow.AddSeconds(-5) // clearly in the past
            };

            // Act
            var actionResult = await controller.Open(dto);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)actionResult;
            Assert.IsNotNull(bad.Value);

            dynamic value = bad.Value;
            string? error = value.errors as string;
            Assert.IsNotNull(error);
            Assert.AreEqual("Deadline must be in the future.", error);
        }

        /// <summary>
        /// Arrange: Controller with valid ModelState and DTO satisfying validations.
        /// Mock: IMediator returns a RegistrationStatusDto.
        /// Act: Call Open.
        /// Assert: Expect OkObjectResult with the same RegistrationStatusDto returned by mediator and Send invoked once.
        /// </summary>
        [TestMethod]
        public async Task Open_ValidInput_ReturnsOk_WithRegistrationStatus()
        {
            // Arrange
            var expected = new RegistrationStatusDto
            {
                IsOpen = true,
                Semester = "Autumn",
                AcademicYear = "2025",
                StartDate = DateTime.UtcNow,
                Deadline = DateTime.UtcNow.AddDays(10),
                OpenYears = new List<int> { 1, 2 },
                EnabledCourses = new List<string> { "C1" },
                DaysLeft = 10
            };

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.IsAny<OpenRegistrationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new RegistrationSettingsController(mediatorMock.Object);

            var dto = new OpenRegistrationDto
            {
                Semester = "Autumn",
                Deadline = DateTime.UtcNow.AddDays(5),
                AcademicYear = "2025",
                StartDate = DateTime.UtcNow,
                OpenYears = new List<int> { 1 },
                EnabledCourses = new List<string> { "C1" }
            };

            // Act
            var actionResult = await controller.Open(dto);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var ok = (OkObjectResult)actionResult;
            Assert.IsNotNull(ok.Value);
            Assert.IsInstanceOfType(ok.Value, typeof(RegistrationStatusDto));

            var actual = (RegistrationStatusDto)ok.Value;
            Assert.AreEqual(expected.IsOpen, actual.IsOpen);
            Assert.AreEqual(expected.Semester, actual.Semester);
            Assert.AreEqual(expected.AcademicYear, actual.AcademicYear);
            Assert.AreEqual(expected.Deadline, actual.Deadline);
            CollectionAssert.AreEqual(expected.OpenYears, actual.OpenYears);
            CollectionAssert.AreEqual(expected.EnabledCourses, actual.EnabledCourses);

            mediatorMock.Verify(m => m.Send(It.IsAny<OpenRegistrationCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Test purpose:
        /// Verifies that when the mediator returns true for CloseRegistrationCommand,
        /// the controller returns an OkObjectResult containing { success = true, message = "Registration window closed." }.
        /// Input conditions:
        /// - IMediator.Send(...) returns true.
        /// Expected result:
        /// - OkObjectResult with anonymous value containing success == true and expected message,
        ///   and IMediator.Send called exactly once with CloseRegistrationCommand.
        /// </summary>
        [TestMethod]
        public async Task Close_MediatorReturnsTrue_ReturnsOkWithSuccessTrueAndClosedMessage()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            mockMediator
                .Setup(m => m.Send(It.IsAny<CloseRegistrationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            var controller = new RegistrationSettingsController(mockMediator.Object);

            // Act
            var actionResult = await controller.Close();

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));

            var okResult = (OkObjectResult)actionResult;
            Assert.IsNotNull(okResult.Value);

            var value = okResult.Value!;
            var valueType = value.GetType();

            var successProp = valueType.GetProperty("success", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(successProp, "Anonymous object should have a 'success' property.");
            var successVal = successProp!.GetValue(value);
            Assert.IsInstanceOfType(successVal, typeof(bool));
            Assert.AreEqual(true, (bool)successVal);

            var messageProp = valueType.GetProperty("message", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(messageProp, "Anonymous object should have a 'message' property.");
            var messageVal = messageProp!.GetValue(value) as string;
            Assert.AreEqual("Registration window closed.", messageVal);

            mockMediator.Verify(m => m.Send(It.IsAny<CloseRegistrationCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMediator.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Test purpose:
        /// Verifies that when the mediator returns false for CloseRegistrationCommand,
        /// the controller returns an OkObjectResult containing { success = false, message = "No active registration window to close." }.
        /// Input conditions:
        /// - IMediator.Send(...) returns false.
        /// Expected result:
        /// - OkObjectResult with anonymous value containing success == false and expected message,
        ///   and IMediator.Send called exactly once with CloseRegistrationCommand.
        /// </summary>
        [TestMethod]
        public async Task Close_MediatorReturnsFalse_ReturnsOkWithSuccessFalseAndNoActiveMessage()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            mockMediator
                .Setup(m => m.Send(It.IsAny<CloseRegistrationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .Verifiable();

            var controller = new RegistrationSettingsController(mockMediator.Object);

            // Act
            var actionResult = await controller.Close();

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));

            var okResult = (OkObjectResult)actionResult;
            Assert.IsNotNull(okResult.Value);

            var value = okResult.Value!;
            var valueType = value.GetType();

            var successProp = valueType.GetProperty("success", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(successProp, "Anonymous object should have a 'success' property.");
            var successVal = successProp!.GetValue(value);
            Assert.IsInstanceOfType(successVal, typeof(bool));
            Assert.AreEqual(false, (bool)successVal);

            var messageProp = valueType.GetProperty("message", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(messageProp, "Anonymous object should have a 'message' property.");
            var messageVal = messageProp!.GetValue(value) as string;
            Assert.AreEqual("No active registration window to close.", messageVal);

            mockMediator.Verify(m => m.Send(It.IsAny<CloseRegistrationCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            mockMediator.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Verifies that the constructor creates an instance when a valid IMediator is provided.
        /// Condition: a non-null mock IMediator is passed to the constructor.
        /// Expected result: an instance of RegistrationSettingsController is created and is not null.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidMediator_InstanceCreated()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();

            // Act
            var controller = new RegistrationSettingsController(mediatorMock.Object);

            // Assert
            Assert.IsNotNull(controller, "Constructor returned null when provided a valid IMediator.");
            Assert.IsInstanceOfType(controller, typeof(ControllerBase), "Controller should inherit from ControllerBase.");
            Assert.IsInstanceOfType(controller, typeof(RegistrationSettingsController), "Instance should be of type RegistrationSettingsController.");
        }

        /// <summary>
        /// Partial verification for constructor behavior when validating internal assignment.
        /// Condition: a mock IMediator is passed to the constructor.
        /// Expected result: constructor completes successfully. Verifying assignment to the private readonly field
        /// _mediator cannot be done without accessing private state; therefore this test is marked inconclusive
        /// and instructs on how to proceed (expose observable behavior or provide an accessor) to enable full verification.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidMediator_InternalAssignmentCannotBeDirectlyVerified()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();

            // Act
            var controller = new RegistrationSettingsController(mediatorMock.Object);

            // Assert - cannot access private readonly _mediator without reflection (disallowed by test guidelines).
            // Recommend: verify the mediator is used by exercising a public method (e.g., GetStatus/Open/Close)
            // and asserting mediator interactions, or add an internal accessor for testing.
            Assert.IsNotNull(controller, "Constructor should create an instance when a valid mediator is supplied.");

            // Mark as inconclusive to indicate manual follow-up is required to fully verify private assignment.
            Assert.Inconclusive("Direct verification of the private readonly _mediator assignment is not possible without reflection or changing visibility. Exercise public methods that use the mediator or add an internal test accessor to fully assert assignment.");
        }
    }
}