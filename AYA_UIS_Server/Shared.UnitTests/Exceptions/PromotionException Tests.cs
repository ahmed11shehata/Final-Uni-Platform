using System;
using System.Collections;
using System.Threading;

#nullable enable
using AYA_UIS.Shared;
using AYA_UIS.Shared.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared;
using Shared.Exceptions;

namespace Shared.Exceptions.UnitTests
{
    /// <summary>
    /// Tests for PromotionException constructors.
    /// </summary>
    [TestClass]
    public partial class PromotionExceptionTests
    {
        /// <summary>
        /// Verifies that the PromotionException ctor (string, Exception) correctly assigns Message and InnerException
        /// for various message inputs and inner exception types. Ensures no exceptions are thrown for valid inputs
        /// and the inner exception reference is preserved.
        /// Inputs tested:
        /// - Normal message
        /// - Empty string
        /// - Whitespace-only string
        /// - Very long string (1000 chars)
        /// - String with special/control characters
        /// Inner exception types tested:
        /// - System.Exception
        /// - System.InvalidOperationException
        /// Expected: instance created, Message equals provided message, InnerException is the same reference.
        /// </summary>
        [TestMethod]
        public void PromotionException_StringAndInnerException_AssignsMessageAndInnerException()
        {
            // Arrange
            var messages = new string[]
            {
                "Standard error message",
                string.Empty,
                "   ",
                new string('A', 1000),
                "Special chars:\n\t\r\u2603\u0001"
            };

            Exception[] innerExceptions = new Exception[]
            {
                new Exception("inner-general"),
                new InvalidOperationException("inner-invalid-op")
            };

            foreach (var msg in messages)
            {
                foreach (var inner in innerExceptions)
                {
                    // Act
                    PromotionException ex = new PromotionException(msg, inner);

                    // Assert
                    Assert.IsNotNull(ex, "Constructor returned null for message: \"" + msg + "\"");
                    Assert.IsInstanceOfType(ex, typeof(PromotionException), "Instance should be PromotionException");
                    Assert.IsInstanceOfType(ex, typeof(AYA_UIS.Shared.Exceptions.BaseException), "Instance should derive from BaseException");
                    Assert.AreEqual(msg, ex.Message, "Message property was not assigned correctly.");
                    Assert.AreSame(inner, ex.InnerException, "InnerException reference was not preserved.");
                }
            }
        }

        /// <summary>
        /// Verifies that the PromotionException ctor (string, Exception) does not throw for a variety of valid messages
        /// and that the HResult / custom code behavior is not asserted here because BaseException implementation is external.
        /// This is a safety test ensuring valid inputs do not throw.
        /// Inputs: typical message and a nested exception with stack trace.
        /// Expected: no exception thrown when constructing.
        /// </summary>
        [TestMethod]
        public void PromotionException_Construction_WithNestedException_DoesNotThrow()
        {
            // Arrange
            var inner = new ArgumentNullException("paramName", "arg was null");
            var message = "Outer promotion failure";

            // Act & Assert
            try
            {
                var ex = new PromotionException(message, inner);
                Assert.IsNotNull(ex);
                Assert.AreEqual(message, ex.Message);
                Assert.AreSame(inner, ex.InnerException);
            }
            catch (Exception e)
            {
                Assert.Fail("Constructor threw an unexpected exception: " + e);
            }
        }

        /// <summary>
        /// Verifies that the PromotionException constructor preserves a variety of message inputs
        /// (empty, whitespace-only, special/control characters, and typical text) without throwing
        /// and that the Exception.Message equals the provided input.
        /// </summary>
        [TestMethod]
        public void PromotionException_WithVariousMessages_PreservesMessage()
        {
            // Arrange
            string[] messages = new string[]
            {
                string.Empty,
                " ",
                "\t\n\r", // control characters
                "Standard error message",
                "特殊字符 - unicode ✓",
                "Message with special chars !@#$%^&*()_+-=[]{};':\",.<>/?\\|`~"
            };

            foreach (string msg in messages)
            {
                // Act
                PromotionException ex = new PromotionException(msg);

                // Assert
                Assert.IsNotNull(ex, "Constructor returned null PromotionException instance.");
                Assert.IsInstanceOfType(ex, typeof(PromotionException), "Object is not of expected type PromotionException.");
                Assert.AreEqual(msg, ex.Message, "Exception.Message was not preserved exactly as provided.");
            }
        }

        /// <summary>
        /// Ensures that a very long message is accepted by the constructor and the full content is preserved.
        /// Tests boundary behavior for large string inputs.
        /// </summary>
        [TestMethod]
        public void PromotionException_WithVeryLongMessage_PreservesFullContentAndLength()
        {
            // Arrange
            int length = 10000;
            string longMessage = new string('A', length);

            // Act
            PromotionException ex = new PromotionException(longMessage);

            // Assert
            Assert.IsNotNull(ex, "Constructor returned null PromotionException instance for long message.");
            Assert.IsInstanceOfType(ex, typeof(PromotionException), "Object is not of expected type PromotionException for long message.");
            Assert.AreEqual(length, ex.Message.Length, "Exception.Message length does not match the provided long message length.");
            Assert.AreEqual(longMessage, ex.Message, "Exception.Message content does not match the provided long message.");
        }
    }
}