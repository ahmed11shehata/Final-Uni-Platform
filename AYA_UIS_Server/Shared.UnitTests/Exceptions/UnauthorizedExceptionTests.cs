using System;
using System.Collections;
using System.Threading;

using AYA_UIS.Shared;
using AYA_UIS.Shared.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AYA_UIS.Shared.Exceptions.UnitTests
{
    [TestClass]
    public partial class UnauthorizedExceptionTests
    {
        /// <summary>
        /// Verifies that the parameterless-optional constructor (i.e. when no message argument is provided)
        /// creates an UnauthorizedException whose Message equals the default text and whose InnerException is null.
        /// Expected: instance created successfully, Message == "Authentication required", InnerException == null,
        /// and the instance is an Exception and a BaseException.
        /// </summary>
        [TestMethod]
        public void UnauthorizedException_NoArgument_DefaultMessageAndNoInnerException()
        {
            // Arrange
            // (no inputs - relying on default parameter)

            // Act
            UnauthorizedException ex = new UnauthorizedException();

            // Assert
            Assert.IsNotNull(ex, "Exception instance should not be null.");
            Assert.IsInstanceOfType(ex, typeof(Exception), "Should be an Exception.");
            Assert.IsInstanceOfType(ex, typeof(BaseException), "Should derive from BaseException.");
            Assert.AreEqual("Authentication required", ex.Message, "Default message must match the declared default.");
            Assert.IsNull(ex.InnerException, "InnerException should be null when not provided.");
        }

        /// <summary>
        /// Tests constructor behavior for a variety of valid string inputs.
        /// Purpose: ensure that the provided message is preserved in the Message property
        /// and that no InnerException is set when only the single-argument constructor is used.
        /// Inputs tested: empty string, whitespace-only, very long string, string with special/control characters,
        /// and a normal informative message.
        /// Expected: Message equals the provided input for each case; InnerException remains null.
        /// </summary>
        [TestMethod]
        public void UnauthorizedException_MessageInputs_MessagePreservedAndNoInnerException()
        {
            // Arrange
            var testInputs = new[]
            {
                string.Empty,
                "   ",
                new string('A', 1024), // very long string
                "SpecialChars:\t\n\r\u0000\u001F",
                "Custom authentication message"
            };

            foreach (var input in testInputs)
            {
                // Act
                UnauthorizedException ex = new UnauthorizedException(input);

                // Assert
                Assert.IsNotNull(ex, "Instance should be created for input: \"" + (input ?? "<null>") + "\"");
                Assert.AreEqual(input, ex.Message, "Message should be preserved exactly for input: \"" + (input ?? "<null>") + "\"");
                Assert.IsNull(ex.InnerException, "InnerException should be null when only message is provided.");
                Assert.IsInstanceOfType(ex, typeof(BaseException), "UnauthorizedException must derive from BaseException.");
            }
        }

        /// <summary>
        /// Verifies that the UnauthorizedException constructor which accepts an inner exception
        /// correctly assigns the Message, InnerException, ErrorCode, and StatusCode properties.
        /// Input conditions: various message values (empty, whitespace, default text, very long string, special characters)
        /// and a non-null inner exception.
        /// Expected result: constructed exception preserves the provided message and inner exception reference,
        /// and sets ErrorCode to "UNAUTHORIZED" and StatusCode to 401.
        /// </summary>
        [TestMethod]
        public void Constructor_WithVariousMessagesAndInnerException_SetsProperties()
        {
            // Arrange
            Exception innerException = new InvalidOperationException("inner-ex");
            string[] testMessages = new[]
            {
                string.Empty,
                "   ",
                "Authentication required",
                new string('x', 10000),
                "special\t\n\u2603"
            };

            foreach (string message in testMessages)
            {
                // Act
                var ex = new UnauthorizedException(message, innerException);

                // Assert
                Assert.AreEqual(message, ex.Message, "The Message property should match the provided message.");
                Assert.AreSame(innerException, ex.InnerException, "The InnerException reference should be preserved.");
                Assert.AreEqual("UNAUTHORIZED", ex.ErrorCode, "ErrorCode should be 'UNAUTHORIZED'.");
                Assert.AreEqual(401, ex.StatusCode, "StatusCode should be 401.");
            }
        }

        /// <summary>
        /// Verifies that when the provided inner exception itself contains an InnerException,
        /// the entire inner exception chain is preserved by the UnauthorizedException constructor.
        /// Input conditions: nested inner exceptions (inner -> innerMost).
        /// Expected result: Outer exception's InnerException is the provided inner and its InnerException chain remains intact.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNestedInnerExceptions_PreservesInnerExceptionChain()
        {
            // Arrange
            var innerMost = new Exception("root-cause");
            var middle = new Exception("middle", innerMost);

            // Act
            var ex = new UnauthorizedException("some message", middle);

            // Assert
            Assert.AreSame(middle, ex.InnerException, "The top-level InnerException must be the exact instance provided.");
            Assert.IsNotNull(ex.InnerException?.InnerException, "The nested InnerException should not be null.");
            Assert.AreSame(innerMost, ex.InnerException?.InnerException, "The nested inner exception instance should be preserved.");
            Assert.AreEqual("UNAUTHORIZED", ex.ErrorCode, "ErrorCode should be 'UNAUTHORIZED'.");
            Assert.AreEqual(401, ex.StatusCode, "StatusCode should be 401.");
        }
    }
}