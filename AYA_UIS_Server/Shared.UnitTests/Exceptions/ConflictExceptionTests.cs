using System;
using System.Collections;
using System.Threading;

using AYA_UIS.Shared;
using AYA_UIS.Shared.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AYA_UIS.Shared.Exceptions.UnitTests
{
    /// <summary>
    /// Tests for AYA_UIS.Shared.Exceptions.ConflictException
    /// </summary>
    [TestClass]
    public partial class ConflictExceptionTests
    {
        /// <summary>
        /// Verifies that the ConflictException(string) constructor creates an instance
        /// and preserves the provided message for a variety of string inputs including
        /// empty, whitespace-only, very long, and special/control character containing strings.
        /// Expected: instance is created, is of type ConflictException, and Message equals the input.
        /// </summary>
        [TestMethod]
        public void Constructor_MessageVariants_InstanceCreatedAndMessagePreserved()
        {
            // Arrange
            var testInputs = new[]
            {
                // empty string
                string.Empty,
                // whitespace-only
                "   \t  ",
                // very long string (~10000 chars)
                new string('x', 10000),
                // special and control characters
                "Line1\nLine2\r\n\t\u0000\u2603"
            };

            foreach (var input in testInputs)
            {
                // Act
                var ex = new ConflictException(input);

                // Assert
                Assert.IsNotNull(ex, "Constructor returned null for input: \"" + TruncateForMessage(input) + "\"");
                Assert.IsInstanceOfType(ex, typeof(ConflictException), "Instance type mismatch for input: \"" + TruncateForMessage(input) + "\"");
                Assert.AreEqual(input, ex.Message, "Exception.Message did not match the provided input.");
            }
        }

        /// <summary>
        /// Helper to truncate potentially very long strings for inclusion in assertion messages.
        /// Keeps tests self-contained as an inner helper per requirements.
        /// </summary>
        private static string TruncateForMessage(string? s)
        {
            if (s is null) return "null";
            const int max = 64;
            return s.Length <= max ? s : s.Substring(0, max) + "...(truncated)";
        }

        /// <summary>
        /// Verifies that the constructor which accepts an inner exception sets the Message and InnerException properties correctly.
        /// Input conditions: a non-null message string and a non-null System.Exception instance as innerException.
        /// Expected result: the created ConflictException preserves the message, preserves the inner exception reference,
        /// and is an instance of both ConflictException and its BaseException base type.
        /// </summary>
        [TestMethod]
        public void ConflictException_Ctor_WithInnerException_SetsMessageAndInnerException()
        {
            // Arrange
            string message = "Conflict occurred while updating resource.";
            Exception inner = new Exception("inner-exception");

            // Act
            var ex = new ConflictException(message, inner);

            // Assert
            Assert.IsNotNull(ex, "Constructor returned null.");
            Assert.IsInstanceOfType(ex, typeof(ConflictException), "Instance is not a ConflictException.");
            Assert.IsInstanceOfType(ex, typeof(BaseException), "Instance is not a BaseException.");
            Assert.AreEqual(message, ex.Message, "Message was not preserved.");
            Assert.AreSame(inner, ex.InnerException, "InnerException reference was not preserved.");
        }

        /// <summary>
        /// Verifies the constructor does not throw and preserves messages for a variety of string inputs.
        /// Input conditions: several non-null message variants (empty, whitespace-only, very long, and special characters)
        /// with a concrete inner exception type.
        /// Expected result: construction succeeds for all inputs and the Message property equals the provided input.
        /// </summary>
        [TestMethod]
        public void ConflictException_Ctor_VariousMessages_DoesNotThrowAndPreservesMessage()
        {
            // Arrange
            string[] messages = new[]
            {
                string.Empty,
                "   ",
                new string('a', 10000), // very long string
                "SpecialChars:\0\n\r\t\u2603!@#€",
                "Normal message"
            };

            Exception inner = new InvalidOperationException("invalid op");

            foreach (string msg in messages)
            {
                // Act
                var ex = new ConflictException(msg, inner);

                // Assert
                Assert.IsNotNull(ex, "Constructor returned null for message variant.");
                Assert.AreEqual(msg, ex.Message, "Message mismatch for input variant.");
                Assert.AreSame(inner, ex.InnerException, "InnerException reference mismatch for input variant.");
            }
        }
    }
}