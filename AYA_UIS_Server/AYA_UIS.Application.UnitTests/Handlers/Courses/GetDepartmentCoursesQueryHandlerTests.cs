using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
#nullable enable
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Courses;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Courses;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.CourseDtos;

namespace AYA_UIS.Application.Handlers.Courses.UnitTests
{
    /// <summary>
    /// Tests for GetDepartmentCoursesQueryHandler constructor behavior.
    /// </summary>
    [TestClass]
    public partial class GetDepartmentCoursesQueryHandlerTests
    {
        /// <summary>
        /// Ensures that constructor creates an instance when valid, non-null dependencies are provided.
        /// Input: mocked IUnitOfWork and IMapper (non-null).
        /// Expected: instance is not null and implements the expected IRequestHandler interface.
        /// </summary>
        [TestMethod]
        public void Ctor_ValidDependencies_InstanceCreatedAndImplementsInterface()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            // Act
            var handler = new GetDepartmentCoursesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Assert
            Assert.IsNotNull(handler, "Constructor returned null for valid dependencies.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetDepartmentCoursesQuery, IEnumerable<CourseDto>>),
                "Handler does not implement the expected IRequestHandler interface.");
        }

        /// <summary>
        /// Verifies that multiple constructions with different dependency instances produce distinct handler instances.
        /// Input: two different mocked dependency pairs.
        /// Expected: two handler instances are not the same reference and both are constructed without throwing.
        /// </summary>
        [TestMethod]
        public void Ctor_DifferentDependencyInstances_ProducesDistinctHandlerInstances()
        {
            // Arrange
            var unitOfWorkMock1 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mapperMock1 = new Mock<IMapper>(MockBehavior.Strict);

            var unitOfWorkMock2 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mapperMock2 = new Mock<IMapper>(MockBehavior.Strict);

            // Act
            var handler1 = new GetDepartmentCoursesQueryHandler(unitOfWorkMock1.Object, mapperMock1.Object);
            var handler2 = new GetDepartmentCoursesQueryHandler(unitOfWorkMock2.Object, mapperMock2.Object);

            // Assert
            Assert.IsNotNull(handler1, "First handler instance should not be null.");
            Assert.IsNotNull(handler2, "Second handler instance should not be null.");
            Assert.AreNotSame(handler1, handler2, "Constructor returned the same instance for different dependency inputs.");
        }

        /// <summary>
        /// Partial test placeholder: The constructor's behavior when null is passed is not defined in source (parameters are non-nullable).
        /// This test is marked inconclusive to avoid assigning null to non-nullable parameters while documenting the scenario.
        /// Input: null for dependencies (not performed).
        /// Expected: clarify desired behavior (throw ArgumentNullException or allow null) before enabling this test.
        /// </summary>
        [TestMethod]
        public void Ctor_NullDependencies_ConclusiveTestRequired()
        {
            // Arrange - production code does not guard against null, so verify construction with nulls
            IUnitOfWork? nullUnitOfWork = null;
            IMapper? nullMapper = null;

            // Act
            var handler = new GetDepartmentCoursesQueryHandler(nullUnitOfWork, nullMapper);

            // Assert
            Assert.IsNotNull(handler, "Constructor returned null when dependencies are null.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetDepartmentCoursesQuery, IEnumerable<CourseDto>>),
                "Handler does not implement the expected IRequestHandler interface when constructed with null dependencies.");
        }

        /// <summary>
        /// Test that Handle returns the mapped CourseDto sequence when the department exists.
        /// Conditions: IUnitOfWork.Departments.GetByIdAsync returns a Department instance and
        /// IUnitOfWork.Courses.GetDepartmentCoursesAsync returns a course enumerable. IMapper.Map returns a known CourseDto collection.
        /// Expected: The returned enumerable is the mapped result and the repository methods are invoked with the provided DepartmentId.
        /// </summary>
        [TestMethod]
        public void Handle_DepartmentExists_ReturnsMappedCourseDtos()
        {
            // Arrange - test a range of DepartmentId boundary values to ensure invocation uses the id parameter correctly
            int[] testIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int id in testIds)
            {
                var deptRepoMock = new Mock<IDepartmentRepository>();
                deptRepoMock
                    .Setup(d => d.GetByIdAsync(id))
                    .ReturnsAsync(new Department()); // department exists

                // Use an empty courses collection - avoid constructing Course instances
                IEnumerable<Course> courses = Array.Empty<Course>();
                var courseRepoMock = new Mock<ICourseRepository>();
                courseRepoMock
                    .Setup(c => c.GetDepartmentCoursesAsync(id))
                    .ReturnsAsync(courses);

                var unitOfWorkMock = new Mock<IUnitOfWork>();
                unitOfWorkMock.Setup(u => u.Departments).Returns(deptRepoMock.Object);
                unitOfWorkMock.Setup(u => u.Courses).Returns(courseRepoMock.Object);

                var expectedDtos = new List<CourseDto>
                {
                    new CourseDto { Id = 1, Code = "C1", Name = "Test Course", Credits = 3, Status = default }
                };

                var mapperMock = new Mock<IMapper>();
                // Map receives the courses object (passed as object in original code)
                mapperMock
                    .Setup(m => m.Map<IEnumerable<CourseDto>>(It.IsAny<object>()))
                    .Returns(expectedDtos);

                var handler = new GetDepartmentCoursesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);
                var request = new GetDepartmentCoursesQuery(id);

                // Act
                IEnumerable<CourseDto> result = handler.Handle(request, CancellationToken.None).GetAwaiter().GetResult();

                // Assert
                Assert.IsNotNull(result, $"Result should not be null for DepartmentId={id}");
                // The handler returns whatever the mapper returned; assert reference equality to ensure mapping result is passed through
                Assert.AreSame(expectedDtos, result, $"Returned sequence must be the mapped instance for DepartmentId={id}");

                // Verify the repository methods were called with the expected id
                courseRepoMock.Verify(c => c.GetDepartmentCoursesAsync(id), Times.Once, $"GetDepartmentCoursesAsync should be called once for DepartmentId={id}");
                deptRepoMock.Verify(d => d.GetByIdAsync(id), Times.Once, $"GetByIdAsync should be called once for DepartmentId={id}");

                // Verify mapper was invoked with the courses enumerable
                mapperMock.Verify(m => m.Map<IEnumerable<CourseDto>>(It.Is<object>(o => ReferenceEquals(o, courses))), Times.Once, $"Mapper.Map should be called once with the courses enumerable for DepartmentId={id}");
            }
        }
    }
}