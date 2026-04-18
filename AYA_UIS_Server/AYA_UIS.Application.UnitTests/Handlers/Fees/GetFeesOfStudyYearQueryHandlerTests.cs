using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
#nullable enable
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Fees;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Fees;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.FeeDtos;

namespace AYA_UIS.Application.Handlers.Fees.UnitTests
{
    /// <summary>
    /// Tests for GetFeesOfStudyYearQueryHandler constructor behavior.
    /// Focus: ensure proper instantiation with provided dependencies and basic contract compliance.
    /// </summary>
    [TestClass]
    public partial class GetFeesOfStudyYearQueryHandlerTests
    {
        /// <summary>
        /// Partial test placeholder when nullability semantics need to be validated.
        /// The constructor parameters are non-nullable in source. Per test-generation rules we must not assign null to non-nullable parameters.
        /// If the project intends to accept nulls, update the source annotations and then implement this test to assert constructor behavior with nulls.
        /// This test is marked inconclusive to indicate manual review is required before enabling such scenarios.
        /// </summary>
        [TestMethod]
        public void Constructor_NullabilityManualReview_Inconclusive()
        {
            // Arrange
            // NOTE: The constructor parameters are non-nullable. Assigning null to non-nullable parameters would violate nullable annotations.
            // If the implementation is expected to guard against nulls (throw ArgumentNullException), modify method signatures/annotations accordingly
            // and replace this inconclusive test with an explicit assertion for the thrown exception.
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mapperMock = new Mock<IMapper>();

            // Act
            var handler = new GetFeesOfStudyYearQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Assert
            Assert.IsNotNull(handler, "Handler should be constructible with non-null dependencies.");
        }

        /// <summary>
        /// Verifies that when the repository returns multiple Fee entities the handler maps each Fee
        /// to a FeeDto and returns a list with the same number of mapped items.
        /// Input conditions: repository returns two Fee instances for the provided StudyYearId.
        /// Expected result: returned list has two elements and mapper.Map&lt;FeeDto&gt; is invoked twice.
        /// </summary>
        [TestMethod]
        public async Task Handle_NonEmptyRepository_MapsAllFeesAndReturnsList()
        {
            // Arrange
            int studyYearId = 2023;
            var fee1 = new Mock<Fee>().Object;
            var fee2 = new Mock<Fee>().Object;
            IEnumerable<Fee> repoResult = new[] { fee1, fee2 };

            var feeRepoMock = new Mock<IFeeRepository>();
            feeRepoMock.Setup(r => r.GetFeesOfStudyYear(studyYearId)).ReturnsAsync(repoResult);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.Fees).Returns(feeRepoMock.Object);

            var mapperMock = new Mock<IMapper>();
            // Return distinct FeeDto instances (properties unknown) - count is important.
            mapperMock
                .Setup(m => m.Map<FeeDto>(It.IsAny<object>()))
                .Returns((object src) => new FeeDto());

            var handler = new GetFeesOfStudyYearQueryHandler(unitOfWorkMock.Object, mapperMock.Object);
            var request = new GetFeesOfStudyYearQuery(studyYearId);

            // Act
            List<FeeDto> result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result, "Result should not be null for non-null repository result.");
            Assert.AreEqual(2, result.Count, "Handler should return same number of mapped FeeDto items as repository returned Fee items.");
            mapperMock.Verify(m => m.Map<FeeDto>(It.IsAny<object>()), Times.Exactly(2));
            feeRepoMock.Verify(r => r.GetFeesOfStudyYear(studyYearId), Times.Once);
        }

        /// <summary>
        /// Verifies behavior across several StudyYearId boundary values when repository returns an empty collection.
        /// Input conditions: repository returns an empty IEnumerable for each tested StudyYearId.
        /// Expected result: returned list is empty and mapper.Map&lt;FeeDto&gt; is not invoked.
        /// This test iterates through int.MinValue, -1, 0, 1, and int.MaxValue to exercise numeric boundaries.
        /// </summary>
        [TestMethod]
        public async Task Handle_EmptyRepositoryForVariousStudyYearIds_ReturnsEmptyListAndDoesNotCallMapper()
        {
            // Arrange - test several numeric boundary values
            int[] studyYearIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int id in studyYearIds)
            {
                var feeRepoMock = new Mock<IFeeRepository>();
                feeRepoMock.Setup(r => r.GetFeesOfStudyYear(id)).ReturnsAsync(Enumerable.Empty<Fee>());

                var unitOfWorkMock = new Mock<IUnitOfWork>();
                unitOfWorkMock.Setup(u => u.Fees).Returns(feeRepoMock.Object);

                var mapperMock = new Mock<IMapper>();
                // If the repository is empty, mapper should not be invoked; default setup is not necessary.

                var handler = new GetFeesOfStudyYearQueryHandler(unitOfWorkMock.Object, mapperMock.Object);
                var request = new GetFeesOfStudyYearQuery(id);

                // Act
                List<FeeDto> result = await handler.Handle(request, CancellationToken.None);

                // Assert
                Assert.IsNotNull(result, $"Result should not be null for studyYearId {id} (empty collection).");
                Assert.AreEqual(0, result.Count, $"Handler should return an empty list when repository returns no fees for studyYearId {id}.");
                mapperMock.Verify(m => m.Map<FeeDto>(It.IsAny<object>()), Times.Never, $"Mapper must not be called when repository returns empty for studyYearId {id}.");
                feeRepoMock.Verify(r => r.GetFeesOfStudyYear(id), Times.Once, $"Repository should be queried once for studyYearId {id}.");
            }
        }

    }
}