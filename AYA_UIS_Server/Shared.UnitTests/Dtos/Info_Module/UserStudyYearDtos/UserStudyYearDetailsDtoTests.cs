#nullable enable
using AYA_UIS.Core.Domain;
using AYA_UIS.Core.Domain.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.UserStudyYearDtos;
using System;


namespace Shared.Dtos.Info_Module.UserStudyYearDtos.UnitTests
{
    [TestClass]
    public class UserStudyYearDetailsDtoTests
    {
        /// <summary>
        /// Verifies that LevelName returns the enum's string representation with underscores replaced by spaces
        /// for every defined Levels enum value.
        /// Input conditions: iterate all values returned by Enum.GetValues(typeof(Levels)).
        /// Expected result: LevelName equals value.ToString().Replace("_", " ") and no exception is thrown.
        /// </summary>
        [TestMethod]
        public void LevelName_AllDefinedEnumValues_ReturnsToStringWithSpacesInsteadOfUnderscores()
        {
            // Arrange
            var dto = new UserStudyYearDetailsDto();

            // Act & Assert
            foreach (Levels level in Enum.GetValues(typeof(Levels)))
            {
                // Arrange for each case
                dto.Level = level;

                // Act
                string actual = dto.LevelName;
                string expected = level.ToString().Replace("_", " ");

                // Assert
                Assert.AreEqual(expected, actual, $"LevelName mismatch for enum value '{level}'.");
            }
        }

        /// <summary>
        /// Verifies that LevelName behaves predictably for enum values created by casting out-of-range integers.
        /// Input conditions: several out-of-range integer values cast to Levels (int.MinValue, -1, large positive, int.MaxValue).
        /// Expected result: LevelName equals the cast enum's ToString().Replace(\"_\", \" \") and does not throw.
        /// </summary>
        [TestMethod]
        public void LevelName_OutOfRangeNumericValues_ReturnsExpectedString()
        {
            // Arrange
            var dto = new UserStudyYearDetailsDto();
            int[] outOfRangeInts = new[] { int.MinValue, -1, 9999999, int.MaxValue };

            // Act & Assert
            foreach (int raw in outOfRangeInts)
            {
                Levels casted = (Levels)raw;
                dto.Level = casted;

                // Act
                string actual = dto.LevelName;
                string expected = casted.ToString().Replace("_", " ");

                // Assert
                Assert.AreEqual(expected, actual, $"LevelName mismatch for out-of-range value '{raw}' (cast as '{casted}').");
            }
        }
    }
}