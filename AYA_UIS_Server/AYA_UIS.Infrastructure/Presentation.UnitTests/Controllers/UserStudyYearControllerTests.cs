using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.UserStudyYears;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.UserStudyYears;
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
using Shared.Dtos.Info_Module.UserStudyYearDtos;
using Shared.Respones;

namespace Presentation.Controllers.UnitTests
{
    [TestClass]
    public class UserStudyYearControllerTests
    {
        /// <summary>
        /// Verifies that when the authenticated user does not contain a NameIdentifier claim,
        /// GetMyTimeline returns Unauthorized and the mediator is not invoked.
        /// Input: ClaimsPrincipal with no NameIdentifier claim.
        /// Expected: UnauthorizedResult and no mediator Send calls.
        /// </summary>
        [TestMethod]
        public async Task GetMyTimeline_NoNameIdentifierClaim_ReturnsUnauthorized()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            var controller = new UserStudyYearController(mockMediator.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity()) // no claims
                    }
                }
            };

            // Act
            IActionResult actionResult = await controller.GetMyTimeline();

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(UnauthorizedResult));
            mockMediator.Verify(m => m.Send(It.IsAny<IRequest<Response<UserStudyYearTimelineDto>>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Verifies that when the NameIdentifier claim exists but is an empty string,
        /// GetMyTimeline returns Unauthorized.
        /// Input: ClaimsPrincipal with NameIdentifier claim value == "".
        /// Expected: UnauthorizedResult and mediator not invoked.
        /// </summary>
        [TestMethod]
        public async Task GetMyTimeline_EmptyNameIdentifierClaim_ReturnsUnauthorized()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, string.Empty) });
            var controller = new UserStudyYearController(mockMediator.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
                }
            };

            // Act
            IActionResult actionResult = await controller.GetMyTimeline();

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(UnauthorizedResult));
            mockMediator.Verify(m => m.Send(It.IsAny<IRequest<Response<UserStudyYearTimelineDto>>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Verifies that when a valid user id is present and mediator returns a successful response,
        /// GetMyTimeline returns OkObjectResult containing the mediator response.
        /// Input: ClaimsPrincipal with NameIdentifier claim value 'user123'; mediator returns Success=true.
        /// Expected: OkObjectResult with the same Response instance and mediator invoked once with matching UserId.
        /// </summary>
        [TestMethod]
        public async Task GetMyTimeline_ValidUserId_MediatorReturnsSuccess_ReturnsOkWithResult()
        {
            // Arrange
            var userId = "user123";
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) });
            var response = new Response<UserStudyYearTimelineDto>
            {
                Success = true,
                Data = null,
                Errors = null,
                Message = "ok"
            };

            var mockMediator = new Mock<IMediator>();
            GetUserStudyYearTimelineQuery? capturedQuery = null;

            mockMediator
                .Setup(m => m.Send(It.IsAny<IRequest<Response<UserStudyYearTimelineDto>>>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<Response<UserStudyYearTimelineDto>>, CancellationToken>((req, ct) =>
                {
                    capturedQuery = req as GetUserStudyYearTimelineQuery;
                })
                .ReturnsAsync(response);

            var controller = new UserStudyYearController(mockMediator.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
                }
            };

            // Act
            IActionResult actionResult = await controller.GetMyTimeline();

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(response, okResult!.Value);
            Assert.IsNotNull(capturedQuery);
            Assert.AreEqual(userId, capturedQuery!.UserId);
            mockMediator.Verify(m => m.Send(It.IsAny<IRequest<Response<UserStudyYearTimelineDto>>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that when mediator returns a failure response (Success = false),
        /// GetMyTimeline returns BadRequestObjectResult containing the mediator response.
        /// Input: ClaimsPrincipal with a valid NameIdentifier; mediator returns Success=false.
        /// Expected: BadRequestObjectResult with the same Response instance.
        /// </summary>
        [TestMethod]
        public async Task GetMyTimeline_ValidUserId_MediatorReturnsFailure_ReturnsBadRequestWithResult()
        {
            // Arrange
            var userId = "user-fail";
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) });
            var response = new Response<UserStudyYearTimelineDto>
            {
                Success = false,
                Data = null,
                Errors = "some error",
                Message = "failed"
            };

            var mockMediator = new Mock<IMediator>();
            mockMediator
                .Setup(m => m.Send(It.IsAny<IRequest<Response<UserStudyYearTimelineDto>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var controller = new UserStudyYearController(mockMediator.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
                }
            };

            // Act
            IActionResult actionResult = await controller.GetMyTimeline();

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));
            var badRequest = actionResult as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreSame(response, badRequest!.Value);
            mockMediator.Verify(m => m.Send(It.IsAny<IRequest<Response<UserStudyYearTimelineDto>>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies behavior when mediator returns null (unexpected).
        /// Input: ClaimsPrincipal with valid NameIdentifier and mediator returns null Task result.
        /// Expected: Method throws NullReferenceException when attempting to access result.Success.
        /// Note: This guards against unexpected nulls from mediator.
        /// </summary>
        [TestMethod]
        public async Task GetMyTimeline_MediatorReturnsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var userId = "user-null";
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) });

            var mockMediator = new Mock<IMediator>();
            // Return a completed Task with a null Response<T> to simulate unexpected null
            mockMediator
                .Setup(m => m.Send(It.IsAny<IRequest<Response<UserStudyYearTimelineDto>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Response<UserStudyYearTimelineDto>?)null!);

            var controller = new UserStudyYearController(mockMediator.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
                }
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<NullReferenceException>(async () => await controller.GetMyTimeline());
            mockMediator.Verify(m => m.Send(It.IsAny<IRequest<Response<UserStudyYearTimelineDto>>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Test purpose:
        /// Verify that when there is no NameIdentifier claim on the current user, the controller returns Unauthorized.
        /// Input conditions:
        /// - HttpContext.User has no claims.
        /// Expected result:
        /// - The action returns an UnauthorizedResult and the mediator is never invoked.
        /// </summary>
        [TestMethod]
        public async Task GetMyCurrentStudyYear_NoNameIdentifierClaim_ReturnsUnauthorized()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            var controller = new UserStudyYearController(mockMediator.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal() // no claims
                    }
                }
            };

            // Act
            var result = await controller.GetMyCurrentStudyYear();

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
            mockMediator.Verify(m => m.Send(It.IsAny<GetCurrentUserStudyYearQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Test purpose:
        /// Verify that when the NameIdentifier claim is present but empty, the controller treats it as unauthenticated and returns Unauthorized.
        /// Input conditions:
        /// - HttpContext.User has a NameIdentifier claim with empty string.
        /// Expected result:
        /// - The action returns an UnauthorizedResult and the mediator is never invoked.
        /// </summary>
        [TestMethod]
        public async Task GetMyCurrentStudyYear_EmptyNameIdentifierClaim_ReturnsUnauthorized()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, string.Empty) });
            var user = new ClaimsPrincipal(identity);

            var controller = new UserStudyYearController(mockMediator.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = user
                    }
                }
            };

            // Act
            var result = await controller.GetMyCurrentStudyYear();

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
            mockMediator.Verify(m => m.Send(It.IsAny<GetCurrentUserStudyYearQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Test purpose:
        /// Validate that for multiple valid (non-empty) NameIdentifier values the controller forwards the query to mediator
        /// and returns Ok with the mediator response when Success == true.
        /// Input conditions:
        /// - HttpContext.User has a NameIdentifier claim with various non-empty strings (normal, whitespace-only, very long, special chars).
        /// Expected result:
        /// - The action returns OkObjectResult containing the same Response instance returned by the mediator,
        ///   and mediator.Send is called with a query whose UserId matches the provided claim value.
        /// </summary>
        [TestMethod]
        public async Task GetMyCurrentStudyYear_MediatorSuccess_ReturnsOk_ForVariousUserIds()
        {
            // Test cases: normal, whitespace-only, very long, special characters
            string[] testUserIds = new[]
            {
                "user-123",
                "   ",
                new string('a', 1000),
                "special-!@#$%^&*()"
            };

            foreach (var userId in testUserIds)
            {
                // Arrange
                var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
                var expectedResponse = new Response<UserStudyYearDto>
                {
                    Success = true,
                    Data = null,
                    Errors = null,
                    Message = "ok"
                };

                // Setup mediator to validate the query.UserId and return expectedResponse
                mockMediator
                    .Setup(m => m.Send(It.Is<GetCurrentUserStudyYearQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedResponse)
                    .Verifiable();

                var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) });
                var user = new ClaimsPrincipal(identity);

                var controller = new UserStudyYearController(mockMediator.Object)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext
                        {
                            User = user
                        }
                    }
                };

                // Act
                var actionResult = await controller.GetMyCurrentStudyYear();

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
                var ok = actionResult as OkObjectResult;
                Assert.IsNotNull(ok);
                Assert.AreSame(expectedResponse, ok!.Value);
                mockMediator.Verify(); // ensures the setup expectation was met
            }
        }

        /// <summary>
        /// Test purpose:
        /// Verify that when the mediator returns a failed Response (Success == false), the controller returns BadRequest with that response.
        /// Input conditions:
        /// - HttpContext.User has a valid NameIdentifier claim.
        /// - Mediator returns Response with Success == false.
        /// Expected result:
        /// - The action returns BadRequestObjectResult containing the mediator response.
        /// </summary>
        [TestMethod]
        public async Task GetMyCurrentStudyYear_MediatorReturnsFailure_ReturnsBadRequest()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            var userId = "user-failed";
            var failedResponse = new Response<UserStudyYearDto>
            {
                Success = false,
                Data = null,
                Errors = "some error",
                Message = "failed"
            };

            mockMediator
                .Setup(m => m.Send(It.Is<GetCurrentUserStudyYearQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResponse)
                .Verifiable();

            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) });
            var user = new ClaimsPrincipal(identity);

            var controller = new UserStudyYearController(mockMediator.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = user
                    }
                }
            };

            // Act
            var result = await controller.GetMyCurrentStudyYear();

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = result as BadRequestObjectResult;
            Assert.IsNotNull(bad);
            Assert.AreSame(failedResponse, bad!.Value);
            mockMediator.Verify();
        }

        /// <summary>
        /// Purpose: Verify that GetUserStudyYears returns OkObjectResult with the mediator response when the response.Success is true.
        /// Conditions: Various valid and edge-case non-null userId values (empty, whitespace, very long, special characters, normal).
        /// Expected: An OkObjectResult containing the same Response<List<UserStudyYearDto>> instance returned by IMediator.Send.
        /// </summary>
        [TestMethod]
        public async Task GetUserStudyYears_ValidUserIds_ReturnsOkObjectResult()
        {
            // Arrange & Act & Assert iterated for multiple userId inputs to avoid multiple nearly-identical tests.
            string[] testUserIds = new[]
            {
                "normal-user",
                "", // empty string
                "   ", // whitespace-only
                new string('a', 1000), // very long string
                "user\n\r\t!@#€" // special/control characters
            };

            foreach (string userId in testUserIds)
            {
                // Arrange
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

                var returnedList = new List<UserStudyYearDto>();
                var response = Response<List<UserStudyYearDto>>.SuccessResponse(returnedList);

                // Setup Expectation: the query should be constructed with the provided userId
                mediatorMock
                    .Setup(m => m.Send(It.Is<GetUserStudyYearsQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(response)
                    .Verifiable();

                var controller = new UserStudyYearController(mediatorMock.Object);

                // Act
                IActionResult actionResult = await controller.GetUserStudyYears(userId);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), $"Expected OkObjectResult for userId='{userId}'");
                var ok = actionResult as OkObjectResult;
                Assert.IsNotNull(ok);
                Assert.AreSame(response, ok!.Value, "Returned response instance should be the same object provided by mediator.");
                mediatorMock.Verify();
            }
        }

        /// <summary>
        /// Purpose: Verify that GetUserStudyYears returns BadRequestObjectResult when mediator returns a failed response (Success = false).
        /// Conditions: A typical non-null userId and a Response.ErrorResponse returned by IMediator.Send.
        /// Expected: A BadRequestObjectResult containing the same Response<List<UserStudyYearDto>> instance returned by IMediator.Send.
        /// </summary>
        [TestMethod]
        public async Task GetUserStudyYears_MediatorReturnsFailure_ReturnsBadRequestObjectResult()
        {
            // Arrange
            string userId = "user123";
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            var errorResponse = Response<List<UserStudyYearDto>>.ErrorResponse("some error happened");

            mediatorMock
                .Setup(m => m.Send(It.Is<GetUserStudyYearsQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(errorResponse)
                .Verifiable();

            var controller = new UserStudyYearController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.GetUserStudyYears(userId);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));
            var bad = actionResult as BadRequestObjectResult;
            Assert.IsNotNull(bad);
            Assert.AreSame(errorResponse, bad!.Value, "Returned error response should be the same object provided by mediator.");
            mediatorMock.Verify();
        }

        /// <summary>
        /// Purpose: Ensure exceptions thrown by IMediator.Send propagate from GetUserStudyYears.
        /// Conditions: IMediator.Send throws an InvalidOperationException for the provided userId.
        /// Expected: The same InvalidOperationException is thrown by the controller method invocation.
        /// </summary>
        [TestMethod]
        public void GetUserStudyYears_MediatorThrows_ExceptionIsPropagated()
        {
            // Arrange
            string userId = "user-ex";
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            mediatorMock
                .Setup(m => m.Send(It.Is<GetUserStudyYearsQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("mediator failure"))
                .Verifiable();

            var controller = new UserStudyYearController(mediatorMock.Object);

            // Act & Assert
            try
            {
                // Use GetAwaiter().GetResult to execute synchronously and allow Assert.ThrowsException to catch the exception.
                Assert.ThrowsException<InvalidOperationException>(() => controller.GetUserStudyYears(userId).GetAwaiter().GetResult());
            }
            finally
            {
                mediatorMock.Verify();
            }
        }

        /// <summary>
        /// Verifies that the constructor creates a non-null controller instance when a valid IMediator is provided.
        /// Input conditions: A non-null mocked IMediator instance.
        /// Expected result: The returned UserStudyYearController instance is not null and is assignable to ControllerBase.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidMediator_InstanceCreated()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Loose);

            // Act
            var controller = new UserStudyYearController(mediatorMock.Object);

            // Assert
            Assert.IsNotNull(controller, "Controller instance should not be null when a valid IMediator is provided.");
            Assert.IsInstanceOfType(controller, typeof(UserStudyYearController), "Created instance should be of type UserStudyYearController.");
            Assert.IsInstanceOfType(controller, typeof(ControllerBase), "UserStudyYearController should derive from ControllerBase.");
        }

        /// <summary>
        /// Test purpose:
        /// Verify that when the mediator returns a successful Response&lt;UserStudyYearDto&gt; the controller
        /// returns an OkObjectResult containing the same Response object.
        /// Input conditions:
        /// - A non-null CreateUserStudyYearDto is provided.
        /// - Mediator.Send returns a successful Response with Data.
        /// Expected result:
        /// - The action result is OkObjectResult and the returned value is the same Response instance returned by mediator.
        /// </summary>
        [TestMethod]
        public async Task CreateUserStudyYear_MediatorReturnsSuccess_ReturnsOkWithResponse()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var dto = new CreateUserStudyYearDto
            {
                UserId = "user-123",
                StudyYearId = 1
                // Level will be defaulted; not required for controller behavior
            };

            var data = new UserStudyYearDto
            {
                Id = 42,
                UserId = dto.UserId,
                StudyYearId = dto.StudyYearId
                // other properties left as defaults
            };

            var response = Response<UserStudyYearDto>.SuccessResponse(data);

            mediatorMock
                .Setup(m => m.Send(It.Is<CreateUserStudyYearCommand>(c => object.ReferenceEquals(c.Dto, dto)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var controller = new UserStudyYearController(mediatorMock.Object);

            // Act
            var result = await controller.CreateUserStudyYear(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreSame(response, ok.Value);
            mediatorMock.Verify(m => m.Send(It.IsAny<CreateUserStudyYearCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Test purpose:
        /// Verify that when the mediator returns an error Response&lt;UserStudyYearDto&gt; the controller
        /// returns a BadRequestObjectResult containing the same Response object.
        /// Input conditions:
        /// - A non-null CreateUserStudyYearDto is provided.
        /// - Mediator.Send returns an error Response with Success == false.
        /// Expected result:
        /// - The action result is BadRequestObjectResult and the returned value is the same Response instance returned by mediator.
        /// </summary>
        [TestMethod]
        public async Task CreateUserStudyYear_MediatorReturnsFailure_ReturnsBadRequestWithResponse()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var dto = new CreateUserStudyYearDto
            {
                UserId = "user-xyz",
                StudyYearId = int.MaxValue // boundary numeric value to ensure controller simply forwards the dto
            };

            var response = Response<UserStudyYearDto>.ErrorResponse("Invalid operation");

            mediatorMock
                .Setup(m => m.Send(It.Is<CreateUserStudyYearCommand>(c => object.ReferenceEquals(c.Dto, dto)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var controller = new UserStudyYearController(mediatorMock.Object);

            // Act
            var result = await controller.CreateUserStudyYear(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            Assert.AreSame(response, bad.Value);
            mediatorMock.Verify(m => m.Send(It.IsAny<CreateUserStudyYearCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that when the mediator returns a successful Response&lt;UserStudyYearDto&gt; the controller
        /// responds with OkObjectResult containing the same response object.
        /// Test inputs: several id edge values (int.MinValue, -1, 0, int.MaxValue) with a non-null dto.
        /// Expected: OkObjectResult is returned and mediator.Send is invoked once per call with matching command.
        /// </summary>
        [TestMethod]
        public async Task UpdateUserStudyYear_MediatorSuccess_ReturnsOk_ForVariousIds()
        {
            // Arrange
            int[] ids = new[] { int.MinValue, -1, 0, int.MaxValue };
            foreach (int id in ids)
            {
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
                UpdateUserStudyYearDto dto = new UpdateUserStudyYearDto { Level = null };
                var expectedResponse = Response<UserStudyYearDto>.SuccessResponse(new UserStudyYearDto { Id = id });

                mediatorMock
                    .Setup(m => m.Send(It.Is<UpdateUserStudyYearCommand>(c => c.Id == id && c.Dto == dto), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedResponse)
                    .Verifiable();

                var controller = new UserStudyYearController(mediatorMock.Object);

                // Act
                IActionResult actionResult = await controller.UpdateUserStudyYear(id, dto).ConfigureAwait(false);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), $"Expected OkObjectResult for id={id}");
                var ok = actionResult as OkObjectResult;
                Assert.IsNotNull(ok);
                Assert.AreSame(expectedResponse, ok!.Value, "Controller should return the exact Response instance returned by mediator.");
                mediatorMock.Verify(m => m.Send(It.Is<UpdateUserStudyYearCommand>(c => c.Id == id && c.Dto == dto), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        /// <summary>
        /// Verifies that when the mediator returns a failure Response&lt;UserStudyYearDto&gt; the controller
        /// responds with BadRequestObjectResult containing the same response object.
        /// Test inputs: several id edge values (int.MinValue, -1, 0, int.MaxValue) with a non-null dto.
        /// Expected: BadRequestObjectResult is returned and mediator.Send is invoked once per call with matching command.
        /// </summary>
        [TestMethod]
        public async Task UpdateUserStudyYear_MediatorFailure_ReturnsBadRequest_ForVariousIds()
        {
            // Arrange
            int[] ids = new[] { int.MinValue, -1, 0, int.MaxValue };
            foreach (int id in ids)
            {
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
                UpdateUserStudyYearDto dto = new UpdateUserStudyYearDto { Level = null };
                var expectedResponse = Response<UserStudyYearDto>.ErrorResponse($"error-for-{id}");

                mediatorMock
                    .Setup(m => m.Send(It.Is<UpdateUserStudyYearCommand>(c => c.Id == id && c.Dto == dto), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedResponse)
                    .Verifiable();

                var controller = new UserStudyYearController(mediatorMock.Object);

                // Act
                IActionResult actionResult = await controller.UpdateUserStudyYear(id, dto).ConfigureAwait(false);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult), $"Expected BadRequestObjectResult for id={id}");
                var bad = actionResult as BadRequestObjectResult;
                Assert.IsNotNull(bad);
                Assert.AreSame(expectedResponse, bad!.Value, "Controller should return the exact Response instance returned by mediator.");
                mediatorMock.Verify(m => m.Send(It.Is<UpdateUserStudyYearCommand>(c => c.Id == id && c.Dto == dto), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        /// <summary>
        /// Verifies that if the mediator throws an exception the controller does not swallow it (exception propagates).
        /// Test inputs: a representative id (0) and a non-null dto.
        /// Expected: the same exception thrown by mediator propagates out of the controller call.
        /// </summary>
        [TestMethod]
        public async Task UpdateUserStudyYear_MediatorThrows_ExceptionPropagated()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            int id = 0;
            UpdateUserStudyYearDto dto = new UpdateUserStudyYearDto { Level = null };
            var ex = new InvalidOperationException("mediator-failure");

            mediatorMock
                .Setup(m => m.Send(It.Is<UpdateUserStudyYearCommand>(c => c.Id == id && c.Dto == dto), It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex)
                .Verifiable();

            var controller = new UserStudyYearController(mediatorMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await controller.UpdateUserStudyYear(id, dto).ConfigureAwait(false);
            });

            mediatorMock.Verify(m => m.Send(It.Is<UpdateUserStudyYearCommand>(c => c.Id == id && c.Dto == dto), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetUserTimeline returns OkObjectResult when mediator returns a successful Response.
        /// Inputs tested: a set of representative userId values including empty string, whitespace-only, very long string, and special characters.
        /// Expected: IActionResult is OkObjectResult and its Value is the same Response instance returned by the mediator.
        /// </summary>
        [TestMethod]
        public async Task GetUserTimeline_MediatorReturnsSuccess_ReturnsOk_ForMultipleUserIds()
        {
            // Arrange: various representative userId inputs (method parameter is non-nullable in source)
            string[] userIds = new[]
            {
                "normal-user-id",
                "",
                "   ",
                new string('a', 1024),
                "special-!@#$%^&*()_+|\\/~"
            };

            foreach (string userId in userIds)
            {
                // Arrange - per iteration
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

                var expectedResponse = new Response<UserStudyYearTimelineDto>
                {
                    Success = true,
                    Data = null,
                    Errors = null,
                    Message = "ok"
                };

                mediatorMock
                    .Setup(m => m.Send(It.Is<GetUserStudyYearTimelineQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedResponse)
                    .Verifiable();

                var controller = new UserStudyYearController(mediatorMock.Object);

                // Act
                IActionResult actionResult = await controller.GetUserTimeline(userId);

                // Assert
                Assert.IsNotNull(actionResult, "ActionResult should not be null");
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), $"Expected OkObjectResult for userId '{userId}'");

                var okResult = actionResult as OkObjectResult;
                Assert.AreSame(expectedResponse, okResult?.Value, "Returned value should be the same Response instance from mediator");

                mediatorMock.Verify();
            }
        }

        /// <summary>
        /// Tests that GetUserTimeline returns BadRequestObjectResult when mediator returns a failed Response.
        /// Inputs tested: a set of representative userId values including empty string, whitespace-only, very long string, and special characters.
        /// Expected: IActionResult is BadRequestObjectResult and its Value is the same Response instance returned by the mediator.
        /// </summary>
        [TestMethod]
        public async Task GetUserTimeline_MediatorReturnsFailure_ReturnsBadRequest_ForMultipleUserIds()
        {
            // Arrange: various representative userId inputs (method parameter is non-nullable in source)
            string[] userIds = new[]
            {
                "normal-user-id",
                "",
                "   ",
                new string('b', 2048),
                "special-\t\n\r"
            };

            foreach (string userId in userIds)
            {
                // Arrange - per iteration
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

                var expectedResponse = new Response<UserStudyYearTimelineDto>
                {
                    Success = false,
                    Data = null,
                    Errors = "some error",
                    Message = "failed"
                };

                mediatorMock
                    .Setup(m => m.Send(It.Is<GetUserStudyYearTimelineQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedResponse)
                    .Verifiable();

                var controller = new UserStudyYearController(mediatorMock.Object);

                // Act
                IActionResult actionResult = await controller.GetUserTimeline(userId);

                // Assert
                Assert.IsNotNull(actionResult, "ActionResult should not be null");
                Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult), $"Expected BadRequestObjectResult for userId '{userId}'");

                var badRequest = actionResult as BadRequestObjectResult;
                Assert.AreSame(expectedResponse, badRequest?.Value, "Returned value should be the same Response instance from mediator");

                mediatorMock.Verify();
            }
        }
    }
}