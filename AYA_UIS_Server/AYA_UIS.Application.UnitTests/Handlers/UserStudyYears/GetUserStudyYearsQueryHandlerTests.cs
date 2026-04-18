using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.UserStudyYears;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.UserStudyYears;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.UserStudyYearDtos;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.UserStudyYears.UnitTests
{
    [TestClass]
    public partial class GetUserStudyYearsQueryHandlerTests
    {
        /// <summary>
        /// Purpose: Verify that when repository returns an empty sequence for a valid user id,
        /// the handler returns a successful Response with an empty Data list.
        /// Input: request.UserId = non-empty string; repository returns empty IEnumerable.
        /// Expected: Response.Success == true and Data is an empty list (Count == 0).
        /// </summary>
        [TestMethod]
        public async Task Handle_NoRecords_ReturnsEmptyListResponse()
        {
            // Arrange
            var userId = "user-empty";
            var mockRepo = new Mock<IUserStudyYearRepository>();
            mockRepo.Setup(r => r.GetByUserIdAsync(userId))
                    .ReturnsAsync(Enumerable.Empty<UserStudyYear>());

            var mockUnit = new Mock<IUnitOfWork>();
            mockUnit.Setup(u => u.UserStudyYears).Returns(mockRepo.Object);

            var handler = new GetUserStudyYearsQueryHandler(mockUnit.Object);
            var request = new GetUserStudyYearsQuery(userId);

            // Act
            Response<List<UserStudyYearDto>> result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(0, result.Data!.Count);
        }

        /// <summary>
        /// Purpose: Verify mapping behavior when repository returns a single UserStudyYear whose StudyYear is null.
        /// Input: repository returns one UserStudyYear with StudyYear == null and Level containing underscore.
        /// Expected: StartYear and EndYear default to 0, IsCurrent defaults to false, LevelName underscores replaced with spaces,
        /// other fields mapped exactly.
        /// </summary>
        [TestMethod]
        public async Task Handle_RecordWithNullStudyYear_MapsDefaultsProperly()
        {
            // Arrange
            var userId = "user-null-studyyear";
            var enrolledAt = new DateTime(2023, 9, 1, 0, 0, 0, DateTimeKind.Utc);

            var entity = new UserStudyYear
            {
                Id = 42,
                UserId = userId,
                StudyYearId = 99,
                StudyYear = null, // critical for default mapping
                Level = Levels.Preparatory_Year,
                EnrolledAt = enrolledAt
            };

            var mockRepo = new Mock<IUserStudyYearRepository>();
            mockRepo.Setup(r => r.GetByUserIdAsync(userId))
                    .ReturnsAsync(new[] { entity });

            var mockUnit = new Mock<IUnitOfWork>();
            mockUnit.Setup(u => u.UserStudyYears).Returns(mockRepo.Object);

            var handler = new GetUserStudyYearsQueryHandler(mockUnit.Object);
            var request = new GetUserStudyYearsQuery(userId);

            // Act
            Response<List<UserStudyYearDto>> result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(1, result.Data!.Count);

            var dto = result.Data![0];
            Assert.AreEqual(entity.Id, dto.Id);
            Assert.AreEqual(entity.UserId, dto.UserId);
            Assert.AreEqual(entity.StudyYearId, dto.StudyYearId);

            // When StudyYear is null, defaults applied
            Assert.AreEqual(0, dto.StartYear);
            Assert.AreEqual(0, dto.EndYear);
            Assert.IsFalse(dto.IsCurrent);

            // Level name underscores replaced with spaces
            Assert.AreEqual(entity.Level, dto.Level);
            Assert.AreEqual(entity.Level.ToString().Replace("_", " "), dto.LevelName);

            // EnrolledAt mapped
            Assert.AreEqual(enrolledAt, dto.EnrolledAt);
        }

        /// <summary>
        /// Purpose: Verify mapping of multiple records including when StudyYear is present (non-null) and IsCurrent true.
        /// Input: repository returns two records: one with StudyYear populated, one without.
        /// Expected: Both records mapped, StartYear/EndYear/IsCurrent preserved from StudyYear when present,
        /// LevelName formatted, and the returned list contains both items.
        /// </summary>
        [TestMethod]
        public async Task Handle_MultipleRecords_MapsAllAndPreservesValues()
        {
            // Arrange
            var userId = "user-multi";
            var now = DateTime.UtcNow;

            var entityWithStudyYear = new UserStudyYear
            {
                Id = 1,
                UserId = userId,
                StudyYearId = 10,
                StudyYear = new StudyYear { StartYear = 2022, EndYear = 2023, IsCurrent = true },
                Level = Levels.Second_Year,
                EnrolledAt = now
            };

            var entityWithoutStudyYear = new UserStudyYear
            {
                Id = 2,
                UserId = userId,
                StudyYearId = 11,
                StudyYear = null,
                Level = Levels.Third_Year,
                EnrolledAt = now.AddDays(-1)
            };

            var mockRepo = new Mock<IUserStudyYearRepository>();
            mockRepo.Setup(r => r.GetByUserIdAsync(userId))
                    .ReturnsAsync(new[] { entityWithStudyYear, entityWithoutStudyYear });

            var mockUnit = new Mock<IUnitOfWork>();
            mockUnit.Setup(u => u.UserStudyYears).Returns(mockRepo.Object);

            var handler = new GetUserStudyYearsQueryHandler(mockUnit.Object);
            var request = new GetUserStudyYearsQuery(userId);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(2, result.Data!.Count);

            var dto1 = result.Data!.First(d => d.Id == entityWithStudyYear.Id);
            Assert.AreEqual(entityWithStudyYear.StudyYear!.StartYear, dto1.StartYear);
            Assert.AreEqual(entityWithStudyYear.StudyYear.EndYear, dto1.EndYear);
            Assert.IsTrue(dto1.IsCurrent);
            Assert.AreEqual(entityWithStudyYear.Level.ToString().Replace("_", " "), dto1.LevelName);
            Assert.AreEqual(entityWithStudyYear.EnrolledAt, dto1.EnrolledAt);

            var dto2 = result.Data!.First(d => d.Id == entityWithoutStudyYear.Id);
            Assert.AreEqual(0, dto2.StartYear);
            Assert.AreEqual(0, dto2.EndYear);
            Assert.IsFalse(dto2.IsCurrent);
            Assert.AreEqual(entityWithoutStudyYear.Level.ToString().Replace("_", " "), dto2.LevelName);
            Assert.AreEqual(entityWithoutStudyYear.EnrolledAt, dto2.EnrolledAt);
        }

        /// <summary>
        /// Ensures that constructing GetUserStudyYearsQueryHandler with a valid IUnitOfWork:
        /// - does not throw,
        /// - returns a non-null instance,
        /// - and the instance implements the expected IRequestHandler interface.
        /// Input: a mocked, non-null IUnitOfWork instance.
        /// Expected: successful construction and correct type.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidUnitOfWork_CreatesInstance()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            IUnitOfWork unitOfWork = unitOfWorkMock.Object;

            // Act
            GetUserStudyYearsQueryHandler? handler = null;
            Exception? thrown = null;
            try
            {
                handler = new GetUserStudyYearsQueryHandler(unitOfWork);
            }
            catch (Exception ex)
            {
                thrown = ex;
            }

            // Assert
            Assert.IsNull(thrown, "Constructor threw an unexpected exception for a valid IUnitOfWork.");
            Assert.IsNotNull(handler, "Handler instance should not be null after construction.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetUserStudyYearsQuery, Response<List<UserStudyYearDto>>>), "Handler should implement the expected IRequestHandler interface.");
        }

        /// <summary>
        /// Sanity check: constructing multiple handlers with different IUnitOfWork instances
        /// should produce distinct handler instances and not share instance identity.
        /// Input: two different mocked IUnitOfWork instances.
        /// Expected: two distinct handler instances (by reference) are created successfully.
        /// </summary>
        [TestMethod]
        public void Constructor_WithDifferentUnitOfWorkInstances_CreatesDistinctHandlerInstances()
        {
            // Arrange
            var mock1 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mock2 = new Mock<IUnitOfWork>(MockBehavior.Strict);

            IUnitOfWork uow1 = mock1.Object;
            IUnitOfWork uow2 = mock2.Object;

            // Act
            var handler1 = new GetUserStudyYearsQueryHandler(uow1);
            var handler2 = new GetUserStudyYearsQueryHandler(uow2);

            // Assert
            Assert.IsNotNull(handler1);
            Assert.IsNotNull(handler2);
            Assert.AreNotSame(handler1, handler2, "Separate constructor invocations should produce distinct handler instances.");
        }
    }
}