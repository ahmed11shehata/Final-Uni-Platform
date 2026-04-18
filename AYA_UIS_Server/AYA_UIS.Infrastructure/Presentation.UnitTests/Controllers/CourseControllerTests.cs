using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Courses;
using AYA_UIS.Application.Commands.CourseUploads;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.CoursePrequisites;
using AYA_UIS.Application.Queries.Courses;
using AYA_UIS.Application.Queries.Dashboard;
using AYA_UIS.Core.Domain;
using AYA_UIS.Core.Domain.Enums;
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
using Shared.Dtos.Info_Module.CourseDtos;
using Shared.Dtos.Info_Module.CourseUploadDtos;
using Shared.Respones;

namespace Presentation.Controllers.UnitTests
{
    [TestClass]
    public class CourseControllerTests
    {
        /// <summary>
        /// Verifies that GetCourseUploads forwards the mediator result as the Ok(...) payload
        /// for several representative courseId numeric values (including boundaries).
        /// Inputs: courseId values tested: int.MinValue, -1, 0, 1, int.MaxValue.
        /// Expected: The controller returns OkObjectResult whose Value is the exact instance returned by IMediator.Send.
        /// Also verifies IMediator.Send is invoked exactly once per call.
        /// </summary>
        [TestMethod]
        public async Task GetCourseUploads_VariousCourseIds_ReturnsOkWithMediatorResult()
        {
            // Arrange
            int[] courseIdsToTest = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int courseId in courseIdsToTest)
            {
                // Create a fresh mock per iteration to isolate invocation counts.
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

                // Use an object instance to be returned; an array instance is sufficient to assert identity.
                IEnumerable<CourseUploadDto> mediatorReturn = Array.Empty<CourseUploadDto>();

                mediatorMock
                    .Setup(m => m.Send(It.IsAny<GetCourseUploadsQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(mediatorReturn)
                    .Verifiable();

                var controller = new CourseController(mediatorMock.Object);

                // Act
                IActionResult actionResult = await controller.GetCourseUploads(courseId);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult for valid mediator response.");
                var ok = (OkObjectResult)actionResult;
                Assert.AreSame(mediatorReturn, ok.Value, "Controller should return the exact instance provided by mediator.");
                mediatorMock.Verify(m => m.Send(It.IsAny<GetCourseUploadsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        /// <summary>
        /// Ensures that when IMediator.Send returns an empty collection, the controller returns Ok with the same empty collection.
        /// Input: an explicit empty IEnumerable<CourseUploadDto>.
        /// Expected: OkObjectResult with the same reference returned by mediator.
        /// </summary>
        [TestMethod]
        public async Task GetCourseUploads_MediatorReturnsEmptyCollection_ReturnsOkWithEmpty()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            IEnumerable<CourseUploadDto> empty = Array.Empty<CourseUploadDto>();

            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetCourseUploadsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(empty)
                .Verifiable();

            var controller = new CourseController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.GetCourseUploads(123);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var ok = (OkObjectResult)actionResult;
            Assert.AreSame(empty, ok.Value);
            mediatorMock.Verify(m => m.Send(It.IsAny<GetCourseUploadsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Ensures that when IMediator.Send returns null, the controller still responds with Ok and a null payload.
        /// Input: mediator returns null for course uploads.
        /// Expected: OkObjectResult whose Value is null.
        /// </summary>
        [TestMethod]
        public async Task GetCourseUploads_MediatorReturnsNull_ReturnsOkWithNull()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetCourseUploadsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<CourseUploadDto>?)null)
                .Verifiable();

            var controller = new CourseController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.GetCourseUploads(42);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var ok = (OkObjectResult)actionResult;
            Assert.IsNull(ok.Value, "Expected null payload when mediator returns null.");
            mediatorMock.Verify(m => m.Send(It.IsAny<GetCourseUploadsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Test purpose:
        /// Verify that when a valid OpenCoursesForLevelDto is provided the controller sends an OpenCoursesForLevelCommand
        /// through IMediator exactly once and returns an OkObjectResult with the expected success message.
        /// Input conditions:
        /// - dto contains typical and boundary-like values (large integers, empty and duplicate course ids).
        /// Expected result:
        /// - IMediator.Send is invoked once with a command wrapping the same dto instance.
        /// - The returned IActionResult is OkObjectResult whose Value equals "Courses opened successfully.".
        /// </summary>
        [TestMethod]
        public async Task OpenCoursesForLevel_ValidDto_CallsMediatorAndReturnsOk()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            var dto = new OpenCoursesForLevelDto
            {
                StudyYearId = int.MaxValue,
                SemesterId = int.MinValue,
                Level = default(Levels),
                CourseIds = new List<int> { 1, 1, 2, int.MaxValue }
            };

            mediatorMock
                .Setup(m => m.Send(
                    It.Is<OpenCoursesForLevelCommand>(c => ReferenceEquals(c.Dto, dto)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value)
                .Verifiable();

            var controller = new CourseController(mediatorMock.Object);

            // Act
            var result = await controller.OpenCoursesForLevel(dto);

            // Assert
            mediatorMock.Verify(m => m.Send(
                It.Is<OpenCoursesForLevelCommand>(c => ReferenceEquals(c.Dto, dto)),
                It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual("Courses opened successfully.", ok.Value);
        }

        /// <summary>
        /// Test purpose:
        /// Ensure that if IMediator.Send throws an exception the controller method does not swallow it
        /// but instead propagates it to the caller.
        /// Input conditions:
        /// - mediator is configured to throw InvalidOperationException when Send is called.
        /// Expected result:
        /// - OpenCoursesForLevel throws the same InvalidOperationException.
        /// </summary>
        [TestMethod]
        public async Task OpenCoursesForLevel_MediatorThrows_PropagatesException()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            var dto = new OpenCoursesForLevelDto
            {
                StudyYearId = 0,
                SemesterId = 0,
                Level = default(Levels),
                CourseIds = new List<int>()
            };

            mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<OpenCoursesForLevelCommand>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("mediator failure"));

            var controller = new CourseController(mediatorMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await controller.OpenCoursesForLevel(dto);
            });

            mediatorMock.Verify(m => m.Send(It.IsAny<OpenCoursesForLevelCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that constructing CourseController with a non-null IMediator instance
        /// successfully creates an instance of CourseController without throwing.
        /// Input: a Moq.Mock of IMediator (non-null).
        /// Expected: constructor returns a non-null CourseController instance.
        /// </summary>
        [TestMethod]
        public void CourseController_WithValidMediator_InstanceCreatedAndNotNull()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            // Act
            CourseController controller = null!;
            Exception? ex = null;
            try
            {
                controller = new CourseController(mediatorMock.Object);
            }
            catch (Exception e)
            {
                ex = e;
            }

            // Assert
            Assert.IsNull(ex, "Constructor threw an unexpected exception.");
            Assert.IsNotNull(controller, "Controller instance should not be null when provided a valid IMediator.");
            Assert.IsInstanceOfType(controller, typeof(CourseController));
        }

        /// <summary>
        /// Verifies that constructing CourseController multiple times with the same IMediator
        /// instance yields independent CourseController objects (no shared-reference equality).
        /// Input: a Moq.Mock of IMediator (non-null) used for two constructions.
        /// Expected: two distinct CourseController instances are created and neither is null.
        /// This ensures the constructor does not return a singleton or reuse instances.
        /// </summary>
        [TestMethod]
        public void CourseController_WithSameMediator_CreatesDistinctInstances()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Loose);

            // Act
            var controller1 = new CourseController(mediatorMock.Object);
            var controller2 = new CourseController(mediatorMock.Object);

            // Assert
            Assert.IsNotNull(controller1, "First controller instance should not be null.");
            Assert.IsNotNull(controller2, "Second controller instance should not be null.");
            Assert.AreNotSame(controller1, controller2, "Constructor should create distinct CourseController instances for separate calls.");
        }

        /// <summary>
        /// Tests that GetCourseDependencies returns OkObjectResult containing the exact list
        /// returned by IMediator.Send and that the mediator is invoked with a query containing
        /// the provided courseId. This test iterates several integer edge values to ensure
        /// the controller forwards the courseId unchanged.
        /// Inputs tested: int.MinValue, -1, 0, 1, int.MaxValue.
        /// Expected: OkObjectResult with the same List{CourseDto} instance returned by mediator,
        /// and Send called once with a GetCourseDependenciesQuery whose CourseId matches input.
        /// </summary>
        [TestMethod]
        public async Task GetCourseDependencies_VariousCourseIds_ReturnsOkWithMediatorResult()
        {
            // Arrange & Act & Assert for a set of edge courseId values
            int[] courseIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int courseId in courseIds)
            {
                // Arrange
                var mediatorMock = new Mock<IMediator>();
                List<CourseDto> expected = new List<CourseDto>(); // empty list is sufficient for reference equality checks

                mediatorMock
                    .Setup(m => m.Send(It.Is<GetCourseDependenciesQuery>(q => q.CourseId == courseId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expected);

                var controller = new CourseController(mediatorMock.Object);

                // Act
                IActionResult actionResult = await controller.GetCourseDependencies(courseId);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), $"Expected OkObjectResult for courseId {courseId}.");
                var okResult = (OkObjectResult)actionResult;
                Assert.AreSame(expected, okResult.Value, "Controller should return the exact object provided by mediator.");
                mediatorMock.Verify(m => m.Send(It.Is<GetCourseDependenciesQuery>(q => q.CourseId == courseId), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        /// <summary>
        /// Tests that GetCourseDependencies returns OkObjectResult with null Value when mediator returns null.
        /// Input: mediator returns null for a sample courseId.
        /// Expected: OkObjectResult with Value == null (no exception thrown).
        /// </summary>
        [TestMethod]
        public async Task GetCourseDependencies_MediatorReturnsNull_ReturnsOkWithNullValue()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetCourseDependenciesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<CourseDto>?)null);

            var controller = new CourseController(mediatorMock.Object);
            int sampleCourseId = 123;

            // Act
            IActionResult actionResult = await controller.GetCourseDependencies(sampleCourseId);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var okResult = (OkObjectResult)actionResult;
            Assert.IsNull(okResult.Value, "When mediator returns null, controller should return Ok with null value.");
            mediatorMock.Verify(m => m.Send(It.Is<GetCourseDependenciesQuery>(q => q.CourseId == sampleCourseId), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that exceptions thrown by IMediator.Send propagate from GetCourseDependencies.
        /// Input: mediator throws InvalidOperationException when called.
        /// Expected: the same exception type is propagated to the caller.
        /// </summary>
        [TestMethod]
        public async Task GetCourseDependencies_MediatorThrows_ExceptionPropagates()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetCourseDependenciesQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("mediator failure"));

            var controller = new CourseController(mediatorMock.Object);
            int courseId = 5;

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await controller.GetCourseDependencies(courseId));
            mediatorMock.Verify(m => m.Send(It.Is<GetCourseDependenciesQuery>(q => q.CourseId == courseId), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that DepartmentCourses forwards the correct GetDepartmentCoursesQuery to IMediator and
        /// returns an OkObjectResult containing the same IEnumerable&lt;CourseDto&gt; instance provided by the mediator.
        /// Tests a set of numeric edge values for departmentId (int.MinValue, -1, 0, 1, int.MaxValue).
        /// Expected: OkObjectResult with the identical IEnumerable&lt;CourseDto&gt; and mediator.Send invoked once with matching DepartmentId.
        /// </summary>
        [TestMethod]
        public async Task DepartmentCourses_MultipleDepartmentIds_ReturnsOkAndForwardsQuery()
        {
            // Arrange
            var departmentIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int deptId in departmentIds)
            {
                var mockMediator = new Mock<IMediator>(MockBehavior.Strict);

                var expectedList = new List<CourseDto> { new CourseDto() };

                mockMediator
                    .Setup(m => m.Send(It.Is<GetDepartmentCoursesQuery>(q => q.DepartmentId == deptId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<CourseDto>)expectedList)
                    .Verifiable();

                var controller = new CourseController(mockMediator.Object);

                // Act
                IActionResult actionResult = await controller.DepartmentCourses(deptId);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult for valid mediator response.");
                var ok = (OkObjectResult)actionResult;
                Assert.AreSame(expectedList, ok.Value, "Controller should return the exact reference returned by mediator.");

                mockMediator.Verify(m => m.Send(It.Is<GetDepartmentCoursesQuery>(q => q.DepartmentId == deptId), It.IsAny<CancellationToken>()), Times.Once);

                // Cleanup verifications for next iteration
                mockMediator.VerifyNoOtherCalls();
            }
        }

        /// <summary>
        /// Verifies that DepartmentCourses returns OkObjectResult with an empty collection when mediator returns an empty list.
        /// Input: departmentId = 5 (representative positive id). Expected: OkObjectResult with an empty IEnumerable&lt;CourseDto&gt;.
        /// </summary>
        [TestMethod]
        public async Task DepartmentCourses_MediatorReturnsEmptyList_ReturnsOkWithEmpty()
        {
            // Arrange
            int departmentId = 5;
            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            var emptyList = new List<CourseDto>();

            mockMediator
                .Setup(m => m.Send(It.Is<GetDepartmentCoursesQuery>(q => q.DepartmentId == departmentId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<CourseDto>)emptyList)
                .Verifiable();

            var controller = new CourseController(mockMediator.Object);

            // Act
            IActionResult actionResult = await controller.DepartmentCourses(departmentId);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult even when mediator returns empty list.");
            var ok = (OkObjectResult)actionResult;
            Assert.IsNotNull(ok.Value, "Value should be an (empty) collection, not null.");
            Assert.IsInstanceOfType(ok.Value, typeof(IEnumerable<CourseDto>));
            var returned = (IEnumerable<CourseDto>)ok.Value;
            CollectionAssert.AreEqual(new List<CourseDto>(emptyList), new List<CourseDto>(returned), "Returned collection should be empty and equal to expected.");

            mockMediator.Verify(m => m.Send(It.Is<GetDepartmentCoursesQuery>(q => q.DepartmentId == departmentId), It.IsAny<CancellationToken>()), Times.Once);
            mockMediator.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Verifies that DepartmentCourses returns OkObjectResult with null value when mediator returns null.
        /// Input: departmentId = 10. Expected: OkObjectResult whose Value is null.
        /// This checks controller's behavior when the query handler returns null (no special handling expected).
        /// </summary>
        [TestMethod]
        public async Task DepartmentCourses_MediatorReturnsNull_ReturnsOkWithNull()
        {
            // Arrange
            int departmentId = 10;
            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);

            mockMediator
                .Setup(m => m.Send(It.Is<GetDepartmentCoursesQuery>(q => q.DepartmentId == departmentId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<CourseDto>?)null)
                .Verifiable();

            var controller = new CourseController(mockMediator.Object);

            // Act
            IActionResult actionResult = await controller.DepartmentCourses(departmentId);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var ok = (OkObjectResult)actionResult;
            Assert.IsNull(ok.Value, "When mediator returns null, controller should return Ok with null value.");

            mockMediator.Verify(m => m.Send(It.Is<GetDepartmentCoursesQuery>(q => q.DepartmentId == departmentId), It.IsAny<CancellationToken>()), Times.Once);
            mockMediator.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Test purpose:
        /// Verify that when a valid CreateCourseDto is provided and the mediator returns a successful Response&lt;CourseDto&gt;,
        /// the controller returns an OkObjectResult containing the same Response instance.
        /// Input conditions:
        /// - A populated CreateCourseDto instance.
        /// - Mediator returns a non-null Response&lt;CourseDto&gt; via Send.
        /// Expected result:
        /// - IActionResult is OkObjectResult and its Value is the exact Response instance returned by the mediator.
        /// </summary>
        [TestMethod]
        public async Task Add_ValidDto_ReturnsOkWithResponse()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var dto = new CreateCourseDto
            {
                Code = "C101",
                Name = "Intro to Testing",
                Credits = 3,
                DepartmentId = 1,
                PrerequisiteCourseCodes = new List<string> { "PRE1" }
            };

            var returnedCourse = new CourseDto
            {
                Id = 123,
                Code = "C101",
                Name = "Intro to Testing",
                Credits = 3,
                Status = CourseStatus.Open
            };

            var expectedResponse = Response<CourseDto>.SuccessResponse(returnedCourse);

            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateCourseCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var controller = new CourseController(mediatorMock.Object);

            // Act
            var actionResult = await controller.Add(dto).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult when mediator returns a response.");
            var ok = actionResult as OkObjectResult;
            Assert.IsNotNull(ok, "OkObjectResult should not be null.");
            Assert.AreSame(expectedResponse, ok.Value, "Controller should return the exact Response instance provided by the mediator.");
            mediatorMock.Verify(m => m.Send(It.IsAny<CreateCourseCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Test purpose:
        /// Verify that when the mediator returns null (unexpected), the controller still returns OkObjectResult with a null Value.
        /// Input conditions:
        /// - A populated CreateCourseDto instance.
        /// - Mediator returns null for Send.
        /// Expected result:
        /// - IActionResult is OkObjectResult and its Value is null.
        /// </summary>
        [TestMethod]
        public async Task Add_MediatorReturnsNull_ReturnsOkWithNullValue()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var dto = new CreateCourseDto
            {
                Code = "C200",
                Name = "Edge Case Course",
                Credits = 0,
                DepartmentId = int.MinValue,
                PrerequisiteCourseCodes = new List<string>()
            };

            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateCourseCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Response<CourseDto>?)null!);

            var controller = new CourseController(mediatorMock.Object);

            // Act
            var actionResult = await controller.Add(dto).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult even when mediator returns null.");
            var ok = actionResult as OkObjectResult;
            Assert.IsNotNull(ok, "OkObjectResult should not be null.");
            Assert.IsNull(ok.Value, "The OkObjectResult Value should be null when mediator returns null.");
            mediatorMock.Verify(m => m.Send(It.IsAny<CreateCourseCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Test purpose:
        /// Verify that exceptions thrown by the mediator propagate from the controller Add method.
        /// Input conditions:
        /// - A populated CreateCourseDto instance.
        /// - Mediator.Send throws an InvalidOperationException.
        /// Expected result:
        /// - The controller Add method throws the same InvalidOperationException.
        /// </summary>
        [TestMethod]
        public async Task Add_MediatorThrows_ExceptionIsPropagated()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var dto = new CreateCourseDto
            {
                Code = "C500",
                Name = "Failing Course",
                Credits = int.MaxValue,
                DepartmentId = int.MaxValue,
                PrerequisiteCourseCodes = new List<string> { "X", "Y" }
            };

            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateCourseCommand>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromException<Response<CourseDto>>(new InvalidOperationException("mediator failure")));

            var controller = new CourseController(mediatorMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await controller.Add(dto).ConfigureAwait(false));
            mediatorMock.Verify(m => m.Send(It.IsAny<CreateCourseCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetAll returns OkObjectResult with the same enumerable instance returned by the mediator.
        /// Condition: mediator returns a non-empty list of FrontendCourseDto.
        /// Expected: OkObjectResult returned and its Value is the identical enumerable instance.
        /// </summary>
        [TestMethod]
        public async Task GetAll_WhenMediatorReturnsList_ReturnsOkWithSameInstance()
        {
            // Arrange
            var expected = new List<FrontendCourseDto> { new FrontendCourseDto() };
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAllCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new CourseController(mediatorMock.Object);

            // Act
            var actionResult = await controller.GetAll();

            // Assert
            Assert.IsNotNull(actionResult);
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult, "Expected OkObjectResult");
            var actual = okResult!.Value as IEnumerable<FrontendCourseDto>;
            Assert.AreSame(expected, actual, "Controller should return the exact enumerable instance from mediator");
            mediatorMock.Verify(m => m.Send(It.IsAny<GetAllCoursesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetAll returns OkObjectResult when mediator returns an empty collection.
        /// Condition: mediator returns an empty list.
        /// Expected: OkObjectResult returned and Value is the same empty list instance.
        /// </summary>
        [TestMethod]
        public async Task GetAll_WhenMediatorReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var expected = new List<FrontendCourseDto>();
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAllCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new CourseController(mediatorMock.Object);

            // Act
            var actionResult = await controller.GetAll();

            // Assert
            Assert.IsNotNull(actionResult);
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult, "Expected OkObjectResult");
            var actual = okResult!.Value as IEnumerable<FrontendCourseDto>;
            Assert.AreSame(expected, actual, "Controller should return the same empty enumerable instance from mediator");
            mediatorMock.Verify(m => m.Send(It.IsAny<GetAllCoursesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetAll returns OkObjectResult whose Value is null when mediator returns null.
        /// Condition: mediator returns null.
        /// Expected: OkObjectResult returned and its Value is null.
        /// </summary>
        [TestMethod]
        public async Task GetAll_WhenMediatorReturnsNull_ReturnsOkWithNullValue()
        {
            // Arrange
            IEnumerable<FrontendCourseDto>? expected = null;
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAllCoursesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new CourseController(mediatorMock.Object);

            // Act
            var actionResult = await controller.GetAll();

            // Assert
            Assert.IsNotNull(actionResult);
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult, "Expected OkObjectResult");
            Assert.IsNull(okResult!.Value, "Expected returned OkObjectResult.Value to be null when mediator returns null");
            mediatorMock.Verify(m => m.Send(It.IsAny<GetAllCoursesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that GetCoursePrequisites forwards the provided courseId to the mediator,
        /// and returns an OkObjectResult containing the mediator's returned list.
        /// Test inputs: a set of integer courseId values including edge values.
        /// Expected outcome: OkObjectResult with the same list instance returned by IMediator.Send and the query contains the same courseId.
        /// </summary>
        [TestMethod]
        public async Task GetCoursePrequisites_ValidCourseIds_ReturnsOkWithListAndPassesCourseIdToMediator()
        {
            // Arrange & Act & Assert for multiple courseId values in a single test to avoid redundant tests.
            int[] testIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int courseId in testIds)
            {
                // Arrange
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

                List<CourseDto>? capturedReturn = new List<CourseDto>(); // specific instance to assert reference equality
                GetCoursePrequisitesQuery? capturedQuery = null;

                mediatorMock
                    .Setup(m => m.Send(It.IsAny<GetCoursePrequisitesQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(capturedReturn)
                    .Callback<IRequest<List<CourseDto>>, CancellationToken>((req, ct) =>
                    {
                        capturedQuery = req as GetCoursePrequisitesQuery;
                    });

                var controller = new CourseController(mediatorMock.Object);

                // Act
                var actionResult = await controller.GetCoursePrequisites(courseId).ConfigureAwait(false);

                // Assert
                Assert.IsNotNull(actionResult, "ActionResult should not be null.");
                var okResult = actionResult as OkObjectResult;
                Assert.IsNotNull(okResult, "Expected OkObjectResult for successful mediator response.");
                Assert.AreSame(capturedReturn, okResult.Value, "Returned value should be the same instance provided by the mediator.");

                Assert.IsNotNull(capturedQuery, "Mediator query should have been provided.");
                Assert.AreEqual(courseId, capturedQuery!.CourseId, $"Query.CourseId should equal the provided courseId ({courseId}).");

                mediatorMock.Verify(m => m.Send(It.IsAny<GetCoursePrequisitesQuery>(), It.IsAny<CancellationToken>()), Times.Once);

                // Reset for next iteration via new mediatorMock in next loop iteration
            }
        }

        /// <summary>
        /// Verifies that GetCoursePrequisites returns OkObjectResult with a null Value when the mediator returns null.
        /// Test input: mediator returns null for a sample courseId.
        /// Expected outcome: OkObjectResult with null Value (200 OK with null body).
        /// </summary>
        [TestMethod]
        public async Task GetCoursePrequisites_MediatorReturnsNull_ReturnsOkWithNullValue()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetCoursePrequisitesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<CourseDto>?)null);

            var controller = new CourseController(mediatorMock.Object);
            int sampleId = 42;

            // Act
            var actionResult = await controller.GetCoursePrequisites(sampleId).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(actionResult, "ActionResult should not be null.");
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult, "Expected OkObjectResult even when mediator returns null.");
            Assert.IsNull(okResult.Value, "OkObjectResult.Value should be null when mediator returns null.");

            mediatorMock.Verify(m => m.Send(It.IsAny<GetCoursePrequisitesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that exceptions thrown by the mediator propagate out of GetCoursePrequisites.
        /// Test input: IMediator.Send throws InvalidOperationException.
        /// Expected outcome: the same InvalidOperationException is thrown to the caller.
        /// </summary>
        [TestMethod]
        public async Task GetCoursePrequisites_MediatorThrowsException_ExceptionPropagates()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetCoursePrequisitesQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Mediator failure"));

            var controller = new CourseController(mediatorMock.Object);
            int sampleId = 7;

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await controller.GetCoursePrequisites(sampleId).ConfigureAwait(false);
            }).ConfigureAwait(false);

            mediatorMock.Verify(m => m.Send(It.IsAny<GetCoursePrequisitesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Test purpose:
        /// Verify that GetDepartmentOpenCourses forwards the provided departmentId to the mediator
        /// via GetDepartmentOpenCoursesQuery and returns an OkObjectResult containing the mediator result.
        /// 
        /// Input conditions:
        /// This test iterates several representative departmentId values, including edge integers:
        /// int.MinValue, -1, 0, 1, int.MaxValue.
        /// 
        /// Expected result:
        /// For each departmentId the controller should call IMediator.Send with a GetDepartmentOpenCoursesQuery
        /// whose DepartmentId equals the input, and the action should return Ok(result) where result is the
        /// same object returned by the mediator.
        /// </summary>
        [TestMethod]
        public async Task GetDepartmentOpenCourses_VariousDepartmentIds_ReturnsOkWithMediatorResult()
        {
            // Arrange / Act / Assert performed per-case to avoid multiple TestMethods while still parameterizing inputs.
            int[] testDepartmentIds = new[]
            {
                int.MinValue,
                -1,
                0,
                1,
                int.MaxValue
            };

            foreach (int departmentId in testDepartmentIds)
            {
                // Arrange
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

                var expected = new List<FrontendCourseDto>(); // empty list is sufficient; avoids constructing DTOs with unknown ctor

                mediatorMock
                    .Setup(m => m.Send(
                        It.Is<IRequest<IEnumerable<FrontendCourseDto>>>(req =>
                            req is GetDepartmentOpenCoursesQuery q && q.DepartmentId == departmentId),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expected)
                    .Verifiable();

                var controller = new CourseController(mediatorMock.Object);

                // Act
                IActionResult actionResult = await controller.GetDepartmentOpenCourses(departmentId);

                // Assert - result type
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), $"Expected OkObjectResult for departmentId={departmentId}.");

                var okResult = actionResult as OkObjectResult;
                Assert.IsNotNull(okResult, "OkObjectResult cast must succeed.");

                // Assert - same object returned by mediator
                Assert.AreSame(expected, okResult!.Value, "Controller must return exactly the mediator result instance.");

                // Verify mediator was called exactly once with expected query
                mediatorMock.Verify(
                    m => m.Send(
                        It.Is<IRequest<IEnumerable<FrontendCourseDto>>>(req =>
                            req is GetDepartmentOpenCoursesQuery q && q.DepartmentId == departmentId),
                        It.IsAny<CancellationToken>()),
                    Times.Once,
                    $"Mediator.Send should be invoked once for departmentId={departmentId}.");

                mediatorMock.VerifyNoOtherCalls();
            }
        }

        /// <summary>
        /// Verifies that when there is no authenticated user (no NameIdentifier claim)
        /// UploadCourseFile returns UnauthorizedResult.
        /// Input conditions: courseId=1, title="t", description="d", type=UploadType.Lecture, file=null, no user claims.
        /// Expected: UnauthorizedResult.
        /// </summary>
        [TestMethod]
        public async Task UploadCourseFile_NoUser_ReturnsUnauthorized()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            var controller = new CourseController(mediatorMock.Object);
            // Ensure there is no NameIdentifier claim in the HttpContext user
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal() // empty identity -> FindFirstValue returns null
                }
            };

            // Act
            var result = await controller.UploadCourseFile(1, "t", "d", UploadType.Lecture, file: null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
            // No mediator interactions should have occurred
            mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<Response<int>>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Verifies that with an authenticated user UploadCourseFile:
        /// - Calls IMediator.Send with CreateCourseUploadCommand that preserves inputs (CourseId, Title, Description, Type, File, UserId)
        /// - Returns OkObjectResult with the mediator response value
        /// Tested across several edge cases:
        ///   * courseId at 0, int.MaxValue, int.MinValue
        ///   * title empty, very long, and with special/control chars
        ///   * description empty and whitespace
        ///   * enum normal and out-of-range (cast)
        ///   * file null and mocked IFormFile
        /// Expected: OkObjectResult and mediator called once per invocation with matching command values.
        /// </summary>
        [TestMethod]
        public async Task UploadCourseFile_WithUser_CallsMediatorAndReturnsOk_ForVariousInputs()
        {
            // Prepare a set of test cases to exercise boundaries and special inputs
            var testCases = new[]
            {
                new
                {
                    CourseId = 0,
                    Title = string.Empty,
                    Description = " ",
                    Type = UploadType.Lecture,
                    HasFile = false
                },
                new
                {
                    CourseId = int.MaxValue,
                    Title = new string('a', 500),
                    Description = "regular description",
                    Type = UploadType.Material,
                    HasFile = true
                },
                new
                {
                    CourseId = int.MinValue,
                    Title = "special\n\t\u2603",
                    Description = string.Empty,
                    Type = (UploadType)999, // out of defined enum range
                    HasFile = false
                }
            };

            foreach (var tc in testCases)
            {
                // Arrange
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

                // Create a unique response per iteration to verify it is returned unchanged
                var expectedResponse = Response<int>.SuccessResponse(123);

                // Setup mediator to capture the command argument and return the expected response
                CreateCourseUploadCommand? capturedCommand = null;
                mediatorMock
                    .Setup(m => m.Send(It.IsAny<CreateCourseUploadCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedResponse)
                    .Callback<CreateCourseUploadCommand, CancellationToken>((cmd, ct) => { capturedCommand = cmd; });

                var controller = new CourseController(mediatorMock.Object);

                // Provide an authenticated user with NameIdentifier
                var userId = Guid.NewGuid().ToString();
                var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "TestAuth");
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(identity)
                    }
                };

                IFormFile? file = null;
                if (tc.HasFile)
                {
                    var fileMock = new Mock<IFormFile>();
                    // Minimal setup: name and length not needed for this code path; keep it simple
                    file = fileMock.Object;
                }

                // Act
                var actionResult = await controller.UploadCourseFile(tc.CourseId, tc.Title, tc.Description, tc.Type, file);

                // Assert: result is OkObjectResult and contains the mediator response
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
                var ok = actionResult as OkObjectResult;
                Assert.IsNotNull(ok);
                Assert.AreSame(expectedResponse, ok!.Value);

                // Assert mediator was called once and capturedCommand reflects inputs
                mediatorMock.Verify(m => m.Send(It.IsAny<CreateCourseUploadCommand>(), It.IsAny<CancellationToken>()), Times.Once);
                Assert.IsNotNull(capturedCommand, "The CreateCourseUploadCommand passed to IMediator.Send was not captured.");

                // Command-level assertions
                Assert.AreEqual(userId, capturedCommand!.UserId, "UserId should be propagated to the command.");
                Assert.AreEqual(tc.CourseId, capturedCommand.CourseUploadDto.CourseId, "CourseId should be propagated to DTO.");
                Assert.AreEqual(tc.Title, capturedCommand.CourseUploadDto.Title, "Title should be propagated to DTO.");
                Assert.AreEqual(tc.Description, capturedCommand.CourseUploadDto.Description, "Description should be propagated to DTO.");
                Assert.AreEqual(tc.Type, capturedCommand.CourseUploadDto.Type, "UploadType should be propagated to DTO.");
                // File may be null or non-null; assert equality
                Assert.AreSame(file, capturedCommand.File, "File should be propagated unchanged to the command.");
            }
        }

        /// <summary>
        /// Verifies that for a variety of valid GrantCourseExceptionDto inputs the controller:
        /// - Sends a GrantCourseExceptionCommand with the same DTO to IMediator.
        /// - Returns an OkObjectResult with the expected message.
        /// Input conditions: multiple DTOS exercising empty, whitespace, long AcademicCode and integer boundary values.
        /// Expected result: mediator.Send is invoked once per call with a GrantCourseExceptionCommand containing the same DTO values,
        /// and the action returns Ok("Exception granted.").
        /// </summary>
        [TestMethod]
        public async Task GrantException_VariousValidDtos_ReturnsOkAndSendsCommand()
        {
            // Arrange: prepare several representative DTOs covering edge numeric and string cases
            var testDtos = new List<GrantCourseExceptionDto?>
            {
                new GrantCourseExceptionDto { AcademicCode = "AC-123", CourseId = 1, StudyYearId = 1, SemesterId = 1 },
                new GrantCourseExceptionDto { AcademicCode = string.Empty, CourseId = 0, StudyYearId = 0, SemesterId = 0 },
                new GrantCourseExceptionDto { AcademicCode = "   ", CourseId = -1, StudyYearId = int.MaxValue, SemesterId = int.MinValue },
                new GrantCourseExceptionDto { AcademicCode = new string('X', 1024), CourseId = int.MaxValue, StudyYearId = int.MinValue, SemesterId = 2 },
                new GrantCourseExceptionDto { AcademicCode = "Spec!\u0001\u0002", CourseId = 42, StudyYearId = 3, SemesterId = 2 }
            };

            foreach (var dto in testDtos)
            {
                // Arrange per-case
                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
                mediatorMock
                    .Setup(m => m.Send(It.IsAny<IRequest<Unit>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Unit.Value)
                    .Verifiable();

                var controller = new CourseController(mediatorMock.Object);

                // Act
                var result = await controller.GrantException(dto!);

                // Assert
                Assert.IsNotNull(result, "Result should not be null.");
                Assert.IsInstanceOfType(result, typeof(OkObjectResult), "Expected OkObjectResult.");
                var okResult = result as OkObjectResult;
                Assert.IsNotNull(okResult);
                Assert.AreEqual("Exception granted.", okResult!.Value, "Ok message mismatch.");

                mediatorMock.Verify(m => m.Send(
                    It.Is<IRequest<Unit>>(req =>
                        req is GrantCourseExceptionCommand cmd
                        && cmd.Dto.AcademicCode == dto!.AcademicCode
                        && cmd.Dto.CourseId == dto.CourseId
                        && cmd.Dto.StudyYearId == dto.StudyYearId
                        && cmd.Dto.SemesterId == dto.SemesterId),
                    It.IsAny<CancellationToken>()),
                    Times.Once, "Expected Send to be called once with a GrantCourseExceptionCommand matching the DTO.");
            }
        }

        /// <summary>
        /// Verifies that if IMediator.Send throws an exception the controller method does not swallow it and it propagates to the caller.
        /// Input conditions: a valid DTO and mediator configured to throw InvalidOperationException.
        /// Expected result: the same InvalidOperationException is propagated.
        /// </summary>
        [TestMethod]
        public async Task GrantException_MediatorThrows_ExceptionPropagates()
        {
            // Arrange
            var dto = new GrantCourseExceptionDto
            {
                AcademicCode = "AC-ERR",
                CourseId = 10,
                StudyYearId = 2,
                SemesterId = 1
            };

            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<IRequest<Unit>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("mediator failure"));

            var controller = new CourseController(mediatorMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await controller.GrantException(dto));
            Assert.AreEqual("mediator failure", ex.Message, "Exception message should be preserved.");
            mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<Unit>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateStatus returns NoContent and sends the correct UpdateCourseStatusCommand
        /// for a range of courseId boundary values and all defined CourseStatus enum values.
        /// Input conditions: courseId values = {int.MinValue, -1, 0, 1, int.MaxValue}; status values = all CourseStatus values.
        /// Expected result: method returns NoContentResult and mediator.Send is invoked exactly once with a command matching the inputs.
        /// </summary>
        [TestMethod]
        public async Task UpdateStatus_ValidCourseIdsAndStatuses_ReturnsNoContentAndSendsCommand()
        {
            // Arrange
            int[] courseIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };
            CourseStatus[] statuses = (CourseStatus[])Enum.GetValues(typeof(CourseStatus));

            foreach (int courseId in courseIds)
            {
                foreach (CourseStatus status in statuses)
                {
                    var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

                    // Expectation: Send is called with a command that has matching CourseId and Status
                    mediatorMock
                        .Setup(m => m.Send(
                            It.Is<UpdateCourseStatusCommand>(cmd => cmd.CourseId == courseId && cmd.Status == status),
                            It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Unit.Value);

                    var controller = new CourseController(mediatorMock.Object);

                    // Act
                    IActionResult result = await controller.UpdateStatus(courseId, status).ConfigureAwait(false);

                    // Assert
                    Assert.IsInstanceOfType(result, typeof(NoContentResult), $"Expected NoContentResult for courseId={courseId}, status={status}");
                    mediatorMock.Verify(m => m.Send(
                        It.Is<UpdateCourseStatusCommand>(cmd => cmd.CourseId == courseId && cmd.Status == status),
                        It.IsAny<CancellationToken>()), Times.Once);

                    mediatorMock.VerifyNoOtherCalls();
                }
            }
        }

        /// <summary>
        /// Tests that exceptions thrown by the mediator are propagated by UpdateStatus.
        /// Input conditions: mediator.Send throws InvalidOperationException.
        /// Expected result: the exception is propagated to the caller.
        /// </summary>
        [TestMethod]
        public async Task UpdateStatus_MediatorThrows_ExceptionPropagates()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);

            mediatorMock
                .Setup(m => m.Send(It.IsAny<UpdateCourseStatusCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("mediator failure"));

            var controller = new CourseController(mediatorMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await controller.UpdateStatus(1, CourseStatus.Opened).ConfigureAwait(false);
            }).ConfigureAwait(false);

            mediatorMock.Verify(m => m.Send(It.IsAny<UpdateCourseStatusCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            mediatorMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Tests that GetCourseYearRegistrations forwards the provided ids to MediatR and returns OkObjectResult
        /// containing the exact Response returned by the mediator.
        /// Test inputs include boundary and special numeric cases: 0, positive, negative, int.MinValue/int.MaxValue.
        /// Expected: OkObjectResult with the same Response instance and MediatR.Send called once per invocation.
        /// </summary>
        [TestMethod]
        public async Task GetCourseYearRegistrations_ValidAndBoundaryIds_ReturnsOkWithMediatorResponse()
        {
            // Arrange
            var testCases = new (int courseId, int yearId)[]
            {
                (0, 0),
                (1, 2023),
                (-1, -2023),
                (int.MinValue, int.MaxValue)
            };

            foreach ((int courseId, int yearId) in testCases)
            {
                // Arrange for each case
                var dto = new CourseWithRegistrationsDto
                {
                    Id = courseId,
                    Code = "C" + courseId.ToString(),
                    Name = "Name" + courseId.ToString(),
                    Credits = 3,
                    StudyYearId = yearId,
                    StudentRegistrations = new List<StudentRegistrationDto>()
                };

                var response = Response<CourseWithRegistrationsDto>.SuccessResponse(dto);

                var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
                mediatorMock
                    .Setup(m => m.Send(It.Is<GetCourseYearRegistrationsQuery>(q => q.CourseId == courseId && q.YearId == yearId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(response)
                    .Verifiable();

                var controller = new CourseController(mediatorMock.Object);

                // Act
                var actionResult = await controller.GetCourseYearRegistrations(courseId, yearId);

                // Assert
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult for valid mediator response.");
                var okResult = actionResult as OkObjectResult;
                Assert.AreSame(response, okResult?.Value, "Controller should return the same Response instance returned by the mediator.");

                mediatorMock.Verify(m => m.Send(It.IsAny<GetCourseYearRegistrationsQuery>(), It.IsAny<CancellationToken>()), Times.Once);

                // Cleanup / prepare for next iteration (mock is scoped per iteration)
            }
        }

        /// <summary>
        /// Tests that when IMediator.Send throws an exception, the controller does not swallow it and the exception propagates.
        /// Input: mediator configured to throw InvalidOperationException for any query.
        /// Expected: InvalidOperationException is thrown from controller action.
        /// </summary>
        [TestMethod]
        public async Task GetCourseYearRegistrations_MediatorThrows_ExceptionPropagated()
        {
            // Arrange
            int courseId = 1;
            int yearId = 2023;

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetCourseYearRegistrationsQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("mediator failure"))
                .Verifiable();

            var controller = new CourseController(mediatorMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await controller.GetCourseYearRegistrations(courseId, yearId);
            });

            mediatorMock.Verify(m => m.Send(It.IsAny<GetCourseYearRegistrationsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that the controller returns OkObjectResult even when the mediator returns an error Response (Data == null).
        /// Input: mediator returns Response.ErrorResponse.
        /// Expected: OkObjectResult with Response whose Success is false and Data is null.
        /// </summary>
        [TestMethod]
        public async Task GetCourseYearRegistrations_MediatorReturnsErrorResponse_ReturnsOkWithNullData()
        {
            // Arrange
            int courseId = 42;
            int yearId = 7;

            var errorResponse = Response<CourseWithRegistrationsDto>.ErrorResponse("not found");

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.Is<GetCourseYearRegistrationsQuery>(q => q.CourseId == courseId && q.YearId == yearId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(errorResponse)
                .Verifiable();

            var controller = new CourseController(mediatorMock.Object);

            // Act
            var actionResult = await controller.GetCourseYearRegistrations(courseId, yearId);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult, "OkObjectResult expected.");
            var returned = okResult.Value as Response<CourseWithRegistrationsDto>;
            Assert.IsNotNull(returned, "Returned value should be a Response<CourseWithRegistrationsDto>.");
            Assert.IsFalse(returned.Success, "Error response should have Success == false.");
            Assert.IsNull(returned.Data, "Error response should have null Data.");

            mediatorMock.Verify(m => m.Send(It.IsAny<GetCourseYearRegistrationsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}