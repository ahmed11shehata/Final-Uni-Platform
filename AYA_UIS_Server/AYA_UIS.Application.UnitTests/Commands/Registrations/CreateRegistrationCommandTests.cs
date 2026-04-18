using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Registrations;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.RegistrationDtos;
using System;
using System.Collections;
using System.Threading;


namespace AYA_UIS.Application.Commands.Registrations.UnitTests
{
    /// <summary>
    /// Tests for CreateRegistrationCommand constructor behavior.
    /// </summary>
    [TestClass]
    public class CreateRegistrationCommandTests
    {
        /// <summary>
        /// Verifies that the constructor assigns the provided RegistrationDto and UserId
        /// without creating copies and preserves the DTO's field values.
        /// Test inputs include boundary numeric values for the DTO's integer fields,
        /// null/empty/whitespace and very long/special-character strings for Reason and UserId.
        /// Expected: The command exposes the exact same DTO instance (reference equality)
        /// and the UserId property equals the provided string for all tested cases.
        /// </summary>
        [TestMethod]
        public void Constructor_AssignsProperties_PreservesReferenceAndValues()
        {
            // Arrange
            var longString = new string('x', 1024);
            var specialReason = "Line1\r\nLine2\t\u0000\uFFFF";
            var cases = new (CreateRegistrationDto dto, string userId, string description)[]
            {
                // boundary: zeros and null reason, empty user id
                (new CreateRegistrationDto { CourseId = 0, StudyYearId = 0, SemesterId = 0, Reason = null }, string.Empty, "ZeroIds_NullReason_EmptyUserId"),
                // large positive ints and long reason/userId
                (new CreateRegistrationDto { CourseId = int.MaxValue, StudyYearId = int.MaxValue, SemesterId = int.MaxValue, Reason = longString }, longString, "MaxInts_LongReason_LongUserId"),
                // large negative ints and whitespace userId
                (new CreateRegistrationDto { CourseId = int.MinValue, StudyYearId = -1, SemesterId = int.MinValue, Reason = " " }, "   ", "MinInts_WhitespaceReason_WhitespaceUserId"),
                // normal small values and special characters in reason/userId
                (new CreateRegistrationDto { CourseId = 1, StudyYearId = 2, SemesterId = 3, Reason = specialReason }, "user!@#", "SmallIds_SpecialReason_SpecialUserId")
            };

            foreach (var (dto, userId, description) in cases)
            {
                // Act
                var command = new CreateRegistrationCommand(dto, userId);

                // Assert - reference equality and value preservation
                Assert.IsNotNull(command, $"Constructor returned null for case: {description}");
                Assert.AreSame(dto, command.RegistrationDto, $"RegistrationDto reference not preserved for case: {description}");
                Assert.AreEqual(userId, command.UserId, $"UserId mismatch for case: {description}");

                // Assert DTO fields preserved through the reference
                Assert.AreEqual(dto.CourseId, command.RegistrationDto.CourseId, $"CourseId mismatch for case: {description}");
                Assert.AreEqual(dto.StudyYearId, command.RegistrationDto.StudyYearId, $"StudyYearId mismatch for case: {description}");
                Assert.AreEqual(dto.SemesterId, command.RegistrationDto.SemesterId, $"SemesterId mismatch for case: {description}");
                Assert.AreEqual(dto.Reason, command.RegistrationDto.Reason, $"Reason mismatch for case: {description}");
            }
        }

        /// <summary>
        /// Ensures that modifications to the DTO instance after constructing the command
        /// are observable via the command's RegistrationDto property, demonstrating that
        /// the constructor does not create a defensive copy.
        /// Input: mutate integer fields and Reason on the original DTO after construction.
        /// Expected: command.RegistrationDto reflects the mutated values (same reference).
        /// </summary>
        [TestMethod]
        public void Constructor_ReferenceIsShared_MutationsAreVisibleThroughCommand()
        {
            // Arrange
            var dto = new CreateRegistrationDto
            {
                CourseId = 10,
                StudyYearId = 20,
                SemesterId = 2,
                Reason = "initial"
            };
            var userId = "tester";

            var command = new CreateRegistrationCommand(dto, userId);

            // Act - mutate original dto
            dto.CourseId = 99;
            dto.StudyYearId = -5;
            dto.SemesterId = 7;
            dto.Reason = null; // reason is nullable

            // Assert - command sees mutations (same reference)
            Assert.AreSame(dto, command.RegistrationDto, "RegistrationDto reference was not shared after construction.");
            Assert.AreEqual(99, command.RegistrationDto.CourseId, "CourseId mutation not visible through command.");
            Assert.AreEqual(-5, command.RegistrationDto.StudyYearId, "StudyYearId mutation not visible through command.");
            Assert.AreEqual(7, command.RegistrationDto.SemesterId, "SemesterId mutation not visible through command.");
            Assert.IsNull(command.RegistrationDto.Reason, "Reason mutation to null not visible through command.");
            Assert.AreEqual(userId, command.UserId, "UserId should remain unchanged.");
        }
    }
}