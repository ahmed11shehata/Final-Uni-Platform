using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.StudyYears;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Semesters;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.SemesterDtos;

namespace AYA_UIS.Application.Handlers.StudyYears.UnitTests
{
    [TestClass]
    public class GetStudyYearSemestersQueryHandlerTests
    {
        /// <summary>
        /// Tests that Handle returns a list of mapped SemesterDto instances for various StudyYearId values.
        /// Input conditions: repository returns a single Semester entity. StudyYearId is exercised with boundary values (int.MinValue, -1, 0, 1, int.MaxValue).
        /// Expected result: the returned list contains one SemesterDto with properties mapped from the Semester entity and the repository is invoked with the provided StudyYearId.
        /// </summary>
        [TestMethod]
        public async Task Handle_VariousStudyYearIds_ReturnsMappedDtos()
        {
            // Arrange: define test ids to exercise boundaries
            int[] testIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int studyYearId in testIds)
            {
                // Arrange: prepare a semester entity to be returned by repository
                var start = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var end = new DateTime(2020, 6, 1, 0, 0, 0, DateTimeKind.Utc);
                var semesterEntity = new Semester
                {
                    Id = 42,
                    Title = (SemesterEnum)1,
                    StartDate = start,
                    EndDate = end,
                    StudyYearId = studyYearId
                };

                var mockSemesterRepo = new Mock<ISemesterRepository>();
                mockSemesterRepo
                    .Setup(r => r.GetByStudyYearIdAsync(It.Is<int>(i => i == studyYearId)))
                    .ReturnsAsync(new List<Semester> { semesterEntity });

                var mockUnitOfWork = new Mock<IUnitOfWork>();
                mockUnitOfWork.Setup(u => u.Semesters).Returns(mockSemesterRepo.Object);

                var handler = new GetStudyYearSemestersQueryHandler(mockUnitOfWork.Object);

                // Act
                var result = await handler.Handle(new GetStudyYearSemestersQuery(studyYearId), CancellationToken.None);

                // Assert
                Assert.IsNotNull(result, "Result should not be null for valid repository return.");
                Assert.AreEqual(1, result.Count, "Expected exactly one mapped SemesterDto.");
                var dto = result.First();
                Assert.AreEqual(semesterEntity.Id, dto.Id, "Id should be mapped correctly.");
                Assert.AreEqual(semesterEntity.Title, dto.Title, "Title should be mapped correctly.");
                Assert.AreEqual(semesterEntity.StartDate, dto.StartDate, "StartDate should be mapped correctly.");
                Assert.AreEqual(semesterEntity.EndDate, dto.EndDate, "EndDate should be mapped correctly.");

                // Verify repository was invoked with the specific StudyYearId
                mockSemesterRepo.Verify(r => r.GetByStudyYearIdAsync(studyYearId), Times.Once);
            }
        }

        /// <summary>
        /// Tests that Handle returns an empty list when the repository returns an empty collection.
        /// Input conditions: repository returns an empty IEnumerable&lt;Semester&gt;.
        /// Expected result: the returned list is empty and no exception is thrown.
        /// </summary>
        [TestMethod]
        public async Task Handle_RepositoryReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            var mockSemesterRepo = new Mock<ISemesterRepository>();
            mockSemesterRepo
                .Setup(r => r.GetByStudyYearIdAsync(It.IsAny<int>()))
                .ReturnsAsync(Enumerable.Empty<Semester>());

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.Semesters).Returns(mockSemesterRepo.Object);

            var handler = new GetStudyYearSemestersQueryHandler(mockUnitOfWork.Object);
            var query = new GetStudyYearSemestersQuery(123);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result, "Result should not be null even when repository returns empty sequence.");
            Assert.AreEqual(0, result.Count, "Expected an empty list when repository returns no semesters.");
            mockSemesterRepo.Verify(r => r.GetByStudyYearIdAsync(123), Times.Once);
        }

        /// <summary>
        /// Verifies that the constructor creates a non-null instance when provided a valid IUnitOfWork.
        /// Condition: A non-null mocked IUnitOfWork is supplied.
        /// Expected result: Constructor does not throw and returns a non-null GetStudyYearSemestersQueryHandler instance.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidUnitOfWork_DoesNotThrowAndCreatesInstance()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Act
            GetStudyYearSemestersQueryHandler? handler = null;
            Exception? caught = null;
            try
            {
                handler = new GetStudyYearSemestersQueryHandler(mockUnitOfWork.Object);
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            // Assert
            Assert.IsNull(caught, "Constructor should not throw when provided a valid IUnitOfWork.");
            Assert.IsNotNull(handler, "Handler instance should not be null after construction.");
            Assert.IsInstanceOfType(handler, typeof(GetStudyYearSemestersQueryHandler), "Constructed object should be of the expected type.");
        }

        /// <summary>
        /// Ensures the constructed handler implements the expected MediatR IRequestHandler interface.
        /// Condition: A valid mocked IUnitOfWork is supplied.
        /// Expected result: The handler implements IRequestHandler<GetStudyYearSemestersQuery, List&lt;SemesterDto&gt;>.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidUnitOfWork_ImplementsIRequestHandlerInterface()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Act
            var handler = new GetStudyYearSemestersQueryHandler(mockUnitOfWork.Object);

            // Assert
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetStudyYearSemestersQuery, List<SemesterDto>>),
                "Handler should implement IRequestHandler<GetStudyYearSemestersQuery, List<SemesterDto>>.");
        }
    }
}