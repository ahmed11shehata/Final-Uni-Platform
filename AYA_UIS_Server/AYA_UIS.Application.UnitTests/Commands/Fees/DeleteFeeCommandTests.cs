using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Fees;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections;
using System.Threading;


namespace AYA_UIS.Application.Commands.Fees.UnitTests
{
    [TestClass]
    public class DeleteFeeCommandTests
    {
        /// <summary>
        /// Verifies that the DeleteFeeCommand constructor correctly assigns the Id property.
        /// Tests multiple integer edge cases including int.MinValue, negative, zero, positive, and int.MaxValue.
        /// Expected result: the Id property equals the value passed into the constructor and no exception is thrown.
        /// </summary>
        [TestMethod]
        public void DeleteFeeCommand_Constructor_AssignsId_ForVariousIntegers()
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

            foreach (int value in testValues)
            {
                // Act
                var command = new DeleteFeeCommand(value);

                // Assert
                Assert.AreEqual(value, command.Id, $"Constructor should set Id to {value}.");
            }
        }
    }
}