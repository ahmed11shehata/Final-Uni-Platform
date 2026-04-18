#nullable enable
using AYA_UIS.Application.Handlers.UserStudyYears;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.UserStudyYears;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Domain.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module.UserStudyYearDtos;
using Shared.Respones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace AYA_UIS.Application.Handlers.UserStudyYears.UnitTests
{
    [TestClass]
    public partial class GetCurrentUserStudyYearQueryHandlerTests
    {
        /// <summary>
        /// Verifies that when there is no current UserStudyYear returned from the repository,
        /// the handler returns an error response with the expected error message.
        /// Input: request.UserId is an empty string and repository returns null.
        /// Expected: Response.Success == false, Data == null, Errors == specific message.
        /// </summary>
        [TestMethod]
        public async Task Handle_NoCurrentUserStudyYear_ReturnsErrorResponse()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var userRepoMock = new Mock<IUserStudyYearRepository>();

            string userId = string.Empty; // test empty string scenario

            userRepoMock
                .Setup(r => r.GetCurrentByUserIdAsync(userId))
                .ReturnsAsync((UserStudyYear?)null);

            unitOfWorkMock.Setup(u => u.UserStudyYears).Returns(userRepoMock.Object);

            var handler = new GetCurrentUserStudyYearQueryHandler(unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(new GetCurrentUserStudyYearQuery(userId), CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success, "Expected Success to be false when no current study year exists.");
            Assert.IsNull(result.Data, "Expected Data to be null for error response.");
            Assert.AreEqual("No current study year found for this user.", result.Errors, "Unexpected error message.");
        }

        /// <summary>
        /// Verifies that when a current UserStudyYear exists and an active semester is present,
        /// the handler returns a successful response with mapped DTO fields and CurrentSemesterId set to active semester Id.
        /// Input: repository returns a UserStudyYear with StudyYear, and semesters repository returns a semester that includes UtcNow.
        /// Expected: Response.Success == true, Data mapping matches source entity values and CurrentSemesterId equals semester.Id.
        /// </summary>
        [TestMethod]
        public async Task Handle_CurrentUserWithActiveSemester_ReturnsSuccessWithCurrentSemesterId()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var userRepoMock = new Mock<IUserStudyYearRepository>();
            var semesterRepoMock = new Mock<ISemesterRepository>();

            string userId = "user-123";
            var now = DateTime.UtcNow;

            var studyYear = new StudyYear
            {
                StartYear = 2022,
                EndYear = 2023,
                IsCurrent = true
            };

            var currentUserStudyYear = new UserStudyYear
            {
                Id = 11,
                UserId = userId,
                StudyYearId = 7,
                StudyYear = studyYear,
                Level = Levels.First_Year,
                EnrolledAt = now.AddDays(-30)
            };

            var activeSemester = new Semester
            {
                Id = 99,
                StartDate = now.AddDays(-1),
                EndDate = now.AddDays(1),
                StudyYearId = studyYear.Id
            };

            userRepoMock
                .Setup(r => r.GetCurrentByUserIdAsync(userId))
                .ReturnsAsync(currentUserStudyYear);

            semesterRepoMock
                .Setup(s => s.GetByStudyYearIdAsync(currentUserStudyYear.StudyYearId))
                .ReturnsAsync(new List<Semester> { activeSemester });

            unitOfWorkMock.Setup(u => u.UserStudyYears).Returns(userRepoMock.Object);
            unitOfWorkMock.Setup(u => u.Semesters).Returns(semesterRepoMock.Object);

            var handler = new GetCurrentUserStudyYearQueryHandler(unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(new GetCurrentUserStudyYearQuery(userId), CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success, "Expected Success for existing current user study year.");
            Assert.IsNotNull(result.Data, "Expected Data to be present for success response.");

            var dto = result.Data!;
            Assert.AreEqual(currentUserStudyYear.Id, dto.Id);
            Assert.AreEqual(currentUserStudyYear.UserId, dto.UserId);
            Assert.AreEqual(currentUserStudyYear.StudyYearId, dto.StudyYearId);
            Assert.AreEqual(currentUserStudyYear.StudyYear!.StartYear, dto.StartYear);
            Assert.AreEqual(currentUserStudyYear.StudyYear.EndYear, dto.EndYear);
            Assert.AreEqual(currentUserStudyYear.Level, dto.Level);
            Assert.AreEqual(currentUserStudyYear.Level.ToString().Replace("_", " "), dto.LevelName);
            Assert.AreEqual(currentUserStudyYear.StudyYear.IsCurrent, dto.IsCurrent);
            Assert.AreEqual(currentUserStudyYear.EnrolledAt, dto.EnrolledAt);
            Assert.AreEqual(activeSemester.Id, dto.CurrentSemesterId);
        }

        /// <summary>
        /// Verifies that when semester repository returns no semesters (empty collection),
        /// the handler still succeeds and sets CurrentSemesterId to null.
        /// Input: repository returns a UserStudyYear and semesters repository returns empty Enumerable.
        /// Expected: Response.Success == true and Data.CurrentSemesterId == null.
        /// </summary>
        [TestMethod]
        public async Task Handle_CurrentUserWithNoSemesters_ReturnsSuccessWithNullCurrentSemesterId()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var userRepoMock = new Mock<IUserStudyYearRepository>();
            var semesterRepoMock = new Mock<ISemesterRepository>();

            string userId = "user-empty-semesters";
            var currentUserStudyYear = new UserStudyYear
            {
                Id = 22,
                UserId = userId,
                StudyYearId = 15,
                StudyYear = new StudyYear { StartYear = 2020, EndYear = 2021, IsCurrent = false },
                Level = Levels.Second_Year,
                EnrolledAt = DateTime.UtcNow.AddMonths(-6)
            };

            userRepoMock
                .Setup(r => r.GetCurrentByUserIdAsync(userId))
                .ReturnsAsync(currentUserStudyYear);

            semesterRepoMock
                .Setup(s => s.GetByStudyYearIdAsync(currentUserStudyYear.StudyYearId))
                .ReturnsAsync(Enumerable.Empty<Semester>());

            unitOfWorkMock.Setup(u => u.UserStudyYears).Returns(userRepoMock.Object);
            unitOfWorkMock.Setup(u => u.Semesters).Returns(semesterRepoMock.Object);

            var handler = new GetCurrentUserStudyYearQueryHandler(unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(new GetCurrentUserStudyYearQuery(userId), CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Data);
            Assert.IsNull(result.Data!.CurrentSemesterId, "Expected CurrentSemesterId to be null when no semesters are returned.");
        }

        /// <summary>
        /// Verifies that exceptions thrown by the semesters repository are non-critical (swallowed)
        /// and the handler still returns a successful response with CurrentSemesterId == null.
        /// Input: semester repository throws an exception.
        /// Expected: Response.Success == true and Data.CurrentSemesterId == null.
        /// </summary>
        [TestMethod]
        public async Task Handle_SemesterRepositoryThrows_ExceptionIsIgnoredAndReturnsSuccessWithNullCurrentSemesterId()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var userRepoMock = new Mock<IUserStudyYearRepository>();
            var semesterRepoMock = new Mock<ISemesterRepository>();

            string userId = "user-exception-semesters";
            var currentUserStudyYear = new UserStudyYear
            {
                Id = 33,
                UserId = userId,
                StudyYearId = 5,
                StudyYear = new StudyYear { StartYear = 2019, EndYear = 2020, IsCurrent = false },
                Level = Levels.Preparatory_Year,
                EnrolledAt = DateTime.UtcNow.AddYears(-1)
            };

            userRepoMock
                .Setup(r => r.GetCurrentByUserIdAsync(userId))
                .ReturnsAsync(currentUserStudyYear);

            semesterRepoMock
                .Setup(s => s.GetByStudyYearIdAsync(It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            unitOfWorkMock.Setup(u => u.UserStudyYears).Returns(userRepoMock.Object);
            unitOfWorkMock.Setup(u => u.Semesters).Returns(semesterRepoMock.Object);

            var handler = new GetCurrentUserStudyYearQueryHandler(unitOfWorkMock.Object);

            // Act
            var result = await handler.Handle(new GetCurrentUserStudyYearQuery(userId), CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success, "Handler should swallow semester-repository exceptions and still succeed.");
            Assert.IsNotNull(result.Data);
            Assert.IsNull(result.Data!.CurrentSemesterId, "Expected CurrentSemesterId to be null when semester retrieval fails.");
        }
    }
}