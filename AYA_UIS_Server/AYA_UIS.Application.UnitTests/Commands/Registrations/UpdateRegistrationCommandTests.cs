#nullable enable
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Registrations;
using AYA_UIS.Core.Domain.Enums;
using MediatR;
using Shared;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.RegistrationDtos;
using System;
using System.Collections;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AYA_UIS.Application.Commands.Registrations.UnitTests
{
    [TestClass]
    public partial class UpdateRegistrationCommandTests
    {
        /// <summary>
        /// Verifies that the constructor assigns RegistrationId and UpdateDto for a variety of numeric edge-case registrationId values.
        /// Input conditions: registrationId takes values int.MinValue, -1, 0, 1, int.MaxValue; updateDto is a valid non-null UpdateRegistrationDto.
        /// Expected result: The constructed command's RegistrationId equals the provided registrationId and UpdateDto is the same instance passed in.
        /// </summary>
        [TestMethod]
        public void UpdateRegistrationCommand_Constructor_AssignsProperties_ForVariousRegistrationIds()
        {
            // Arrange
            int[] registrationIds = new[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int id in registrationIds)
            {
                var dto = new UpdateRegistrationDto
                {
                    // Use a valid enum value via default casting to ensure compilation without assuming enum members' names
                    Status = (RegistrationStatus)0,
                    Reason = $"reason-{id}",
                    Grade = null
                };

                // Act
                var command = new UpdateRegistrationCommand(id, dto);

                // Assert
                Assert.AreEqual(id, command.RegistrationId, $"RegistrationId was not set correctly for value {id}.");
                Assert.AreSame(dto, command.UpdateDto, $"UpdateDto instance was not preserved for RegistrationId {id}.");
                Assert.AreEqual(dto.Reason, command.UpdateDto.Reason, "UpdateDto.Reason mismatch after construction.");
            }
        }

        /// <summary>
        /// Ensures the constructor preserves the reference to the provided UpdateRegistrationDto (i.e., changes to the DTO after construction are visible on the command).
        /// Input conditions: a single UpdateRegistrationDto instance is created, passed to constructor, then mutated.
        /// Expected result: command.UpdateDto reflects the mutation, indicating the reference was preserved (no defensive copy).
        /// </summary>
        [TestMethod]
        public void UpdateRegistrationCommand_Constructor_PreservesUpdateDtoReference_WhenDtoIsMutatedAfterConstruction()
        {
            // Arrange
            var dto = new UpdateRegistrationDto
            {
                Status = (RegistrationStatus)0,
                Reason = "initial",
                Grade = null
            };

            var command = new UpdateRegistrationCommand(42, dto);

            // Act
            dto.Reason = "modified";

            // Assert
            Assert.AreSame(dto, command.UpdateDto, "UpdateDto reference should be the same instance provided to constructor.");
            Assert.AreEqual("modified", command.UpdateDto.Reason, "Mutation of the original DTO should be visible via the command's UpdateDto property.");
            Assert.AreEqual(42, command.RegistrationId, "RegistrationId should remain the value provided at construction.");
        }
    }
}