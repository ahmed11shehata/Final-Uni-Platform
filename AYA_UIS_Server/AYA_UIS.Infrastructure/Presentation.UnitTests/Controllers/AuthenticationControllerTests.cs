using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application;
using AYA_UIS.Application.Contracts;
using AYA_UIS.Core.Abstractions;
using AYA_UIS.Core.Abstractions.Contracts;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Presentation;
using Presentation.Controllers;
using Shared.Dtos;
using Shared.Dtos.Auth_Module;

namespace Presentation.Controllers.UnitTests
{
    [TestClass]
    public class AuthenticationControllerTests
    {
        /// <summary>
        /// Test purpose:
        /// Verifies that when ModelState is invalid the controller returns a BadRequestObjectResult
        /// containing the validation error messages.
        /// Input conditions:
        /// - ModelState contains one or more errors.
        /// - A non-null LoginDto is provided (required by method signature).
        /// Expected result:
        /// - The action result is a BadRequestObjectResult.
        /// - The returned object's 'errors' property contains the ModelState error messages.
        /// - The authentication service is NOT invoked.
        /// </summary>
        [TestMethod]
        public async Task LoginAsync_ModelStateInvalid_ReturnsBadRequestWithErrors()
        {
            // Arrange
            var mockAuthService = new Mock<IAuthenticationService>(MockBehavior.Loose);
            var mockServiceManager = new Mock<IServiceManager>(MockBehavior.Strict);
            mockServiceManager.SetupGet(sm => sm.AuthenticationService).Returns(mockAuthService.Object);

            var controller = new AuthenticationController(mockServiceManager.Object);

            // Add model state errors to simulate invalid model
            controller.ModelState.AddModelError("Email", "Email is required");
            controller.ModelState.AddModelError("Password", "Password is required");

            var loginDto = new LoginDto { Email = string.Empty, Password = string.Empty };

            // Act
            var actionResult = await controller.LoginAsync(loginDto);

            // Assert - result type
            Assert.IsNotNull(actionResult);
            Assert.IsNotNull(actionResult.Result);
            Assert.IsInstanceOfType(actionResult.Result, typeof(BadRequestObjectResult));

            var badReq = (BadRequestObjectResult)actionResult.Result;
            Assert.IsNotNull(badReq.Value, "BadRequest should contain a value with errors property");

            // Assert - the anonymous object's 'errors' property contains the expected messages
            var valueType = badReq.Value.GetType();
            var errorsProp = valueType.GetProperty("errors");
            Assert.IsNotNull(errorsProp, "Returned object should expose an 'errors' property");

            var errorsObj = errorsProp.GetValue(badReq.Value) as IEnumerable<string>;
            Assert.IsNotNull(errorsObj, "errors property should be an IEnumerable<string>");

            var errorsList = errorsObj!.ToList();
            CollectionAssert.Contains(errorsList, "Email is required");
            CollectionAssert.Contains(errorsList, "Password is required");

            // Verify authentication service was not called
            mockAuthService.Verify(s => s.LoginAsync(It.IsAny<LoginDto>()), Times.Never);
        }

