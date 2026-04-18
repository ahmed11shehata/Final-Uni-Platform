using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
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
    public partial class GetRegisteredCoursesQueryHandlerTests
    {
        /// <summary>
        /// Verifies that when the repository returns null registrations, the mapper is invoked with null
        /// and the handler returns whatever the mapper produces (null in this test).
        /// Input: repository returns null; mapper configured to return null.
        /// Expected: returned value is null and repository GetByUserAsync is called with the provided StudentId.
        /// </summary>
        [TestMethod]
        public async Task Handle_WithNullRegistrations_MapperReceivesNullAndReturnsNull()
        {
            // Arrange
            var studentId = string.Empty; // valid non-null StudentId (edge case: empty string)
            var request = new GetRegisteredCoursesQuery(studentId);

            var registrationRepoMock = new Mock<IRegistrationRepository>();
            registrationRepoMock
                .Setup(r => r.GetByUserAsync(studentId, It.IsAny<int?>()))
                .ReturnsAsync((IEnumerable<global::AYA_UIS.Core.Domain.Entities.Models.Registration>?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .Setup(u => u.Registrations)
                .Returns(registrationRepoMock.Object);

            var mapperMock = new Mock<IMapper>();
            // Configure mapper to return null when mapping null
            mapperMock
                .Setup(m => m.Map<List<RegistrationCourseDto>>(It.Is<object?>(o => o == null)))
                .Returns((List<RegistrationCourseDto>?)null!);

            var handler = new GetRegisteredCoursesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Act
            var result = await handler.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result, "Expected null result when mapper returns null for null registrations.");
            registrationRepoMock.Verify(r => r.GetByUserAsync(studentId, It.IsAny<int?>()), Times.Once);
            mapperMock.Verify(m => m.Map<List<RegistrationCourseDto>>(It.Is<object?>(o => o == null)), Times.Once);
        }

        /// <summary>
        /// Verifies that when the repository returns an empty collection, the mapper is invoked with that collection
        /// and the handler returns the mapped empty list.
        /// Input: repository returns an empty registrations collection; mapper returns an empty list.
        /// Expected: returned list is non-null and has Count == 0.
        /// </summary>
        [TestMethod]
        public async Task Handle_WithEmptyRegistrations_ReturnsEmptyList()
        {
            // Arrange
            var studentId = "student-empty";
            var request = new GetRegisteredCoursesQuery(studentId);

            var emptyRegistrations = Array.Empty<global::AYA_UIS.Core.Domain.Entities.Models.Registration>();

            var registrationRepoMock = new Mock<IRegistrationRepository>();
            registrationRepoMock
                .Setup(r => r.GetByUserAsync(studentId, It.IsAny<int?>()))
                .ReturnsAsync((IEnumerable<global::AYA_UIS.Core.Domain.Entities.Models.Registration>)emptyRegistrations);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .Setup(u => u.Registrations)
                .Returns(registrationRepoMock.Object);

            var expectedList = new List<RegistrationCourseDto>(); // empty mapped list
            var mapperMock = new Mock<IMapper>();
            mapperMock
                .Setup(m => m.Map<List<RegistrationCourseDto>>(It.IsAny<object>()))
                .Returns(expectedList);

            var handler = new GetRegisteredCoursesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Act
            var result = await handler.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result, "Expected non-null list even when source registrations are empty.");
            Assert.AreEqual(0, result.Count, "Expected an empty mapped list for empty registrations.");
            registrationRepoMock.Verify(r => r.GetByUserAsync(studentId, It.IsAny<int?>()), Times.Once);
            mapperMock.Verify(m => m.Map<List<RegistrationCourseDto>>(It.IsAny<object>()), Times.Once);
        }

        /// <summary>
        /// Verifies that when the repository returns registrations, the mapper's returned list is returned by the handler.
        /// Input: repository returns a non-empty collection; mapper returns a corresponding list with items.
        /// Expected: returned list equals the mapper's return value and repository was queried with the given StudentId.
        /// </summary>
        [TestMethod]
        public async Task Handle_WithRegistrations_ReturnsMappedListAndCallsRepoWithStudentId()
        {
            // Arrange
            var studentId = "student-123";
            var request = new GetRegisteredCoursesQuery(studentId);

            var someRegistrations = new List<global::AYA_UIS.Core.Domain.Entities.Models.Registration>
            {
                // No need to populate properties; mapper is mocked to return expected output based on object identity
                // We create two placeholder instances to simulate multiple registrations.
                (global::AYA_UIS.Core.Domain.Entities.Models.Registration?)Activator.CreateInstance(typeof(global::AYA_UIS.Core.Domain.Entities.Models.Registration))!,
                (global::AYA_UIS.Core.Domain.Entities.Models.Registration?)Activator.CreateInstance(typeof(global::AYA_UIS.Core.Domain.Entities.Models.Registration))!
            };

            var registrationRepoMock = new Mock<IRegistrationRepository>();
            registrationRepoMock
                .Setup(r => r.GetByUserAsync(studentId, It.IsAny<int?>()))
                .ReturnsAsync((IEnumerable<global::AYA_UIS.Core.Domain.Entities.Models.Registration>)someRegistrations);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .Setup(u => u.Registrations)
                .Returns(registrationRepoMock.Object);

            var expectedList = new List<RegistrationCourseDto>
            {
                new RegistrationCourseDto(),
                new RegistrationCourseDto()
            };

            var mapperMock = new Mock<IMapper>();
            // Ensure mapper is invoked with the registrations object reference and returns expectedList
            mapperMock
                .Setup(m => m.Map<List<RegistrationCourseDto>>(It.Is<object>(o => ReferenceEquals(o, someRegistrations))))
                .Returns(expectedList);

            var handler = new GetRegisteredCoursesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Act
            var result = await handler.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result, "Expected mapped list to be returned for non-empty registrations.");
            Assert.AreEqual(expectedList.Count, result.Count, "Expected the returned list to have the same count as the mapper result.");
            Assert.AreSame(expectedList, result, "Handler should return the exact list object produced by the mapper.");
            registrationRepoMock.Verify(r => r.GetByUserAsync(studentId, It.IsAny<int?>()), Times.Once);
            mapperMock.Verify(m => m.Map<List<RegistrationCourseDto>>(It.IsAny<object>()), Times.Once);
        }

        /// <summary>
        /// Verifies that the handler forwards various StudentId edge-case values to the repository unchanged.
        /// Input: different StudentId values (empty, whitespace, long, special chars).
        /// Expected: repository GetByUserAsync is invoked with the exact StudentId for each case.
        /// </summary>
        [TestMethod]
        public async Task Handle_WithVariousStudentIds_CallsRepositoryWithSameStudentId()
        {
            // Arrange - various non-null StudentId edge cases
            var studentIds = new[]
            {
                "", // empty
                "   ", // whitespace-only
                new string('a', 1024), // very long
                "user\nwith\tspecial\u0000chars" // control/special characters
            };

            foreach (var studentId in studentIds)
            {
                var request = new GetRegisteredCoursesQuery(studentId);

                var registrationRepoMock = new Mock<IRegistrationRepository>();
                registrationRepoMock
                    .Setup(r => r.GetByUserAsync(studentId, It.IsAny<int?>()))
                    .ReturnsAsync((IEnumerable<global::AYA_UIS.Core.Domain.Entities.Models.Registration>)Array.Empty<global::AYA_UIS.Core.Domain.Entities.Models.Registration>());

                var unitOfWorkMock = new Mock<IUnitOfWork>();
                unitOfWorkMock
                    .Setup(u => u.Registrations)
                    .Returns(registrationRepoMock.Object);

                var mapperMock = new Mock<IMapper>();
                mapperMock
                    .Setup(m => m.Map<List<RegistrationCourseDto>>(It.IsAny<object>()))
                    .Returns(new List<RegistrationCourseDto>());

                var handler = new GetRegisteredCoursesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

                // Act
                var result = await handler.Handle(request, CancellationToken.None).ConfigureAwait(false);

                // Assert
                Assert.IsNotNull(result, "Result should not be null for valid mapper return value.");
                registrationRepoMock.Verify(r => r.GetByUserAsync(studentId, It.IsAny<int?>()), Times.Once);

                // Reset verifications for next iteration
                registrationRepoMock.Invocations.Clear();
            }
        }

        /// <summary>
        /// Verifies that the constructor creates a non-null instance when provided with valid dependencies.
        /// Input conditions: a valid IUnitOfWork mock and a valid IMapper mock.
        /// Expected result: the constructor returns a non-null instance that implements IRequestHandler&lt;GetRegisteredCoursesQuery, List&lt;RegistrationCourseDto&gt;&gt; and does not throw.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesHandlerAndImplementsInterface()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mapperMock = new Mock<IMapper>();

            // Act
            var handler = new GetRegisteredCoursesQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Assert
            Assert.IsNotNull(handler, "Constructor returned null when valid dependencies were provided.");
            Assert.IsInstanceOfType(
                handler,
                typeof(IRequestHandler<GetRegisteredCoursesQuery, List<RegistrationCourseDto>>),
                "Instance does not implement the expected IRequestHandler interface."
            );
        }

        /// <summary>
        /// Ensures constructor is tolerant to different mock behaviors and produces distinct, usable instances.
        /// Input conditions: two different configurations of mocks (combinations of MockBehavior.Strict and MockBehavior.Loose).
        /// Expected result: no exceptions are thrown, each constructed handler is non-null and implements the expected interface,
        /// and handlers created with different dependency instances are distinct objects.
        /// </summary>
        [TestMethod]
        public void Constructor_WithVariousMockBehaviors_DoesNotThrowAndCreatesDistinctInstances()
        {
            // Arrange
            var configs = new List<(Mock<IUnitOfWork> uow, Mock<IMapper> mapper)>
            {
                (new Mock<IUnitOfWork>(MockBehavior.Loose), new Mock<IMapper>(MockBehavior.Loose)),
                (new Mock<IUnitOfWork>(MockBehavior.Strict), new Mock<IMapper>(MockBehavior.Strict))
            };

            var createdHandlers = new List<IRequestHandler<GetRegisteredCoursesQuery, List<RegistrationCourseDto>>>();

            foreach (var (uowMock, mapperMock) in configs)
            {
                // Act
                IRequestHandler<GetRegisteredCoursesQuery, List<RegistrationCourseDto>> handler = null!;
                Exception? caught = null;
                try
                {
                    handler = new GetRegisteredCoursesQueryHandler(uowMock.Object, mapperMock.Object);
                }
                catch (Exception ex)
                {
                    caught = ex;
                }

                // Assert - per iteration
                Assert.IsNull(caught, $"Constructor threw an exception for mock configuration: {caught?.Message}");
                Assert.IsNotNull(handler, "Constructor returned null for a valid mock configuration.");
                Assert.IsInstanceOfType(
                    handler,
                    typeof(IRequestHandler<GetRegisteredCoursesQuery, List<RegistrationCourseDto>>),
                    "Constructed instance does not implement the expected interface."
                );

                createdHandlers.Add(handler);
            }

            // Final asserts: ensure instances from different configurations are distinct
            Assert.AreEqual(2, createdHandlers.Count, "Unexpected number of handlers created.");
            Assert.AreNotSame(createdHandlers[0], createdHandlers[1], "Handlers created with different dependencies should be distinct instances.");
        }
    }
}