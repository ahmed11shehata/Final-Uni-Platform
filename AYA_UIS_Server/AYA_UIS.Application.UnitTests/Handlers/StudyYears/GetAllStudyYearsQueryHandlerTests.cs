using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.StudyYears;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.StudyYears;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.StudyYearDtos;

namespace AYA_UIS.Application.Handlers.StudyYears.UnitTests;

[TestClass]
public class GetAllStudyYearsQueryHandlerTests
{
    /// <summary>
    /// Verifies that Handle returns an IEnumerable of StudyYearDto ordered by StartYear descending
    /// and that each StudyYear is mapped correctly to StudyYearDto.
    /// Input conditions: repository returns multiple StudyYear instances with varied StartYear values
    /// (including int.MinValue and int.MaxValue and duplicates).
    /// Expected result: returned DTOs are ordered descending on StartYear and properties are mapped one-to-one.
    /// </summary>
    [TestMethod]
    public async Task Handle_MultipleStudyYears_ReturnsDtosOrderedByStartYearDescendingAndMapped()
    {
        // Arrange
        var studyYears = new List<StudyYear>
            {
                new StudyYear { Id = 1, StartYear = 2022, EndYear = 2023, IsCurrent = false },
                new StudyYear { Id = 2, StartYear = int.MaxValue, EndYear = int.MaxValue, IsCurrent = true },
                new StudyYear { Id = 3, StartYear = 0, EndYear = 1, IsCurrent = false },
                new StudyYear { Id = 4, StartYear = int.MinValue, EndYear = int.MinValue, IsCurrent = false },
                new StudyYear { Id = 5, StartYear = 2022, EndYear = 2023, IsCurrent = true } // duplicate StartYear
            }.AsEnumerable();

        var studyYearRepoMock = new Mock<IStudyYearRepository>();
        studyYearRepoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(studyYears);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.StudyYears).Returns(studyYearRepoMock.Object);

        var handler = new GetAllStudyYearsQueryHandler(uowMock.Object);

        // Act
        var result = await handler.Handle(new GetAllStudyYearsQuery(), CancellationToken.None);
        var resultList = result.ToList();

        // Assert
        Assert.AreEqual(5, resultList.Count, "Expected five DTOs returned.");

        // Expected StartYear order descending
        var expectedOrder = studyYears
            .OrderByDescending(sy => sy.StartYear)
            .Select(sy => sy.StartYear)
            .ToList();

        var actualOrder = resultList.Select(dto => dto.StartYear).ToList();
        CollectionAssert.AreEqual(expectedOrder, actualOrder, "StartYear ordering mismatch.");

        // Validate mapping for each item by matching on StartYear and Id
        foreach (var dto in resultList)
        {
            var matchingSource = studyYears.First(sy => sy.Id == dto.Id);
            Assert.AreEqual(matchingSource.EndYear, dto.EndYear, $"EndYear mismatch for Id {dto.Id}");
            Assert.AreEqual(matchingSource.IsCurrent, dto.IsCurrent, $"IsCurrent mismatch for Id {dto.Id}");
            Assert.AreEqual(matchingSource.StartYear, dto.StartYear, $"StartYear mismatch for Id {dto.Id}");
        }
    }

    /// <summary>
    /// Verifies that Handle returns an empty enumerable when the repository returns an empty collection.
    /// Input conditions: repository returns empty collection.
    /// Expected result: the returned IEnumerable is empty and no exception is thrown.
    /// </summary>
    [TestMethod]
    public async Task Handle_EmptyRepository_ReturnsEmptyEnumerable()
    {
        // Arrange
        var emptyList = new List<StudyYear>().AsEnumerable();

        var studyYearRepoMock = new Mock<IStudyYearRepository>();
        studyYearRepoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(emptyList);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.StudyYears).Returns(studyYearRepoMock.Object);

        var handler = new GetAllStudyYearsQueryHandler(uowMock.Object);

        // Act
        var result = await handler.Handle(new GetAllStudyYearsQuery(), CancellationToken.None);
        var resultList = result.ToList();

        // Assert
        Assert.IsNotNull(resultList, "Result should not be null for an empty repository result.");
        Assert.AreEqual(0, resultList.Count, "Expected no DTOs returned for empty repository.");
    }

    /// <summary>
    /// Verifies that the constructor creates a non-null instance when a valid IUnitOfWork is provided.
    /// Input: a Moq-created IUnitOfWork (loose behavior).
    /// Expected: instance is created successfully and is of the correct type; no exception is thrown.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidUnitOfWork_InstanceCreated()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Loose);

        // Act
        var handler = new GetAllStudyYearsQueryHandler(unitOfWorkMock.Object);

        // Assert
        Assert.IsNotNull(handler, "Handler instance should not be null when constructed with a valid IUnitOfWork.");
        Assert.IsInstanceOfType(handler, typeof(GetAllStudyYearsQueryHandler), "Handler should be of type GetAllStudyYearsQueryHandler.");
    }

    /// <summary>
    /// Ensures that constructing the handler with a strict IUnitOfWork does not cause any calls to the dependency.
    /// Input: a Moq-created IUnitOfWork with MockBehavior.Strict.
    /// Expected: instance is created and no calls were made to the mock during construction.
    /// </summary>
    [TestMethod]
    public void Constructor_StrictUnitOfWork_NoCallsOnDependencyDuringConstruction()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

        // Act
        var handler = new GetAllStudyYearsQueryHandler(unitOfWorkMock.Object);

        // Assert - verify instance created
        Assert.IsNotNull(handler, "Handler instance should not be null when constructed with a strict IUnitOfWork.");
        Assert.IsInstanceOfType(handler, typeof(GetAllStudyYearsQueryHandler), "Handler should be of type GetAllStudyYearsQueryHandler.");

        // Assert - verify no interactions occurred with the mock during construction
        // If the constructor had tried to call any members, the strict mock would have thrown or recorded calls.
        unitOfWorkMock.VerifyNoOtherCalls();
    }
}