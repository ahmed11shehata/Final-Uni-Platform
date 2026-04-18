using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.CourseResults;
using AYA_UIS.Application.Queries;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Presentation;
using Presentation.Controllers;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.CourseResultDtos;

namespace Presentation.Controllers.UnitTests
{
    [TestClass]
    public class ResultsControllerTests
    {
        /// <summary>
        /// Verifies that the ResultsController constructor successfully creates an instance
        /// when provided a non-null IMediator implementation.
        /// Input conditions: a valid, non-null Mock of MediatR.IMediator.
        /// Expected result: an instance of ResultsController is returned and no exception is thrown.
        /// </summary>
        [TestMethod]
        public void ResultsController_WithValidMediator_CreatesInstance()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();

            // Act
            ResultsController controller = new ResultsController(mediatorMock.Object);

            // Assert
            Assert.IsNotNull(controller, "Constructor should return a non-null ResultsController instance when mediator is provided.");
            Assert.IsInstanceOfType(controller, typeof(ControllerBase), "ResultsController should inherit from ControllerBase.");
            Assert.AreEqual(typeof(ResultsController), controller.GetType(), "Created instance should be of exact type ResultsController.");
        }

        /// <summary>
        /// Documents the null-mediator behavior for the constructor.
        /// Input conditions: null mediator.
        /// Expected result: This test is marked inconclusive because the current constructor
        /// implementation does not include explicit null validation. Decide desired behavior
        /// (throw ArgumentNullException or accept null) and update the production code and test accordingly.
        /// </summary>
        [TestMethod]
        public void ResultsController_NullMediator_Inconclusive_NullHandlingRequested()
        {
            // Arrange
            IMediator? mediator = null;

            // Act & Assert
            // The source constructor does not validate null. Reflection or accessing private fields is not allowed.
            // Marking this test inconclusive to prompt explicit desired behavior for null mediator.
            Assert.Inconclusive("Constructor currently does not validate null mediator. Specify desired behavior (throw or allow) and implement validation to enable a deterministic test.");
        }

        /// <summary>
        /// Test that AddStudentResults sends an AddStudentResultsCommand containing the same DTO instance
        /// and returns an OkObjectResult with the expected success message when mediator completes successfully.
        /// Input conditions: a valid AddStudentResultsDto with typical values.
        /// Expected result: OkObjectResult with message and mediator.Send called exactly once with the same DTO instance.
        /// </summary>
        [TestMethod]
        public async Task AddStudentResults_ValidDto_ReturnsOkAndSendsCommand()
        {
            // Arrange
            var dto = new AddStudentResultsDto
            {
                AcademicCode = "AC123",
                StudyYearId = 2023,
                Results = new List<CourseResultItemDto>
                {
                    new CourseResultItemDto { CourseId = 1, IsPassed = true, Grade = 3.5m },
                    new CourseResultItemDto { CourseId = 2, IsPassed = false, Grade = 1.0m }
                }
            };

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.IsAny<IRequest<Unit>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            var controller = new ResultsController(mediatorMock.Object);

            // Act
            IActionResult actionResult = await controller.AddStudentResults(dto);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(OkObjectResult));
            var ok = (OkObjectResult)actionResult;
            Assert.AreEqual("Results added, GPA updated", ok.Value);

            mediatorMock.Verify(
                m => m.Send(It.Is<AddStudentResultsCommand>(c => ReferenceEquals(c.Dto, dto)), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Test that AddStudentResults propagates exceptions thrown by the mediator.
        /// Input conditions: a valid AddStudentResultsDto and mediator configured to throw.
        /// Expected result: the same exception is thrown to the caller and mediator.Send is invoked once.
        /// </summary>
        [TestMethod]
        public async Task AddStudentResults_MediatorThrows_ExceptionPropagated()
        {
            // Arrange
            var dto = new AddStudentResultsDto
            {
                AcademicCode = "AC_ERR",
                StudyYearId = 0,
                Results = new List<CourseResultItemDto>()
            };

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.IsAny<IRequest<Unit>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("mediator failed"));

            var controller = new ResultsController(mediatorMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await controller.AddStudentResults(dto);
            });

            mediatorMock.Verify(
                m => m.Send(It.Is<AddStudentResultsCommand>(c => ReferenceEquals(c.Dto, dto)), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Test that AddStudentResults forwards DTOs with extreme and boundary values unchanged to mediator
        /// and still returns the expected Ok response when mediator succeeds.
        /// Input conditions: DTO with extreme StudyYearId and long/whitespace AcademicCode and duplicate results.
        /// Expected result: OkObjectResult with message and mediator.Send called with same DTO instance.
        /// </summary>
        [TestMethod]
        public async Task AddStudentResults_ExtremeValues_PassesDtoThroughAndReturnsOk()
        {
            // Arrange
            var longAcademicCode = new string('X', 10000); // very long string
            var dto = new AddStudentResultsDto
            {
                AcademicCode = longAcademicCode + " ",
                StudyYearId = int.MaxValue,
                Results = new List<CourseResultItemDto>
                {
                    new CourseResultItemDto { CourseId = int.MaxValue, IsPassed = true, Grade = decimal.MaxValue },
                    new CourseResultItemDto { CourseId = int.MaxValue, IsPassed = true, Grade = decimal.MaxValue } // duplicate entry
                }
            };

            var mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
            mediatorMock
                .Setup(m => m.Send(It.IsAny<IRequest<Unit>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            var controller = new ResultsController(mediatorMock.Object);

            // Act
            IActionResult result = await controller.AddStudentResults(dto);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual("Results added, GPA updated", ok.Value);

            mediatorMock.Verify(
                m => m.Send(It.Is<AddStudentResultsCommand>(c => ReferenceEquals(c.Dto, dto)), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}