        /// <summary>
        /// Test purpose:
        /// Verifies that when ModelState is valid the controller calls the authentication service
        /// and returns an OkObjectResult containing a FrontendLoginResponseDto that maps from the returned UserResultDto.
        /// Input conditions:
        /// - A valid LoginDto is provided.
        /// - The AuthenticationService.LoginAsync returns a populated UserResultDto.
        /// Expected result:
        /// - The action result is an OkObjectResult.
        /// - The returned FrontendLoginResponseDto.Token equals the UserResultDto.Token.
        /// - The returned FrontendLoginResponseDto.User.Email equals the UserResultDto.Email.
        /// - The returned role is the lower-cased form of the UserResultDto.Role.
        /// </summary>
        [TestMethod]
        public async Task LoginAsync_ValidModel_ReturnsOkWithFrontendDto()
        {
            // Arrange
            var expectedUser = new UserResultDto
            {
                Id = "user-123",
                DisplayName = "John Doe",
                Email = "john@example.com",
                Token = "jwt-token",
                AcademicCode = "AC123",
                PhoneNumber = "555-0000",
                Role = "Admin",
                UserName = "johnd",
                TotalCredits = 100,
                AllowedCredits = 20,
                TotalGPA = 3.75m,
                Specialization = "CS",
                Level = null,
                DepartmentName = "Computer Science",
                DepartmentId = 5,
                ProfilePicture = null,
                Gender = (AYA_UIS.Core.Domain.Enums.Gender)0,
                CurrentSemesterId = 1,
                CurrentStudyYearId = 2026
            };

            var mockAuthService = new Mock<IAuthenticationService>(MockBehavior.Strict);
            mockAuthService
                .Setup(s => s.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync(expectedUser);

            var mockServiceManager = new Mock<IServiceManager>(MockBehavior.Strict);
            mockServiceManager.SetupGet(sm => sm.AuthenticationService).Returns(mockAuthService.Object);

            var controller = new AuthenticationController(mockServiceManager.Object);

            var loginDto = new LoginDto { Email = "john@example.com", Password = "securepassword" };

            // Act
            var actionResult = await controller.LoginAsync(loginDto);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsNotNull(actionResult.Result);
            Assert.IsInstanceOfType(actionResult.Result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)actionResult.Result;
            Assert.IsNotNull(okResult.Value);
            Assert.IsInstanceOfType(okResult.Value, typeof(FrontendLoginResponseDto));

            var frontendDto = (FrontendLoginResponseDto)okResult.Value;
            Assert.AreEqual(expectedUser.Token, frontendDto.Token, "Token should be copied to frontend DTO");
            Assert.IsNotNull(frontendDto.User, "User sub-object should not be null");
            Assert.AreEqual(expectedUser.Email, frontendDto.User.Email, "User email should be copied to frontend DTO");
            Assert.AreEqual(expectedUser.Role.ToLower(), frontendDto.User.Role, "Role should be lower-cased in frontend DTO");

            // Verify service was called exactly once with the provided loginDto
            mockAuthService.Verify(s => s.LoginAsync(It.Is<LoginDto>(ld => ld.Email == loginDto.Email && ld.Password == loginDto.Password)), Times.Once);
        }

        /// <summary>
        /// Verifies that when the ModelState is invalid, ResetPasswordByAdmin returns BadRequest
        /// and the authentication service is not invoked.
        /// Input: ResetPasswordDto with valid-looking values but a ModelState error is added to the controller.
        /// Expected: BadRequestObjectResult is returned and ResetPasswordAsync is never called.
        /// </summary>
        [TestMethod]
        public async Task ResetPasswordByAdmin_ModelStateInvalid_ReturnsBadRequestAndDoesNotCallService()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthenticationService>();
            var serviceManagerMock = new Mock<IServiceManager>();
            serviceManagerMock.SetupGet(s => s.AuthenticationService).Returns(authServiceMock.Object);

            var controller = new AuthenticationController(serviceManagerMock.Object);

            // Add an error to ModelState to simulate validation failure
            controller.ModelState.AddModelError("Email", "Email is required");

            var dto = new ResetPasswordDto
            {
                Email = "user@example.com",
                NewPassword = "NewP@ssw0rd"
            };

            // Act
            IActionResult actionResult = await controller.ResetPasswordByAdmin(dto);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));
            // Ensure the service was not called due to invalid model state
            authServiceMock.Verify(a => a.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Verifies that when ModelState is valid and the authentication service returns a non-null result,
        /// ResetPasswordByAdmin calls ResetPasswordAsync with the provided email and new password, and returns Ok with the result string.
        /// Input: Valid ResetPasswordDto.
        /// Expected: OkObjectResult containing the returned string and service invoked exactly once with expected arguments.
        /// </summary>
        [TestMethod]
        public async Task ResetPasswordByAdmin_ValidModel_CallsServiceAndReturnsOkWithResult()
        {
            // Arrange
            var expectedResult = "Password reset successfully";
            var email = "student@uni.edu";
            var newPassword = "S3cur3P@ss";

            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(a => a.ResetPasswordAsync(email, newPassword))
                .ReturnsAsync(expectedResult);

            var serviceManagerMock = new Mock<IServiceManager>();
            serviceManagerMock.SetupGet(s => s.AuthenticationService).Returns(authServiceMock.Object);

            var controller = new AuthenticationController(serviceManagerMock.Object);

            var dto = new ResetPasswordDto
            {
                Email = email,
                NewPassword = newPassword
            };

            // Act
            IActionResult actionResult = await controller.ResetPasswordByAdmin(dto);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var okResult = (OkObjectResult)actionResult;
            Assert.IsNotNull(okResult.Value);
            Assert.AreEqual(expectedResult, okResult.Value);
            authServiceMock.Verify(a => a.ResetPasswordAsync(email, newPassword), Times.Once);
        }

        /// <summary>
        /// Verifies behavior when the authentication service returns null.
        /// Input: Valid ResetPasswordDto and service returns null.
        /// Expected: OkObjectResult is returned and its Value is null; service invoked once.
        /// This exercises handling of a possibly unexpected null service response.
        /// </summary>
        [TestMethod]
        public async Task ResetPasswordByAdmin_ServiceReturnsNull_ReturnsOkWithNullValue()
        {
            // Arrange
            string? expectedResult = null;
            var email = "nullable@domain.test";
            var newPassword = "AnotherP@ss1";

            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(a => a.ResetPasswordAsync(email, newPassword))
                .ReturnsAsync(expectedResult);

            var serviceManagerMock = new Mock<IServiceManager>();
            serviceManagerMock.SetupGet(s => s.AuthenticationService).Returns(authServiceMock.Object);

            var controller = new AuthenticationController(serviceManagerMock.Object);

            var dto = new ResetPasswordDto
            {
                Email = email,
                NewPassword = newPassword
            };

            // Act
            IActionResult actionResult = await controller.ResetPasswordByAdmin(dto);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var okResult = (OkObjectResult)actionResult;
            Assert.IsNull(okResult.Value);
            authServiceMock.Verify(a => a.ResetPasswordAsync(email, newPassword), Times.Once);
        }

        /// <summary>
        /// Test purpose:
        /// Verifies that calling Logout on AuthenticationController returns an OkObjectResult
        /// containing an anonymous payload with a 'message' property set to "Logged out successfully".
        /// Input conditions:
        /// - No input parameters (method is parameterless).
        /// Expected result:
        /// - An OkObjectResult (HTTP 200) is returned.
        /// - The returned object's 'message' property equals "Logged out successfully".
        /// </summary>
        [TestMethod]
        public async Task Logout_WhenCalled_ReturnsOkObjectWithExpectedMessage()
        {
            // Arrange
            var mockServiceManager = new Mock<IServiceManager>(MockBehavior.Strict);
            // No setups required because Logout does not interact with IServiceManager.
            var controller = new AuthenticationController(mockServiceManager.Object);

            // Act
            var actionResult = await controller.Logout().ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(actionResult, "Expected a non-null IActionResult from Logout.");

            // Assert result is OkObjectResult
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult from Logout.");
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult, "OkObjectResult should not be null.");

            // Assert status code is 200
            // OkObjectResult.StatusCode may be set to 200 explicitly.
            Assert.AreEqual(200, okResult.StatusCode, "Expected HTTP 200 status code.");

            // Assert payload has 'message' property with expected value
            var value = okResult.Value;
            Assert.IsNotNull(value, "Expected non-null value in OkObjectResult.");

            var messageProp = value.GetType().GetProperty("message", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(messageProp, "Expected the returned object to have a 'message' property.");

            var messageValue = messageProp.GetValue(value) as string;
            Assert.IsNotNull(messageValue, "Expected 'message' property to be a non-null string.");
            Assert.AreEqual("Logged out successfully", messageValue, "The logout message did not match the expected value.");
        }

        /// <summary>
        /// Tests that when ModelState is invalid, RegisterAsync returns a BadRequestObjectResult
        /// containing the model errors and does NOT call the authentication service.
        /// Input conditions:
        ///  - registerDto: minimal non-null RegisterDto instance
        ///  - controller.ModelState: contains one model error
        /// Expected result:
        ///  - ActionResult.Result is a BadRequestObjectResult
        ///  - The returned object's 'errors' collection contains the model error message
        ///  - The underlying IAuthenticationService.RegisterAsync is not invoked
        /// </summary>
        [TestMethod]
        public async Task RegisterAsync_ModelStateInvalid_ReturnsBadRequestWithErrors()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            var serviceManagerMock = new Mock<IServiceManager>(MockBehavior.Strict);
            serviceManagerMock.SetupGet(s => s.AuthenticationService).Returns(authServiceMock.Object);

            var controller = new AuthenticationController(serviceManagerMock.Object);

            // Make model state invalid
            controller.ModelState.AddModelError("Email", "Invalid email format");

            var registerDto = new RegisterDto();

            // Act
            var action = await controller.RegisterAsync(registerDto, "Student");

            // Assert
            Assert.IsNotNull(action);
            Assert.IsNotNull(action.Result);
            Assert.IsInstanceOfType(action.Result, typeof(BadRequestObjectResult));

            var badRequest = (BadRequestObjectResult)action.Result!;
            Assert.IsNotNull(badRequest.Value);

            // The controller returns an anonymous object with property 'errors' => IEnumerable<string>
            dynamic? anon = badRequest.Value;
            IEnumerable<string>? errors = anon?.errors as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.IsTrue(errors!.Contains("Invalid email format"));

            // Verify service was not called
            authServiceMock.Verify(s => s.RegisterAsync(It.IsAny<RegisterDto>(), It.IsAny<string>()), Times.Never);
            serviceManagerMock.VerifyGet(s => s.AuthenticationService, Times.Once);
        }

