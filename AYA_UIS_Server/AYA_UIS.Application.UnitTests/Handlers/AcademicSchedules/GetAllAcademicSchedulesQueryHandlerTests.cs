using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.AcademicSchedules;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.AcademicSchedules;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Shared;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.AcademicSheduleDtos;

namespace AYA_UIS.Application.Handlers.AcademicSchedules.UnitTests
{
    [TestClass]
    public class GetAllAcademicSchedulesQueryHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor creates a non-null instance when valid dependencies are provided.
        /// Input conditions: valid, non-null IUnitOfWork and IMapper implementations (mocks).
        /// Expected result: an instance is created and implements IRequestHandler&lt;GetAllAcademicSchedulesQuery, List&lt;AcademicSchedulesDto&gt;&gt;.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidDependencies_CreatesInstanceImplementingInterface()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mapperMock = new Mock<IMapper>();

            // Act
            var handler = new GetAllAcademicSchedulesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Assert
            Assert.IsNotNull(handler, "Handler instance should not be null when valid dependencies are provided.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetAllAcademicSchedulesQuery, List<AcademicSchedulesDto>>), "Handler should implement the expected IRequestHandler interface.");
        }

        /// <summary>
        /// Test purpose:
        /// Verify that when the repository returns a non-empty collection, the handler invokes repository and mapper
        /// and returns the exact mapped list instance.
        /// Input conditions:
        /// - Repository.GetAllWithDetailsAsync returns a list with one AcademicSchedule.
        /// - Mapper.Map returns a concrete List&lt;AcademicSchedulesDto&gt; instance.
        /// Expected result:
        /// - Handler returns the same mapped list instance.
        /// - Repository.GetAllWithDetailsAsync and Mapper.Map are invoked exactly once.
        /// </summary>
        [TestMethod]
        public async Task Handle_RepoReturnsItems_ReturnsMappedListAndInvokesDependencies()
        {
            // Arrange
            var schedule = new AcademicSchedule { Title = "Term1", Url = "http://example" };
            List<AcademicSchedule> schedules = new() { schedule };

            var repoMock = new Mock<IAcademicScheduleRepository>();
            repoMock.Setup(r => r.GetAllWithDetailsAsync()).ReturnsAsync((IEnumerable<AcademicSchedule>)schedules);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.AcademicSchedules).Returns(repoMock.Object);

            var expectedDtos = new List<AcademicSchedulesDto>
            {
                new AcademicSchedulesDto { Id = 1, Title = "MappedTitle", Url = "http://mapped" }
            };

            var mapperMock = new Mock<IMapper>();
            mapperMock
                .Setup(m => m.Map<List<AcademicSchedulesDto>>(It.IsAny<object>()))
                .Returns(expectedDtos);

            var handler = new GetAllAcademicSchedulesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Act
            List<AcademicSchedulesDto>? result = await handler.Handle(new GetAllAcademicSchedulesQuery(), CancellationToken.None);

            // Assert
            Assert.IsNotNull(result, "Result should not be null when mapper returns a list.");
            Assert.AreSame(expectedDtos, result, "Handler should return the exact list instance returned by the mapper.");
            repoMock.Verify(r => r.GetAllWithDetailsAsync(), Times.Once, "Repository.GetAllWithDetailsAsync should be called once.");
            mapperMock.Verify(m => m.Map<List<AcademicSchedulesDto>>(It.Is<object>(o => object.ReferenceEquals(o, schedules))), Times.Once, "Mapper.Map should be called with the repository result.");
        }

        /// <summary>
        /// Test purpose:
        /// Verify handler behavior when repository returns null and mapper returns null.
        /// Input conditions:
        /// - Repository.GetAllWithDetailsAsync returns null.
        /// - Mapper.Map returns null for a null source.
        /// Expected result:
        /// - Handler returns null (propagates mapper output).
        /// </summary>
        [TestMethod]
        public async Task Handle_RepoReturnsNull_MapperReturnsNull_HandlerReturnsNull()
        {
            // Arrange
            var repoMock = new Mock<IAcademicScheduleRepository>();
            repoMock.Setup(r => r.GetAllWithDetailsAsync()).ReturnsAsync((IEnumerable<AcademicSchedule>?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.AcademicSchedules).Returns(repoMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock
                .Setup(m => m.Map<List<AcademicSchedulesDto>>(It.IsAny<object>()))
                .Returns((List<AcademicSchedulesDto>?)null);

            var handler = new GetAllAcademicSchedulesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Act
            List<AcademicSchedulesDto>? result = await handler.Handle(new GetAllAcademicSchedulesQuery(), CancellationToken.None);

            // Assert
            Assert.IsNull(result, "Handler should return null when mapper returns null.");
            repoMock.Verify(r => r.GetAllWithDetailsAsync(), Times.Once);
            mapperMock.Verify(m => m.Map<List<AcademicSchedulesDto>>(It.Is<object>(o => o == null)), Times.Once);
        }

    }
}