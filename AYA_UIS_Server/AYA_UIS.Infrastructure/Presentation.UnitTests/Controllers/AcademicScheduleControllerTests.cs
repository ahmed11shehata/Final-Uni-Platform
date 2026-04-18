using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.AcademicSchedules;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.AcademicSchedules;
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
using Shared.Dtos.Info_Module.AcademicSheduleDtos;

namespace Presentation.Controllers.UnitTests
{
    [TestClass]
    public class AcademicScheduleControllerTests
    {
        /// <summary>
        /// Test purpose:
        /// Verifies that GetAll returns an OkObjectResult containing the same empty list instance provided by the mediator.
        /// Input conditions:
        /// - IMediator.Send returns an empty List&lt;AcademicSchedulesDto&gt; (not null).
        /// Expected result:
        /// - The controller returns OkObjectResult whose Value is the same empty list instance and mediator Send was invoked exactly once.
        /// </summary>
        [TestMethod]
        public async Task GetAll_MediatorReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            List<AcademicSchedulesDto>? expected = new List<AcademicSchedulesDto>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAllAcademicSchedulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new AcademicScheduleController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.GetAll().ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(actionResult, "ActionResult should not be null.");
            var ok = actionResult as OkObjectResult;
            Assert.IsNotNull(ok, "Expected an OkObjectResult.");
            // OkObjectResult.StatusCode may be null in some ASP.NET Core versions, default is 200 for OkObjectResult.
            int status = ok.StatusCode ?? StatusCodes.Status200OK;
            Assert.AreEqual(StatusCodes.Status200OK, status, "Expected HTTP 200 OK status.");
            Assert.AreSame(expected, ok.Value, "Expected the controller to return the exact list instance from the mediator.");
            mediatorMock.Verify(m => m.Send(It.IsAny<GetAllAcademicSchedulesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            mediatorMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Test purpose:
        /// Verifies that GetAll returns an OkObjectResult whose Value is null when the mediator returns null.
        /// Input conditions:
        /// - IMediator.Send returns null for the List&lt;AcademicSchedulesDto&gt;.
        /// Expected result:
        /// - The controller returns OkObjectResult with a null Value and mediator Send was invoked exactly once.
        /// </summary>
        [TestMethod]
        public async Task GetAll_MediatorReturnsNull_ReturnsOkWithNullValue()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            List<AcademicSchedulesDto>? expected = null;
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAllAcademicSchedulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new AcademicScheduleController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.GetAll().ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(actionResult, "ActionResult should not be null even when mediator returns null.");
            var ok = actionResult as OkObjectResult;
            Assert.IsNotNull(ok, "Expected an OkObjectResult.");
            Assert.IsNull(ok.Value, "Expected the OkObjectResult.Value to be null when mediator returns null.");
            mediatorMock.Verify(m => m.Send(It.IsAny<GetAllAcademicSchedulesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            mediatorMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Tests that GetById forwards the provided id to the mediator by constructing
        /// GetAcademicScheduleByIdQuery and that the returned AcademicScheduleDto is
        /// returned inside an OkObjectResult. This test iterates multiple numeric edge
        /// cases for the id parameter: int.MinValue, -1, 0, 1 and int.MaxValue.
        /// Expected result: OkObjectResult containing the same AcademicScheduleDto instance
        /// returned by the mediator and mediator.Send invoked exactly once per call.
        /// </summary>
        [TestMethod]
        public async Task GetById_VariousNumericEdgeIds_ReturnsOkWithDtoAndInvokesMediator()
        {
            // Arrange & Act & Assert per id to keep each case isolated
            int[] testIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int id in testIds)
            {
                // Arrange
                var expectedDto = new AcademicScheduleDto
                {
                    // Id has init-only; initialize via object initializer
                    Id = id,
                    Title = $"Title-{id}",
                    Url = $"http://example/{id}",
                    Description = $"Desc-{id}",
                    CreatedAt = DateTime.UtcNow
                };

                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
                mediatorMock
                    .Setup(m => m.Send(It.Is<GetAcademicScheduleByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedDto)
                    .Verifiable();

                var controller = new AcademicScheduleController(mediatorMock.Object);

                // Act
                var actionResult = await controller.GetById(id);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
                var okResult = actionResult as OkObjectResult;
                Assert.IsNotNull(okResult, "OkObjectResult should not be null.");
                // Ensure the exact DTO instance returned by mediator is the value
                Assert.AreSame(expectedDto, okResult.Value, "Returned value should be the same instance provided by mediator.");

                mediatorMock.Verify(m => m.Send(It.Is<GetAcademicScheduleByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()), Times.Once());
                mediatorMock.VerifyNoOtherCalls();
            }
        }

        /// <summary>
        /// Tests that GetById handles a null response from the mediator gracefully by
        /// returning OkObjectResult with a null Value. Input: arbitrary valid int id (5).
        /// Expected result: OkObjectResult whose Value is null and mediator.Send invoked once.
        /// </summary>
        [TestMethod]
        public async Task GetById_MediatorReturnsNull_ReturnsOkWithNullValue()
        {
            // Arrange
            int id = 5;
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.Is<GetAcademicScheduleByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AcademicScheduleDto?)null)
                .Verifiable();

            var controller = new AcademicScheduleController(mediatorMock.Object);

            // Act
            var actionResult = await controller.GetById(id);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult, "OkObjectResult should not be null.");
            Assert.IsNull(okResult.Value, "Expected returned Value to be null when mediator returns null.");

            mediatorMock.Verify(m => m.Send(It.Is<GetAcademicScheduleByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()), Times.Once());
            mediatorMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Verifies that GetByTitle returns OkObjectResult containing the AcademicSchedulesDto produced by IMediator.
        /// Tests multiple title inputs including empty, whitespace, very long, special characters, and a normal title.
        /// Expected: For each valid (non-null) string title the controller invokes IMediator.Send with a GetAcademicScheduleByTitleQuery
        /// containing the same title and returns Ok(result) where result is the dto returned by mediator.
        /// </summary>
        [TestMethod]
        public async Task GetByTitle_ValidTitles_ReturnsOkWithDto()
        {
            // Arrange
            string veryLong = new string('A', 1000);
            string[] titles = new[]
            {
                "Mathematics",
                "",
                "   ",
                veryLong,
                "\0\0\n\t!@#$%^&*()"
            };

            foreach (string title in titles)
            {
                // Arrange for each case
                var expectedDto = new AcademicSchedulesDto
                {
                    Id = 1,
                    Title = title,
                    Url = "http://example.local",
                    Description = "desc"
                };

                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
                mediatorMock
                    .Setup(m => m.Send(It.Is<GetAcademicScheduleByTitleQuery>(q => q.ScheduleTitle == title), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedDto)
                    .Verifiable();

                var controller = new AcademicScheduleController(mediatorMock.Object);

                // Act
                IActionResult actionResult = await controller.GetByTitle(title).ConfigureAwait(false);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
                var okResult = (OkObjectResult)actionResult;
                Assert.AreSame(expectedDto, okResult.Value);
                mediatorMock.Verify(m => m.Send(It.Is<GetAcademicScheduleByTitleQuery>(q => q.ScheduleTitle == title), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        /// <summary>
        /// Verifies that when IMediator returns null the controller still responds with Ok(null).
        /// Input: title = "unknown-title"
        /// Expected: OkObjectResult with Value == null and Send was called once with the same title.
        /// </summary>
        [TestMethod]
        public async Task GetByTitle_MediatorReturnsNull_ReturnsOkWithNull()
        {
            // Arrange
            string title = "unknown-title";
            AcademicSchedulesDto? expectedDto = null;

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.Is<GetAcademicScheduleByTitleQuery>(q => q.ScheduleTitle == title), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto)
                .Verifiable();

            var controller = new AcademicScheduleController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.GetByTitle(title).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var okResult = (OkObjectResult)actionResult;
            Assert.IsNull(okResult.Value);
            mediatorMock.Verify(m => m.Send(It.Is<GetAcademicScheduleByTitleQuery>(q => q.ScheduleTitle == title), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that exceptions thrown by IMediator propagate from the controller action.
        /// Input: any valid title (e.g., "bad") and mediator configured to throw InvalidOperationException.
        /// Expected: the same exception bubbles up when calling GetByTitle.
        /// </summary>
        [TestMethod]
        public async Task GetByTitle_MediatorThrowsException_PropagatesException()
        {
            // Arrange
            string title = "bad";
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAcademicScheduleByTitleQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("mediator failure"))
                .Verifiable();

            var controller = new AcademicScheduleController(mediatorMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => controller.GetByTitle(title)).ConfigureAwait(false);
            mediatorMock.Verify(m => m.Send(It.Is<GetAcademicScheduleByTitleQuery>(q => q.ScheduleTitle == title), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that Update forwards the provided UpdateAcademicScheduleCommand to IMediator.Send
        /// and returns OkResult when the mediator completes successfully.
        /// Inputs tested: a variety of Title and Description values (empty, whitespace, long, special chars)
        /// and File being present or set to null after construction.
        /// Expected result: OkResult returned and mediator's Send invoked exactly once with the same command instance.
        /// </summary>
        [TestMethod]
        public async Task Update_CommandVariants_ReturnsOkAndSendsCommand()
        {
            // Arrange
            // Create mediator mock
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            // Prepare a set of diverse command instances to exercise string edge cases and null File property
            var longString = new string('x', 1024);
            var specialString = "special!\n\r\t\u2603";
            var whitespace = "   ";
            var testCases = new List<UpdateAcademicScheduleCommand>();

            // Helper to create a mock IFormFile
            IFormFile CreateMockFile()
            {
                var fileMock = new Mock<IFormFile>();
                // Minimal setup: Length and FileName are commonly used properties; not required by controller but safe
                fileMock.SetupGet(f => f.Length).Returns(123);
                fileMock.SetupGet(f => f.FileName).Returns("file.bin");
                return fileMock.Object;
            }

            // Cases:
            // 1) Empty title/description, valid file
            testCases.Add(new UpdateAcademicScheduleCommand(string.Empty, string.Empty, CreateMockFile()));
            // 2) Whitespace title, special chars description, valid file
            testCases.Add(new UpdateAcademicScheduleCommand(whitespace, specialString, CreateMockFile()));
            // 3) Long title/description, valid file
            testCases.Add(new UpdateAcademicScheduleCommand(longString, longString, CreateMockFile()));
            // 4) Normal title/description, file provided then nulled out via property (File is nullable property)
            var cmdWithNullFile = new UpdateAcademicScheduleCommand("Normal", "Desc", CreateMockFile());
            cmdWithNullFile.File = null;
            testCases.Add(cmdWithNullFile);

            foreach (var command in testCases)
            {
                // For each command, arrange mediator to accept that exact command instance
                mediatorMock
                    .Setup(m => m.Send<Unit>(It.Is<UpdateAcademicScheduleCommand>(c => ReferenceEquals(c, command)), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Unit.Value)
                    .Verifiable();

                var controller = new AcademicScheduleController(mediatorMock.Object);

                // Act
                var result = await controller.Update(command);

                // Assert
                Assert.IsInstanceOfType(result, typeof(OkResult), "Update should return OkResult for successful mediator call.");
                mediatorMock.Verify(m => m.Send<Unit>(It.Is<UpdateAcademicScheduleCommand>(c => ReferenceEquals(c, command)), It.IsAny<CancellationToken>()), Times.Once, "Mediator.Send should be called exactly once with the provided command instance.");

                // Reset setups/verifications for next iteration
                mediatorMock.Reset();
            }
        }

        /// <summary>
        /// Verifies that if IMediator.Send throws an exception, Update does not swallow it and the exception propagates.
        /// Input: a valid UpdateAcademicScheduleCommand instance.
        /// Expected result: the same exception type and message thrown by IMediator.Send is observed by the caller.
        /// </summary>
        [TestMethod]
        public async Task Update_MediatorThrows_ExceptionPropagated()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            var fileMock = new Mock<IFormFile>();
            fileMock.SetupGet(f => f.Length).Returns(10L);
            var command = new UpdateAcademicScheduleCommand("T", "D", fileMock.Object);

            var expectedEx = new InvalidOperationException("mediator-failure");
            mediatorMock
                .Setup(m => m.Send<Unit>(It.IsAny<UpdateAcademicScheduleCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedEx);

            var controller = new AcademicScheduleController(mediatorMock.Object);

            // Act & Assert
            var thrown = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await controller.Update(command));
            Assert.AreEqual(expectedEx.Message, thrown.Message, "The propagated exception should carry the original message from the mediator.");
            mediatorMock.Verify(m => m.Send<Unit>(It.Is<UpdateAcademicScheduleCommand>(c => ReferenceEquals(c, command)), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that Delete returns OkObjectResult containing 'true' when the mediator returns true.
        /// Input conditions: DeleteAcademicScheduleByIdCommand with Id = int.MaxValue. Mediator mocked to return true.
        /// Expected result: IActionResult is OkObjectResult and its Value equals true; mediator.Send invoked once with the provided command.
        /// </summary>
        [TestMethod]
        public async Task Delete_WhenMediatorReturnsTrue_ReturnsOkWithTrue()
        {
            // Arrange
            int id = int.MaxValue;
            var command = new DeleteAcademicScheduleByIdCommand(id);

            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            mockMediator
                .Setup(m => m.Send(It.Is<DeleteAcademicScheduleByIdCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            var controller = new AcademicScheduleController(mockMediator.Object);

            // Act
            IActionResult actionResult = await controller.Delete(command);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult when mediator returns true.");
            var okResult = (OkObjectResult)actionResult;
            Assert.IsNotNull(okResult.Value, "OkObjectResult.Value should not be null.");
            Assert.IsInstanceOfType(okResult.Value, typeof(bool), "OkObjectResult.Value should be a boolean.");
            Assert.AreEqual(true, (bool)okResult.Value, "Returned boolean should match mediator result (true).");

            mockMediator.Verify(m => m.Send(It.Is<DeleteAcademicScheduleByIdCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that Delete returns OkObjectResult containing 'false' when the mediator returns false.
        /// Input conditions: DeleteAcademicScheduleByIdCommand with Id = int.MinValue. Mediator mocked to return false.
        /// Expected result: IActionResult is OkObjectResult and its Value equals false; mediator.Send invoked once with the provided command.
        /// </summary>
        [TestMethod]
        public async Task Delete_WhenMediatorReturnsFalse_ReturnsOkWithFalse()
        {
            // Arrange
            int id = int.MinValue;
            var command = new DeleteAcademicScheduleByIdCommand(id);

            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            mockMediator
                .Setup(m => m.Send(It.Is<DeleteAcademicScheduleByIdCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .Verifiable();

            var controller = new AcademicScheduleController(mockMediator.Object);

            // Act
            IActionResult actionResult = await controller.Delete(command);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult when mediator returns false.");
            var okResult = (OkObjectResult)actionResult;
            Assert.IsNotNull(okResult.Value, "OkObjectResult.Value should not be null even when false.");
            Assert.IsInstanceOfType(okResult.Value, typeof(bool), "OkObjectResult.Value should be a boolean.");
            Assert.AreEqual(false, (bool)okResult.Value, "Returned boolean should match mediator result (false).");

            mockMediator.Verify(m => m.Send(It.Is<DeleteAcademicScheduleByIdCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that for a variety of semesterId values the controller calls IMediator with a GetAcademicSchedulesBySemesterIdQuery
        /// containing the same semesterId and returns an OkObjectResult with the mediator's result.
        /// Input conditions: tests multiple semesterId values including int.MinValue, negative, zero, positive, int.MaxValue.
        /// Expected result: OkObjectResult whose Value is the same instance returned by IMediator and IMediator.Send was invoked with a query having the same SemesterId.
        /// </summary>
        [TestMethod]
        public async Task GetSemesterAcademicSchedules_VariousSemesterIds_ReturnsOkAndMediatorReceivedId()
        {
            // Arrange
            int[] testIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int id in testIds)
            {
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

                var returnedList = new List<AcademicScheduleDto> { new AcademicScheduleDto() };

                mediatorMock
                    .Setup(m => m.Send(It.IsAny<GetAcademicSchedulesBySemesterIdQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(returnedList)
                    .Verifiable();

                var controller = new AcademicScheduleController(mediatorMock.Object);

                // Act
                IActionResult actionResult = await controller.GetSemesterAcademicSchedules(id).ConfigureAwait(false);

                // Assert
                Assert.IsNotNull(actionResult, "Expected a non-null IActionResult.");
                var okResult = actionResult as OkObjectResult;
                Assert.IsNotNull(okResult, "Expected OkObjectResult for successful mediator response.");
                Assert.AreSame(returnedList, okResult.Value, "Controller should return the exact value provided by IMediator.");

                mediatorMock.Verify(m =>
                    m.Send(It.Is<GetAcademicSchedulesBySemesterIdQuery>(q => q != null && q.SemesterId == id), It.IsAny<CancellationToken>()),
                    Times.Once, $"IMediator.Send should be called once with SemesterId = {id}.");
            }
        }

        /// <summary>
        /// Verifies that when IMediator returns an empty collection the controller returns Ok with an empty enumerable.
        /// Input conditions: mediator returns an empty IEnumerable&lt;AcademicScheduleDto&gt;.
        /// Expected result: OkObjectResult whose Value is an empty IEnumerable&lt;AcademicScheduleDto&gt;.
        /// </summary>
        [TestMethod]
        public async Task GetSemesterAcademicSchedules_MediatorReturnsEmptyCollection_ReturnsOkWithEmptyCollection()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            var empty = new List<AcademicScheduleDto>();

            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAcademicSchedulesBySemesterIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<AcademicScheduleDto>)empty)
                .Verifiable();

            var controller = new AcademicScheduleController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.GetSemesterAcademicSchedules(42).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(actionResult, "Expected a non-null IActionResult.");
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult, "Expected OkObjectResult for successful mediator response.");
            Assert.IsInstanceOfType(okResult.Value, typeof(IEnumerable<AcademicScheduleDto>), "Returned value should be an IEnumerable<AcademicScheduleDto>.");

            var returnedEnumerable = okResult.Value as IEnumerable<AcademicScheduleDto>;
            Assert.IsNotNull(returnedEnumerable, "Returned enumerable should not be null.");
            CollectionAssert.AreEqual(new List<AcademicScheduleDto>(empty), new List<AcademicScheduleDto>(returnedEnumerable), "Expected an empty collection.");

            mediatorMock.Verify(m =>
                m.Send(It.IsAny<GetAcademicSchedulesBySemesterIdQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that if IMediator.Send throws an exception the controller method propagates the exception.
        /// Input conditions: IMediator.Send throws InvalidOperationException.
        /// Expected result: the exception is propagated to the caller.
        /// </summary>
        [TestMethod]
        public async Task GetSemesterAcademicSchedules_MediatorThrows_ExceptionPropagates()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAcademicSchedulesBySemesterIdQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("mediator failure"))
                .Verifiable();

            var controller = new AcademicScheduleController(mediatorMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await controller.GetSemesterAcademicSchedules(7).ConfigureAwait(false);
            }).ConfigureAwait(false);

            mediatorMock.Verify(m =>
                m.Send(It.IsAny<GetAcademicSchedulesBySemesterIdQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Test that when the current user has no NameIdentifier claim or an empty NameIdentifier,
        /// the Create method returns Unauthorized and does not call IMediator.Send.
        /// Inputs tested: missing claim and empty claim value.
        /// Expected result: UnauthorizedResult and mediator Send is never invoked.
        /// </summary>
        [TestMethod]
        public async Task Create_MissingOrEmptyUserId_ReturnsUnauthorizedAndDoesNotCallMediator()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            var controller = new AcademicScheduleController(mediatorMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var dto = new CreateSemesterAcademicScheduleDto
            {
                Title = "title",
                File = new Mock<IFormFile>().Object,
                Description = null
            };

            // Act & Assert - Case 1: Missing NameIdentifier claim (no claims)
            var resultMissing = await controller.Create(1, 1, 1, dto);
            Assert.IsInstanceOfType(resultMissing, typeof(UnauthorizedResult), "Expected Unauthorized when NameIdentifier claim is absent.");
            mediatorMock.Verify(m => m.Send(It.IsAny<CreateSemesterAcademicScheduleCommand>(), It.IsAny<CancellationToken>()), Times.Never);

            // Act & Assert - Case 2: Present but empty NameIdentifier claim
            var emptyIdPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, string.Empty) }, "test"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = emptyIdPrincipal }
            };

            var resultEmpty = await controller.Create(1, 1, 1, dto);
            Assert.IsInstanceOfType(resultEmpty, typeof(UnauthorizedResult), "Expected Unauthorized when NameIdentifier claim is empty.");
            mediatorMock.Verify(m => m.Send(It.IsAny<CreateSemesterAcademicScheduleCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Test that when a valid user id is present, Create constructs and sends CreateSemesterAcademicScheduleCommand
        /// with the correct UploadedByUserId, StudyYearId, DepartmentId, SemesterId, and DTO reference,
        /// and that the controller returns Ok.
        /// Inputs tested (numeric boundaries): int.MinValue, -1, 0, 1, int.MaxValue for each numeric parameter via multiple iterations.
        /// Expected result: IMediator.Send invoked once per call with a command containing matching values and OkResult returned.
        /// </summary>
        [TestMethod]
        public async Task Create_ValidUser_CallsMediatorWithExpectedCommandAndReturnsOk_ForNumericBoundaries()
        {
            // Arrange - numeric test vectors to exercise boundaries and special values
            var numericValues = new int[] { int.MinValue, -1, 0, 1, int.MaxValue };
            var userId = "user-123";

            foreach (var studyYearId in numericValues)
            {
                foreach (var departmentId in numericValues)
                {
                    foreach (var semesterId in numericValues)
                    {
                        var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

                        CreateSemesterAcademicScheduleCommand? capturedCommand = null;

                        mediatorMock
                            .Setup(m => m.Send(It.IsAny<CreateSemesterAcademicScheduleCommand>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(Unit.Value)
                            .Callback<CreateSemesterAcademicScheduleCommand, CancellationToken>((cmd, ct) => capturedCommand = cmd);

                        var controller = new AcademicScheduleController(mediatorMock.Object);

                        // Set ClaimsPrincipal with a valid NameIdentifier
                        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "test"));
                        controller.ControllerContext = new ControllerContext
                        {
                            HttpContext = new DefaultHttpContext { User = principal }
                        };

                        var fileMock = new Mock<IFormFile>();
                        var dto = new CreateSemesterAcademicScheduleDto
                        {
                            Title = "Title",
                            File = fileMock.Object,
                            Description = "Desc"
                        };

                        // Act
                        var actionResult = await controller.Create(studyYearId, departmentId, semesterId, dto);

                        // Assert - response is OK
                        Assert.IsInstanceOfType(actionResult, typeof(OkResult), "Expected OkResult when a valid user id is present.");

                        // Assert - mediator send was called once and the command contains expected values
                        mediatorMock.Verify(m => m.Send(It.IsAny<CreateSemesterAcademicScheduleCommand>(), It.IsAny<CancellationToken>()), Times.Once);

                        Assert.IsNotNull(capturedCommand, "Expected command to be captured by mediator setup callback.");
                        Assert.AreEqual(userId, capturedCommand!.UploadedByUserId, "UploadedByUserId must match the current user id.");
                        Assert.AreEqual(studyYearId, capturedCommand.StudyYearId, "StudyYearId must match the input value.");
                        Assert.AreEqual(departmentId, capturedCommand.DepartmentId, "DepartmentId must match the input value.");
                        Assert.AreEqual(semesterId, capturedCommand.SemesterId, "SemesterId must match the input value.");
                        // DTO instance should be the same reference passed in
                        Assert.AreSame(dto, capturedCommand.CreateAcademicScheduleDto, "DTO reference passed to command should be identical to the input DTO.");
                    }
                }
            }
        }

        /// <summary>
        /// Tests that the AcademicScheduleController constructor creates an instance
        /// when provided a valid IMediator. Ensures no exception is thrown and the
        /// resulting object inherits from ControllerBase.
        /// Arrange: A Mock&lt;IMediator&gt; is provided.
        /// Act: The constructor is invoked.
        /// Assert: The created instance is not null and is a ControllerBase.
        /// </summary>
        [TestMethod]
        public void AcademicScheduleController_Constructor_ValidMediator_InstanceCreated()
        {
            // Arrange
            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);

            // Act
            AcademicScheduleController? controller = null;
            Exception? caught = null;
            try
            {
                controller = new AcademicScheduleController(mockMediator.Object);
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            // Assert
            Assert.IsNull(caught, "Constructor should not throw for a valid IMediator instance.");
            Assert.IsNotNull(controller, "Controller instance should be created.");
            Assert.IsInstanceOfType(controller, typeof(ControllerBase), "Controller should derive from ControllerBase.");
        }

        /// <summary>
        /// The source constructor does not perform null-checking on the IMediator parameter.
        /// Because the parameter is annotated as non-nullable, passing null may indicate a
        /// misuse of the API. This test is marked inconclusive to document the undefined
        /// behavior and to avoid assigning null to a non-nullable parameter in generated tests.
        /// If you wish to assert explicit behavior for null mediator (for example, that the
        /// constructor should throw ArgumentNullException), update the production code or
        /// provide guidance and remove the Inconclusive call.
        /// Arrange: mediator is null (conceptually).
        /// Act & Assert: This test is inconclusive because nullability and lack of guard make
        /// the expected behavior ambiguous in the current implementation.
        /// </summary>
        [TestMethod]
        public void AcademicScheduleController_Constructor_NullMediator_Inconclusive()
        {
            // This test intentionally does not pass null to a non-nullable parameter.
            // Instead, document the ambiguity and mark the test as inconclusive per guidance.
            Assert.Inconclusive("Constructor parameter 'mediator' is non-nullable and the constructor does not guard against null. Decide expected behavior (throw or allow) and update the test accordingly.");
        }
    }
}