        /// <summary>
        /// Tests that when ModelState is valid, RegisterAsync calls the authentication service with the provided role
        /// and returns an OkObjectResult wrapping the UserResultDto returned by the service.
        /// Input conditions:
        ///  - registerDto: non-null RegisterDto instance
        ///  - role: empty string (edge-case forwarded to service)
        /// Expected result:
        ///  - IAuthenticationService.RegisterAsync called once with the same registerDto and role
        ///  - ActionResult.Result is an OkObjectResult whose Value is the exact UserResultDto returned by the service
        /// </summary>
        [TestMethod]
        public async Task RegisterAsync_ValidModel_CallsServiceAndReturnsOk()
        {
            // Arrange
            var registerDto = new RegisterDto();
            string role = string.Empty; // edge-case: empty role forwarded to service

            var expectedUserResult = new UserResultDto();

            var authServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            authServiceMock
                .Setup(s => s.RegisterAsync(registerDto, role))
                .ReturnsAsync(expectedUserResult);

            var serviceManagerMock = new Mock<IServiceManager>(MockBehavior.Strict);
            serviceManagerMock.SetupGet(s => s.AuthenticationService).Returns(authServiceMock.Object);

            var controller = new AuthenticationController(serviceManagerMock.Object);

            // Ensure model state is valid (default)
            Assert.IsTrue(controller.ModelState.IsValid);

            // Act
            var action = await controller.RegisterAsync(registerDto, role);

            // Assert
            Assert.IsNotNull(action);

            // The controller returns Ok(userResult) which manifests as an ActionResult with Result = OkObjectResult
            Assert.IsNotNull(action.Result);
            Assert.IsInstanceOfType(action.Result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)action.Result!;
            Assert.IsNotNull(okResult.Value);
            Assert.AreSame(expectedUserResult, okResult.Value);

            // Verify service call
            authServiceMock.Verify(s => s.RegisterAsync(registerDto, role), Times.Once);
            serviceManagerMock.VerifyGet(s => s.AuthenticationService, Times.AtLeastOnce);
        }
    }
}