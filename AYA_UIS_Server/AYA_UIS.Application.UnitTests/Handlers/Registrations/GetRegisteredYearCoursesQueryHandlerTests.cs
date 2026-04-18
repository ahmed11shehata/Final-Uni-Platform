using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
#nullable enable
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Registrations;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Registrations;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.RegistrationDtos;

namespace AYA_UIS.Application.Handlers.Registrations.UnitTests
{
    [TestClass]
    public partial class GetRegisteredYearCoursesQueryHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor initializes a non-null instance when provided valid non-null dependencies.
        /// Input conditions: valid (mocked) IUnitOfWork and IMapper instances.
        /// Expected result: an instance of GetRegisteredYearCoursesQueryHandler is created and implements the expected IRequestHandler interface.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mockMapper = new Mock<IMapper>(MockBehavior.Strict);

            // Act
            var handler = new GetRegisteredYearCoursesQueryHandler(mockUnitOfWork.Object, mockMapper.Object);

            // Assert
            Assert.IsNotNull(handler, "Constructor returned null for valid dependencies.");
            Assert.IsInstanceOfType(
                handler,
                typeof(IRequestHandler<GetRegisteredYearCoursesQuery, List<RegistrationCourseDto>>),
                "Handler does not implement the expected IRequestHandler interface.");
        }

        /// <summary>
        /// Ensures that constructing multiple handlers with different dependency instances produces distinct instances and does not throw.
        /// Input conditions: two different pairs of mocked IUnitOfWork and IMapper instances.
        /// Expected result: both constructions succeed and yield distinct handler instances.
        /// </summary>
        [TestMethod]
        public void Constructor_MultipleDistinctDependencies_ProducesDistinctInstances()
        {
            // Arrange
            var mockUnitOfWorkA = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mockMapperA = new Mock<IMapper>(MockBehavior.Strict);

            var mockUnitOfWorkB = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mockMapperB = new Mock<IMapper>(MockBehavior.Strict);

            // Act
            GetRegisteredYearCoursesQueryHandler handlerA = null!;
            GetRegisteredYearCoursesQueryHandler handlerB = null!;

            Exception? exA = null;
            Exception? exB = null;

            try
            {
                handlerA = new GetRegisteredYearCoursesQueryHandler(mockUnitOfWorkA.Object, mockMapperA.Object);
            }
            catch (Exception e)
            {
                exA = e;
            }

            try
            {
                handlerB = new GetRegisteredYearCoursesQueryHandler(mockUnitOfWorkB.Object, mockMapperB.Object);
            }
            catch (Exception e)
            {
                exB = e;
            }

            // Assert
            Assert.IsNull(exA, "Constructor threw an unexpected exception for first set of dependencies.");
            Assert.IsNull(exB, "Constructor threw an unexpected exception for second set of dependencies.");
            Assert.IsNotNull(handlerA, "First handler instance is null after construction.");
            Assert.IsNotNull(handlerB, "Second handler instance is null after construction.");
            Assert.AreNotSame(handlerA, handlerB, "Two handlers constructed with different dependencies should not be the same instance.");
        }

        /// <summary>
        /// Ensures that when the repository returns a registrations enumerable the handler
        /// forwards that enumerable to AutoMapper and returns the mapped list unchanged.
        /// Input: studentId = non-empty string, year = 2023, repository returns empty enumerable.
        /// Expected: mapper.Map is invoked with the same registrations object and the handler returns
        /// exactly the list returned by the mapper.
        /// </summary>
        [TestMethod]
        public async Task Handle_WhenRepositoryReturnsRegistrations_MapsAndReturnsList()
        {
            // Arrange
            string studentId = "student-1";
            int year = 2023;

            IEnumerable<AYA_UIS.Core.Domain.Entities.Models.Registration> registrations = Array.Empty<AYA_UIS.Core.Domain.Entities.Models.Registration>();
            var expectedMapped = new List<RegistrationCourseDto> { new RegistrationCourseDto { Id = 42 } };

            var registrationsRepoMock = new Mock<IRegistrationRepository>();
            registrationsRepoMock
                .Setup(r => r.GetByUserAndStudyYearAsync(studentId, year))
                .ReturnsAsync(registrations);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .Setup(u => u.Registrations)
                .Returns(registrationsRepoMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock
                .Setup(m => m.Map<List<RegistrationCourseDto>>(registrations))
                .Returns(expectedMapped);

            var handler = new GetRegisteredYearCoursesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);
            var request = new GetRegisteredYearCoursesQuery(studentId, year);

            // Act
            List<RegistrationCourseDto> result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.AreSame(expectedMapped, result, "Handler must return exactly the list produced by the mapper.");
            registrationsRepoMock.Verify(r => r.GetByUserAndStudyYearAsync(studentId, year), Times.Once);
            mapperMock.Verify(m => m.Map<List<RegistrationCourseDto>>(registrations), Times.Once);
        }

        /// <summary>
        /// Verifies that the handler calls the repository with the provided year for a variety
        /// of boundary and special int values and returns whatever the mapper produces.
        /// Inputs: studentId non-empty, years tested: int.MinValue, -1, 0, 1, int.MaxValue.
        /// Expected: no exceptions; repository invoked with each year; handler returns mapper result.
        /// </summary>
        [TestMethod]
        public async Task Handle_WithVariousYearBoundaries_InvokesRepositoryWithGivenYearAndReturnsMapperResult()
        {
            // Arrange common values
            string studentId = "boundary-student";
            int[] yearsToTest = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int year in yearsToTest)
            {
                IEnumerable<AYA_UIS.Core.Domain.Entities.Models.Registration> registrations = Array.Empty<AYA_UIS.Core.Domain.Entities.Models.Registration>();
                var expectedMapped = new List<RegistrationCourseDto>(); // mapper returns an (empty) list for simplicity

                var registrationsRepoMock = new Mock<IRegistrationRepository>();
                registrationsRepoMock
                    .Setup(r => r.GetByUserAndStudyYearAsync(studentId, year))
                    .ReturnsAsync(registrations);

                var unitOfWorkMock = new Mock<IUnitOfWork>();
                unitOfWorkMock
                    .Setup(u => u.Registrations)
                    .Returns(registrationsRepoMock.Object);

                var mapperMock = new Mock<IMapper>();
                mapperMock
                    .Setup(m => m.Map<List<RegistrationCourseDto>>(registrations))
                    .Returns(expectedMapped);

                var handler = new GetRegisteredYearCoursesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);
                var request = new GetRegisteredYearCoursesQuery(studentId, year);

                // Act & Assert: ensure no exception and correct wiring for each year
                List<RegistrationCourseDto> result = await handler.Handle(request, CancellationToken.None);
                Assert.IsNotNull(result, $"Result for year {year} should not be null.");
                Assert.AreSame(expectedMapped, result, $"Returned list must be the mapper result for year {year}.");
                registrationsRepoMock.Verify(r => r.GetByUserAndStudyYearAsync(studentId, year), Times.Once, $"Repository should be called once for year {year}.");
                mapperMock.Verify(m => m.Map<List<RegistrationCourseDto>>(registrations), Times.Once, $"Mapper should be called once for year {year}.");
            }
        }
    }
}