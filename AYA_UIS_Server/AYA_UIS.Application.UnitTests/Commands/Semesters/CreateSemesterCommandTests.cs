using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Semesters;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.SemesterDtos;
using System;
using System.Collections;
using System.Threading;


namespace AYA_UIS.Application.Commands.Semesters.UnitTests
{
    [TestClass]
    public class CreateSemesterCommandTests
    {
        /// <summary>
        /// Verifies that the constructor assigns the StudyYearId and preserves the same CreateSemesterDto instance
        /// for a variety of integer boundary values.
        /// Inputs: studyYearId ∈ {int.MinValue, -1, 0, 1, int.MaxValue}, a valid non-null CreateSemesterDto.
        /// Expected: The command's StudyYearId equals the input studyYearId and SemesterDto references the same object.
        /// </summary>
        [TestMethod]
        public void CreateSemesterCommand_VariousStudyYearIds_AssignsProperties()
        {
            // Arrange
            var testDto = new CreateSemesterDto
            {
                // Title left as default (0) to avoid depending on enum specifics.
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            int[] studyYearIds = new[]
            {
                int.MinValue,
                -1,
                0,
                1,
                int.MaxValue
            };

            foreach (var id in studyYearIds)
            {
                // Act
                var command = new CreateSemesterCommand(id, testDto);

                // Assert
                Assert.AreEqual(id, command.StudyYearId, "StudyYearId was not assigned correctly.");
                Assert.AreSame(testDto, command.SemesterDto, "SemesterDto reference was not preserved by constructor.");
            }
        }

        /// <summary>
        /// Ensures the constructor preserves the CreateSemesterDto reference and its internal values,
        /// including edge DateTime values and scenarios where StartDate equals or is after EndDate.
        /// Inputs: multiple CreateSemesterDto instances covering Min/Max dates, equal dates, and inverted dates.
        /// Expected: The command.SemesterDto is the same instance and its properties match the original dto.
        /// </summary>
        [TestMethod]
        public void CreateSemesterCommand_PreservesSemesterDtoValues_ForEdgeDates()
        {
            // Arrange - prepare several DTO edge cases
            var dtoMinMax = new CreateSemesterDto
            {
                StartDate = DateTime.MinValue,
                EndDate = DateTime.MaxValue
            };

            var dtoEqualDates = new CreateSemesterDto
            {
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2020, 1, 1)
            };

            var dtoStartAfterEnd = new CreateSemesterDto
            {
                StartDate = new DateTime(2030, 1, 2),
                EndDate = new DateTime(2030, 1, 1)
            };

            var testCases = new[]
            {
                dtoMinMax,
                dtoEqualDates,
                dtoStartAfterEnd
            };

            foreach (var dto in testCases)
            {
                // Act
                var command = new CreateSemesterCommand(42, dto);

                // Assert - reference preserved
                Assert.AreSame(dto, command.SemesterDto, "Constructor did not preserve the DTO reference.");

                // Assert - values preserved
                Assert.AreEqual(dto.StartDate, command.SemesterDto.StartDate, "StartDate was not preserved correctly.");
                Assert.AreEqual(dto.EndDate, command.SemesterDto.EndDate, "EndDate was not preserved correctly.");
                // Title preserved as well (may be default value)
                Assert.AreEqual(dto.Title, command.SemesterDto.Title, "Title was not preserved correctly.");
            }
        }
    }
}