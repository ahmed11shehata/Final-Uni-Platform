using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Registrations;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Registrations;
using AYA_UIS.Core.Domain.Entities.Models;
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
    public class GetRegisteredSemesterCoursesQueryHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor creates a non-null instance when valid, non-null dependencies are provided.
        /// Input conditions: valid Mock&lt;IUnitOfWork&gt; and Mock&lt;IMapper&gt; objects are passed (non-null).
        /// Expected result: an instance of GetRegisteredSemesterCoursesQueryHandler is created and implements the expected IRequestHandler interface.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidDependencies_ShouldCreateInstanceAndImplementIRequestHandler()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            // Act
            var handler = new GetRegisteredSemesterCoursesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Assert
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetRegisteredSemesterCoursesQuery, List<RegistrationCourseDto>>));
        }

        /// <summary>
        /// Ensures that constructing multiple handlers with different dependency instances yields distinct handler instances.
        /// Input conditions: two different pairs of Mock&lt;IUnitOfWork&gt; and Mock&lt;IMapper&gt; objects.
        /// Expected result: both handler instances are non-null and not the same reference.
        /// </summary>
        [TestMethod]
        public void Constructor_MultipleDistinctDependencies_ShouldCreateIndependentInstances()
        {
            // Arrange
            var unitOfWorkMock1 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mapperMock1 = new Mock<IMapper>(MockBehavior.Strict);

            var unitOfWorkMock2 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mapperMock2 = new Mock<IMapper>(MockBehavior.Strict);

            // Act
            var handler1 = new GetRegisteredSemesterCoursesQueryHandler(unitOfWorkMock1.Object, mapperMock1.Object);
            var handler2 = new GetRegisteredSemesterCoursesQueryHandler(unitOfWorkMock2.Object, mapperMock2.Object);

            // Assert
            Assert.IsNotNull(handler1);
            Assert.IsNotNull(handler2);
            Assert.AreNotSame(handler1, handler2);
            Assert.IsInstanceOfType(handler1, typeof(IRequestHandler<GetRegisteredSemesterCoursesQuery, List<RegistrationCourseDto>>));
            Assert.IsInstanceOfType(handler2, typeof(IRequestHandler<GetRegisteredSemesterCoursesQuery, List<RegistrationCourseDto>>));
        }

        /// <summary>
        /// Test that when the registration repository returns an empty collection the handler
        /// calls the repository with the provided parameters, calls the mapper with the repository result,
        /// and returns the mapper's result.
        /// Input: StudyYearId = 1, SemesterId = 1, StudentId = "student1", repository returns empty enumerable.
        /// Expected: repository invoked once with the exact parameters, mapper invoked with the empty enumerable,
        /// and the handler returns the exact list instance produced by the mapper.
        /// </summary>
        [TestMethod]
        public async Task Handle_RepositoryReturnsEmpty_MapperInvokedAndReturnedListIsReturned()
        {
            // Arrange
            var studentId = "student1";
            var studyYearId = 1;
            var semesterId = 1;
            var query = new GetRegisteredSemesterCoursesQuery(studyYearId, semesterId, studentId);

            var repoMock = new Mock<IRegistrationRepository>(MockBehavior.Strict);
            IEnumerable<Registration> repoResult = Enumerable.Empty<Registration>();
            repoMock
                .Setup(r => r.GetByUserAndStudyYearAndSemseterAsync(studentId, studyYearId, semesterId))
                .ReturnsAsync(repoResult)
                .Verifiable();

            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            unitOfWorkMock.Setup(u => u.Registrations).Returns(repoMock.Object);

            var expectedMapped = new List<RegistrationCourseDto>();
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            mapperMock
                .Setup(m => m.Map<List<RegistrationCourseDto>>(It.Is<object>(o => object.ReferenceEquals(o, repoResult))))
                .Returns(expectedMapped)
                .Verifiable();

            var handler = new GetRegisteredSemesterCoursesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.AreSame(expectedMapped, result, "Handler should return the exact list instance returned by the mapper.");
            repoMock.Verify(r => r.GetByUserAndStudyYearAndSemseterAsync(studentId, studyYearId, semesterId), Times.Once);
            mapperMock.Verify(m => m.Map<List<RegistrationCourseDto>>(It.Is<object>(o => object.ReferenceEquals(o, repoResult))), Times.Once);
        }

        /// <summary>
        /// Test that when the registration repository returns null the handler forwards null to the mapper
        /// and returns whatever the mapper returns.
        /// Input: StudyYearId = 2, SemesterId = 3, StudentId = "student-null", repository returns null.
        /// Expected: mapper is invoked with null and the handler returns the mapper's result.
        /// </summary>
        [TestMethod]
        public async Task Handle_RepositoryReturnsNull_MapperReceivesNullAndReturnedListIsReturned()
        {
            // Arrange
            var studentId = "student-null";
            var studyYearId = 2;
            var semesterId = 3;
            var query = new GetRegisteredSemesterCoursesQuery(studyYearId, semesterId, studentId);

            var repoMock = new Mock<IRegistrationRepository>(MockBehavior.Strict);
            IEnumerable<Registration>? repoResult = null;
            repoMock
                .Setup(r => r.GetByUserAndStudyYearAndSemseterAsync(studentId, studyYearId, semesterId))
                .ReturnsAsync(repoResult)
                .Verifiable();

            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            unitOfWorkMock.Setup(u => u.Registrations).Returns(repoMock.Object);

            var expectedMapped = new List<RegistrationCourseDto> { new RegistrationCourseDto() };
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            mapperMock
                .Setup(m => m.Map<List<RegistrationCourseDto>>(It.Is<object>(o => o == null)))
                .Returns(expectedMapped)
                .Verifiable();

            var handler = new GetRegisteredSemesterCoursesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.AreSame(expectedMapped, result, "Handler should return the exact list instance returned by the mapper when repository returns null.");
            repoMock.Verify(r => r.GetByUserAndStudyYearAndSemseterAsync(studentId, studyYearId, semesterId), Times.Once);
            mapperMock.Verify(m => m.Map<List<RegistrationCourseDto>>(It.Is<object>(o => o == null)), Times.Once);
        }

        /// <summary>
        /// Test that various edge-case parameter values are passed unchanged to the repository.
        /// Inputs: multiple StudyYearId, SemesterId (including int.MinValue/int.MaxValue/0/-1) and StudentId values
        /// (empty, whitespace, long, special chars).
        /// Expected: repository is invoked with the exact tuple of parameters for each case.
        /// </summary>
        [TestMethod]
        public async Task Handle_VariousEdgeParameterValues_PassedToRepositoryExactly()
        {
            // Arrange - list of test cases
            var testCases = new List<(int studyYearId, int semesterId, string studentId)>
            {
                (int.MinValue, int.MinValue, string.Empty),
                (int.MaxValue, int.MaxValue, "   "), // whitespace-only student id
                (0, 0, "normalStudent"),
                (-1, -1, "special\u0000chars"),
                (1, 2, new string('x', 1000)) // very long student id
            };

            foreach (var (studyYearId, semesterId, studentId) in testCases)
            {
                var query = new GetRegisteredSemesterCoursesQuery(studyYearId, semesterId, studentId);

                var repoMock = new Mock<IRegistrationRepository>(MockBehavior.Strict);
                var repoResult = Enumerable.Empty<Registration>();
                repoMock
                    .Setup(r => r.GetByUserAndStudyYearAndSemseterAsync(studentId, studyYearId, semesterId))
                    .ReturnsAsync(repoResult)
                    .Verifiable();

                var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
                unitOfWorkMock.Setup(u => u.Registrations).Returns(repoMock.Object);

                var mapperMock = new Mock<IMapper>(MockBehavior.Strict);
                mapperMock
                    .Setup(m => m.Map<List<RegistrationCourseDto>>(It.IsAny<object>()))
                    .Returns(new List<RegistrationCourseDto>())
                    .Verifiable();

                var handler = new GetRegisteredSemesterCoursesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

                // Act
                var result = await handler.Handle(query, CancellationToken.None);

                // Assert
                Assert.IsNotNull(result, "Handler should return a non-null list (mapper result).");
                repoMock.Verify(r => r.GetByUserAndStudyYearAndSemseterAsync(studentId, studyYearId, semesterId), Times.Once,
                    $"Repository should be called once with studyYearId={studyYearId}, semesterId={semesterId}, studentId='{studentId}'.");
                mapperMock.Verify(m => m.Map<List<RegistrationCourseDto>>(It.IsAny<object>()), Times.Once);

                // Cleanup verifications for next iteration (Moq Strict ensures no leftover setups)
            }
        }
    }
}