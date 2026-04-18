using AutoMapper;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.AcademicSchedules;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.AcademicSchedules;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Shared;
using AYA_UIS.Shared.Exceptions;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.AcademicSheduleDtos;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AYA_UIS.Application.Handlers.AcademicSchedules.UnitTests
{
    [TestClass]
    public class GetAcademicScheduleByTitleQueryHandlerTests
    {
        /// <summary>
        /// Verifies that when an academic schedule exists for the given title,
        /// the handler returns the DTO produced by IMapper.Map.
        /// Input conditions: repository returns a non-null AcademicSchedule for the provided title.
        /// Expected result: the returned AcademicSchedulesDto is the same object returned by the mapper,
        /// and repository and mapper are invoked with the expected arguments.
        /// </summary>
        [TestMethod]
        public async Task Handle_WhenScheduleExists_ReturnsMappedDto()
        {
            // Arrange
            const string title = "Test Schedule";
            var schedule = new AcademicSchedule
            {
                Id = 5,
                Title = title,
                Url = "http://example.com",
                Description = "desc"
            };
            var mappedDto = new AcademicSchedulesDto
            {
                Id = schedule.Id,
                Title = schedule.Title,
                Url = schedule.Url,
                Description = schedule.Description
            };
            var mockRepo = new Mock<IAcademicScheduleRepository>();
            mockRepo.Setup(r => r.GetByTitleAsync(It.Is<string>(s => s == title))).ReturnsAsync(schedule);
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.AcademicSchedules).Returns(mockRepo.Object);
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<AcademicSchedulesDto>(It.Is<object>(o => ReferenceEquals(o, schedule)))).Returns(mappedDto);
            var handler = new GetAcademicScheduleByTitleQueryHandler(mockUnitOfWork.Object, mockMapper.Object);
            var request = new GetAcademicScheduleByTitleQuery(title);
            // Act
            AcademicSchedulesDto? result = await handler.Handle(request, CancellationToken.None);
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mappedDto, result, "Handler should return the DTO produced by the mapper.");
            mockRepo.Verify(r => r.GetByTitleAsync(It.Is<string>(s => s == title)), Times.Once);
            mockMapper.Verify(m => m.Map<AcademicSchedulesDto>(It.Is<object>(o => ReferenceEquals(o, schedule))), Times.Once);
        }

        /// <summary>
        /// Verifies that when no academic schedule is found for various schedule title inputs,
        /// the handler throws a NotFoundException with the expected message containing the title.
        /// Input conditions: repository returns null for the provided titles.
        /// Expected result: NotFoundException is thrown and its message matches the expected format.
        /// This test iterates a set of representative title edge cases (empty, whitespace, long, special chars).
        /// </summary>
        [TestMethod]
        public async Task Handle_WhenScheduleNotFound_ThrowsNotFoundException_ForVariousTitles()
        {
            // Arrange: representative title edge cases (non-nullable per source code)
            var titles = new[]
            {
                string.Empty,
                "   ",
                new string ('A', 500), // very long string
                "Title_With_Special_Chars_!@#$%^&*()\t\n"
            };
            foreach (var title in titles)
            {
                var mockRepo = new Mock<IAcademicScheduleRepository>();
                mockRepo.Setup(r => r.GetByTitleAsync(It.Is<string>(s => s == title))).ReturnsAsync((AcademicSchedule? )null);
                var mockUnitOfWork = new Mock<IUnitOfWork>();
                mockUnitOfWork.Setup(u => u.AcademicSchedules).Returns(mockRepo.Object);
                var mockMapper = new Mock<IMapper>(); // mapper should not be invoked in this scenario
                var handler = new GetAcademicScheduleByTitleQueryHandler(mockUnitOfWork.Object, mockMapper.Object);
                var request = new GetAcademicScheduleByTitleQuery(title);
                // Act & Assert
                try
                {
                    await handler.Handle(request, CancellationToken.None);
                    Assert.Fail($"Expected NotFoundException for title: '{title ?? "<null>"}' but no exception was thrown.");
                }
                catch (NotFoundException ex)
                {
                    var expectedMessage = $"Academic schedule with title '{title}' not found.";
                    Assert.AreEqual(expectedMessage, ex.Message, "Exception message should include the requested schedule title.");
                }

                mockRepo.Verify(r => r.GetByTitleAsync(It.Is<string>(s => s == title)), Times.Once);
                mockMapper.Verify(m => m.Map<AcademicSchedulesDto>(It.IsAny<object>()), Times.Never);
            }
        }

        /// <summary>
        /// Verifies that the constructor creates a non-null instance when valid (mocked) dependencies are provided.
        /// Input conditions: mocked IUnitOfWork and IMapper passed to constructor.
        /// Expected result: instance is created successfully and implements the expected IRequestHandler interface.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidDependencies_InstanceCreatedAndImplementsInterface()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mockMapper = new Mock<IMapper>(MockBehavior.Strict);
            // Act
            var handler = new GetAcademicScheduleByTitleQueryHandler(mockUnitOfWork.Object, mockMapper.Object);
            // Assert
            Assert.IsNotNull(handler, "Handler instance should not be null when valid dependencies are provided.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetAcademicScheduleByTitleQuery, AcademicSchedulesDto>), "Handler should implement IRequestHandler<GetAcademicScheduleByTitleQuery, AcademicSchedulesDto>.");
        }

        /// <summary>
        /// Ensures constructing multiple handlers with different dependency instances does not share state or throw.
        /// Input conditions: two distinct mocked dependency instances.
        /// Expected result: both handler instances are created successfully and are distinct objects.
        /// </summary>
        [TestMethod]
        public void Constructor_MultipleDistinctDependencies_CreatesDistinctHandlerInstances()
        {
            // Arrange
            var mockUnitOfWork1 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mockMapper1 = new Mock<IMapper>(MockBehavior.Strict);
            var mockUnitOfWork2 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mockMapper2 = new Mock<IMapper>(MockBehavior.Strict);
            // Act
            var handler1 = new GetAcademicScheduleByTitleQueryHandler(mockUnitOfWork1.Object, mockMapper1.Object);
            var handler2 = new GetAcademicScheduleByTitleQueryHandler(mockUnitOfWork2.Object, mockMapper2.Object);
            // Assert
            Assert.IsNotNull(handler1, "First handler instance should not be null.");
            Assert.IsNotNull(handler2, "Second handler instance should not be null.");
            Assert.AreNotSame(handler1, handler2, "Handlers constructed with different dependencies should be distinct instances.");
            Assert.IsInstanceOfType(handler1, typeof(IRequestHandler<GetAcademicScheduleByTitleQuery, AcademicSchedulesDto>));
            Assert.IsInstanceOfType(handler2, typeof(IRequestHandler<GetAcademicScheduleByTitleQuery, AcademicSchedulesDto>));
        }
    }
}