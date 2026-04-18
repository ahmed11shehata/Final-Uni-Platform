using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Dashboard;
using AYA_UIS.Application.Queries.RegistrationSettings;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Identity;
using MediatR;
using Microsoft;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Presentation;
using Presentation.Controllers;
using Shared.Dtos.Info_Module.DashboardDtos;
using Shared.Dtos.Info_Module.RegistrationSettingsDtos;

namespace Presentation.Controllers.UnitTests
{
    [TestClass]
    public partial class AdminDashboardControllerTests
    {
        /// <summary>
        /// Test that GetDashboard returns OkObjectResult containing the same AdminDashboardDto
        /// provided by the mediator.
        /// Conditions:
        ///  - mediator.Send successfully returns a populated AdminDashboardDto instance.
        /// Expected:
        ///  - The action result is OkObjectResult.
        ///  - The returned value is the same instance and contains the expected property values.
        /// </summary>
        [TestMethod]
        public async Task GetDashboard_WhenMediatorReturnsDto_ReturnsOkWithDto()
        {
            // Arrange
            var dto = new AdminDashboardDto
            {
                TotalStudents = 42,
                TotalInstructors = 7,
                TotalCourses = 13,
                ActiveRegistrations = 100,
                RegistrationOpen = true,
                CurrentStudyYear = new CurrentStudyYearDto { Id = 1, StartYear = 2023, EndYear = 2024 },
                RecentActivity = new List<object> { "a", 1, new { x = "y" } }
            };

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAdminDashboardQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // UserManager is not used by GetDashboard; provide a mocked instance
            var userStoreMock = new Mock<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object,
                /* IOptions<IdentityOptions> */ null,
                /* IPasswordHasher<User> */ null,
                /* IEnumerable<IUserValidator<User>> */ null,
                /* IEnumerable<IPasswordValidator<User>> */ null,
                /* ILookupNormalizer */ null,
                /* IdentityErrorDescriber */ null,
                /* IServiceProvider */ null,
                /* ILogger<UserManager<User>> */ null);

            var controller = new AdminDashboardController(mediatorMock.Object, userManagerMock.Object);

