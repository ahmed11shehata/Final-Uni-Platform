using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Dashboard;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Presentation;
using Presentation.Controllers;
using Shared.Dtos.Info_Module.DashboardDtos;

namespace Presentation.Controllers.UnitTests
{
    [TestClass]
    public partial class InstructorDashboardControllerTests
    {
        /// <summary>
        /// Verifies that the constructor creates a non-null instance when provided a valid IMediator.
        /// Input conditions: a mocked IMediator instance is provided.
        /// Expected result: an InstructorDashboardController instance is created and is not null.
        /// </summary>
        [TestMethod]
        public void InstructorDashboardController_WithValidMediator_InstanceCreated()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            // Act
            var controller = new InstructorDashboardController(mediatorMock.Object);

            // Assert
            Assert.IsNotNull(controller, "Constructor returned null for a valid IMediator.");
            Assert.IsInstanceOfType(controller, typeof(InstructorDashboardController));
        }

        /// <summary>
        /// Ensures that when constructed with a valid IMediator, controller methods use that mediator.
        /// Input conditions: IMediator mock set up to return a specific object; HttpContext.User contains NameIdentifier claim.
        /// Expected result: GetDashboard invokes IMediator.Send exactly once and returns OkObjectResult containing the mediator's result.
        /// </summary>
        [TestMethod]
        public async Task GetDashboard_WithAuthenticatedUser_InvokesMediatorAndReturnsOk()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var expectedResult = new Dictionary<string, object?>()
            {
                ["Courses"] = new List<object>() { 1, 2, 3 }
            };

            object? capturedRequest = null;

            mediatorMock
                .Setup(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Callback<object, CancellationToken>((req, ct) => capturedRequest = req)
                .ReturnsAsync(expectedResult!);

            var controller = new InstructorDashboardController(mediatorMock.Object);

            // Set authenticated user with a NameIdentifier claim to exercise CurrentUserId property.
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "instructor-42")
            }, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var actionResult = await controller.GetDashboard().ConfigureAwait(false);

            // Assert
            mediatorMock.Verify(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once,
                "Expected IMediator.Send to be invoked exactly once by GetDashboard.");

            // Validate returned IActionResult is OkObjectResult and contains the expected sentinel object.
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(expectedResult, okResult!.Value, "The returned OkObjectResult.Value should be the same object returned from IMediator.Send.");

            // Validate that the request passed to mediator is of the expected query type (type only, not content).
            Assert.IsNotNull(capturedRequest, "Expected a request object to be passed to IMediator.Send.");
            Assert.IsInstanceOfType(capturedRequest!, typeof(GetInstructorDashboardQuery), "GetDashboard should send a GetInstructorDashboardQuery to the mediator.");
        }

        /// <summary>
        /// Purpose:
        /// Verifies that GetDashboard sends a GetInstructorDashboardQuery with the current user's id
        /// and returns an OkObjectResult containing the DTO returned by IMediator.
        /// Input:
        /// - Authenticated user with ClaimTypes.NameIdentifier = "user123".
        /// - IMediator returns a populated InstructorDashboardDto.
        /// Expected:
        /// - OkObjectResult is returned.
        /// - The returned value is the same instance provided by IMediator.
        /// - IMediator.Send is invoked once with a GetInstructorDashboardQuery whose UserId == "user123".
        /// </summary>
        [TestMethod]
        public async Task GetDashboard_UserWithId_ReturnsOkWithDto()
        {
            // Arrange
            var expectedDto = new InstructorDashboardDto
            {
                Courses = new List<InstructorCourseDto>
                {
                    new InstructorCourseDto { Id = "c1", Code = "CODE1", Name = "Name1", Students = 5, Progress = 50 }
                },
                GradeSummary = new Dictionary<string, object> { { "A", 1 } },
                RecentActivity = new List<object> { "activity" },
                Upcoming = new List<object> { "upcoming" }
            };

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.Is<GetInstructorDashboardQuery>(q => q.UserId == "user123"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var controller = new InstructorDashboardController(mediatorMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, "user123")
                        }, "TestAuth"))
                    }
                }
            };

            // Act
            var actionResult = await controller.GetDashboard().ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var ok = actionResult as OkObjectResult;
            Assert.AreSame(expectedDto, ok?.Value);
            mediatorMock.Verify(m => m.Send(It.Is<GetInstructorDashboardQuery>(q => q.UserId == "user123"), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Purpose:
        /// Ensures that when the current user has no NameIdentifier claim, CurrentUserId becomes empty string,
        /// and GetDashboard still calls IMediator with an empty user id and returns Ok with the mediator result.
        /// Input:
        /// - Unauthenticated or user without NameIdentifier claim.
        /// - IMediator returns an empty InstructorDashboardDto instance.
        /// Expected:
        /// - OkObjectResult is returned with the dto.
        /// - IMediator.Send invoked once with UserId == string.Empty.
        /// </summary>
        [TestMethod]
        public async Task GetDashboard_NoUserId_PassesEmptyUserIdAndReturnsOk()
        {
            // Arrange
            var expectedDto = new InstructorDashboardDto(); // minimal, valid DTO
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.Is<GetInstructorDashboardQuery>(q => q.UserId == string.Empty), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var controller = new InstructorDashboardController(mediatorMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        // No claims identity or identity without NameIdentifier
                        User = new ClaimsPrincipal(new ClaimsIdentity())
                    }
                }
            };

            // Act
            var actionResult = await controller.GetDashboard().ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var ok = actionResult as OkObjectResult;
            Assert.AreSame(expectedDto, ok?.Value);
            mediatorMock.Verify(m => m.Send(It.Is<GetInstructorDashboardQuery>(q => q.UserId == string.Empty), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Purpose:
        /// Validates behavior when IMediator returns null (no DTO). GetDashboard should not throw and should
        /// return Ok with a null value.
        /// Input:
        /// - Authenticated user with ClaimTypes.NameIdentifier = "u-null".
        /// - IMediator returns null.
        /// Expected:
        /// - OkObjectResult with Value == null.
        /// - IMediator.Send invoked once with the provided user id.
        /// </summary>
        [TestMethod]
        public async Task GetDashboard_MediatorReturnsNull_ReturnsOkWithNullValue()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.Is<GetInstructorDashboardQuery>(q => q.UserId == "u-null"), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InstructorDashboardDto?)null);

            var controller = new InstructorDashboardController(mediatorMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, "u-null")
                        }, "TestAuth"))
                    }
                }
            };

            // Act
            var actionResult = await controller.GetDashboard().ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var ok = actionResult as OkObjectResult;
            Assert.IsNull(ok?.Value);
            mediatorMock.Verify(m => m.Send(It.Is<GetInstructorDashboardQuery>(q => q.UserId == "u-null"), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}