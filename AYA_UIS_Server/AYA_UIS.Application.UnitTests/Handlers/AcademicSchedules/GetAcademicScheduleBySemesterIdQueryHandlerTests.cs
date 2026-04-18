using System;
using System.Collections;
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
using AYA_UIS.Shared.Exceptions;
using Domain;
using Domain.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.AcademicSheduleDtos;

namespace AYA_UIS.Application.Handlers.AcademicSchedules.UnitTests
{
    [TestClass]
    public class GetAcademicSchedulesBySemesterIdQueryHandlerTests
    {
        /// <summary>
        /// Tests that Handle returns the mapper's result when the semester exists.
        /// Input conditions: semester exists; test both when repository returns a non-null AcademicSchedule and when it returns null.
        /// Expected result: the handler returns exactly the value produced by IMapper.Map and AcademicSchedules.GetBySemesterIdAsync is invoked with the request.SemesterId.
        /// </summary>
        [TestMethod]
        public async Task Handle_SemesterExists_ReturnsMappedAcademicSchedules()
        {
            // Arrange
            var semesterRepoMock = new Mock<ISemesterRepository>();
            var academicScheduleRepoMock = new Mock<IAcademicScheduleRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mapperMock = new Mock<IMapper>();

            unitOfWorkMock.SetupGet(u => u.Semesters).Returns(semesterRepoMock.Object);
            unitOfWorkMock.SetupGet(u => u.AcademicSchedules).Returns(academicScheduleRepoMock.Object);

            // Semester exists
            semesterRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                            .ReturnsAsync(new Semester { Id = 1 });

            var handler = new GetAcademicSchedulesBySemesterIdQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Prepare two scenarios: schedules instance and null schedules
            var scheduleInstance = new AcademicSchedule { Id = 100, Title = "Test" };
            var scenarios = new (int semesterId, AcademicSchedule? repoResult)[]
            {
                (semesterId: 1, repoResult: scheduleInstance),
                (semesterId: 2, repoResult: null)
            };

            foreach (var scenario in scenarios)
            {
                // Setup repository return for this scenario
                academicScheduleRepoMock.Setup(r => r.GetBySemesterIdAsync(scenario.semesterId))
                                        .ReturnsAsync(scenario.repoResult);

                // Prepare the mapped DTO list we expect the mapper to return
                var mappedDtos = new List<AcademicScheduleDto>
                {
                    new AcademicScheduleDto()
                };

                // Setup mapper to return our mappedDtos for the exact source object passed
                mapperMock.Setup(m => m.Map<IEnumerable<AcademicScheduleDto>>(It.Is<object>(o => object.ReferenceEquals(o, scenario.repoResult))))
                          .Returns(mappedDtos);

                var request = new GetAcademicSchedulesBySemesterIdQuery(scenario.semesterId);

                // Act
                var result = await handler.Handle(request, CancellationToken.None);

                // Assert
                // The handler should return exactly what the mapper produced
                Assert.AreSame(mappedDtos, result);

                // AcademicSchedules repository must be called with the provided semester id
                academicScheduleRepoMock.Verify(r => r.GetBySemesterIdAsync(scenario.semesterId), Times.Once);

                // Mapper must be invoked with the exact object returned by repository (may be null)
                mapperMock.Verify(m => m.Map<IEnumerable<AcademicScheduleDto>>(It.Is<object>(o => object.ReferenceEquals(o, scenario.repoResult))), Times.Once);

                // Reset calls for the next iteration
                academicScheduleRepoMock.Invocations.Clear();
                mapperMock.Invocations.Clear();
            }
        }

        /// <summary>
        /// Verifies that when valid (mocked) IUnitOfWork and IMapper instances are passed,
        /// the constructor creates a non-null instance that implements the expected IRequestHandler interface.
        /// Input conditions: valid mocks for IUnitOfWork and IMapper.
        /// Expected result: instance is created and is assignable to IRequestHandler<GetAcademicSchedulesBySemesterIdQuery, IEnumerable<AcademicScheduleDto>>.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidDependencies_InstanceCreatedAndImplementsInterface()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            // Act
            var handler = new GetAcademicSchedulesBySemesterIdQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Assert
            Assert.IsNotNull(handler, "Constructor returned null which is unexpected for valid dependencies.");
            Assert.IsInstanceOfType(handler, typeof(MediatR.IRequestHandler<GetAcademicSchedulesBySemesterIdQuery, IEnumerable<AcademicScheduleDto>>),
                "Handler does not implement the expected IRequestHandler interface.");
        }

        /// <summary>
        /// Ensures that constructing multiple instances with different mock objects succeeds.
        /// Input conditions: two different pairs of mocks.
        /// Expected result: both instances are created independently and are not the same reference.
        /// This validates that the constructor does not use any unexpected shared/static state.
        /// </summary>
        [TestMethod]
        public void Constructor_MultipleCalls_IndependentInstancesCreated()
        {
            // Arrange - first set of mocks
            var unitOfWorkMock1 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mapperMock1 = new Mock<IMapper>(MockBehavior.Strict);

            // Arrange - second set of mocks
            var unitOfWorkMock2 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mapperMock2 = new Mock<IMapper>(MockBehavior.Strict);

            // Act
            var handler1 = new GetAcademicSchedulesBySemesterIdQueryHandler(unitOfWorkMock1.Object, mapperMock1.Object);
            var handler2 = new GetAcademicSchedulesBySemesterIdQueryHandler(unitOfWorkMock2.Object, mapperMock2.Object);

            // Assert
            Assert.IsNotNull(handler1, "First handler instance should not be null.");
            Assert.IsNotNull(handler2, "Second handler instance should not be null.");
            Assert.AreNotSame(handler1, handler2, "Constructor returned the same instance for different inputs, indicating unexpected shared state.");
        }
    }
}