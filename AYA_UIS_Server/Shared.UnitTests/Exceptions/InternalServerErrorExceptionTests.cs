using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

#nullable enable
using AYA_UIS.Shared;
using AYA_UIS.Shared.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AYA_UIS.Shared.Exceptions.UnitTests
{
    /// <summary>
    /// Tests for AYA_UIS.Shared.Exceptions.InternalServerErrorException
    /// Focused on the constructor InternalServerErrorException(string message, Exception innerException)
    /// </summary>
    [TestClass]
    public class InternalServerErrorExceptionTests
    {
        /// <summary>
        /// Verifies that the constructor sets the Message and InnerException properties correctly
        /// for a variety of message inputs (empty, whitespace, very long, special/control characters).
        /// Input conditions: non-null message values and a valid non-null inner exception.
        /// Expected result: constructed exception has Message equal to provided message and InnerException references the provided instance.
        /// </summary>
        [TestMethod]
        public void Constructor_WithVariousMessagesAndInnerException_PreservesMessageAndInner()
        {
            // Arrange
            var messages = new[]
            {
                string.Empty,
                "   ",
                new string('A', 10000), // very long string
                "SpecialChars\t\n\r\u0000\u001F!@#$%^&*()_+-=[]{};':\",.<>/?\\|"
            };

            foreach (var message in messages)
            {
                // Act
                var inner = new InvalidOperationException("inner message");
                var ex = new InternalServerErrorException(message, inner);

                // Assert
                Assert.IsInstanceOfType(ex, typeof(InternalServerErrorException), "Instance should be of type InternalServerErrorException");
                Assert.AreEqual(message, ex.Message, "Message should match the provided message");
                Assert.AreSame(inner, ex.InnerException, "InnerException should be the exact instance passed to the constructor");
            }
        }

        /// <summary>
        /// Verifies that the constructor preserves the exact InnerException reference for different exception types.
        /// Input conditions: a standard Exception, InvalidOperationException, and AggregateException (with inner).
        /// Expected result: constructed exception's InnerException is the same object passed in (reference equality).
        /// </summary>
        [TestMethod]
        public void Constructor_WithDifferentInnerExceptionTypes_PreservesReference()
        {
            // Arrange
            var inners = new Exception[]
            {
                new Exception("base exception"),
                new InvalidOperationException("invalid op"),
                new AggregateException(new Exception("agg inner"))
            };

            foreach (var inner in inners)
            {
                // Act
                var ex = new InternalServerErrorException("test message", inner);

                // Assert
                Assert.IsInstanceOfType(ex, typeof(InternalServerErrorException));
                Assert.AreSame(inner, ex.InnerException, "InnerException should be the exact instance passed in");
                Assert.AreEqual("test message", ex.Message, "Message should match the provided message");
            }
        }

        /// <summary>
        /// Verifies that constructing without arguments does not throw and that the default message is observed when
        /// the type inherits from System.Exception. This test covers the parameterless invocation of the constructor
        /// which uses the default message "Internal server error".
        /// Expected: instance is created, not null, and if the instance is an Exception its Message equals the default.
        /// </summary>
        [TestMethod]
        public void Constructor_NoArguments_DoesNotThrowAndUsesDefaultMessageWhenAvailable()
        {
            // Arrange
            // (No setup required)

            // Act
            InternalServerErrorException exception = null!;
            Exception? asException = null;
            try
            {
                exception = new InternalServerErrorException();
                asException = exception as Exception;
            }
            catch (Exception ex)
            {
                Assert.Fail($"Constructor threw an unexpected exception: {ex}");
            }

            // Assert
            Assert.IsNotNull(exception, "Constructor returned null instance.");
            Assert.IsInstanceOfType(exception, typeof(InternalServerErrorException), "Instance is not of expected type.");

            if (asException is not null)
            {
                // Only assert message equality when the runtime type exposes Exception.Message
                Assert.AreEqual("Internal server error", asException.Message, "Default message was not preserved in Exception.Message.");
            }
        }

        /// <summary>
        /// Verifies that constructing with a variety of message inputs does not throw and (when possible) preserves the provided message.
        /// Inputs tested:
        /// - empty string
        /// - whitespace-only string
        /// - very long string (5000 characters)
        /// - string with special/control characters
        /// Expected: instance is created and, if the instance is an Exception, its Message equals the provided message.
        /// </summary>
        [TestMethod]
        public void Constructor_VariousMessages_DoesNotThrowAndPreservesMessageWhenAvailable()
        {
            // Arrange
            string longString = new string('A', 5000);
            string[] messages = new[]
            {
                string.Empty,
                "   ",
                longString,
                "Special chars: \t\n\r\u2603\u0000!@#$%^&*()_+-=[]{};':\",.<>/?|\\"
            };

            foreach (var msg in messages)
            {
                // Act & Assert per-case to isolate failures
                InternalServerErrorException instance = null!;
                Exception? asException = null;
                try
                {
                    instance = new InternalServerErrorException(msg);
                    asException = instance as Exception;
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Constructor threw an unexpected exception for message [{TruncateForDisplay(msg)}]: {ex}");
                }

                // Assert common expectations
                Assert.IsNotNull(instance, $"Instance was null for message [{TruncateForDisplay(msg)}].");
                Assert.IsInstanceOfType(instance, typeof(InternalServerErrorException), $"Incorrect type for message [{TruncateForDisplay(msg)}].");

                if (asException is not null)
                {
                    Assert.AreEqual(msg, asException.Message, $"Provided message not preserved for input [{TruncateForDisplay(msg)}].");
                }
            }

            static string TruncateForDisplay(string s)
            {
                if (s is null) return "null";
                return s.Length <= 64 ? s : s.Substring(0, 61) + "...";
            }
        }
    }
}