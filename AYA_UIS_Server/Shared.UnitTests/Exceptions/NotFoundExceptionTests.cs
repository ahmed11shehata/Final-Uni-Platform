using System;
using System.Collections;
using System.Threading;

#nullable enable
using AYA_UIS.Shared;
using AYA_UIS.Shared.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AYA_UIS.Shared.Exceptions.UnitTests
{
    /// <summary>
    /// Tests for AYA_UIS.Shared.Exceptions.NotFoundException
    /// </summary>
    [TestClass]
    public class NotFoundExceptionTests
    {
        /// <summary>
        /// Verifies that the constructor which accepts (string message, Exception innerException)
        /// correctly preserves the provided message and inner exception reference for various
        /// message edge cases (empty, whitespace-only, very long string, control/special chars).
        /// Expected: Created instance is of type NotFoundException, Message equals provided message,
        /// and InnerException is the same reference passed in.
        /// </summary>
        [TestMethod]
        public void Constructor_WithVariousMessagesAndInnerExceptions_PreservesMessageAndInnerException()
        {
            // Arrange
            string[] messages = new[]
            {
                string.Empty,
                "   ",
                new string('x', 10_000),
                "special\t\n\u0001"
            };

            var inner = new Exception("inner-exception");

            // Act & Assert
            foreach (var message in messages)
            {
                // Act
                var ex = new NotFoundException(message, inner);

                // Assert
                Assert.IsInstanceOfType(ex, typeof(NotFoundException), "Exception should be NotFoundException.");
                Assert.AreEqual(message, ex.Message, "Message should be preserved exactly.");
                Assert.AreSame(inner, ex.InnerException, "InnerException reference should be preserved.");
            }
        }

        /// <summary>
        /// Verifies that different concrete Exception types passed as innerException are preserved
        /// exactly (reference equality) and do not alter the outer exception's Message.
        /// Inputs: ArgumentException, InvalidOperationException, generic Exception.
        /// Expected: InnerException is the same instance and Message equals the provided message.
        /// </summary>
        [TestMethod]
        public void Constructor_WithDifferentInnerExceptionTypes_PreservesReferenceAndMessage()
        {
            // Arrange
            var message = "resource not found";
            Exception[] innerExceptions = new Exception[]
            {
                new ArgumentException("arg problem"),
                new InvalidOperationException("invalid op"),
                new Exception("generic")
            };

            // Act & Assert
            foreach (var inner in innerExceptions)
            {
                // Act
                var ex = new NotFoundException(message, inner);

                // Assert
                Assert.IsInstanceOfType(ex, typeof(NotFoundException), "Type should be NotFoundException.");
                Assert.AreEqual(message, ex.Message, "Message must match the constructor argument.");
                Assert.AreSame(inner, ex.InnerException, "Inner exception instance should be preserved.");
            }
        }
    }
}