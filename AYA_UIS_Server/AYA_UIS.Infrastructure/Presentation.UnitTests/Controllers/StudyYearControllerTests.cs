using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.StudyYears;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.StudyYears;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Presentation;
using Presentation.Controllers;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.StudyYearDtos;

namespace Presentation.Controllers.UnitTests
{
    [TestClass]
    public partial class StudyYearControllerTests
    {
        /// <summary>
        /// Verifies that GetAllStudyYears returns OkObjectResult containing the same reference returned by IMediator.Send when a non-null (empty) collection is returned.
        /// Condition: IMediator returns an empty enumerable (non-null).
        /// Expected: OkObjectResult with the exact enumerable instance and mediator.Send invoked once.
        /// </summary>
        [TestMethod]
        public async Task GetAllStudyYears_MediatorReturnsEmptyCollection_ReturnsOkWithSameInstance()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            IEnumerable<StudyYearDto> expected = new List<StudyYearDto>(); // empty collection instance

            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAllStudyYearsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new StudyYearController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.GetAllStudyYears().ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult when mediator returns a collection (empty).");
            var ok = (OkObjectResult)actionResult;
            // The controller should return the same object reference that the mediator returned.
            Assert.AreSame(expected, ok.Value, "Returned value should be the exact enumerable instance provided by mediator.");
            mediatorMock.Verify(m => m.Send(It.IsAny<GetAllStudyYearsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            mediatorMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Verifies that GetAllStudyYears returns OkObjectResult containing an enumerable with a single (possibly null) element when mediator returns such a collection.
        /// Condition: IMediator returns a single-item collection (contains null element).
        /// Expected: OkObjectResult with the same enumerable instance and mediator.Send invoked once.
        /// </summary>
        [TestMethod]
        public async Task GetAllStudyYears_MediatorReturnsSingleItemCollection_ReturnsOkWithSameInstance()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            // Use a single-item collection. The item itself can be null (tests single-item behavior without assuming StudyYearDto constructor).
            IEnumerable<StudyYearDto?> expected = new StudyYearDto?[] { null };

            // Return as IEnumerable<StudyYearDto> to match signature (null element allowed)
            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAllStudyYearsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<StudyYearDto>)expected)
                .Verifiable();

