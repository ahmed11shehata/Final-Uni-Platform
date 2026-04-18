using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Assignment;
using AYA_UIS.Application.Commands.CreateAssignment;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Assignments;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Presentation;
using Presentation.Controllers;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.AssignmentDto;
using Shared.Respones;

namespace Presentation.Controllers.UnitTests
{
    [TestClass]
    public partial class AssignmentsControllerTests
    {
        /// <summary>
        /// Test that CreateAssignment creates the expected CreateAssignmentCommand,
        /// forwards it to IMediator.Send and returns Ok(result).
        /// Inputs: typical non-null title/description, reasonable points, deadline, courseId and a non-null IFormFile.
        /// Expected: IMediator.Send is invoked once with a command whose properties match the provided inputs
        /// and the controller returns OkObjectResult containing the mediator response.
        /// </summary>
        [TestMethod]
        public async Task CreateAssignment_WithValidInputs_ReturnsOkAndSendsCommand()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            var expectedResponse = Response<int>.SuccessResponse(123);
            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateAssignmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var controller = new AssignmentsController(mediatorMock.Object);

            // Provide a ClaimsPrincipal that contains a NameIdentifier claim
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "instructor-42") };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            string title = "Assignment 1";
            string description = "Solve problems";
            int points = 100;
            DateTime deadline = new DateTime(2025, 1, 1);
            int courseId = 7;
            var fileMock = new Mock<IFormFile>(MockBehavior.Strict);
            IFormFile file = fileMock.Object;

            // Act
            IActionResult actionResult = await controller.CreateAssignment(courseId, title, description, points, deadline, file);

            // Assert
            // 1) Response is OkObjectResult and contains the mediator response
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var ok = (OkObjectResult)actionResult;
            Assert.AreSame(expectedResponse, ok.Value);

            // 2) Mediator.Send was called with a command matching the provided inputs, including instructor id
            mediatorMock.Verify(m =>
                m.Send(
                    It.Is<CreateAssignmentCommand>(c =>
                        c != null &&
                        c.AssignmentDto != null &&
                        c.AssignmentDto.Title == title &&
                        c.AssignmentDto.Description == description &&
                        c.AssignmentDto.Points == points &&
                        c.AssignmentDto.Deadline == deadline &&
                        c.AssignmentDto.CourseId == courseId &&
                        ReferenceEquals(c.File, file) &&
                        c.InstructorId == "instructor-42"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Test CreateAssignment with edge-case inputs: extreme numeric values and unusual strings.
        /// Inputs: int.MinValue for points, whitespace title, empty description, DateTime.MinValue, and extreme courseId.
        /// Also tests behavior when the current user has no NameIdentifier claim (InstructorId missing).
        /// Expected: Controller still constructs a command and sends it to IMediator.Send; the command's InstructorId
        /// is empty or null (string may be empty or null depending on runtime), and the returned Ok contains mediator response.
        /// </summary>
        [TestMethod]
        public async Task CreateAssignment_WithEdgeInputs_MediatorCalledAndInstructorMissingHandled()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            var expectedResponse = Response<int>.ErrorResponse("fail");
            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateAssignmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var controller = new AssignmentsController(mediatorMock.Object);

            // No NameIdentifier claim provided
            var principal = new ClaimsPrincipal(new ClaimsIdentity()); // empty identity
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            string title = "   "; // whitespace-only
            string description = string.Empty; // empty
            int points = int.MinValue;
            DateTime deadline = DateTime.MinValue;
            int courseId = int.MaxValue;
            var fileMock = new Mock<IFormFile>(MockBehavior.Loose);
            IFormFile file = fileMock.Object;

            // Act
            IActionResult actionResult = await controller.CreateAssignment(courseId, title, description, points, deadline, file);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var ok = (OkObjectResult)actionResult;
            Assert.AreSame(expectedResponse, ok.Value);

            // Verify that mediator was called with appropriate command values and that InstructorId is absent or empty
            mediatorMock.Verify(m =>
                m.Send(
                    It.Is<CreateAssignmentCommand>(c =>
                        c != null &&
                        c.AssignmentDto != null &&
                        c.AssignmentDto.Title == title &&
                        c.AssignmentDto.Description == description &&
                        c.AssignmentDto.Points == points &&
                        c.AssignmentDto.Deadline == deadline &&
                        c.AssignmentDto.CourseId == courseId &&
                        ReferenceEquals(c.File, file) &&
                        // InstructorId should be null or empty when claim is missing; accept either
                        string.IsNullOrEmpty(c.InstructorId)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that the AssignmentsController constructor creates an instance when provided a valid IMediator.
        /// Input conditions: a non-null mocked IMediator instance.
        /// Expected result: constructor completes without throwing and returns a non-null AssignmentsController instance.
        /// </summary>
        [TestMethod]
        public void AssignmentsController_ValidMediator_InstanceCreated()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            // Act
            var controller = new AssignmentsController(mediatorMock.Object);

            // Assert
            Assert.IsNotNull(controller, "Constructor returned null when provided a valid IMediator.");
            Assert.IsInstanceOfType(controller, typeof(AssignmentsController), "Instance is not of expected type AssignmentsController.");
        }

        /// <summary>
        /// Ensures AssignmentsController constructed with a mocked IMediator is assignable to ControllerBase.
        /// Input conditions: a non-null mocked IMediator instance.
        /// Expected result: resulting object derives from ControllerBase allowing controller behaviors.
        /// </summary>
        [TestMethod]
        public void AssignmentsController_ValidMediator_IsAssignableToControllerBase()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Loose);

            // Act
            var controller = new AssignmentsController(mediatorMock.Object);

            // Assert
            Assert.IsNotNull(controller);
            Assert.IsInstanceOfType(controller, typeof(ControllerBase), "AssignmentsController should derive from ControllerBase.");
        }

        /// <summary>
        /// Tests that GetAssignments returns OkObjectResult containing the same enumerable instance provided by IMediator.
        /// Input conditions: various integer courseId values including int.MinValue, negative, zero, positive, and int.MaxValue.
        /// Expected result: Controller calls IMediator.Send with a GetAssignmentsByCourseQuery whose CourseId equals the provided courseId,
        /// and the returned IActionResult is OkObjectResult containing the same reference returned by the mediator.
        /// </summary>
        [TestMethod]
        public async Task GetAssignments_ValidCourseIds_ReturnsOkWithMediatorResult()
        {
            // Arrange & Act & Assert for multiple courseId edge cases
            int[] testCourseIds = new[]
            {
                int.MinValue,
                -1,
                0,
                1,
                int.MaxValue
            };

            foreach (int courseId in testCourseIds)
            {
                // Arrange
                var expectedList = new List<AssignmentDto>
                {
                    new AssignmentDto()
                };

                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
                mediatorMock
                    .Setup(m => m.Send<IEnumerable<AssignmentDto>>(It.Is<GetAssignmentsByCourseQuery>(q => q.CourseId == courseId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedList)
                    .Verifiable();

                var controller = new AssignmentsController(mediatorMock.Object);

                // Act
                IActionResult actionResult = await controller.GetAssignments(courseId);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult for valid mediator response.");
                var ok = actionResult as OkObjectResult;
                // The controller should return exactly the object provided by mediator (reference equality)
                Assert.AreSame(expectedList, ok?.Value as IEnumerable<AssignmentDto>, "Controller did not return the same reference provided by IMediator.");

                mediatorMock.Verify(m => m.Send<IEnumerable<AssignmentDto>>(It.Is<GetAssignmentsByCourseQuery>(q => q.CourseId == courseId), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        /// <summary>
        /// Tests that GetAssignments returns OkObjectResult with null value when the mediator returns null.
        /// Input conditions: mediator returns null for a sample courseId.
        /// Expected result: IActionResult is OkObjectResult and its Value is null.
        /// </summary>
        [TestMethod]
        public async Task GetAssignments_MediatorReturnsNull_ReturnsOkWithNull()
        {
            // Arrange
            int courseId = 123;
            IEnumerable<AssignmentDto>? mediatorResponse = null;

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send<IEnumerable<AssignmentDto>>(It.Is<GetAssignmentsByCourseQuery>(q => q.CourseId == courseId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResponse)
                .Verifiable();

            var controller = new AssignmentsController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.GetAssignments(courseId);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult even when mediator returns null.");
            var ok = actionResult as OkObjectResult;
            Assert.IsNull(ok?.Value, "Expected OkObjectResult.Value to be null when mediator returns null.");

            mediatorMock.Verify(m => m.Send<IEnumerable<AssignmentDto>>(It.Is<GetAssignmentsByCourseQuery>(q => q.CourseId == courseId), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that when valid inputs are provided, the controller builds a SubmitAssignmentCommand
        /// with the expected AssignmentId, Academic_Code (from ClaimsPrincipal), and File, then sends it
        /// via IMediator and returns an OkObjectResult wrapping the mediator response.
        /// Inputs: assignmentId = 42, claim NameIdentifier = "student-123", non-empty IFormFile mock.
        /// Expected: mediator receives a command with matching properties and controller returns Ok(result).
        /// </summary>
        [TestMethod]
        public async Task SubmitAssignment_ValidInputs_CallsMediatorAndReturnsOk()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            var expectedResponse = new Response<int> { /* Assuming default ctor and settable properties */ };
            // Setup mediator to return expectedResponse when Send is called with a matching command
            mediatorMock
                .Setup(m => m.Send(It.Is<SubmitAssignmentCommand>(c =>
                    c != null
                    && c.AssignmentId == 42
                    && c.Academic_Code == "student-123"
                    && c.File != null
                ), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse)
                .Verifiable();

            var controller = new AssignmentsController(mediatorMock.Object);

            // Setup ControllerContext with ClaimsPrincipal containing NameIdentifier claim
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "student-123") };
            var identity = new ClaimsIdentity(claims, "test");
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Create a non-empty IFormFile mock
            var fileMock = new Mock<IFormFile>(MockBehavior.Strict);
            fileMock.SetupGet(f => f.Length).Returns(123L);

            // Act
            IActionResult actionResult = await controller.SubmitAssignment(42, fileMock.Object);

            // Assert
            mediatorMock.Verify(m => m.Send(It.IsAny<SubmitAssignmentCommand>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var ok = (OkObjectResult)actionResult;
            Assert.AreSame(expectedResponse, ok.Value);
        }

        /// <summary>
        /// Ensures that when the current user has no NameIdentifier claim, the Academic_Code passed to the mediator is null.
        /// Inputs: assignmentId = 7, ClaimsPrincipal with no NameIdentifier claim, non-empty IFormFile mock.
        /// Expected: mediator receives a command whose Academic_Code is null and controller returns Ok(result).
        /// </summary>
        [TestMethod]
        public async Task SubmitAssignment_NoNameIdentifierClaim_PassesNullAcademicCodeToMediator()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            Response<int> expectedResponse = new Response<int>();
            mediatorMock
                .Setup(m => m.Send(It.Is<SubmitAssignmentCommand>(c =>
                    c != null
                    && c.AssignmentId == 7
                    && c.Academic_Code == null
                ), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse)
                .Verifiable();

            var controller = new AssignmentsController(mediatorMock.Object);

            // Principal without NameIdentifier claim
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("some-other-claim", "x") }, "test"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var fileMock = new Mock<IFormFile>(MockBehavior.Strict);
            fileMock.SetupGet(f => f.Length).Returns(1L);

            // Act
            IActionResult actionResult = await controller.SubmitAssignment(7, fileMock.Object);

            // Assert
            mediatorMock.Verify(m => m.Send(It.IsAny<SubmitAssignmentCommand>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var ok = (OkObjectResult)actionResult;
            Assert.AreSame(expectedResponse, ok.Value);
        }

        /// <summary>
        /// Validates that extreme integer values for assignmentId are correctly forwarded to the mediator.
        /// Inputs: assignmentId = int.MinValue and int.MaxValue, NameIdentifier = "s", non-empty IFormFile mock.
        /// Expected: mediator is called for each assignmentId with matching value and controller returns Ok with mediator result.
        /// </summary>
        [TestMethod]
        public async Task SubmitAssignment_ExtremeAssignmentId_ForwardedToMediatorForEachValue()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            var responses = new Dictionary<int, Response<int>>
            {
                { int.MinValue, new Response<int>() },
                { int.MaxValue, new Response<int>() }
            };

            mediatorMock
                .Setup(m => m.Send(It.IsAny<SubmitAssignmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SubmitAssignmentCommand cmd, CancellationToken _) =>
                {
                    // Return response based on assignment id to allow verification of forwarded value
                    if (responses.TryGetValue(cmd.AssignmentId, out var resp))
                        return resp;
                    return new Response<int>();
                });

            var controller = new AssignmentsController(mediatorMock.Object);

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "s") };
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) }
            };

            var fileMock = new Mock<IFormFile>(MockBehavior.Strict);
            fileMock.SetupGet(f => f.Length).Returns(5L);

            // Act & Assert for int.MinValue
            IActionResult resultMin = await controller.SubmitAssignment(int.MinValue, fileMock.Object);
            mediatorMock.Verify(m => m.Send(It.Is<SubmitAssignmentCommand>(c => c.AssignmentId == int.MinValue && c.Academic_Code == "s"), It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsInstanceOfType(resultMin, typeof(OkObjectResult));
            Assert.AreSame(responses[int.MinValue], ((OkObjectResult)resultMin).Value);

            // Act & Assert for int.MaxValue
            IActionResult resultMax = await controller.SubmitAssignment(int.MaxValue, fileMock.Object);
            mediatorMock.Verify(m => m.Send(It.Is<SubmitAssignmentCommand>(c => c.AssignmentId == int.MaxValue && c.Academic_Code == "s"), It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsInstanceOfType(resultMax, typeof(OkObjectResult));
            Assert.AreSame(responses[int.MaxValue], ((OkObjectResult)resultMax).Value);
        }

        /// <summary>
        /// Verifies that when a valid GradeSubmissionCommand is sent, the controller forwards the command
        /// to IMediator and returns an OkObjectResult containing the exact Response&lt;int&gt; instance returned by the mediator.
        /// Input: non-null GradeSubmissionCommand; mediator returns a non-null Response&lt;int&gt; instance.
        /// Expected: OkObjectResult with the same Response&lt;int&gt; instance and mediator.Send called once with the same command.
        /// </summary>
        [TestMethod]
        public async Task GradeSubmission_CommandValid_ReturnsOkWithResponse()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            var command = new GradeSubmissionCommand
            {
                SubmissionId = 1,
                Grade = 90,
                Feedback = "Good"
            };

            var expectedResponse = new Response<int>(); // Use instance identity for assertion

            mockMediator
                .Setup(m => m.Send(It.Is<GradeSubmissionCommand>(c => object.ReferenceEquals(c, command)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse)
                .Verifiable();

            var controller = new AssignmentsController(mockMediator.Object);

            // Act
            var actionResult = await controller.GradeSubmission(command).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult when mediator returns a response.");
            var okResult = (OkObjectResult)actionResult;
            Assert.AreSame(expectedResponse, okResult.Value, "Controller should return the exact response instance provided by mediator.");
            mockMediator.Verify(m => m.Send(It.IsAny<GradeSubmissionCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies behavior when the mediator returns null.
        /// Input: non-null GradeSubmissionCommand; mediator returns null.
        /// Expected: OkObjectResult with null value and mediator.Send called once.
        /// </summary>
        [TestMethod]
        public async Task GradeSubmission_MediatorReturnsNull_ReturnsOkWithNull()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            var command = new GradeSubmissionCommand
            {
                SubmissionId = int.MaxValue,
                Grade = 0,
                Feedback = string.Empty
            };

            Response<int>? expectedResponse = null;

            mockMediator
                .Setup(m => m.Send(It.IsAny<GradeSubmissionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse)
                .Verifiable();

            var controller = new AssignmentsController(mockMediator.Object);

            // Act
            var actionResult = await controller.GradeSubmission(command).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult even when mediator returns null.");
            var okResult = (OkObjectResult)actionResult;
            Assert.IsNull(okResult.Value, "Expected OkObjectResult.Value to be null when mediator returns null.");
            mockMediator.Verify(m => m.Send(It.IsAny<GradeSubmissionCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that exceptions thrown by the mediator propagate through the controller action.
        /// Input: non-null GradeSubmissionCommand; mediator.Send throws InvalidOperationException.
        /// Expected: the same exception type bubbles up from the controller method.
        /// </summary>
        [TestMethod]
        public async Task GradeSubmission_MediatorThrows_ExceptionPropagates()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            var command = new GradeSubmissionCommand
            {
                SubmissionId = -1,
                Grade = -10,
                Feedback = "Edge"
            };

            var expectedException = new InvalidOperationException("mediator failure");

            mockMediator
                .Setup(m => m.Send(It.IsAny<GradeSubmissionCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException)
                .Verifiable();

            var controller = new AssignmentsController(mockMediator.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await controller.GradeSubmission(command).ConfigureAwait(false);
            });

            mockMediator.Verify(m => m.Send(It.IsAny<GradeSubmissionCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that when the user has a NameIdentifier claim, GetMyGrades forwards the courseId and studentId
        /// to IMediator.Send and returns OkObjectResult containing the mediator result.
        /// Input: courseId = 42, studentId = "student-123", mediator returns a non-null enumerable.
        /// Expected: OkObjectResult with the same enumerable instance and mediator invoked once with matching query.
        /// </summary>
        [TestMethod]
        public async Task GetMyGrades_StudentClaimPresent_ReturnsOkWithMediatorResult()
        {
            // Arrange
            int courseId = 42;
            string studentId = "student-123";
            IEnumerable<StudentAssignmentGradeDto> mediatorResult = new List<StudentAssignmentGradeDto>();

            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            mockMediator
                .Setup(m => m.Send(It.Is<GetStudentAssignmentGradesQuery>(q => q.CourseId == courseId && q.StudentId == studentId),
                                  It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult);

            var controller = new AssignmentsController(mockMediator.Object);
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, studentId) });
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            // Act
            IActionResult actionResult = await controller.GetMyGrades(courseId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var ok = (OkObjectResult)actionResult;
            Assert.AreSame(mediatorResult, ok.Value);
            mockMediator.Verify(m => m.Send(It.IsAny<GetStudentAssignmentGradesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies behavior when the NameIdentifier claim is absent (studentId becomes null) and mediator returns null.
        /// Input: courseId = int.MinValue, no claim present, mediator returns null.
        /// Expected: OkObjectResult with null Value and mediator invoked once with StudentId == null.
        /// </summary>
        [TestMethod]
        public async Task GetMyGrades_MissingStudentClaimAndNullResult_ReturnsOkWithNull()
        {
            // Arrange
            int courseId = int.MinValue;
            string? studentId = null;
            IEnumerable<StudentAssignmentGradeDto>? mediatorResult = null;

            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            mockMediator
                .Setup(m => m.Send(It.Is<GetStudentAssignmentGradesQuery>(q => q.CourseId == courseId && q.StudentId == studentId),
                                  It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult);

            var controller = new AssignmentsController(mockMediator.Object);
            // No NameIdentifier claim set
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            // Act
            IActionResult actionResult = await controller.GetMyGrades(courseId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var ok = (OkObjectResult)actionResult;
            Assert.IsNull(ok.Value);
            mockMediator.Verify(m => m.Send(It.Is<GetStudentAssignmentGradesQuery>(q => q.CourseId == courseId && q.StudentId == studentId), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies multiple edge-case studentId values are forwarded unchanged to mediator and result is returned in OkObjectResult.
        /// Inputs: several studentId variants (empty, whitespace, long, special/control characters).
        /// Expected: mediator invoked with the exact studentId each iteration and OkObjectResult contains the mediator result.
        /// </summary>
        [TestMethod]
        public async Task GetMyGrades_VariousStudentIdValues_PassesThroughToMediatorAndReturnsOk()
        {
            // Arrange
            int courseId = 7;
            var testStudentIds = new string?[]
            {
                string.Empty,
                "   ",
                new string('a', 5000),
                "spéc!@l\u0000chars"
            };

            foreach (string? testStudentId in testStudentIds)
            {
                IEnumerable<StudentAssignmentGradeDto> mediatorResult = new List<StudentAssignmentGradeDto>();

                var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
                mockMediator
                    .Setup(m => m.Send(It.Is<GetStudentAssignmentGradesQuery>(q => q.CourseId == courseId && q.StudentId == testStudentId),
                                      It.IsAny<CancellationToken>()))
                    .ReturnsAsync(mediatorResult);

                var controller = new AssignmentsController(mockMediator.Object);
                ClaimsPrincipal principal;
                if (testStudentId is null)
                {
                    principal = new ClaimsPrincipal(new ClaimsIdentity());
                }
                else
                {
                    var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, testStudentId) });
                    principal = new ClaimsPrincipal(identity);
                }

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = principal }
                };

                // Act
                IActionResult actionResult = await controller.GetMyGrades(courseId).ConfigureAwait(false);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
                var ok = (OkObjectResult)actionResult;
                Assert.AreSame(mediatorResult, ok.Value);
                mockMediator.Verify(m => m.Send(It.Is<GetStudentAssignmentGradesQuery>(q => q.CourseId == courseId && q.StudentId == testStudentId), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        /// <summary>
        /// Verifies that GetSubmissions forwards the provided assignmentId to the mediator
        /// and returns an OkObjectResult whose Value is the exact enumerable returned by the mediator.
        /// Tests multiple integer edge values including int.MinValue, -1, 0, 1, and int.MaxValue.
        /// Expected: For each assignmentId the controller calls mediator.Send with a GetAssignmentSubmissionsQuery
        /// containing the same AssignmentId and returns Ok(result).
        /// </summary>
        [TestMethod]
        public async Task GetSubmissions_AssignmentId_VariousValues_ReturnsOkWithMediatorResult()
        {
            // Arrange & Act & Assert are done per-case to simulate parameterization without DataTestMethod.
            int[] testIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int assignmentId in testIds)
            {
                // Arrange
                var expected = new List<AssignmentSubmissionDto>();
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
                mediatorMock
                    .Setup(m => m.Send(It.IsAny<GetAssignmentSubmissionsQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expected);

                var controller = new AssignmentsController(mediatorMock.Object);

                // Act
                IActionResult actionResult = await controller.GetSubmissions(assignmentId);

                // Assert
                Assert.IsNotNull(actionResult, "Expected a non-null IActionResult.");
                var okResult = actionResult as OkObjectResult;
                Assert.IsNotNull(okResult, "Expected OkObjectResult.");
                // Should return the exact object returned by mediator
                Assert.AreSame(expected, okResult.Value, "Returned value should be the same instance returned by mediator.");

                mediatorMock.Verify(
                    m => m.Send(
                        It.Is<GetAssignmentSubmissionsQuery>(q => q != null && q.AssignmentId == assignmentId),
                        It.IsAny<CancellationToken>()),
                    Times.Once);

                // Cleanup mock invocations for next iteration by disposing mock (implicit).
            }
        }

        /// <summary>
        /// Verifies that if the mediator returns null the controller still returns Ok with a null Value.
        /// Input: assignmentId = 42, mediator returns null.
        /// Expected: OkObjectResult with Value == null.
        /// </summary>
        [TestMethod]
        public async Task GetSubmissions_WhenMediatorReturnsNull_ReturnsOkWithNull()
        {
            // Arrange
            int assignmentId = 42;
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAssignmentSubmissionsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<AssignmentSubmissionDto>?)null);

            var controller = new AssignmentsController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.GetSubmissions(assignmentId);

            // Assert
            Assert.IsNotNull(actionResult);
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNull(okResult.Value, "Expected returned Value to be null when mediator returns null.");

            mediatorMock.Verify(
                m => m.Send(
                    It.Is<GetAssignmentSubmissionsQuery>(q => q != null && q.AssignmentId == assignmentId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that if the mediator throws an exception the controller does not swallow it
        /// and the exception propagates to the caller.
        /// Input: mediator configured to throw InvalidOperationException.
        /// Expected: InvalidOperationException is thrown when calling GetSubmissions.
        /// </summary>
        [TestMethod]
        public async Task GetSubmissions_WhenMediatorThrowsException_PropagatesException()
        {
            // Arrange
            int assignmentId = 7;
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAssignmentSubmissionsQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("mediator failure"));

            var controller = new AssignmentsController(mediatorMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await controller.GetSubmissions(assignmentId);
            });

            mediatorMock.Verify(
                m => m.Send(
                    It.Is<GetAssignmentSubmissionsQuery>(q => q != null && q.AssignmentId == assignmentId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}