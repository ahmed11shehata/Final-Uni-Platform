using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
#nullable enable
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.CoursePrequisites;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.CoursePrequisites;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.CourseDtos;

namespace AYA_UIS.Application.Handlers.CoursePrequisites.UnitTests
{
    [TestClass]
    public partial class GetCourseDependenciesQueryHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor successfully creates an instance when valid dependencies are provided.
        /// Input conditions: non-null IUnitOfWork and IMapper mocks.
        /// Expected result: instance is constructed and implements the expected IRequestHandler interface; no exception is thrown.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidDependencies_InstanceCreated()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockMapper = new Mock<IMapper>();

            // Act
            var handler = new GetCourseDependenciesQueryHandler(mockUnitOfWork.Object, mockMapper.Object);

            // Assert
            Assert.IsNotNull(handler, "Constructor returned null when valid dependencies were provided.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetCourseDependenciesQuery, List<CourseDto>>), "Constructed object does not implement the expected IRequestHandler interface.");
        }

        /// <summary>
        /// Partial test for null dependency behavior.
        /// Purpose: Document and surface the fact that the constructor does not declare nullability for parameters and no explicit null checks exist.
        /// Input conditions: attempting to pass null for IUnitOfWork or IMapper.
        /// Expected result: This test is marked Inconclusive to avoid making assumptions about intended null-handling behavior.
        /// Guidance: If the constructor should guard against null arguments, add ArgumentNullException checks in production code and then replace this Inconclusive test with explicit exception assertions.
        /// </summary>
        [TestMethod]
        public void Constructor_NullDependency_Inconclusive_NeedSpecification()
        {
            // Arrange & Act & Assert
            // The source constructor parameters are non-nullable reference types (no '?' annotation).
            // The production constructor currently does not perform ArgumentNullException checks,
            // therefore constructing with null should not throw. We assert that an instance is created
            // and implements the expected interface. If you later add null-checks to the constructor,
            // update this test to expect ArgumentNullException.
            var handler = new GetCourseDependenciesQueryHandler((IUnitOfWork)null!, (IMapper)null!);
            Assert.IsNotNull(handler, "Constructor returned null when null dependencies were provided.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetCourseDependenciesQuery, List<CourseDto>>), "Constructed object does not implement the expected IRequestHandler interface.");
        }

        /// <summary>
        /// Tests that for various CourseId values the handler calls the repository with the same id
        /// and returns exactly what the mapper returns.
        /// Input conditions: multiple CourseId values including int.MinValue, negative, zero and int.MaxValue.
        /// Expected result: repository GetCourseDependenciesAsync invoked with same CourseId and returned mapped list instance is returned by handler.
        /// </summary>
        [TestMethod]
        public async Task Handle_VariousCourseIds_InvokesRepositoryAndReturnsMappedDtos()
        {
            // Arrange
            int[] courseIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int id in courseIds)
            {
                // create fresh mocks per iteration to avoid cross-test interactions
                var uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
                var courseRepoMock = new Mock<Domain.Contracts.ICourseRepository>(MockBehavior.Strict);
                uowMock.Setup(u => u.Courses).Returns(courseRepoMock.Object);

                // repository returns an empty sequence (no need to construct Course instances)
                IEnumerable<global::AYA_UIS.Core.Domain.Entities.Models.Course> dependencies = Array.Empty<global::AYA_UIS.Core.Domain.Entities.Models.Course>();

                // mapper will return a concrete list instance that the handler should return
                var mappedList = new List<CourseDto>
                {
                    new CourseDto { Id = 42, Code = "C42", Name = "Course42", Credits = 3, Status = default }
                };

                int capturedId = id; // avoid modified closure issues
                courseRepoMock
                    .Setup(r => r.GetCourseDependenciesAsync(It.Is<int>(v => v == capturedId)))
                    .ReturnsAsync(dependencies);

                var mapperMock = new Mock<IMapper>(MockBehavior.Strict);
                mapperMock
                    .Setup(m => m.Map<List<CourseDto>>(It.Is<object>(o => ReferenceEquals(o, dependencies))))
                    .Returns(mappedList);

                var handler = new GetCourseDependenciesQueryHandler(uowMock.Object, mapperMock.Object);

                // Act
                List<CourseDto>? result = await handler.Handle(new GetCourseDependenciesQuery(capturedId), CancellationToken.None);

                // Assert
                Assert.IsNotNull(result, "Handler returned null for a valid mapper return; expected the mapped list instance.");
                Assert.AreSame(mappedList, result, "Handler should return the exact list instance produced by the mapper.");
                courseRepoMock.Verify(r => r.GetCourseDependenciesAsync(It.Is<int>(v => v == capturedId)), Times.Once);
                mapperMock.Verify(m => m.Map<List<CourseDto>>(It.Is<object>(o => ReferenceEquals(o, dependencies))), Times.Once);
            }
        }

        /// <summary>
        /// Tests behavior when repository returns null.
        /// Input conditions: repository returns null (no dependencies), mapper is expected to receive null and may return null.
        /// Expected result: handler returns whatever the mapper returns (null in this test).
        /// </summary>
        [TestMethod]
        public async Task Handle_RepositoryReturnsNull_MapperReceivesNullAndReturnsNull_ResultIsNull()
        {
            // Arrange
            var uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var courseRepoMock = new Mock<Domain.Contracts.ICourseRepository>(MockBehavior.Strict);
            uowMock.Setup(u => u.Courses).Returns(courseRepoMock.Object);

            courseRepoMock
                .Setup(r => r.GetCourseDependenciesAsync(It.IsAny<int>()))
                .ReturnsAsync((IEnumerable<global::AYA_UIS.Core.Domain.Entities.Models.Course>?)null);

            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            mapperMock
                .Setup(m => m.Map<List<CourseDto>>(It.Is<object>(o => o == null)))
                .Returns((List<CourseDto>?)null);

            var handler = new GetCourseDependenciesQueryHandler(uowMock.Object, mapperMock.Object);

            // Act
            List<CourseDto>? result = await handler.Handle(new GetCourseDependenciesQuery(10), CancellationToken.None);

            // Assert
            Assert.IsNull(result, "When repository returns null and mapper maps null to null, handler should return null.");
            courseRepoMock.Verify(r => r.GetCourseDependenciesAsync(It.IsAny<int>()), Times.Once);
            mapperMock.Verify(m => m.Map<List<CourseDto>>(It.Is<object>(o => o == null)), Times.Once);
        }

    }
}