            var controller = new StudyYearController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.GetAllStudyYears().ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult when mediator returns single-item collection.");
            var ok = (OkObjectResult)actionResult;
            Assert.AreSame(expected, ok.Value, "Returned value should be the exact enumerable instance provided by mediator.");
            mediatorMock.Verify(m => m.Send(It.IsAny<GetAllStudyYearsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            mediatorMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Verifies that GetAllStudyYears returns OkObjectResult containing null when IMediator.Send returns null.
        /// Condition: IMediator returns null.
        /// Expected: OkObjectResult with a null Value and mediator.Send invoked once.
        /// </summary>
        [TestMethod]
        public async Task GetAllStudyYears_MediatorReturnsNull_ReturnsOkWithNull()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            IEnumerable<StudyYearDto>? expected = null;

            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAllStudyYearsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            var controller = new StudyYearController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.GetAllStudyYears().ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult even when mediator returns null.");
            var ok = (OkObjectResult)actionResult;
            Assert.IsNull(ok.Value, "OkObjectResult.Value should be null when mediator returns null.");
            mediatorMock.Verify(m => m.Send(It.IsAny<GetAllStudyYearsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            mediatorMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Verifies that the StudyYearController constructor creates an instance when a non-null IMediator is provided.
        /// Input: a valid mocked IMediator instance.
        /// Expected: the constructor returns a non-null StudyYearController instance and it derives from ControllerBase.
        /// </summary>
        [TestMethod]
        public void StudyYearController_WithValidMediator_InstanceCreatedAndIsControllerBase()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();

            // Act
            var controller = new StudyYearController(mediatorMock.Object);

            // Assert
            Assert.IsNotNull(controller, "Constructor returned null for a valid IMediator instance.");
            Assert.IsInstanceOfType(controller, typeof(ControllerBase), "StudyYearController should derive from ControllerBase.");
        }

        /// <summary>
        /// Ensures constructing multiple StudyYearController instances with different valid IMediator instances succeeds.
        /// Input: two distinct mocked IMediator instances.
        /// Expected: both constructor calls produce non-null, distinct StudyYearController instances without throwing.
        /// </summary>
        [TestMethod]
        public void StudyYearController_WithDifferentMediatorInstances_MultipleInstancesCreated()
        {
            // Arrange
            var mediatorMockA = new Mock<IMediator>();
            var mediatorMockB = new Mock<IMediator>();

            // Act
            var controllerA = new StudyYearController(mediatorMockA.Object);
            var controllerB = new StudyYearController(mediatorMockB.Object);

            // Assert
            Assert.IsNotNull(controllerA, "First constructor invocation returned null.");
            Assert.IsNotNull(controllerB, "Second constructor invocation returned null.");
            Assert.AreNotSame(controllerA, controllerB, "Constructor should produce distinct instances for separate invocations.");
        }

        /// <summary>
        /// Test that CreateStudyYear returns OkObjectResult containing the integer returned by IMediator.Send.
        /// This test exercises several representative DTO numeric edge values (including int.MinValue, 0, int.MaxValue)
        /// and verifies the controller forwards the same DTO instance inside the CreateStudyYearCommand to IMediator.
        /// Expected: OkObjectResult with the mediator-returned integer and IMediator.Send invoked once with a command
        /// that contains the exact DTO instance.
        /// </summary>
        [TestMethod]
        public async Task CreateStudyYear_ValidDtos_ReturnsOkWithMediatorResult()
        {
            // Arrange & Act & Assert for multiple cases to avoid redundant tests.
            var testCases = new (CreateStudyYearDto dto, int mediatorResult)[]
            {
                (new CreateStudyYearDto { StartYear = 0, EndYear = 0 }, 0),
                (new CreateStudyYearDto { StartYear = 2000, EndYear = 2001 }, 1),
                (new CreateStudyYearDto { StartYear = int.MinValue, EndYear = int.MinValue + 1 }, int.MinValue),
                (new CreateStudyYearDto { StartYear = int.MaxValue - 1, EndYear = int.MaxValue }, int.MaxValue)
            };

            foreach (var (dto, mediatorResult) in testCases)
            {
                // Arrange
                var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
                mockMediator
                    .Setup(m => m.Send(It.IsAny<CreateStudyYearCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(mediatorResult)
                    .Verifiable();

                var controller = new StudyYearController(mockMediator.Object);

                // Act
                var actionResult = await controller.CreateStudyYear(dto).ConfigureAwait(false);

                // Assert
                Assert.IsNotNull(actionResult, "Expected a non-null IActionResult.");
                Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult), "Expected OkObjectResult for successful mediator response.");

                var okResult = actionResult as OkObjectResult;
                Assert.IsNotNull(okResult);
                Assert.AreEqual(mediatorResult, okResult.Value, "OkObjectResult should contain the integer returned from IMediator.");

                // Verify that Send was called with a CreateStudyYearCommand that contains the same DTO instance.
                mockMediator.Verify(m => m.Send(It.Is<CreateStudyYearCommand>(c => ReferenceEquals(c.StudyYearDto, dto)), It.IsAny<CancellationToken>()), Times.Once);

                mockMediator.Verify();
            }
        }

        /// <summary>
        /// Test that when IMediator.Send throws an exception (e.g., due to validation or handler failure),
        /// the controller does not swallow it and the exception propagates to the caller.
        /// Input: valid-looking DTO (StartYear < EndYear) but mediator is configured to throw.
        /// Expected: the same exception type is thrown by CreateStudyYear.
        /// </summary>
        [TestMethod]
        public async Task CreateStudyYear_MediatorThrows_ExceptionPropagated()
        {
            // Arrange
            var dto = new CreateStudyYearDto { StartYear = 2024, EndYear = 2025 };
            var mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            var expectedException = new InvalidOperationException("mediator failure");
            mockMediator
                .Setup(m => m.Send(It.IsAny<CreateStudyYearCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException)
                .Verifiable();

            var controller = new StudyYearController(mockMediator.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await controller.CreateStudyYear(dto).ConfigureAwait(false);
            });

            // Verify Send was invoked once and with a command containing the provided dto instance.
            mockMediator.Verify(m => m.Send(It.Is<CreateStudyYearCommand>(c => ReferenceEquals(c.StudyYearDto, dto)), It.IsAny<CancellationToken>()), Times.Once);
            mockMediator.Verify();
        }
    }
}