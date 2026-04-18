#nullable enable
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Registrations;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared;
using System;
using System.Collections;
using System.Threading;


namespace AYA_UIS.Application.Commands.Registrations.UnitTests
{
    /// <summary>
    /// Tests for AYA_UIS.Application.Commands.Registrations.DeleteRegistrationCommand constructor.
    /// </summary>
    [TestClass]
    public partial class DeleteRegistrationCommandTests
    {
        /// <summary>
        /// Verifies that the constructor assigns the RegistrationId property for a variety of integer inputs,
        /// including boundary values (int.MinValue, int.MaxValue), zero, and typical negative/positive values.
        /// Expected result: RegistrationId equals the input value and no exception is thrown.
        /// </summary>
        [TestMethod]
        public void DeleteRegistrationCommand_Constructor_AssignsRegistrationId_ForVariousIntegers()
        {
            // Arrange
            int[] testValues = new[]
            {
                int.MinValue,
                -1,
                0,
                1,
                int.MaxValue
            };

            foreach (int registrationId in testValues)
            {
                // Act
                DeleteRegistrationCommand command = null!;
                Exception? caught = null;
                try
                {
                    command = new DeleteRegistrationCommand(registrationId);
                }
                catch (Exception ex)
                {
                    caught = ex;
                }

                // Assert
                Assert.IsNull(caught, $"Constructor threw an exception for registrationId={registrationId}: {caught}");
                Assert.IsNotNull(command, "Command instance should not be null after construction.");
                Assert.AreEqual(registrationId, command.RegistrationId, $"RegistrationId was not set correctly for input {registrationId}.");
            }
        }

        /// <summary>
        /// Ensures the constructed instance preserves the exact integer value for several edge-case inputs.
        /// This test further documents the expectation that no normalization or validation occurs in constructor.
        /// Inputs tested: int.MinValue, int.MaxValue.
        /// Expected result: RegistrationId equals input value.
        /// </summary>
        [TestMethod]
        public void DeleteRegistrationCommand_Constructor_PreservesEdgeValues_NoNormalization()
        {
            // Arrange
            int[] edgeValues = new[] { int.MinValue, int.MaxValue };

            foreach (int val in edgeValues)
            {
                // Act
                var command = new DeleteRegistrationCommand(val);

                // Assert
                Assert.AreEqual(val, command.RegistrationId, $"Constructor did not preserve edge value {val}.");
            }
        }
    }
}