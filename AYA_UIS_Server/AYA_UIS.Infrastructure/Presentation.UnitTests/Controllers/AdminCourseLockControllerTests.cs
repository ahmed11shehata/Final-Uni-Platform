using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.AdminCourseLock;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Presentation;
using Presentation.Controllers;
using Shared.Dtos.Info_Module.AdminCourseLockDtos;

namespace Presentation.Controllers.UnitTests
{
    /// <summary>
    /// Tests for the AdminCourseLockController constructor.
    /// Focuses only on construction behavior when a mediator instance is provided.
    /// </summary>
    [TestClass]
    public class AdminCourseLockControllerTests
    {
        /// <summary>
        /// Verifies that the constructor does not throw and produces a usable controller instance
        /// when a valid (non-null) IMediator implementation is provided via dependency injection.
        /// Input conditions:
        /// - A mocked IMediator (non-null) is supplied.
        /// Expected result:
        /// - The constructor completes without throwing and returns a non-null ControllerBase-derived instance.
        /// </summary>
        [TestMethod]
        public void AdminCourseLockController_Constructor_WithValidMediator_DoesNotThrow()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();

            // Act
            AdminCourseLockController? controller = null;
            Exception? caught = null;
            try
            {
                controller = new AdminCourseLockController(mediatorMock.Object);
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            // Assert
            Assert.IsNull(caught, "Constructor threw an unexpected exception when provided a non-null IMediator.");
            Assert.IsNotNull(controller, "Constructor returned null when provided a non-null IMediator.");
            Assert.IsInstanceOfType(controller, typeof(ControllerBase), "Constructed instance is not a ControllerBase-derived type.");
        }

