using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Registrations;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Registrations;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Presentation;
using Presentation.Controllers;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.RegistrationDtos;

namespace Presentation.Controllers.UnitTests
{
    [TestClass]
    public class RegistrationControllerTests
    {
        /// <summary>
        /// Test that when the current user has no NameIdentifier claim the controller returns Unauthorized
        /// and does not call the mediator.
        /// Condition: HttpContext.User has no ClaimTypes.NameIdentifier claim.
        /// Expected: UnauthorizedResult and mediator.Send is not invoked.
        /// </summary>
        [TestMethod]
        public async Task GetRegisteredCourses_UserMissingClaim_ReturnsUnauthorized()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            var controller = new RegistrationController(mockMediator.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                // No user or claims provided -> User.FindFirstValue will be null
            };

            // Act
            IActionResult? actionResult = await controller.GetRegisteredCourses();

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(UnauthorizedResult));
            mockMediator.Verify(m => m.Send(It.IsAny<GetRegisteredCoursesQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Test that when the current user has an empty NameIdentifier claim the controller returns Unauthorized
        /// and does not call the mediator.
        /// Condition: HttpContext.User contains ClaimTypes.NameIdentifier with empty string.
        /// Expected: UnauthorizedResult and mediator.Send is not invoked.
        /// </summary>
        [TestMethod]
        public async Task GetRegisteredCourses_UserIdEmptyClaim_ReturnsUnauthorized()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>();
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, string.Empty) };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            var controller = new RegistrationController(mockMediator.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            IActionResult? actionResult = await controller.GetRegisteredCourses();

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(UnauthorizedResult));
            mockMediator.Verify(m => m.Send(It.IsAny<GetRegisteredCoursesQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Test that when a valid non-empty user id is present the controller sends a GetRegisteredCoursesQuery
        /// with the same student id and returns Ok with the mediator result.
        /// Conditions tested: typical id and a very long id to exercise boundary for strings.
        /// Expected: OkObjectResult whose Value is the same list returned from mediator, and mediator.Send is invoked once per call.
        /// </summary>
        [TestMethod]
        public async Task GetRegisteredCourses_ValidUserId_ReturnsOkWithResultAndSendsQuery()
        {
            // Arrange
            string[] userIds = new[]
            {
                "user-123",
                new string('x', 5000) // very long user id
            };

            foreach (string userId in userIds)
            {
                var expectedList = new List<RegistrationCourseDto> { new RegistrationCourseDto() };

                var mockMediator = new Mock<IMediator>();
                // Setup mediator to return expected list when query's StudentId matches the provided userId
                mockMediator
                    .Setup(m => m.Send(It.Is<GetRegisteredCoursesQuery>(q => q.StudentId == userId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedList);

                var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
                var identity = new ClaimsIdentity(claims, "Test");
                var principal = new ClaimsPrincipal(identity);

                var controller = new RegistrationController(mockMediator.Object);
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = principal }
                };

                // Act
                IActionResult? actionResult = await controller.GetRegisteredCourses();

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
                var okResult = actionResult as OkObjectResult;
                Assert.IsNotNull(okResult);
                // Value should be the exact object returned by the mediator
                Assert.AreSame(expectedList, okResult!.Value);

                mockMediator.Verify(m => m.Send(It.Is<GetRegisteredCoursesQuery>(q => q.StudentId == userId), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        /// <summary>
        /// Verifies that when there is no authenticated user (no NameIdentifier claim or an empty claim value),
        /// GetRegisteredYearCourses returns Unauthorized and does not call the mediator.
        /// Conditions:
        ///  - Case A: HttpContext.User has no NameIdentifier claim.
        ///  - Case B: HttpContext.User has a NameIdentifier claim with an empty string.
        /// Expected:
        ///  - Method returns UnauthorizedResult in both cases and mediator.Send is never invoked.
        /// </summary>
        [TestMethod]
        public async Task GetRegisteredYearCourses_NoUser_ReturnsUnauthorized()
        {
            // Arrange - Case A: no claim
            var mediatorMockA = new Mock<IMediator>();
            var controllerA = new RegistrationController(mediatorMockA.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext() // no user claims set
                }
            };

            // Act - Case A
            var actionResultA = await controllerA.GetRegisteredYearCourses(1);

            // Assert - Case A
            Assert.IsInstanceOfType(actionResultA, typeof(UnauthorizedResult));
            mediatorMockA.Verify(m => m.Send(It.IsAny<GetRegisteredYearCoursesQuery>(), It.IsAny<CancellationToken>()), Times.Never);

            // Arrange - Case B: empty NameIdentifier claim
            var mediatorMockB = new Mock<IMediator>();
            var emptyClaimPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, string.Empty) }));
            var controllerB = new RegistrationController(mediatorMockB.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext() { User = emptyClaimPrincipal }
                }
            };

            // Act - Case B
            var actionResultB = await controllerB.GetRegisteredYearCourses(2);

            // Assert - Case B
            Assert.IsInstanceOfType(actionResultB, typeof(UnauthorizedResult));
            mediatorMockB.Verify(m => m.Send(It.IsAny<GetRegisteredYearCoursesQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Verifies that when an authenticated user exists, GetRegisteredYearCourses:
        ///  - calls IMediator.Send with a GetRegisteredYearCoursesQuery containing the same student id and studyYearId,
        ///  - and returns OkObjectResult wrapping the mediator result.
        /// Conditions:
        ///  - userId is present as a NameIdentifier claim ("test-user").
        ///  - studyYearId is exercised with several boundary and typical values:
        ///      int.MinValue, -1, 0, 1, int.MaxValue
        /// Expected:
        ///  - For each studyYearId, the method returns OkObjectResult whose Value is the exact list returned by mediator.Send,
        ///    and the mediator is invoked exactly once with matching StudentId and Year.
        /// </summary>
        [TestMethod]
        public async Task GetRegisteredYearCourses_UserPresent_ReturnsOkWithMediatorResult_ForVariousStudyYearIds()
        {
            // Test values to exercise boundaries and typical integers
            int[] studyYearIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };
            const string userId = "test-user";

            foreach (int studyYearId in studyYearIds)
            {
                // Arrange
                var expectedList = new List<RegistrationCourseDto> { new RegistrationCourseDto() };
                var mediatorMock = new Mock<IMediator>();
                mediatorMock
                    .Setup(m => m.Send(It.Is<GetRegisteredYearCoursesQuery>(q => q.StudentId == userId && q.Year == studyYearId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedList);

                var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));
                var controller = new RegistrationController(mediatorMock.Object)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext() { User = userPrincipal }
                    }
                };

                // Act
                var actionResult = await controller.GetRegisteredYearCourses(studyYearId);

                // Assert - result is OkObjectResult and wraps the same reference returned by mediator
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
                var okResult = actionResult as OkObjectResult;
                Assert.IsNotNull(okResult);
                Assert.AreSame(expectedList, okResult!.Value);

                // Verify mediator was called exactly once with expected query
                mediatorMock.Verify(m => m.Send(It.Is<GetRegisteredYearCoursesQuery>(q => q.StudentId == userId && q.Year == studyYearId), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        /// <summary>
        /// Verifies that for a variety of integer id boundary values the controller:
        /// - Sends an UpdateRegistrationCommand to IMediator exactly once.
        /// - Returns a NoContentResult (HTTP 204).
        /// Inputs tested: int.MinValue, -1, 0, 1, int.MaxValue.
        /// Expected: no exception, mediator.Send called once per invocation, and NoContentResult returned.
        /// </summary>
        [TestMethod]
        public async Task UpdateRegistration_MultipleIdBoundaries_CallsMediatorAndReturnsNoContent()
        {
            // Arrange
            var testIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (var id in testIds)
            {
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

                // We assume UpdateRegistrationCommand uses MediatR.Unit as response (common pattern for updates).
                mediatorMock
                    .Setup(m => m.Send(It.IsAny<UpdateRegistrationCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Unit.Value)
                    .Verifiable();

                var controller = new RegistrationController(mediatorMock.Object);

                // Create a non-null UpdateRegistrationDto instance. Use Activator to avoid assuming constructor details.
                var updateDto = Activator.CreateInstance<UpdateRegistrationDto>() ?? throw new InvalidOperationException("Unable to create UpdateRegistrationDto");

                // Act
                var result = await controller.UpdateRegistration(id, updateDto);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NoContentResult), $"Expected NoContentResult for id={id}");

                mediatorMock.Verify(m => m.Send(It.IsAny<UpdateRegistrationCommand>(), It.IsAny<CancellationToken>()), Times.Once,
                    $"Mediator.Send should be called once for id={id}");
            }
        }

        /// <summary>
        /// Ensures that if IMediator.Send throws an exception, it propagates out of the controller method.
        /// Input: mediator configured to throw InvalidOperationException when sending the UpdateRegistrationCommand.
        /// Expected: InvalidOperationException is thrown by UpdateRegistration.
        /// </summary>
        [TestMethod]
        public async Task UpdateRegistration_WhenMediatorThrows_ExceptionIsPropagated()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            mediatorMock
                .Setup(m => m.Send(It.IsAny<UpdateRegistrationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("mediator failure"))
                .Verifiable();

            var controller = new RegistrationController(mediatorMock.Object);

            var updateDto = Activator.CreateInstance<UpdateRegistrationDto>() ?? throw new InvalidOperationException("Unable to create UpdateRegistrationDto");

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await controller.UpdateRegistration(42, updateDto);
            });

            mediatorMock.Verify(m => m.Send(It.IsAny<UpdateRegistrationCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that DeleteRegistration sends a DeleteRegistrationCommand with the provided id
        /// and returns NoContentResult for a variety of integer id edge values.
        /// Inputs tested: int.MinValue, -1, 0, 1, int.MaxValue.
        /// Expected: mediator.Send called once with a DeleteRegistrationCommand whose RegistrationId equals the input id,
        /// and the controller returns a NoContentResult (204).
        /// </summary>
        [TestMethod]
        public async Task DeleteRegistration_ValidIds_ReturnsNoContentAndSendsCommand()
        {
            // Arrange
            int[] testIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int id in testIds)
            {
                var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
                mockMediator
                    .Setup(m => m.Send(It.Is<DeleteRegistrationCommand>(c => c.RegistrationId == id), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Unit.Value)
                    .Verifiable();

                var controller = new RegistrationController(mockMediator.Object);

                // Act
                IActionResult result = await controller.DeleteRegistration(id).ConfigureAwait(false);

                // Assert
                Assert.IsInstanceOfType(result, typeof(NoContentResult));
                mockMediator.Verify(m => m.Send(It.Is<DeleteRegistrationCommand>(c => c.RegistrationId == id), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        /// <summary>
        /// Tests that when the mediator throws an exception during Send, the controller does not swallow it
        /// and the exception is propagated to the caller.
        /// Input: id = 42 (representative).
        /// Expected: InvalidOperationException is thrown from DeleteRegistration.
        /// </summary>
        [TestMethod]
        public async Task DeleteRegistration_WhenMediatorThrows_PropagatesException()
        {
            // Arrange
            int id = 42;
            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteRegistrationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("mediator failure"));

            var controller = new RegistrationController(mockMediator.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await controller.DeleteRegistration(id).ConfigureAwait(false));
            mockMediator.Verify(m => m.Send(It.IsAny<DeleteRegistrationCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that when the current user does NOT have a NameIdentifier claim,
        /// RegisterForCourse returns UnauthorizedResult.
        /// Input conditions:
        /// - A CreateRegistrationDto instance is provided.
        /// - The controller's User does not contain ClaimTypes.NameIdentifier.
        /// Expected result:
        /// - The action returns UnauthorizedResult and mediator.Send is not invoked.
        /// </summary>
        [TestMethod]
        public async Task RegisterForCourse_UserMissingNameIdentifier_ReturnsUnauthorized()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            var controller = new RegistrationController(mediatorMock.Object);

            // Set an empty ClaimsPrincipal (no NameIdentifier claim)
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity()) // no claims
                }
            };

            var registrationDto = new CreateRegistrationDto();

            // Act
            IActionResult actionResult = await controller.RegisterForCourse(registrationDto);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(UnauthorizedResult));
            // Ensure mediator was never called
            mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<int>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Verifies that when the current user has a valid NameIdentifier claim,
        /// RegisterForCourse calls mediator.Send with a CreateRegistrationCommand containing the provided DTO and userId,
        /// and returns OkObjectResult wrapping the integer result returned by mediator.
        /// Input conditions:
        /// - A CreateRegistrationDto instance is provided.
        /// - The controller's User contains ClaimTypes.NameIdentifier.
        /// - The mediator returns various integer results (boundary and typical values).
        /// Expected result:
        /// - The action returns OkObjectResult whose Value equals the mediator returned int,
        ///   and mediator.Send is invoked exactly once per request with a command that has matching UserId and RegistrationDto reference.
        /// </summary>
        [TestMethod]
        public async Task RegisterForCourse_ValidUser_ReturnsOkWithMediatorResult_ForVariousIntResults()
        {
            // Arrange
            var testValues = new[] { int.MinValue, -1, 0, 1, int.MaxValue };
            var registrationDto = new CreateRegistrationDto();

            foreach (int expectedResult in testValues)
            {
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

                string expectedUserId = Guid.NewGuid().ToString();

                // Setup mediator to validate the incoming command and return the expected result
                mediatorMock
                    .Setup(m => m.Send(It.Is<CreateRegistrationCommand>(cmd =>
                            object.ReferenceEquals(cmd.RegistrationDto, registrationDto) &&
                            cmd.UserId == expectedUserId),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedResult)
                    .Verifiable();

                var controller = new RegistrationController(mediatorMock.Object);

                // Provide a ClaimsPrincipal containing the NameIdentifier claim
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, expectedUserId)
                        }))
                    }
                };

                // Act
                IActionResult actionResult = await controller.RegisterForCourse(registrationDto);

                // Assert
                Assert.IsNotNull(actionResult);
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
                var okResult = (OkObjectResult)actionResult;
                Assert.AreEqual(expectedResult, okResult.Value);

                mediatorMock.Verify(m => m.Send(It.IsAny<CreateRegistrationCommand>(), It.IsAny<CancellationToken>()), Times.Once);

                // Cleanup verify for this iteration
                mediatorMock.Verify();
            }
        }

        /// <summary>
        /// Verifies that the RegistrationController constructor succeeds when provided with a valid IMediator implementation.
        /// Arrange: a mocked IMediator is created.
        /// Act: the constructor is invoked with the mocked mediator.
        /// Assert: no exception is thrown and the constructed controller instance is not null.
        /// </summary>
        [TestMethod]
        public void RegistrationController_WithValidMediator_DoesNotThrow()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();

            // Act
            RegistrationController? controller = null;
            Exception? exception = null;
            try
            {
                controller = new RegistrationController(mediatorMock.Object);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception, $"Constructor threw an unexpected exception: {exception}");
            Assert.IsNotNull(controller, "Constructor returned null RegistrationController instance.");
        }

