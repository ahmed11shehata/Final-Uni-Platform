using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.StudyYears;
using MediatR;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.StudyYearDtos;
using System;
using System.Collections;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AYA_UIS.Application.Commands.StudyYears.UnitTests
{
    /// <summary>
    /// Tests for CreateStudyYearCommand constructor behavior and property assignment.
    /// </summary>
    [TestClass]
    public partial class CreateStudyYearCommandTests
    {
        /// <summary>
        /// Verifies that the constructor stores the provided CreateStudyYearDto reference without copying
        /// and does not throw for a typical valid input.
        /// Condition: non-null DTO with normal year values (StartYear &lt; EndYear).
        /// Expected: StudyYearDto property references the same instance and values match.
        /// </summary>
        [TestMethod]
        public void CreateStudyYearCommand_Constructor_StoresGivenDtoReference()
        {
            // Arrange
            var dto = new CreateStudyYearDto { StartYear = 2024, EndYear = 2025 };

            // Act
            var command = new CreateStudyYearCommand(dto);

            // Assert
            Assert.IsNotNull(command);
            Assert.IsNotNull(command.StudyYearDto);
            // reference equality to ensure constructor did not create a copy
            Assert.AreSame(dto, command.StudyYearDto);
            Assert.AreEqual(2024, command.StudyYearDto.StartYear);
            Assert.AreEqual(2025, command.StudyYearDto.EndYear);
        }

        /// <summary>
        /// Verifies constructor behavior with boundary and extreme year values.
        /// Condition: DTO contains int.MinValue and int.MaxValue for StartYear and EndYear.
        /// Expected: No exception thrown and property holds the provided extreme values.
        /// </summary>
        [TestMethod]
        public void CreateStudyYearCommand_Constructor_AllowsExtremeIntValues()
        {
            // Arrange
            var dto = new CreateStudyYearDto { StartYear = int.MinValue, EndYear = int.MaxValue };

            // Act
            var command = new CreateStudyYearCommand(dto);

            // Assert
            Assert.IsNotNull(command);
            Assert.IsNotNull(command.StudyYearDto);
            Assert.AreSame(dto, command.StudyYearDto);
            Assert.AreEqual(int.MinValue, command.StudyYearDto.StartYear);
            Assert.AreEqual(int.MaxValue, command.StudyYearDto.EndYear);
        }

        /// <summary>
        /// Verifies constructor behavior when StartYear is greater than EndYear and when they are equal.
        /// Condition: DTO with StartYear &gt; EndYear and DTO with StartYear == EndYear.
        /// Expected: Constructor does not validate values and simply stores them as provided.
        /// </summary>
        [TestMethod]
        public void CreateStudyYearCommand_Constructor_AcceptsNonMonotonicAndEqualYearRanges()
        {
            // Arrange - StartYear greater than EndYear
            var dtoNonMonotonic = new CreateStudyYearDto { StartYear = 2030, EndYear = 2025 };

            // Act
            var commandNonMonotonic = new CreateStudyYearCommand(dtoNonMonotonic);

            // Assert
            Assert.IsNotNull(commandNonMonotonic);
            Assert.AreSame(dtoNonMonotonic, commandNonMonotonic.StudyYearDto);
            Assert.AreEqual(2030, commandNonMonotonic.StudyYearDto.StartYear);
            Assert.AreEqual(2025, commandNonMonotonic.StudyYearDto.EndYear);

            // Arrange - StartYear equals EndYear
            var dtoEqual = new CreateStudyYearDto { StartYear = 2025, EndYear = 2025 };

            // Act
            var commandEqual = new CreateStudyYearCommand(dtoEqual);

            // Assert
            Assert.IsNotNull(commandEqual);
            Assert.AreSame(dtoEqual, commandEqual.StudyYearDto);
            Assert.AreEqual(2025, commandEqual.StudyYearDto.StartYear);
            Assert.AreEqual(2025, commandEqual.StudyYearDto.EndYear);
        }
    }
}