            // Act
            IActionResult actionResult = await controller.GetDashboard();

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));

            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnedDto = okResult!.Value as AdminDashboardDto;
            Assert.IsNotNull(returnedDto);
            Assert.AreSame(dto, returnedDto, "Controller should return the exact DTO instance provided by the mediator.");
            Assert.AreEqual(42, returnedDto.TotalStudents);
            Assert.AreEqual(7, returnedDto.TotalInstructors);
            Assert.AreEqual(13, returnedDto.TotalCourses);
            Assert.IsTrue(returnedDto.RegistrationOpen);
            Assert.IsNotNull(returnedDto.CurrentStudyYear);
            Assert.AreEqual(2023, returnedDto.CurrentStudyYear!.StartYear);

            mediatorMock.Verify(m => m.Send(It.IsAny<GetAdminDashboardQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Test that GetDashboard propagates exceptions from the mediator.
        /// Conditions:
        ///  - mediator.Send throws InvalidOperationException.
        /// Expected:
        ///  - The controller method throws the same InvalidOperationException.
        /// </summary>
        [TestMethod]
        public async Task GetDashboard_WhenMediatorThrows_ThrowsException()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAdminDashboardQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("mediator-failure"));

            var userStoreMock = new Mock<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object,
                /* IOptions<IdentityOptions> */ null,
                /* IPasswordHasher<User> */ null,
                /* IEnumerable<IUserValidator<User>> */ null,
                /* IEnumerable<IPasswordValidator<User>> */ null,
                /* ILookupNormalizer */ null,
                /* IdentityErrorDescriber */ null,
                /* IServiceProvider */ null,
                /* ILogger<UserManager<User>> */ null);

            var controller = new AdminDashboardController(mediatorMock.Object, userManagerMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await controller.GetDashboard());

            mediatorMock.Verify(m => m.Send(It.IsAny<GetAdminDashboardQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Test that GetDashboard handles a null response from the mediator by returning Ok with null value.
        /// Conditions:
        ///  - mediator.Send returns null.
        /// Expected:
        ///  - The action result is OkObjectResult with a null Value.
        /// </summary>
        [TestMethod]
        public async Task GetDashboard_WhenMediatorReturnsNull_ReturnsOkWithNull()
        {
            // Arrange
            AdminDashboardDto? dto = null;

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAdminDashboardQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var userStoreMock = new Mock<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object,
                /* IOptions<IdentityOptions> */ null,
                /* IPasswordHasher<User> */ null,
                /* IEnumerable<IUserValidator<User>> */ null,
                /* IEnumerable<IPasswordValidator<User>> */ null,
                /* ILookupNormalizer */ null,
                /* IdentityErrorDescriber */ null,
                /* IServiceProvider */ null,
                /* ILogger<UserManager<User>> */ null);

            var controller = new AdminDashboardController(mediatorMock.Object, userManagerMock.Object);

            // Act
            IActionResult actionResult = await controller.GetDashboard();

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNull(okResult!.Value, "Expected OkObjectResult.Value to be null when mediator returns null.");

            mediatorMock.Verify(m => m.Send(It.IsAny<GetAdminDashboardQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that GetUsers calls mediator.Send with a GetAdminUsersQuery and returns OkObjectResult
        /// for a variety of search and role inputs (null, empty, whitespace, long, special characters).
        /// Expected: OkObjectResult with the same IEnumerable returned by mediator and mediator.Send invoked once per call.
        /// </summary>
        [TestMethod]
        public async Task GetUsers_VariousSearchAndRoleInputs_ReturnsOkAndInvokesMediator()
        {
            // Arrange: prepare a set of test inputs covering edge string cases
            (string? search, string? role)[] cases = new (string?, string?)[]
            {
                (null, null),
                (string.Empty, string.Empty),
                ("   ", "admin"),
                (new string('a', 1000), "role"),
                ("\n\t\u0000!@#", "r")
            };

            foreach ((string? search, string? role) in cases)
            {
                // Arrange
                var expected = new List<AdminUserDto>(); // empty result set
                var mockMediator = new Mock<IMediator>();
                mockMediator
                    .Setup(m => m.Send(It.IsAny<GetAdminUsersQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expected);

                var mockUserStore = Mock.Of<IUserStore<User>>();
                var mockUserManager = new Mock<UserManager<User>>(mockUserStore, null, null, null, null, null, null, null, null);

                var controller = new AdminDashboardController(mockMediator.Object, mockUserManager.Object);

                // Act
                IActionResult actionResult = await controller.GetUsers(search, role);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult for any valid inputs.");
                var ok = (OkObjectResult)actionResult;
                Assert.AreSame(expected, ok.Value, "Controller should return the same IEnumerable instance returned by mediator.");
                mockMediator.Verify(m => m.Send(It.IsAny<GetAdminUsersQuery>(), It.IsAny<CancellationToken>()), Times.Once,
                    "Mediator.Send should be invoked exactly once per controller call.");

                // Cleanup: Moq instances go out of scope; proceed to next case
            }
        }

        /// <summary>
        /// Verifies that GetUsers returns OkObjectResult whose Value is null when mediator returns null.
        /// Input conditions: mediator.Send returns null.
        /// Expected: OkObjectResult with a null Value and mediator.Send invoked once.
        /// </summary>
        [TestMethod]
        public async Task GetUsers_MediatorReturnsNull_ReturnsOkWithNullValue()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            mockMediator
                .Setup(m => m.Send(It.IsAny<GetAdminUsersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<AdminUserDto>?)null);

            var mockUserStore = Mock.Of<IUserStore<User>>();
            var mockUserManager = new Mock<UserManager<User>>(mockUserStore, null, null, null, null, null, null, null, null);

            var controller = new AdminDashboardController(mockMediator.Object, mockUserManager.Object);

            // Act
            IActionResult actionResult = await controller.GetUsers(null, null);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult even when mediator returns null.");
            var ok = (OkObjectResult)actionResult;
            Assert.IsNull(ok.Value, "Expected OkObjectResult.Value to be null when mediator returns null.");
            mockMediator.Verify(m => m.Send(It.IsAny<GetAdminUsersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that GetUsers returns the exact collection instance provided by mediator when there is a single user.
        /// Input conditions: mediator.Send returns a single-item list.
        /// Expected: OkObjectResult with the same list instance and mediator.Send invoked once.
        /// </summary>
        [TestMethod]
        public async Task GetUsers_WithSingleUser_ReturnsOkWithThatUser()
        {
            // Arrange
            var singleUserMock = Mock.Of<AdminUserDto>();
            var expected = new List<AdminUserDto> { singleUserMock };

            var mockMediator = new Mock<IMediator>();
            mockMediator
                .Setup(m => m.Send(It.IsAny<GetAdminUsersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var mockUserStore = Mock.Of<IUserStore<User>>();
            var mockUserManager = new Mock<UserManager<User>>(mockUserStore, null, null, null, null, null, null, null, null);

            var controller = new AdminDashboardController(mockMediator.Object, mockUserManager.Object);

            // Act
            IActionResult actionResult = await controller.GetUsers("search-term", "role-name");

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult for valid mediator response.");
            var ok = (OkObjectResult)actionResult;
            var returned = ok.Value as IEnumerable<AdminUserDto>;
            Assert.IsNotNull(returned, "Expected returned value to be an IEnumerable<AdminUserDto>.");
            Assert.AreSame(expected, returned, "Expected controller to return the exact collection instance provided by mediator.");
            mockMediator.Verify(m => m.Send(It.IsAny<GetAdminUsersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Test that GetUser returns BadRequest when academicCode is empty or whitespace.
        /// Conditions: academicCode is either empty string or whitespace-only string (null is not used because the parameter is non-nullable).
        /// Expected: BadRequestObjectResult with message "Academic code is required."
        /// </summary>
        [TestMethod]
        public async Task GetUser_EmptyOrWhitespaceAcademicCode_ReturnsBadRequest()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            var userStore = Mock.Of<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(userStore, null, null, null, null, null, null, null, null);

            var controller = new AdminDashboardController(mediatorMock.Object, userManagerMock.Object);

            string[] inputs = new[] { string.Empty, "   " };

            foreach (var input in inputs)
            {
                // Act
                IActionResult actionResult = await controller.GetUser(input).ConfigureAwait(false);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));
                var badReq = actionResult as BadRequestObjectResult;
                Assert.IsNotNull(badReq);

                // Anonymous object contains a public 'message' property; reflect to assert value.
                var messageProp = badReq.Value?.GetType().GetProperty("message", BindingFlags.Public | BindingFlags.Instance);
                Assert.IsNotNull(messageProp, "Expected anonymous error object to contain a 'message' property.");
                var messageVal = messageProp!.GetValue(badReq.Value) as string;
                Assert.AreEqual("Academic code is required.", messageVal);
            }

            // Verify mediator was never called for invalid input
            mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<AdminUserDto?>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Test that GetUser returns NotFound when mediator returns null (user not found).
        /// Conditions: valid academicCode provided; mediator.Send returns null.
        /// Expected: NotFoundObjectResult with message "User '{academicCode}' not found."
        /// </summary>
        [TestMethod]
        public async Task GetUser_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<IRequest<AdminUserDto?>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AdminUserDto?)null);

            var userStore = Mock.Of<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(userStore, null, null, null, null, null, null, null, null);

            var controller = new AdminDashboardController(mediatorMock.Object, userManagerMock.Object);

            string academicCode = "AC123";

            // Act
            IActionResult actionResult = await controller.GetUser(academicCode).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(NotFoundObjectResult));
            var notFound = actionResult as NotFoundObjectResult;
            Assert.IsNotNull(notFound);

            var messageProp = notFound.Value?.GetType().GetProperty("message", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(messageProp, "Expected anonymous not-found object to contain a 'message' property.");
            var messageVal = messageProp!.GetValue(notFound.Value) as string;
            Assert.AreEqual($"User '{academicCode}' not found.", messageVal);

            mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<AdminUserDto?>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Test that GetUser returns Ok with the AdminUserDto when mediator returns a DTO.
        /// Conditions: valid academicCode provided; mediator.Send returns a non-null AdminUserDto instance.
        /// Expected: OkObjectResult with the same AdminUserDto instance.
        /// </summary>
        [TestMethod]
        public async Task GetUser_UserFound_ReturnsOkWithDto()
        {
            // Arrange
            var expectedDto = new AdminUserDto();

            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<IRequest<AdminUserDto?>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var userStore = Mock.Of<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(userStore, null, null, null, null, null, null, null, null);

            var controller = new AdminDashboardController(mediatorMock.Object, userManagerMock.Object);

            string academicCode = "VALID_CODE";

            // Act
            IActionResult actionResult = await controller.GetUser(academicCode).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var ok = actionResult as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreSame(expectedDto, ok.Value);

            mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<AdminUserDto?>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that empty or whitespace academic codes are rejected with a BadRequest and the expected message.
        /// Input conditions: academicCode = empty string or whitespace-only string, dto is a valid non-null DTO.
        /// Expected result: BadRequestObjectResult with message "Academic code is required."
        /// </summary>
        [TestMethod]
        public async Task UpdateUser_AcademicCodeEmptyOrWhitespace_ReturnsBadRequest()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var storeMock = new Mock<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(storeMock.Object, null, null, null, null, null, null, null, null);
            // Users is not needed because method returns early, but provide an empty async queryable to be safe
            userManagerMock.Setup(u => u.Users).Returns(new TestAsyncEnumerable<User>(Array.Empty<User>()).AsQueryable());

            var controller = new AdminDashboardController(mediatorMock.Object, userManagerMock.Object);
            var dto = new AdminUpdateUserDto();

            var inputs = new[] { string.Empty, "   " };

            foreach (var academicCode in inputs)
            {
                // Act
                IActionResult result = await controller.UpdateUser(academicCode, dto);

                // Assert
                Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult), "Expected BadRequest for invalid academic code.");
                var bad = (BadRequestObjectResult)result;
                dynamic? value = bad.Value;
                Assert.IsNotNull(value);
                Assert.AreEqual("Academic code is required.", (string?)value.message);
            }
        }

        /// <summary>
        /// Verifies that when no user matches the given academic code, NotFound is returned with the proper message.
        /// Input: academicCode that does not exist in the Users queryable.
        /// Expected: NotFoundObjectResult with message containing the academic code.
        /// </summary>
        [TestMethod]
        public async Task UpdateUser_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var storeMock = new Mock<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(storeMock.Object, null, null, null, null, null, null, null, null);

            // Provide a user with a different academic code
            var existing = new User { Id = "1", Academic_Code = "other" };
            userManagerMock.Setup(u => u.Users).Returns(new TestAsyncEnumerable<User>(new[] { existing }).AsQueryable());

            var controller = new AdminDashboardController(mediatorMock.Object, userManagerMock.Object);
            var dto = new AdminUpdateUserDto();

            // Act
            var result = await controller.UpdateUser("notfound", dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFound = (NotFoundObjectResult)result;
            dynamic? value = notFound.Value;
            Assert.IsNotNull(value);
            Assert.AreEqual("User 'notfound' not found.", (string?)value.message);
        }

        /// <summary>
        /// Verifies that out-of-range AdminMaxCredits values cause a BadRequest with the expected message.
        /// Input: AdminMaxCredits = -1 and 31 (outside 0..30)
        /// Expected: BadRequestObjectResult with message "Allowed credits must be between 0 and 30."
        /// </summary>
        [TestMethod]
        public async Task UpdateUser_AdminMaxCreditsOutOfRange_ReturnsBadRequest()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var storeMock = new Mock<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(storeMock.Object, null, null, null, null, null, null, null, null);

            var user = new User { Id = "u1", Academic_Code = "code1", AllowedCredits = 10 };
            userManagerMock.Setup(u => u.Users).Returns(new TestAsyncEnumerable<User>(new[] { user }).AsQueryable());

            var controller = new AdminDashboardController(mediatorMock.Object, userManagerMock.Object);

            var testValues = new[] { -1, 31 };

            foreach (var val in testValues)
            {
                var dto = new AdminUpdateUserDto { AdminMaxCredits = val };

                // Act
                var result = await controller.UpdateUser("code1", dto);

                // Assert
                Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
                var bad = (BadRequestObjectResult)result;
                dynamic? value = bad.Value;
                Assert.IsNotNull(value);
                Assert.AreEqual("Allowed credits must be between 0 and 30.", (string?)value.message);
            }
        }

        /// <summary>
        /// Verifies lockout behavior when Active flag is toggled and that UpdateAsync is invoked.
        /// Input: Active = true (unlock) and Active = false (lock)
        /// Expected: Calls to SetLockoutEnabledAsync and SetLockoutEndDateAsync with correct parameters and final Ok result.
        /// </summary>
        [TestMethod]
        public async Task UpdateUser_ActiveFlag_TogglesLockoutAndReturnsOk()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var storeMock = new Mock<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(storeMock.Object, null, null, null, null, null, null, null, null);

            var user = new User { Id = "u2", Academic_Code = "code2", AllowedCredits = 5 };
            userManagerMock.Setup(u => u.Users).Returns(new TestAsyncEnumerable<User>(new[] { user }).AsQueryable());

            userManagerMock.Setup(u => u.SetLockoutEnabledAsync(It.IsAny<User>(), It.IsAny<bool>()))
                .ReturnsAsync(IdentityResult.Success)
                .Verifiable();

            userManagerMock.Setup(u => u.SetLockoutEndDateAsync(It.IsAny<User>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(IdentityResult.Success)
                .Verifiable();

            userManagerMock.Setup(u => u.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success)
                .Verifiable();

            var controller = new AdminDashboardController(mediatorMock.Object, userManagerMock.Object);

            var cases = new[]
            {
                new { Active = true, ExpectedEnabled = false, ExpectedEnd = (DateTimeOffset?)null },
                new { Active = false, ExpectedEnabled = true, ExpectedEnd = (DateTimeOffset?)DateTimeOffset.MaxValue }
            };

            foreach (var c in cases)
            {
                var dto = new AdminUpdateUserDto { Active = c.Active };

                // Act
                var result = await controller.UpdateUser("code2", dto);

                // Assert
                Assert.IsInstanceOfType(result, typeof(OkObjectResult));
                var ok = (OkObjectResult)result;
                dynamic? value = ok.Value;
                Assert.IsNotNull(value);
                Assert.AreEqual("User updated successfully.", (string?)value.message);

                // Verify lockout calls for the last invocation (calls are cumulative across loop iterations,
                // so verify with AtLeastOnce to remain stable)
                userManagerMock.Verify(u => u.SetLockoutEnabledAsync(It.Is<User>(x => object.ReferenceEquals(x, user)), It.Is<bool>(b => b == c.ExpectedEnabled)), Times.AtLeastOnce());
                userManagerMock.Verify(u => u.SetLockoutEndDateAsync(It.Is<User>(x => object.ReferenceEquals(x, user)), It.Is<DateTimeOffset?>(d => NullableDateTimeOffsetEquals(d, c.ExpectedEnd))), Times.AtLeastOnce());
                userManagerMock.Verify(u => u.UpdateAsync(It.Is<User>(x => object.ReferenceEquals(x, user))), Times.AtLeastOnce());
            }

            static bool NullableDateTimeOffsetEquals(DateTimeOffset? a, DateTimeOffset? b)
            {
                if (a.HasValue != b.HasValue) return false;
                if (!a.HasValue) return true;
                return a.Value.Equals(b.Value);
            }
        }

        /// <summary>
        /// Verifies that when UpdateAsync fails, the controller returns BadRequest with the error descriptions.
        /// Input: UpdateAsync returns IdentityResult.Failed with provided IdentityError descriptions.
        /// Expected: BadRequestObjectResult with anonymous object containing errors collection of descriptions.
        /// </summary>
        [TestMethod]
        public async Task UpdateUser_UpdateAsyncFailure_ReturnsBadRequestWithErrors()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var storeMock = new Mock<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(storeMock.Object, null, null, null, null, null, null, null, null);

            var user = new User { Id = "u3", Academic_Code = "code3", AllowedCredits = 12 };
            userManagerMock.Setup(u => u.Users).Returns(new TestAsyncEnumerable<User>(new[] { user }).AsQueryable());

            var errors = new[] { new IdentityError { Description = "err1" }, new IdentityError { Description = "err2" } };
            userManagerMock.Setup(u => u.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Failed(errors));

            var controller = new AdminDashboardController(mediatorMock.Object, userManagerMock.Object);
            var dto = new AdminUpdateUserDto { AdminMaxCredits = 15 };

            // Act
            var result = await controller.UpdateUser("code3", dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            dynamic? value = bad.Value;
            Assert.IsNotNull(value);

            // errors is IEnumerable<string>
            IEnumerable? errs = value.errors as IEnumerable;
            Assert.IsNotNull(errs);

            var list = errs.Cast<object?>().Select(o => o?.ToString()).ToArray();
            CollectionAssert.AreEquivalent(new[] { "err1", "err2" }, list);
        }

        /// <summary>
        /// Verifies that valid AdminMaxCredits boundary values are accepted, assigned to the user, and the controller returns Ok.
        /// Input: AdminMaxCredits = 0 and AdminMaxCredits = 30 (boundaries)
        /// Expected: User.AllowedCredits updated and Ok result.
        /// </summary>
        [TestMethod]
        public async Task UpdateUser_AdminMaxCreditsBoundaries_SetAllowedCreditsAndReturnOk()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var storeMock = new Mock<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(storeMock.Object, null, null, null, null, null, null, null, null);

            var user = new User { Id = "u4", Academic_Code = "code4", AllowedCredits = 5 };
            userManagerMock.Setup(u => u.Users).Returns(new TestAsyncEnumerable<User>(new[] { user }).AsQueryable());

            userManagerMock.Setup(u => u.SetLockoutEnabledAsync(It.IsAny<User>(), It.IsAny<bool>()))
                .ReturnsAsync(IdentityResult.Success);

            userManagerMock.Setup(u => u.SetLockoutEndDateAsync(It.IsAny<User>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(IdentityResult.Success);

            userManagerMock.Setup(u => u.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new AdminDashboardController(mediatorMock.Object, userManagerMock.Object);

            var testValues = new[] { 0, 30 };

            foreach (var v in testValues)
            {
                var dto = new AdminUpdateUserDto { AdminMaxCredits = v };

                // Act
                var result = await controller.UpdateUser("code4", dto);

                // Assert
                Assert.IsInstanceOfType(result, typeof(OkObjectResult));
                var ok = (OkObjectResult)result;
                dynamic? value = ok.Value;
                Assert.IsNotNull(value);
                Assert.AreEqual("User updated successfully.", (string?)value.message);

                // Verify AllowedCredits changed on the user instance provided by Users IQueryable
                Assert.AreEqual(v, user.AllowedCredits);
            }
        }

        #region EF Core async queryable helpers (inner classes)

        // Minimal async query provider and enumerable to satisfy EF Core's FirstOrDefaultAsync usage in the controller.
        private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;
            public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

            public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);

            public object? Execute(Expression expression) => _inner.Execute(expression);

            public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

            public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            {
                return new TestAsyncEnumerable<TResult>(expression);
            }

            public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                var result = Execute<TResult>(expression);
                return Task.FromResult(result);
            }
        }

        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
            public TestAsyncEnumerable(Expression expression) : base(expression) { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;
            public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
            public T Current => _inner.Current;
            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return new ValueTask(Task.CompletedTask);
            }

            public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(Task.FromResult(_inner.MoveNext()));
        }

        #endregion

        /// <summary>
        /// Test purpose:
        /// Validate that GetRegistrationStatus returns an OkObjectResult containing the exact RegistrationStatusDto
        /// provided by the mediator.
        /// Input conditions:
        /// - IMediator.Send returns a non-null RegistrationStatusDto instance.
        /// Expected result:
        /// - The controller returns OkObjectResult and the Value is the same instance returned by IMediator.
        /// </summary>
        [TestMethod]
        public async Task GetRegistrationStatus_ReturnsOkWithDto_WhenMediatorReturnsDto()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var expected = new RegistrationStatusDto
            {
                IsOpen = true,
                Semester = "Fall",
                AcademicYear = "2025-2026",
                StartDate = DateTime.UtcNow,
                Deadline = DateTime.UtcNow.AddDays(7),
                OpenYears = new List<int> { 1, 2 },
                EnabledCourses = new List<string> { "C1" },
                DaysLeft = 7
            };

            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetRegistrationStatusQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var userStoreMock = new Mock<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            var controller = new AdminDashboardController(mediatorMock.Object, userManagerMock.Object);

            // Act
            var result = await controller.GetRegistrationStatus().ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreSame(expected, ok.Value);
        }

        /// <summary>
        /// Test purpose:
        /// Ensure that exceptions thrown by the mediator propagate from GetRegistrationStatus.
        /// Input conditions:
        /// - IMediator.Send throws InvalidOperationException.
        /// Expected result:
        /// - The controller action throws the same InvalidOperationException (no internal catch).
        /// </summary>
        [TestMethod]
        public async Task GetRegistrationStatus_PropagatesException_WhenMediatorThrows()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetRegistrationStatusQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Mediator failure"));

            var userStoreMock = new Mock<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            var controller = new AdminDashboardController(mediatorMock.Object, userManagerMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await controller.GetRegistrationStatus().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Test that DeleteUser returns BadRequest when academicCode is empty or whitespace.
        /// Input conditions: academicCode is either empty string or whitespace-only string.
        /// Expected result: BadRequestObjectResult with message "Academic code is required.".
        /// </summary>
        [TestMethod]
        public async Task DeleteUser_InvalidAcademicCode_ReturnsBadRequest()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var userStoreMock = Mock.Of<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(userStoreMock, null, null, null, null, null, null, null, null);
            var controller = new AdminDashboardController(mediatorMock.Object, userManagerMock.Object);

            var invalidCodes = new[] { string.Empty, "   " };

            foreach (var code in invalidCodes)
            {
                // Act
                IActionResult actionResult = await controller.DeleteUser(code);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));
                var bad = (BadRequestObjectResult)actionResult;
                Assert.IsNotNull(bad.Value);

                var messageProp = bad.Value.GetType().GetProperty("message");
                Assert.IsNotNull(messageProp, "Response object must contain 'message' property.");
                var message = messageProp.GetValue(bad.Value) as string;
                Assert.AreEqual("Academic code is required.", message);
            }
        }

        /// <summary>
        /// Test that DeleteUser returns NotFound when no user matches the provided academic code.
        /// Input conditions: academicCode is a non-empty string not present in Users.
        /// Expected result: NotFoundObjectResult with message "User '{academicCode}' not found.".
        /// </summary>
        [TestMethod]
        public async Task DeleteUser_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var userStoreMock = Mock.Of<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(userStoreMock, null, null, null, null, null, null, null, null);

            // No users in store
            var emptyUsers = new TestAsyncEnumerable<User>(new List<User>());
            userManagerMock.SetupGet(m => m.Users).Returns(emptyUsers);

            var controller = new AdminDashboardController(mediatorMock.Object, userManagerMock.Object);

            string academicCode = "AC123";

            // Act
            IActionResult actionResult = await controller.DeleteUser(academicCode);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(NotFoundObjectResult));
            var notFound = (NotFoundObjectResult)actionResult;
            Assert.IsNotNull(notFound.Value);

            var messageProp = notFound.Value.GetType().GetProperty("message");
            Assert.IsNotNull(messageProp, "Response object must contain 'message' property.");
            var message = messageProp.GetValue(notFound.Value) as string;
            Assert.AreEqual($"User '{academicCode}' not found.", message);
        }

        /// <summary>
        /// Test that DeleteUser returns BadRequest when deletion fails and includes error descriptions.
        /// Input conditions: a matching user exists and UserManager.DeleteAsync returns a failed IdentityResult with errors.
        /// Expected result: BadRequestObjectResult with an 'errors' property containing the error descriptions.
        /// </summary>
        [TestMethod]
        public async Task DeleteUser_DeleteFails_ReturnsBadRequestWithErrors()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var userStoreMock = Mock.Of<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(userStoreMock, null, null, null, null, null, null, null, null);

            var user = new User { Academic_Code = "AC123" };
            var users = new TestAsyncEnumerable<User>(new[] { user });

            userManagerMock.SetupGet(m => m.Users).Returns(users);
            userManagerMock.Setup(m => m.DeleteAsync(It.Is<User>(u => u.Academic_Code == user.Academic_Code)))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "err1" }, new IdentityError { Description = "err2" }));

            var controller = new AdminDashboardController(mediatorMock.Object, userManagerMock.Object);

            // Act
            IActionResult actionResult = await controller.DeleteUser(user.Academic_Code);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)actionResult;
            Assert.IsNotNull(bad.Value);

            var errorsProp = bad.Value.GetType().GetProperty("errors");
            Assert.IsNotNull(errorsProp, "Response object must contain 'errors' property.");

            var errorsValue = errorsProp.GetValue(bad.Value) as IEnumerable<object>;
            Assert.IsNotNull(errorsValue, "errors property should be enumerable.");

            var errorsList = errorsValue.Cast<string>().ToList();
            CollectionAssert.AreEqual(new List<string> { "err1", "err2" }, errorsList);
        }

        /// <summary>
        /// Test that DeleteUser returns Ok when deletion succeeds.
        /// Input conditions: a matching user exists and UserManager.DeleteAsync returns IdentityResult.Success.
        /// Expected result: OkObjectResult with message "User '{academicCode}' deleted.".
        /// </summary>
        [TestMethod]
        public async Task DeleteUser_DeleteSucceeds_ReturnsOkMessage()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var userStoreMock = Mock.Of<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(userStoreMock, null, null, null, null, null, null, null, null);

            var user = new User { Academic_Code = "AC123" };
            var users = new TestAsyncEnumerable<User>(new[] { user });

            userManagerMock.SetupGet(m => m.Users).Returns(users);
            userManagerMock.Setup(m => m.DeleteAsync(It.Is<User>(u => u.Academic_Code == user.Academic_Code)))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new AdminDashboardController(mediatorMock.Object, userManagerMock.Object);

            // Act
            IActionResult actionResult = await controller.DeleteUser(user.Academic_Code);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var ok = (OkObjectResult)actionResult;
            Assert.IsNotNull(ok.Value);

            var messageProp = ok.Value.GetType().GetProperty("message");
            Assert.IsNotNull(messageProp, "Response object must contain 'message' property.");
            var message = messageProp.GetValue(ok.Value) as string;
            Assert.AreEqual($"User '{user.Academic_Code}' deleted.", message);
        }

        // Helper classes to allow async EF Core style queries against in-memory collections
        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IQueryable<T>, IAsyncEnumerable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
            public TestAsyncEnumerable(Expression expression) : base(expression) { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        private class TestAsyncQueryProvider<TEntity> : IQueryProvider
        {
            private readonly IQueryProvider _inner;

            public TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                var elementType = expression.Type.GetGenericArguments().FirstOrDefault() ?? typeof(object);
                var enumerableType = typeof(TestAsyncEnumerable<>).MakeGenericType(elementType);
                return (IQueryable)Activator.CreateInstance(enumerableType, expression)!;
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new TestAsyncEnumerable<TElement>(expression);
            }

            public object? Execute(Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }
        }

        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public T Current => _inner.Current;

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return ValueTask.CompletedTask;
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(_inner.MoveNext());
            }
        }

        /// <summary>
        /// Ensures that constructing AdminDashboardController with non-null IMediator and UserManager&lt;User&gt;
        /// succeeds without throwing and returns an instance of ControllerBase.
        /// Uses a mocked IMediator and a mocked UserManager using Moq with required constructor args.
        /// Expected: No exception and returned instance is not null and is ControllerBase.
        /// </summary>
        [TestMethod]
        public void AdminDashboardController_Constructor_ValidDependencies_ConstructsController()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            // Create a mocked IUserStore required by UserManager constructor
            var userStoreMock = new Mock<IUserStore<User>>(MockBehavior.Strict);

            // Required dependencies for UserManager
            var options = Options.Create(new IdentityOptions());
            var passwordHasherMock = new Mock<IPasswordHasher<User>>(MockBehavior.Strict);
            var userValidators = new List<IUserValidator<User>> { new Mock<IUserValidator<User>>().Object };
            var passwordValidators = new List<IPasswordValidator<User>> { new Mock<IPasswordValidator<User>>().Object };
            var keyNormalizerMock = new Mock<ILookupNormalizer>(MockBehavior.Strict);
            var errors = new IdentityErrorDescriber();
            var services = Mock.Of<IServiceProvider>();
            var logger = new Mock<ILogger<UserManager<User>>>(MockBehavior.Strict);

            var userManager = new UserManager<User>(
                userStoreMock.Object,
                options,
                passwordHasherMock.Object,
                userValidators,
                passwordValidators,
                keyNormalizerMock.Object,
                errors,
                services,
                logger.Object
            );

            // Act
            var controller = new AdminDashboardController(mediatorMock.Object, userManager);

            // Assert
            Assert.IsNotNull(controller, "Controller instance should not be null when valid dependencies are provided.");
            Assert.IsInstanceOfType(controller, typeof(ControllerBase), "Controller should inherit from ControllerBase.");
        }

        /// <summary>
        /// Verifies that AdminDashboardController accepts different mock behaviors for IMediator and can be constructed.
        /// This test tries both Loose and Strict behaviors for the IMediator mock to ensure constructor does not interact with mediator.
        /// Expected: Construction succeeds in both cases without invoking IMediator.
        /// </summary>
        [TestMethod]
        public void AdminDashboardController_Constructor_VariousMediatorMockBehaviors_NoInteractionDuringConstruction()
        {
            // Arrange
            var behaviors = new[] { MockBehavior.Loose, MockBehavior.Strict };

            // Prepare a simple UserManager instance similar to previous test
            var userStoreMock = new Mock<IUserStore<User>>(MockBehavior.Loose);
            var options = Options.Create(new IdentityOptions());
            var passwordHasherMock = new Mock<IPasswordHasher<User>>(MockBehavior.Loose);
            var userValidators = new List<IUserValidator<User>> { new Mock<IUserValidator<User>>().Object };
            var passwordValidators = new List<IPasswordValidator<User>> { new Mock<IPasswordValidator<User>>().Object };
            var keyNormalizerMock = new Mock<ILookupNormalizer>(MockBehavior.Loose);
            var errors = new IdentityErrorDescriber();
            var services = Mock.Of<IServiceProvider>();
            var logger = new Mock<ILogger<UserManager<User>>>(MockBehavior.Loose);

            var userManager = new UserManager<User>(
                userStoreMock.Object,
                options,
                passwordHasherMock.Object,
                userValidators,
                passwordValidators,
                keyNormalizerMock.Object,
                errors,
                services,
                logger.Object
            );

            foreach (var behavior in behaviors)
            {
                var mediatorMock = new Mock<IMediator>(behavior);

                // Act & Assert - should not throw
                var controller = new AdminDashboardController(mediatorMock.Object, userManager);
                Assert.IsNotNull(controller, $"Controller construction failed with mediator MockBehavior.{behavior}.");

                // Ensure no calls were made to mediator during construction for Strict behavior
                if (behavior == MockBehavior.Strict)
                {
                    mediatorMock.VerifyNoOtherCalls();
                }
            }
        }

        /// <summary>
        /// Partial / guidance test:
        /// The constructor parameters are non-nullable. Per test generation rules, do not pass null to non-nullable parameters.
        /// If you need to validate behavior when DI supplies null (for defensive coding), manually add a test that expects
        /// an ArgumentNullException or NullReferenceException depending on desired behavior.
        /// This placeholder documents that explicit null tests are intentionally omitted because parameters are annotated non-nullable.
        /// </summary>
        [TestMethod]
        public void AdminDashboardController_Constructor_NullParameters_OmittedByDesign()
        {
            // Arrange & Act & Assert
            Assert.Inconclusive("Constructor parameters are non-nullable. To test null handling, modify the production constructor to perform null checks, then implement tests expecting ArgumentNullException.");
        }
    }
}