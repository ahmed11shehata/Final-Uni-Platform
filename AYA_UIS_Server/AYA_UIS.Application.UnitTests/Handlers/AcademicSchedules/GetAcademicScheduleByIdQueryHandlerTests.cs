using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.AcademicSchedules;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.AcademicSchedules;
using AYA_UIS.Core.Domain.Entities;
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

namespace AYA_UIS.Application.Handlers.AcademicSchedules.UnitTests
{
    [TestClass]
    public partial class GetAcademicScheduleByIdQueryHandlerTests
    {
        /// <summary>
        /// Verifies that when an AcademicSchedule exists for the provided Id the handler maps all expected properties
        /// to AcademicScheduleDto and returns it.
        /// Input conditions tested:
        /// - Several CreatedAt boundary values (DateTime.MinValue, DateTime.MaxValue)
        /// - Title as a very long string, Url with special characters, Description as empty or whitespace.
        /// Expected result:
        /// - Returned AcademicScheduleDto properties exactly match the source AcademicSchedule fields.
        /// </summary>
        [TestMethod]
        public async Task Handle_ExistingSchedule_ReturnsMappedDto_ForVariousCreatedAtAndDescriptions()
        {
            // Arrange
            int testId = 42;
            var longTitle = new string('A', 1000);
            var specialUrl = "https://example.com/αβγ?query=1&flag=true";
            var testCases = new (DateTime createdAt, string description)[]
            {
                (DateTime.MinValue, string.Empty),
                (DateTime.MaxValue, "   ") // whitespace description
            };

            foreach (var (createdAt, description) in testCases)
            {
                var schedule = new AcademicSchedule
                {
                    Id = testId,
                    Title = longTitle,
                    Url = specialUrl,
                    Description = description,
                    CreatedAt = createdAt
                };

                var mockAcademicSchedules = new Mock<IAcademicScheduleRepository>();
                mockAcademicSchedules
                    .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(schedule);

                var mockUnitOfWork = new Mock<IUnitOfWork>();
                mockUnitOfWork.SetupGet(u => u.AcademicSchedules).Returns(mockAcademicSchedules.Object);

                var handler = new GetAcademicScheduleByIdQueryHandler(mockUnitOfWork.Object);
                var query = new GetAcademicScheduleByIdQuery(testId);
                CancellationToken ct = CancellationToken.None;

                // Act
                var result = await handler.Handle(query, ct).ConfigureAwait(false);

                // Assert
                Assert.IsNotNull(result, "Expected non-null AcademicScheduleDto for existing schedule.");
                Assert.AreEqual(schedule.Id, result.Id, "Id should be mapped exactly.");
                Assert.AreEqual(schedule.Title, result.Title, "Title should be mapped exactly (including long strings).");
                Assert.AreEqual(schedule.Url, result.Url, "Url should be mapped exactly (including special characters).");
                Assert.AreEqual(schedule.Description ?? string.Empty, result.Description, "Description should be mapped exactly (empty/whitespace handled).");
                Assert.AreEqual(schedule.CreatedAt, result.CreatedAt, "CreatedAt should be mapped exactly (boundary values).");
            }
        }

        /// <summary>
        /// Verifies that when no AcademicSchedule exists for the provided Ids the handler throws NotFoundException
        /// with an informative message containing the requested id.
        /// Input conditions tested:
        /// - A range of integer ids including int.MinValue, negative, zero, positive, int.MaxValue.
        /// Expected result:
        /// - NotFoundException is thrown and the message contains the id value.
        /// </summary>
        [TestMethod]
        public async Task Handle_ScheduleNotFound_ThrowsNotFoundException_ForVariousIds()
        {
            // Arrange
            var idsToTest = new int[] { int.MinValue, -1, 0, 1, int.MaxValue };

            var mockAcademicSchedules = new Mock<IAcademicScheduleRepository>();
            mockAcademicSchedules
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((AcademicSchedule?)null);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.SetupGet(u => u.AcademicSchedules).Returns(mockAcademicSchedules.Object);

            var handler = new GetAcademicScheduleByIdQueryHandler(mockUnitOfWork.Object);
            CancellationToken ct = CancellationToken.None;

            foreach (var id in idsToTest)
            {
                var query = new GetAcademicScheduleByIdQuery(id);

                // Act & Assert
                try
                {
                    await handler.Handle(query, ct).ConfigureAwait(false);
                    Assert.Fail($"Expected NotFoundException for id {id}, but no exception was thrown.");
                }
                catch (NotFoundException ex)
                {
                    StringAssert.Contains(ex.Message, $"Academic Schedule with id {id} not found.");
                }
            }
        }

        /// <summary>
        /// Verifies that when a valid IUnitOfWork implementation is provided the constructor
        /// creates an instance successfully and that the created instance implements the expected MediatR interface.
        /// Input conditions: a non-null mocked IUnitOfWork.
        /// Expected result: instance is non-null and is an IRequestHandler for GetAcademicScheduleByIdQuery returning AcademicScheduleDto.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidUnitOfWork_CreatesInstanceAndImplementsInterface()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            var handler = new GetAcademicScheduleByIdQueryHandler(mockUnitOfWork.Object);

            // Assert
            Assert.IsNotNull(handler, "Handler instance should not be null when constructed with a valid IUnitOfWork.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetAcademicScheduleByIdQuery, AcademicScheduleDto>), "Handler should implement IRequestHandler<GetAcademicScheduleByIdQuery, AcademicScheduleDto>.");
        }

        /// <summary>
        /// Documents the null-handling behavior for the constructor.
        /// Input conditions: passing null for the IUnitOfWork parameter (undefined behavior in source).
        /// Expected result: test marked inconclusive because the source code does not specify whether the constructor
        /// should guard against null. If the intended behavior is to throw ArgumentNullException, update this test
        /// to assert that. This avoids making incorrect assumptions about unspecified null semantics.
        /// </summary>
        [TestMethod]
        public void Constructor_NullUnitOfWork_InconclusiveDueToUnspecifiedNullHandling()
        {
            // Arrange
            // NOTE: The constructor parameter is non-nullable in source. The class does not explicitly guard against null.
            // Per generation rules we must not assume behavior. This test asserts the current behavior: constructor accepts null.
            IUnitOfWork? nullUow = null;

            // Act
            var handler = new GetAcademicScheduleByIdQueryHandler(nullUow);

            // Assert
            Assert.IsNotNull(handler, "Handler instance should be created even when constructed with a null IUnitOfWork (current behavior).");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetAcademicScheduleByIdQuery, AcademicScheduleDto>), "Handler should implement IRequestHandler<GetAcademicScheduleByIdQuery, AcademicScheduleDto>.");
        }
    }
}