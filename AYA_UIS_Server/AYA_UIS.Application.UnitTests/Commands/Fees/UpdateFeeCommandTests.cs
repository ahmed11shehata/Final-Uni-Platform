using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Fees;
using AYA_UIS.Core.Domain.Enums;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.FeeDtos;
using System;
using System.Collections;
using System.Threading;


namespace AYA_UIS.Application.Commands.Fees.UnitTests
{
    /// <summary>
    /// Tests for UpdateFeeCommand constructor behavior (assignment and reference semantics).
    /// </summary>
    [TestClass]
    public partial class UpdateFeeCommandTests
    {
        /// <summary>
        /// Verifies that the constructor assigns Id and FeeDto correctly across a variety of id and DTO values.
        /// Inputs tested: int.MinValue, negative, zero, positive, int.MaxValue; DTO descriptions include null, empty, whitespace, long and special chars; DTO amounts include decimal extremes and typical values.
        /// Expected result: The command's Id equals the provided id and FeeDto references the same instance with identical property values.
        /// </summary>
        [TestMethod]
        public void Constructor_AssignsIdAndFeeDtoProperties_ForMultipleCases()
        {
            // Arrange
            var longString = new string('x', 1000);
            var specialString = "Line1\nLine2\t\u2603"; // includes newline, tab, unicode snowman

            var testCases = new (int Id, UpdateFeeDto Dto, string CaseName)[]
            {
                (int.MinValue,
                    new UpdateFeeDto
                    {
                        Type = default(FeeType),
                        Level = default(Levels),
                        Description = "min id case",
                        Amount = decimal.MinValue
                    },
                    "IntMinValue with decimal.MinValue"),
                (-1,
                    new UpdateFeeDto
                    {
                        Type = default(FeeType),
                        Level = default(Levels),
                        Description = string.Empty,
                        Amount = 0m
                    },
                    "Negative id with empty description"),
                (0,
                    new UpdateFeeDto
                    {
                        Type = default(FeeType),
                        Level = default(Levels),
                        Description = "   ",
                        Amount = 1.23m
                    },
                    "Zero id with whitespace description"),
                (1,
                    new UpdateFeeDto
                    {
                        Type = default(FeeType),
                        Level = default(Levels),
                        Description = longString,
                        Amount = decimal.MaxValue
                    },
                    "Positive id with long description and decimal.MaxValue"),
                (int.MaxValue,
                    new UpdateFeeDto
                    {
                        Type = default(FeeType),
                        Level = default(Levels),
                        Description = null,
                        Amount = -123.45m
                    },
                    "IntMaxValue with null description and negative amount"),
                (42,
                    new UpdateFeeDto
                    {
                        Type = default(FeeType),
                        Level = default(Levels),
                        Description = specialString,
                        Amount = 999.99m
                    },
                    "Typical id with special chars in description")
            };

            foreach (var (Id, Dto, CaseName) in testCases)
            {
                // Act
                var cmd = new UpdateFeeCommand(Id, Dto);

                // Assert
                Assert.AreEqual(Id, cmd.Id, $"Id mismatch for case: {CaseName}");
                Assert.IsNotNull(cmd.FeeDto, $"FeeDto should not be null for case: {CaseName}");
                Assert.AreSame(Dto, cmd.FeeDto, $"FeeDto reference should be the same instance for case: {CaseName}");

                // Verify DTO property passthrough
                Assert.AreEqual(Dto.Amount, cmd.FeeDto.Amount, $"Amount mismatch for case: {CaseName}");
                Assert.AreEqual(Dto.Description, cmd.FeeDto.Description, $"Description mismatch for case: {CaseName}");
                Assert.AreEqual(Dto.Type, cmd.FeeDto.Type, $"Type mismatch for case: {CaseName}");
                Assert.AreEqual(Dto.Level, cmd.FeeDto.Level, $"Level mismatch for case: {CaseName}");
            }
        }

        /// <summary>
        /// Verifies that the constructor does not create a defensive copy of the provided UpdateFeeDto.
        /// Input conditions: create DTO, construct command, mutate DTO after construction.
        /// Expected result: Mutations to the original DTO are observed through the command's FeeDto reference.
        /// </summary>
        [TestMethod]
        public void Constructor_UsesReference_NoDefensiveCopy_ChangesReflected()
        {
            // Arrange
            var dto = new UpdateFeeDto
            {
                Type = default(FeeType),
                Level = default(Levels),
                Description = "initial",
                Amount = 10m
            };

            var id = 5;

            // Act
            var cmd = new UpdateFeeCommand(id, dto);

            // Mutate the DTO after construction
            dto.Amount = 20.5m;
            dto.Description = null;

            // Assert
            Assert.AreEqual(id, cmd.Id, "Id should remain equal to provided value");
            Assert.IsNotNull(cmd.FeeDto, "FeeDto should not be null");
            Assert.AreSame(dto, cmd.FeeDto, "FeeDto should reference the same instance passed into constructor");
            Assert.AreEqual(20.5m, cmd.FeeDto.Amount, "Changed Amount should be reflected on command's FeeDto");
            Assert.IsNull(cmd.FeeDto.Description, "Changed Description (null) should be reflected on command's FeeDto");
        }
    }
}