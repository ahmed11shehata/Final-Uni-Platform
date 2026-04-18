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
using AYA_UIS.Core.Domain.Entities.Models;
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
    public class GetCoursePrequisitesQueryHandlerTests
    {
        /// <summary>
        /// Verifies that Handle forwards the CourseId to the repository and returns
        /// the exact mapped list produced by AutoMapper for several representative CourseId values.
        /// Inputs: various CourseId extremes and typical values (int.MinValue, -1, 0, 1, int.MaxValue).
        /// Expected: the repository is called with the same CourseId and the handler returns the same
        /// List&lt;CourseDto&gt; instance produced by the mapper without throwing.
        /// </summary>
        [TestMethod]
        public async Task Handle_ForVariousCourseIds_ReturnsMappedDtosAndCallsRepository()
        {
            // Arrange
            int[] testIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int courseId in testIds)
            {
                var course = new Course
                {
                    Id = 7,
                    Code = "C-101",
                    Name = "Sample",
                    Credits = 3,
                    DepartmentId = 1,
                    Status = 0
                };

                IEnumerable<Course>? prerequisites = new List<Course> { course };

                var mappedList = new List<CourseDto>
                {
                    new CourseDto { Id = 7, Code = "C-101", Name = "Sample", Credits = 3, Status = 0 }
                };

                var repoMock = new Mock<ICourseRepository>(MockBehavior.Strict);
                repoMock
                    .Setup(r => r.GetCoursePrerequisitesAsync(courseId))
                    .ReturnsAsync(prerequisites)
                    .Verifiable();

                var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
                unitOfWorkMock
                    .SetupGet(u => u.Courses)
                    .Returns(repoMock.Object)
                    .Verifiable();

                var mapperMock = new Mock<IMapper>(MockBehavior.Strict);
                mapperMock
                    .Setup(m => m.Map<List<CourseDto>>(It.Is<object>(o => ReferenceEquals(o, prerequisites))))
                    .Returns(mappedList)
                    .Verifiable();

                var handler = new GetCoursePrequisitesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);
                var request = new GetCoursePrequisitesQuery(courseId);

                // Act
                List<CourseDto>? result = await handler.Handle(request, CancellationToken.None);

                // Assert
                Assert.IsNotNull(result, "Result should not be null for non-null mapper return.");
                Assert.AreSame(mappedList, result, "Handler must return the exact list instance produced by the mapper.");
                unitOfWorkMock.VerifyGet(u => u.Courses, Times.Once, "UnitOfWork.Courses should be accessed exactly once.");
                repoMock.Verify(r => r.GetCoursePrerequisitesAsync(courseId), Times.Once, $"Repository should be called once with CourseId {courseId}.");
                mapperMock.Verify(m => m.Map<List<CourseDto>>(It.Is<object>(o => ReferenceEquals(o, prerequisites))), Times.Once);

                // Cleanup verifications for next iteration
                unitOfWorkMock.VerifyNoOtherCalls();
                repoMock.VerifyNoOtherCalls();
                mapperMock.VerifyNoOtherCalls();
            }
        }

        /// <summary>
        /// Ensures that when the repository returns an empty collection the mapper is invoked
        /// and the handler returns an empty list (as produced by the mapper).
        /// Inputs: repository returns an empty IEnumerable&lt;Course&gt;.
        /// Expected: returned list is empty and the repository and mapper are each called once.
        /// </summary>
        [TestMethod]
        public async Task Handle_RepositoryReturnsEmpty_ReturnsEmptyMappedList()
        {
            // Arrange
            int courseId = 42;
            IEnumerable<Course> prerequisites = Enumerable.Empty<Course>();
            var mappedEmpty = new List<CourseDto>();

            var repoMock = new Mock<ICourseRepository>(MockBehavior.Strict);
            repoMock
                .Setup(r => r.GetCoursePrerequisitesAsync(courseId))
                .ReturnsAsync(prerequisites)
                .Verifiable();

            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            unitOfWorkMock
                .SetupGet(u => u.Courses)
                .Returns(repoMock.Object)
                .Verifiable();

            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            mapperMock
                .Setup(m => m.Map<List<CourseDto>>(It.Is<object>(o => ReferenceEquals(o, prerequisites))))
                .Returns(mappedEmpty)
                .Verifiable();

            var handler = new GetCoursePrequisitesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);
            var request = new GetCoursePrequisitesQuery(courseId);

            // Act
            List<CourseDto>? result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result, "Resulting list should not be null when mapper returns an empty list.");
            Assert.AreEqual(0, result.Count, "Expected an empty mapped list.");
            unitOfWorkMock.VerifyGet(u => u.Courses, Times.Once);
            repoMock.Verify(r => r.GetCoursePrerequisitesAsync(courseId), Times.Once);
            mapperMock.Verify(m => m.Map<List<CourseDto>>(It.Is<object>(o => ReferenceEquals(o, prerequisites))), Times.Once);

            unitOfWorkMock.VerifyNoOtherCalls();
            repoMock.VerifyNoOtherCalls();
            mapperMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Tests behavior when the repository returns null. The handler should pass null to the mapper
        /// and return whatever the mapper returns (including null) without throwing.
        /// Inputs: repository returns null, mapper returns null.
        /// Expected: handler returns null and no exceptions are thrown.
        /// </summary>
        [TestMethod]
        public async Task Handle_RepositoryReturnsNull_MapperReceivesNullAndHandlerReturnsMapperResult()
        {
            // Arrange
            int courseId = 99;
            IEnumerable<Course>? prerequisites = null;
            List<CourseDto>? mapperResult = null;

            var repoMock = new Mock<ICourseRepository>(MockBehavior.Strict);
            repoMock
                .Setup(r => r.GetCoursePrerequisitesAsync(courseId))
                .ReturnsAsync(prerequisites)
                .Verifiable();

            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            unitOfWorkMock
                .SetupGet(u => u.Courses)
                .Returns(repoMock.Object)
                .Verifiable();

            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            mapperMock
                .Setup(m => m.Map<List<CourseDto>>(It.Is<object>(o => o == null)))
                .Returns(mapperResult)
                .Verifiable();

            var handler = new GetCoursePrequisitesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);
            var request = new GetCoursePrequisitesQuery(courseId);

            // Act
            List<CourseDto>? result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsNull(result, "Handler should return null when mapper returns null for a null source.");
            unitOfWorkMock.VerifyGet(u => u.Courses, Times.Once);
            repoMock.Verify(r => r.GetCoursePrerequisitesAsync(courseId), Times.Once);
            mapperMock.Verify(m => m.Map<List<CourseDto>>(It.Is<object>(o => o == null)), Times.Once);

            unitOfWorkMock.VerifyNoOtherCalls();
            repoMock.VerifyNoOtherCalls();
            mapperMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Verifies that the constructor creates an instance when valid, non-null dependencies are provided.
        /// Input conditions: Mock implementations of IUnitOfWork and IMapper are supplied.
        /// Expected result: An instance of GetCoursePrequisitesQueryHandler is returned and implements
        /// IRequestHandler&lt;GetCoursePrequisitesQuery, List&lt;CourseDto&gt;&gt; without throwing.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mapperMock = new Mock<IMapper>();

            // Act
            GetCoursePrequisitesQueryHandler? handler = null;
            Exception? exception = null;
            try
            {
                handler = new GetCoursePrequisitesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception, "Constructor threw an unexpected exception when passed valid mocks.");
            Assert.IsNotNull(handler, "Constructor returned null for a valid set of dependencies.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetCoursePrequisitesQuery, List<CourseDto>>), "Instance does not implement the expected IRequestHandler interface.");
        }
    }
}