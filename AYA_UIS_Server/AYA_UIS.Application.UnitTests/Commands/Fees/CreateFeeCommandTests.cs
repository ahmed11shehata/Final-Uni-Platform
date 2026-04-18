using AYA_UIS.Application.Commands.Fees;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.FeeDtos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using AYA_UIS.Core.Domain.Enums;


namespace AYA_UIS.Application.Commands.Fees.UnitTests
{
    /// <summary>
    /// Tests for CreateFeeCommand constructor behavior.
    /// </summary>
    [TestClass]
    public partial class CreateFeeCommandTests
    {
        /// <summary>
        /// Verifies that the constructor assigns the provided CreateFeeDto instance to the FeeDto property
        /// and that the same reference and values are preserved.
        /// Input: a normal CreateFeeDto with typical values.
        /// Expected: FeeDto property references the same instance and preserves all property values.
        /// </summary>
        [TestMethod]
        public void CreateFeeCommand_Constructor_AssignsFeeDtoReferenceAndValues()
        {
            // Arrange
            var dto = new CreateFeeDto
            {
                Amount = 100.50m,
                Type = (FeeType)1,
                Level = (Levels)1,
                Description = "Standard fee",
                StudyYearId = 2023,
                DepartmentId = 5
            };

            // Act
            var command = new CreateFeeCommand(dto);

            // Assert
            Assert.AreSame(dto, command.FeeDto, "Constructor should preserve the same CreateFeeDto reference.");
            Assert.AreEqual(100.50m, command.FeeDto.Amount, "Amount should be preserved.");
            Assert.AreEqual((FeeType)1, command.FeeDto.Type, "Type should be preserved.");
            Assert.AreEqual((Levels)1, command.FeeDto.Level, "Level should be preserved.");
            Assert.AreEqual("Standard fee", command.FeeDto.Description, "Description should be preserved.");
            Assert.AreEqual(2023, command.FeeDto.StudyYearId, "StudyYearId should be preserved.");
            Assert.AreEqual(5, command.FeeDto.DepartmentId, "DepartmentId should be preserved.");
        }

        /// <summary>
        /// Verifies constructor behavior across a range of edge-case CreateFeeDto instances.
        /// Inputs:
        /// - decimals at min/max/zero
        /// - ints at min/max/zero
        /// - descriptions null, empty, and very long
        /// - enum fields set to in-range and out-of-range values via casting
        /// Expected:
        /// - FeeDto property references the same instance for each input
        /// - All provided values are preserved exactly
        /// </summary>
        [TestMethod]
        public void CreateFeeCommand_Constructor_WithEdgeCaseDtos_PreservesReferenceAndValues()
        {
            // Arrange
            var longDescription = new string('x', 5000);

            var testDtos = new List<CreateFeeDto>
            {
                new CreateFeeDto
                {
                    Amount = decimal.MinValue,
                    Type = (FeeType)0,
                    Level = (Levels)0,
                    Description = null,
                    StudyYearId = int.MinValue,
                    DepartmentId = int.MinValue
                },
                new CreateFeeDto
                {
                    Amount = decimal.MaxValue,
                    Type = (FeeType)int.MaxValue,
                    Level = (Levels)int.MaxValue,
                    Description = string.Empty,
                    StudyYearId = int.MaxValue,
                    DepartmentId = int.MaxValue
                },
                new CreateFeeDto
                {
                    Amount = 0m,
                    Type = (FeeType)(-1),
                    Level = (Levels)(-1),
                    Description = longDescription,
                    StudyYearId = 0,
                    DepartmentId = 0
                }
            };

            // Act & Assert
            foreach (var dto in testDtos)
            {
                var cmd = new CreateFeeCommand(dto);

                // Reference preservation
                Assert.AreSame(dto, cmd.FeeDto, "Constructor should preserve the same CreateFeeDto reference for edge cases.");

                // Value preservation - Amount
                Assert.AreEqual(dto.Amount, cmd.FeeDto.Amount, "Amount should be preserved for edge-case dto.");

                // Value preservation - Type
                Assert.AreEqual(dto.Type, cmd.FeeDto.Type, "Type should be preserved for edge-case dto.");

                // Value preservation - Level
                Assert.AreEqual(dto.Level, cmd.FeeDto.Level, "Level should be preserved for edge-case dto.");

                // Value preservation - Description (may be null/empty/long)
                Assert.AreEqual(dto.Description, cmd.FeeDto.Description, "Description should be preserved for edge-case dto.");

                // Value preservation - StudyYearId
                Assert.AreEqual(dto.StudyYearId, cmd.FeeDto.StudyYearId, "StudyYearId should be preserved for edge-case dto.");

                // Value preservation - DepartmentId
                Assert.AreEqual(dto.DepartmentId, cmd.FeeDto.DepartmentId, "DepartmentId should be preserved for edge-case dto.");
            }
        }
    }
}