        /// <summary>
        /// Verifies that the RegistrationController constructor accepts a null mediator without throwing.
        /// Arrange: a null IMediator reference is prepared.
        /// Act: the constructor is invoked with null.
        /// Assert: no exception is thrown and the constructed controller instance is not null.
        /// Note: The mediator parameter's nullability in source is not explicitly annotated. This test ensures
        /// constructor behavior when null is provided.
        /// </summary>
        [TestMethod]
        public void RegistrationController_WithNullMediator_DoesNotThrow()
        {
            // Arrange
            IMediator? nullMediator = null;

            // Act
            RegistrationController? controller = null;
            Exception? exception = null;
            try
            {
                // Use null-forgiving to match constructor signature while keeping local variable nullable.
                controller = new RegistrationController(nullMediator!);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception, $"Constructor threw an unexpected exception when mediator is null: {exception}");
            Assert.IsNotNull(controller, "Constructor should return a RegistrationController instance even when mediator is null.");
        }

        /// <summary>
        /// Test that when the user identifier (NameIdentifier) is missing or empty the controller returns Unauthorized
        /// and mediator.Send is not invoked.
        /// Conditions:
        /// - Case A: ClaimsPrincipal has no NameIdentifier claim (FindFirstValue returns null).
        /// - Case B: ClaimsPrincipal has NameIdentifier claim with empty string.
        /// Expected:
        /// - The action returns UnauthorizedResult.
        /// - IMediator.Send is never called.
        /// </summary>
        [TestMethod]
        public async Task GetRegisteredSemesterCourses_NoUserIdOrEmpty_ReturnsUnauthorized()
        {
            // Arrange - two scenarios: no claim, empty claim
            var scenarios = new List<ClaimsPrincipal?>
            {
                // No claim present -> FindFirstValue returns null
                new ClaimsPrincipal(new ClaimsIdentity()),
                // Claim present but empty -> FindFirstValue returns empty string
                new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, string.Empty) }))
            };

            foreach (var principal in scenarios)
            {
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

                var controller = new RegistrationController(mediatorMock.Object)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext { User = principal ?? new ClaimsPrincipal() }
                    }
                };

                // Act
                var actionResult = await controller.GetRegisteredSemesterCourses(1, 1);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(UnauthorizedResult), "Expected UnauthorizedResult when user id is null or empty.");
                mediatorMock.Verify(m => m.Send(It.IsAny<GetRegisteredSemesterCoursesQuery>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        /// <summary>
        /// Test that when a valid user id is present the controller calls mediator.Send with a GetRegisteredSemesterCoursesQuery
        /// containing the provided studyYearId, semesterId and userId, and returns Ok with the mediator result.
        /// Conditions tested for studyYearId and semesterId include boundary and typical values:
        /// - int.MinValue, -1, 0, 1, int.MaxValue
        /// Expected:
        /// - IMediator.Send is invoked exactly once per call with a query matching the parameters.
        /// - The action returns OkObjectResult and the Value equals the list returned by the mediator.
        /// </summary>
        [TestMethod]
        public async Task GetRegisteredSemesterCourses_ValidUser_CallsMediatorAndReturnsOk_ForVariousIds()
        {
            // Arrange
            var testInts = new[] { int.MinValue, -1, 0, 1, int.MaxValue };
            const string userId = "valid-user-123";
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));

            foreach (var studyYearId in testInts)
            {
                foreach (var semesterId in testInts)
                {
                    // Prepare expected result
                    var expected = new List<RegistrationCourseDto> { new RegistrationCourseDto() };

                    var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
                    mediatorMock
                        .Setup(m => m.Send(It.Is<GetRegisteredSemesterCoursesQuery>(q =>
                            q.StudyYearId == studyYearId &&
                            q.SemesterId == semesterId &&
                            q.StudentId == userId
                        ), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(expected)
                        .Verifiable();

                    var controller = new RegistrationController(mediatorMock.Object)
                    {
                        ControllerContext = new ControllerContext
                        {
                            HttpContext = new DefaultHttpContext { User = principal }
                        }
                    };

                    // Act
                    var actionResult = await controller.GetRegisteredSemesterCourses(studyYearId, semesterId);

                    // Assert
                    Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult for valid user id.");
                    var ok = actionResult as OkObjectResult;
                    Assert.IsNotNull(ok);
                    Assert.AreSame(expected, ok!.Value, "Returned value should be the same instance as mediator result.");
                    mediatorMock.Verify(m => m.Send(It.IsAny<GetRegisteredSemesterCoursesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
                    mediatorMock.Verify();
                }
            }
        }

        /// <summary>
        /// Test that a whitespace-only NameIdentifier is treated as a non-empty value by the controller:
        /// Conditions:
        /// - ClaimsPrincipal has NameIdentifier claim with whitespace-only string.
        /// Expected:
        /// - The controller does NOT return Unauthorized and calls mediator.Send with the whitespace value.
        /// - Returns Ok with mediator result.
        /// Note: This documents current behavior (whitespace is not considered empty by string.IsNullOrEmpty).
        /// </summary>
        [TestMethod]
        public async Task GetRegisteredSemesterCourses_WhitespaceUserId_IsAcceptedAndPassedToMediator()
        {
            // Arrange
            string whitespaceUser = "   ";
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, whitespaceUser) }));

            int studyYearId = 2022;
            int semesterId = 2;

            var expected = new List<RegistrationCourseDto> { new RegistrationCourseDto() };

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.Is<GetRegisteredSemesterCoursesQuery>(q =>
                    q.StudyYearId == studyYearId &&
                    q.SemesterId == semesterId &&
                    q.StudentId == whitespaceUser
                ), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new RegistrationController(mediatorMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = principal }
                }
            };

            // Act
            var actionResult = await controller.GetRegisteredSemesterCourses(studyYearId, semesterId);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var ok = actionResult as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreSame(expected, ok!.Value);
            mediatorMock.Verify(m => m.Send(It.IsAny<GetRegisteredSemesterCoursesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            mediatorMock.Verify();
        }
    }
}