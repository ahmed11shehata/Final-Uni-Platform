#nullable enable
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.UserStudyYears;
using AYA_UIS.Core.Domain.Enums;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.UserStudyYearDtos;
using System;
using System.Collections.Generic;


namespace AYA_UIS.Application.Commands.UserStudyYears.UnitTests
{
    [TestClass]
    public class CreateUserStudyYearCommandTests
    {
        /// <summary>
        /// Verifies that the constructor assigns the provided CreateUserStudyYearDto instance to the Dto property
        /// and preserves all inner values. This test iterates multiple edge-case DTO instances including:
        /// - numeric extremes for StudyYearId (int.MinValue, int.MaxValue, 0, negative),
        /// - various UserId edge cases (empty, whitespace, very long, control/special chars),
        /// - a selection of enum Levels values.
        /// Expected: No exception is thrown and the command.Dto reference and its values match the supplied dto.
        /// </summary>
        [TestMethod]
        public void CreateUserStudyYearCommand_WithVariousDtos_SetsDtoReferenceAndValues()
        {
            // Arrange
            var longUserId = new string('a', 2000);
            var testDtos = new List<CreateUserStudyYearDto>
            {
                new CreateUserStudyYearDto { UserId = "normalUser", StudyYearId = 1, Level = Levels.First_Year },
                new CreateUserStudyYearDto { UserId = string.Empty, StudyYearId = 0, Level = Levels.Preparatory_Year },
                new CreateUserStudyYearDto { UserId = "   ", StudyYearId = -1, Level = Levels.Second_Year },
                new CreateUserStudyYearDto { UserId = longUserId, StudyYearId = int.MaxValue, Level = Levels.Graduate },
                new CreateUserStudyYearDto { UserId = "special\u0000Chars\n\t", StudyYearId = int.MinValue, Level = Levels.Third_Year }
            };

            // Act & Assert
            foreach (var dto in testDtos)
            {
                // Act
                var command = new CreateUserStudyYearCommand(dto);

                // Assert - reference preserved
                Assert.IsNotNull(command);
                Assert.AreSame(dto, command.Dto, "Constructor should preserve reference equality for Dto property.");

                // Assert - values preserved
                Assert.AreEqual(dto.UserId, command.Dto.UserId, "UserId should be preserved exactly.");
                Assert.AreEqual(dto.StudyYearId, command.Dto.StudyYearId, "StudyYearId should be preserved exactly.");
                Assert.AreEqual(dto.Level, command.Dto.Level, "Level enum should be preserved exactly.");
            }
        }

        /// <summary>
        /// Ensures that the constructor correctly preserves every defined Levels enum value.
        /// Input: for each enum value, build a DTO with that value and construct the command.
        /// Expected: the command.Dto.Level equals the supplied enum value for all defined enum members.
        /// </summary>
        [TestMethod]
        public void CreateUserStudyYearCommand_WithEachLevelValue_PreservesEnumValue()
        {
            // Arrange
            var levels = (Levels[])Enum.GetValues(typeof(Levels));
            foreach (var level in levels)
            {
                var dto = new CreateUserStudyYearDto
                {
                    UserId = "enumTestUser",
                    StudyYearId = 10,
                    Level = level
                };

                // Act
                var command = new CreateUserStudyYearCommand(dto);

                // Assert
                Assert.IsNotNull(command);
                Assert.AreEqual(level, command.Dto.Level, $"Level should be preserved for enum value '{level}'.");
                Assert.AreSame(dto, command.Dto, "Dto reference should remain the same instance.");
            }
        }
    }
}