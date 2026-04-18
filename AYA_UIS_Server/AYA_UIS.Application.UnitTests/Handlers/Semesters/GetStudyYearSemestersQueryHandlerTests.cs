using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Semesters;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Semesters;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.SemesterDtos;

namespace AYA_UIS.Application.Handlers.Semesters.UnitTests
{
    [TestClass]
    public partial class GetStudyYearSemestersQueryHandlerTests
    {
        /// <summary>
        /// Tests that Handle calls the repository with the provided StudyYearId and returns whatever the mapper produces.
        /// Conditions: iterates several StudyYearId edge values (int.MinValue, -1, 0, 1, int.MaxValue).
        /// Expected: repository.GetByStudyYearIdAsync is invoked with the same id and the result returned by mapper.Map is forwarded.
        /// </summary>
        [TestMethod]
        public async Task Handle_VariousStudyYearIds_InvokesRepositoryAndReturnsMappedDtos()
        {
            // Arrange
            int[] testIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int id in testIds)
            {
                var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
                var semesterRepoMock = new Mock<ISemesterRepository>(MockBehavior.Strict);
                unitOfWorkMock.Setup(u => u.Semesters).Returns(semesterRepoMock.Object);

                // Create a mock IEnumerable<Semester> instance so we don't need to construct domain entities.
                var semestersEnumerableMock = new Mock<IEnumerable<Semester>>(MockBehavior.Strict);

                semesterRepoMock
                    .Setup(r => r.GetByStudyYearIdAsync(id))
                    .ReturnsAsync(semestersEnumerableMock.Object);

                var mapperMock = new Mock<IMapper>(MockBehavior.Strict);

                // Expected DTOs: use a mock IEnumerable<SemesterDto> to avoid relying on DTO constructors.
                var expectedDtosMock = new Mock<IEnumerable<SemesterDto>>(MockBehavior.Strict);

                // Setup mapper to return the expected DTOs when passed the repository result object.
                mapperMock
                    .Setup(m => m.Map<IEnumerable<SemesterDto>>(semestersEnumerableMock.Object))
                    .Returns(expectedDtosMock.Object);

                var handler = new GetStudyYearSemestersQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

                // Act
                var result = await handler.Handle(new GetStudyYearSemestersQuery(id), CancellationToken.None).ConfigureAwait(false);

                // Assert
                Assert.AreSame(expectedDtosMock.Object, result, $"Returned value should be the same instance produced by the mapper for StudyYearId={id}.");
                semesterRepoMock.Verify(r => r.GetByStudyYearIdAsync(id), Times.Once, $"Repository should be called once with StudyYearId={id}.");
                mapperMock.Verify(m => m.Map<IEnumerable<SemesterDto>>(semestersEnumerableMock.Object), Times.Once, $"Mapper should be called once with the repository result for StudyYearId={id}.");

                // Cleanup verifications for next iteration
                unitOfWorkMock.Verify(u => u.Semesters, Times.AtLeastOnce);
            }
        }

        /// <summary>
        /// Tests that Handle forwards a null repository result to the mapper and returns the mapper output.
        /// Conditions: repository returns null for the semesters enumerable.
        /// Expected: mapper.Map is invoked with null and its return (null or other) is returned by Handle.
        /// </summary>
        [TestMethod]
        public async Task Handle_WhenRepositoryReturnsNull_MapperCalledWithNullAndResultReturned()
        {
            // Arrange
            int studyYearId = 42;
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var semesterRepoMock = new Mock<ISemesterRepository>(MockBehavior.Strict);
            unitOfWorkMock.Setup(u => u.Semesters).Returns(semesterRepoMock.Object);

            semesterRepoMock
                .Setup(r => r.GetByStudyYearIdAsync(It.IsAny<int>()))
                .ReturnsAsync((IEnumerable<Semester>?)null);

            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            // Mapper should accept null source and we simulate it returning null result.
            mapperMock
                .Setup(m => m.Map<IEnumerable<SemesterDto>>(It.Is<object?>(o => o == null)))
                .Returns((IEnumerable<SemesterDto>?)null);

            var handler = new GetStudyYearSemestersQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Act
            var result = await handler.Handle(new GetStudyYearSemestersQuery(studyYearId), CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result, "When repository returns null and mapper returns null, the handler should return null.");
            semesterRepoMock.Verify(r => r.GetByStudyYearIdAsync(studyYearId), Times.Once);
            mapperMock.Verify(m => m.Map<IEnumerable<SemesterDto>>(It.Is<object?>(o => o == null)), Times.Once);
            unitOfWorkMock.Verify(u => u.Semesters, Times.AtLeastOnce);
        }

        /// <summary>
        /// Verifies that the constructor initializes an instance when valid dependencies are provided.
        /// Input conditions: valid non-null IUnitOfWork and IMapper mocks.
        /// Expected result: instance is created and implements the expected MediatR IRequestHandler interface.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstanceAndImplementsInterface()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mockMapper = new Mock<IMapper>(MockBehavior.Loose);

            // Act
            var handler = new GetStudyYearSemestersQueryHandler(mockUnitOfWork.Object, mockMapper.Object);

            // Assert
            Assert.IsNotNull(handler, "Handler should not be null when constructed with valid dependencies.");
            Assert.IsInstanceOfType(
                handler,
                typeof(IRequestHandler<GetStudyYearSemestersQuery, IEnumerable<SemesterDto>>),
                "Handler should implement IRequestHandler<GetStudyYearSemestersQuery, IEnumerable<SemesterDto>>.");
        }

        /// <summary>
        /// Ensures constructor behaves consistently across different mock configurations and produces distinct instances.
        /// Input conditions: two sets of mocks using different MockBehavior values (Loose and Strict).
        /// Expected result: no exceptions thrown, both instances created, and instances are not the same reference.
        /// </summary>
        [TestMethod]
        public void Constructor_VariousMockConfigurations_DoesNotThrowAndCreatesDistinctInstances()
        {
            // Arrange & Act for first configuration (Loose)
            var mockUnitOfWork1 = new Mock<IUnitOfWork>(MockBehavior.Loose);
            var mockMapper1 = new Mock<IMapper>(MockBehavior.Loose);
            var handler1 = new GetStudyYearSemestersQueryHandler(mockUnitOfWork1.Object, mockMapper1.Object);

            // Arrange & Act for second configuration (Strict)
            var mockUnitOfWork2 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mockMapper2 = new Mock<IMapper>(MockBehavior.Strict);
            var handler2 = new GetStudyYearSemestersQueryHandler(mockUnitOfWork2.Object, mockMapper2.Object);

            // Assert
            Assert.IsNotNull(handler1, "First handler instance should not be null.");
            Assert.IsNotNull(handler2, "Second handler instance should not be null.");
            Assert.AreNotSame(handler1, handler2, "Each constructor call should produce a distinct instance.");
        }
    }
}