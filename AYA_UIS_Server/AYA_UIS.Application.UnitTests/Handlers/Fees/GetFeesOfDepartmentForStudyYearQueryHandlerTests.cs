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

namespace AYA_UIS.Application.Handlers.Fees.UnitTests
{
    [TestClass]
    public class GetFeesOfDepartmentForStudyYearQueryHandlerTests
    {
        /// <summary>
        /// Ensures the constructor does not invoke dependency members during construction.
        /// Condition: Provide mocked dependencies and construct the handler.
        /// Expected result: No calls are made on the mocks as a result of construction.
        /// </summary>
        [TestMethod]
        public void Constructor_DoesNotCallDependencyMembers_OnConstruction()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            // Act
            var handler = new GetFeesOfDepartmentForStudyYearQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Assert
            // Verify that no calls were made to the mocks during construction.
            // If constructor had invoked any method, the strict mocks would have thrown already,
            // but we also call VerifyNoOtherCalls to be explicit about no interactions.
            unitOfWorkMock.VerifyNoOtherCalls();
            mapperMock.VerifyNoOtherCalls();

            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Verifies that the handler forwards DepartmentId and StudyYearId unchanged to the repository for extreme integer values.
        /// Input conditions: three pairs of (departmentId, studyYearId): int.MinValue, 0, int.MaxValue.
        /// Expected: repository GetFeesOfDepartmentForStudyYear is invoked with the same values for each call.
        /// </summary>
        [TestMethod]
        public async Task Handle_PassesThroughDepartmentAndStudyYear_ToRepository()
        {
            // Arrange & Act & Assert for multiple extreme inputs
            var testPairs = new (int dept, int year)[]
            {
                (int.MinValue, int.MinValue),
                (0, 0),
                (int.MaxValue, int.MaxValue)
            };

            foreach (var (dept, year) in testPairs)
            {
                // Arrange
                var feeRepoMock = new Mock<IFeeRepository>();
                feeRepoMock
                    .Setup(r => r.GetFeesOfDepartmentForStudyYear(dept, year))
                    .ReturnsAsync(new List<Fee>()); // empty result to keep flow simple

                var unitOfWorkMock = new Mock<IUnitOfWork>();
                unitOfWorkMock.Setup(u => u.Fees).Returns(feeRepoMock.Object);

                var mapperMock = new Mock<IMapper>();

                var handler = new GetFeesOfDepartmentForStudyYearQueryHandler(unitOfWorkMock.Object, mapperMock.Object);
                var request = new GetFeesOfDepartmentForStudyYearQuery(dept, year);

                // Act
                var result = await handler.Handle(request, CancellationToken.None);

                // Assert
                Assert.IsNotNull(result, "Result should not be null even when repository returns empty collection.");
                feeRepoMock.Verify(r => r.GetFeesOfDepartmentForStudyYear(dept, year), Times.Once,
                    $"Repository should be called once with departmentId={dept} and studyYearId={year}.");
            }
        }

    }
}