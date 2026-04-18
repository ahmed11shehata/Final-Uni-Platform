using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using AYA_UIS.Shared;
using AYA_UIS.Shared.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AYA_UIS.Shared.Exceptions.UnitTests
{
    [TestClass]
    public class ForbiddenExceptionTests
    {
        /// <summary>
        /// Verifies that invoking the parameterless (default) ForbiddenException constructor
        /// (i.e. calling the constructor without passing the 'message' argument)
        /// sets the Exception.Message to the declared default ("Access forbidden")
        /// and that no inner exception is set.
        /// </summary>
        [TestMethod]
        public void ForbiddenException_Constructor_NoArgs_SetsDefaultMessageAndNoInnerException()
        {
            // Arrange
            // (no setup required)

            // Act
            var exception = new ForbiddenException();

            // Assert
            Assert.AreEqual("Access forbidden", exception.Message, "Default message should be 'Access forbidden'.");
            Assert.IsNull(exception.InnerException, "InnerException should be null when not provided.");
        }

        /// <summary>
        /// Verifies that invoking the ForbiddenException constructor with explicit message values
        /// assigns the provided message to Exception.Message and does not set an inner exception.
        /// Tested message inputs include empty string, whitespace-only string, very long string,
        /// and strings with special/control/unicode characters to exercise boundary and special cases.
        /// </summary>
        [TestMethod]
        public void ForbiddenException_Constructor_WithVariousMessages_SetsProvidedMessageAndNoInnerException()
        {
            // Arrange
            var longString = new string('A', 10_000); // very long string
            string[] testMessages = new[]
            {
                string.Empty,
                "   ",
                longString,
                "special\u0000chars\u001F",
                "こんにちは",             // unicode
                "path/with\\slashes"
            };

            foreach (var msg in testMessages)
            {
                // Act
                var ex = new ForbiddenException(msg);

                // Assert
                Assert.AreEqual(msg, ex.Message, $"Constructor should preserve the provided message. Failed for message: [{TruncateForAssert(msg)}]");
                Assert.IsNull(ex.InnerException, "InnerException should be null when not provided.");
            }

            static string TruncateForAssert(string? s)
            {
                if (s is null) return "<null>";
                return s.Length <= 128 ? s : s.Substring(0, 125) + "...";
            }
        }

        /// <summary>
        /// Verifies that the ForbiddenException constructor that accepts a message and inner exception
        /// assigns the Message, InnerException, ErrorCode and StatusCode properties correctly.
        /// 
        /// Tested input conditions:
        /// - several message values (empty, whitespace-only, long, special characters, normal text)
        /// - inner exceptions of different specific types (InvalidOperationException, ArgumentNullException)
        /// 
        /// Expected result:
        /// - The Exception.Message equals the provided message.
        /// - The Exception.InnerException is the same instance as the provided inner exception.
        /// - The ErrorCode is 'FORBIDDEN' and StatusCode is 403.
        /// </summary>
        [TestMethod]
        public void Constructor_WithMessageAndInnerException_AssignsPropertiesCorrectly()
        {
            // Arrange
            var longMessage = new string('A', 10000);
            var testCases = new (string message, Exception inner)[]
            {
                (message: "Standard forbidden message", inner: new InvalidOperationException("inner-1")),
                (message: string.Empty, inner: new InvalidOperationException("inner-empty")),
                (message: "   ", inner: new InvalidOperationException("inner-whitespace")),
                (message: longMessage, inner: new InvalidOperationException("inner-long")),
                (message: "SpecialChars:\n\t\r\u2603", inner: new ArgumentNullException("param", "inner-argnull"))
            };

            foreach (var (message, inner) in testCases)
            {
                // Act
                var ex = new ForbiddenException(message, inner);

                // Assert
                // Message should match the provided message
                Assert.AreEqual(message, ex.Message, "The exception Message did not match the provided message.");

                // InnerException should be the exact instance provided
                Assert.AreSame(inner, ex.InnerException, "The InnerException instance was not preserved.");

                // ErrorCode and StatusCode come from BaseException initialization in ForbiddenException
                Assert.AreEqual("FORBIDDEN", ex.ErrorCode, "ErrorCode should be 'FORBIDDEN'.");
                Assert.AreEqual(403, ex.StatusCode, "StatusCode should be 403.");
            }
        }

        /// <summary>
        /// Ensures that when a specific inner exception type is passed, its concrete type is preserved.
        /// 
        /// Input conditions:
        /// - message: non-empty string
        /// - innerException: ArgumentOutOfRangeException instance
        /// 
        /// Expected:
        /// - The InnerException property references an ArgumentOutOfRangeException and its Message matches.
        /// </summary>
        [TestMethod]
        public void Constructor_WithArgumentOutOfRangeException_PreservesInnerExceptionTypeAndMessage()
        {
            // Arrange
            var message = "Forbidden with specific inner";
            var inner = new ArgumentOutOfRangeException("paramName", "value out of range");

            // Act
            var ex = new ForbiddenException(message, inner);

            // Assert
            Assert.IsInstanceOfType(ex.InnerException, typeof(ArgumentOutOfRangeException), "InnerException should be of type ArgumentOutOfRangeException.");
            Assert.AreSame(inner, ex.InnerException, "InnerException instance should be preserved exactly.");
            Assert.AreEqual("FORBIDDEN", ex.ErrorCode, "ErrorCode should be 'FORBIDDEN'.");
            Assert.AreEqual(403, ex.StatusCode, "StatusCode should be 403.");
            Assert.AreEqual(message, ex.Message, "Message should match the provided message.");
        }
    }
}