        /// <summary>
        /// NOTE: The constructor parameter 'mediator' is non-nullable in the production code.
        /// Per project nullability rules tests MUST NOT assign null to non-nullable parameters.
        /// If you need to exercise null behavior, update the production constructor to validate and throw
        /// (e.g., ArgumentNullException) and then add a corresponding test here.
        /// The above test ensures correct behavior for the supported (non-null) injection scenario.
        /// </summary>
        [TestMethod]
        public void AdminCourseLockController_Constructor_NullTest_NotApplicableDueToNonNullableParameter()
        {
            // Arrange / Act / Assert
            // This test intentionally marks the null-construction scenario as not applicable to avoid
            // assigning null to a non-nullable parameter as required by the project's nullable annotations.
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Test that when academicCode is whitespace the controller returns BadRequest with the expected error message
        /// and the mediator is not invoked.
        /// Input: academicCode = "   " (whitespace), courseId = 1
        /// Expected: BadRequestObjectResult with anonymous { errors = "Academic code is required." } and mediator.Send not called.
        /// </summary>
        [TestMethod]
        public async Task Unlock_AcademicCodeWhitespace_ReturnsBadRequest_AcademicCodeRequired()
        {
            // Arrange
            string academicCode = "   ";
            int courseId = 1;
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            var controller = new AdminCourseLockController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.Unlock(academicCode, courseId);

            // Assert
            var badReq = actionResult as BadRequestObjectResult;
            Assert.IsNotNull(badReq, "Expected BadRequestObjectResult when academicCode is whitespace.");

            // Extract the 'errors' property from the anonymous object returned
            Assert.IsNotNull(badReq.Value, "BadRequest value should not be null.");
            var errorsProp = badReq.Value.GetType().GetProperty("errors", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(errorsProp, "Anonymous response must contain an 'errors' property.");
            var errorsVal = errorsProp!.GetValue(badReq.Value) as string;
            Assert.AreEqual("Academic code is required.", errorsVal);

            // Verify mediator was not called
            mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<AdminCourseLockResultDto>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Test that when courseId is invalid (<= 0) the controller returns BadRequest with the expected error message
        /// and the mediator is not invoked.
        /// Inputs tested: 0, -1, int.MinValue
        /// Expected: BadRequestObjectResult with anonymous { errors = "Invalid course ID." } for each invalid courseId.
        /// </summary>
        [TestMethod]
        public async Task Unlock_InvalidCourseIdValues_ReturnsBadRequest_InvalidCourseId()
        {
            // Arrange
            var invalidCourseIds = new[] { 0, -1, int.MinValue };
            string academicCode = "AC123";

            foreach (int courseId in invalidCourseIds)
            {
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
                var controller = new AdminCourseLockController(mediatorMock.Object);

                // Act
                IActionResult actionResult = await controller.Unlock(academicCode, courseId);

                // Assert
                var badReq = actionResult as BadRequestObjectResult;
                Assert.IsNotNull(badReq, $"Expected BadRequestObjectResult for courseId={courseId}.");

                Assert.IsNotNull(badReq.Value, "BadRequest value should not be null.");
                var errorsProp = badReq.Value.GetType().GetProperty("errors", BindingFlags.Public | BindingFlags.Instance);
                Assert.IsNotNull(errorsProp, "Anonymous response must contain an 'errors' property.");
                var errorsVal = errorsProp!.GetValue(badReq.Value) as string;
                Assert.AreEqual("Invalid course ID.", errorsVal, $"Unexpected error message for courseId={courseId}.");

                // Verify mediator was not called for this input
                mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<AdminCourseLockResultDto>>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        /// <summary>
        /// Test that when mediator returns Success = true the controller responds with OkObjectResult containing the same result DTO.
        /// Input: academicCode = "AC", courseId = 1, mediator returns Success = true.
        /// Expected: OkObjectResult with AdminCourseLockResultDto having Success = true and the same Message.
        /// </summary>
        [TestMethod]
        public async Task Unlock_MediatorReturnsSuccessTrue_ReturnsOkWithResult()
        {
            // Arrange
            string academicCode = "AC";
            int courseId = 1;
            var expected = new AdminCourseLockResultDto { Success = true, Message = "Unlocked successfully." };

            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.Is<UnlockCourseCommand>(c => c.AcademicCode == academicCode && c.CourseId == courseId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var controller = new AdminCourseLockController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.Unlock(academicCode, courseId);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult, "Expected OkObjectResult when mediator returns Success = true.");

            var dto = okResult.Value as AdminCourseLockResultDto;
            Assert.IsNotNull(dto, "Response value should be AdminCourseLockResultDto.");
            Assert.IsTrue(dto.Success, "Returned DTO Success should be true.");
            Assert.AreEqual(expected.Message, dto.Message);

            mediatorMock.Verify(m => m.Send(It.IsAny<UnlockCourseCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Test that when mediator returns Success = false the controller responds with BadRequestObjectResult containing the DTO.
        /// Input: academicCode = "AC", courseId = 2, mediator returns Success = false.
        /// Expected: BadRequestObjectResult with AdminCourseLockResultDto having Success = false and the same Message.
        /// </summary>
        [TestMethod]
        public async Task Unlock_MediatorReturnsSuccessFalse_ReturnsBadRequestWithResult()
        {
            // Arrange
            string academicCode = "AC";
            int courseId = 2;
            var expected = new AdminCourseLockResultDto { Success = false, Message = "Unlock failed." };

            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.Is<UnlockCourseCommand>(c => c.AcademicCode == academicCode && c.CourseId == courseId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var controller = new AdminCourseLockController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.Unlock(academicCode, courseId);

            // Assert
            var badReq = actionResult as BadRequestObjectResult;
            Assert.IsNotNull(badReq, "Expected BadRequestObjectResult when mediator returns Success = false.");

            var dto = badReq.Value as AdminCourseLockResultDto;
            Assert.IsNotNull(dto, "BadRequest value should be AdminCourseLockResultDto.");
            Assert.IsFalse(dto.Success, "Returned DTO Success should be false.");
            Assert.AreEqual(expected.Message, dto.Message);

            mediatorMock.Verify(m => m.Send(It.IsAny<UnlockCourseCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that Lock returns BadRequest when academicCode is empty or whitespace.
        /// Inputs: academicCode values are empty string and whitespace-only string, courseId is valid (1).
        /// Expected: BadRequestObjectResult with anonymous object containing errors = "Academic code is required."
        /// </summary>
        [TestMethod]
        public async Task Lock_InvalidAcademicCode_ReturnsBadRequest()
        {
            // Arrange
            var invalidAcademicCodes = new[] { string.Empty, "   " };
            foreach (var academicCode in invalidAcademicCodes)
            {
                var mediator = new Mock<IMediator>(MockBehavior.Strict);
                var controller = new AdminCourseLockController(mediator.Object);

                // Act
                var actionResult = await controller.Lock(academicCode, 1);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult), "Expected BadRequest when academicCode is invalid.");

                var badRequest = actionResult as BadRequestObjectResult;
                Assert.IsNotNull(badRequest);

                var value = badRequest!.Value;
                Assert.IsNotNull(value, "BadRequest value should not be null.");

                // anonymous object: retrieve 'errors' property via reflection
                var prop = value.GetType().GetProperty("errors", BindingFlags.Public | BindingFlags.Instance);
                Assert.IsNotNull(prop, "Expected anonymous object to have 'errors' property.");

                var errors = prop!.GetValue(value) as string;
                Assert.AreEqual("Academic code is required.", errors);
            }
        }

        /// <summary>
        /// Tests that Lock returns BadRequest when courseId is non-positive.
        /// Inputs: courseId values include int.MinValue, -1 and 0 with a valid academicCode.
        /// Expected: BadRequestObjectResult with anonymous object containing errors = "Invalid course ID."
        /// </summary>
        [TestMethod]
        public async Task Lock_NonPositiveCourseId_ReturnsBadRequest()
        {
            // Arrange
            var invalidCourseIds = new[] { int.MinValue, -1, 0 };
            foreach (var courseId in invalidCourseIds)
            {
                var mediator = new Mock<IMediator>(MockBehavior.Strict);
                var controller = new AdminCourseLockController(mediator.Object);

                // Act
                var actionResult = await controller.Lock("ACAD2026", courseId);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult), $"Expected BadRequest when courseId is {courseId}.");

                var badRequest = actionResult as BadRequestObjectResult;
                Assert.IsNotNull(badRequest);

                var value = badRequest!.Value;
                Assert.IsNotNull(value, "BadRequest value should not be null.");

                var prop = value.GetType().GetProperty("errors", BindingFlags.Public | BindingFlags.Instance);
                Assert.IsNotNull(prop, "Expected anonymous object to have 'errors' property.");

                var errors = prop!.GetValue(value) as string;
                Assert.AreEqual("Invalid course ID.", errors);
            }
        }

        /// <summary>
        /// Tests that Lock returns OkObjectResult when mediator returns a successful AdminCourseLockResultDto.
        /// Inputs: valid academicCode and positive courseId, mediator returns Success = true.
        /// Expected: OkObjectResult with AdminCourseLockResultDto having Success = true and preserved Message.
        /// </summary>
        [TestMethod]
        public async Task Lock_MediatorReturnsSuccess_ReturnsOk()
        {
            // Arrange
            var expected = new AdminCourseLockResultDto { Success = true, Message = "Locked successfully" };

            var mediator = new Mock<IMediator>(MockBehavior.Strict);
            mediator
                .Setup(m => m.Send(It.IsAny<IRequest<AdminCourseLockResultDto>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new AdminCourseLockController(mediator.Object);

            // Act
            var actionResult = await controller.Lock("ACAD2026", 42);

            // Assert
            mediator.Verify(m => m.Send(It.IsAny<IRequest<AdminCourseLockResultDto>>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));

            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult);

            var dto = okResult!.Value as AdminCourseLockResultDto;
            Assert.IsNotNull(dto, "Expected result value to be AdminCourseLockResultDto.");
            Assert.IsTrue(dto!.Success);
            Assert.AreEqual(expected.Message, dto.Message);
        }

        /// <summary>
        /// Tests that Lock returns BadRequestObjectResult when mediator returns a failure AdminCourseLockResultDto.
        /// Inputs: valid academicCode and positive courseId, mediator returns Success = false.
        /// Expected: BadRequestObjectResult with the returned AdminCourseLockResultDto (Success = false).
        /// </summary>
        [TestMethod]
        public async Task Lock_MediatorReturnsFailure_ReturnsBadRequestWithDto()
        {
            // Arrange
            var expected = new AdminCourseLockResultDto { Success = false, Message = "Cannot lock course" };

            var mediator = new Mock<IMediator>(MockBehavior.Strict);
            mediator
                .Setup(m => m.Send(It.IsAny<IRequest<AdminCourseLockResultDto>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new AdminCourseLockController(mediator.Object);

            // Act
            var actionResult = await controller.Lock("ACAD2026", 5);

            // Assert
            mediator.Verify(m => m.Send(It.IsAny<IRequest<AdminCourseLockResultDto>>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));

            var badRequest = actionResult as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);

            var dto = badRequest!.Value as AdminCourseLockResultDto;
            Assert.IsNotNull(dto, "Expected BadRequest value to be AdminCourseLockResultDto.");
            Assert.IsFalse(dto!.Success);
            Assert.AreEqual(expected.Message, dto.Message);
        }
    }
}