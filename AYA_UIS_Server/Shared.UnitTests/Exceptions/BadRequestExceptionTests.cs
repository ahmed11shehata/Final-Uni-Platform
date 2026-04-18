using System;
using System.Collections;
using System.Threading;

using AYA_UIS.Shared;
using AYA_UIS.Shared.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AYA_UIS.Shared.Exceptions.UnitTests
{
    [TestClass]
    public class BadRequestExceptionTests
    {
        /// <summary>
        /// Verifies that the BadRequestException constructor which accepts a message and inner exception:
        /// - preserves the provided message,
        /// - preserves the provided inner exception reference,
        /// - sets the ErrorCode to "BAD_REQUEST",
        /// - sets the StatusCode to 400,
        /// - results in an object that is a BaseException and an Exception.
        /// 
        /// Tested inputs:
        /// - normal string, empty string, whitespace-only string, very long string, and string with control characters.
        /// - multiple inner exception types including nested inner exceptions.
        /// Expected result: No exceptions thrown during construction and properties set as described above.
        /// </summary>
        [TestMethod]
        public void Ctor_WithVariousMessagesAndInnerException_SetsPropertiesCorrectly()
        {
            // Arrange
            string[] messages = new[]
            {
                "normal message",
                string.Empty,
                "   ",
                new string('x', 10_000),
                "special\u0001\u0002chars"
            };

            Exception[] innerExceptions = new Exception[]
            {
                new InvalidOperationException("invalid"),
                new ArgumentNullException("param"),
                new Exception("generic", new Exception("nested"))
            };

            // Act / Assert
            foreach (string message in messages)
            {
                foreach (Exception inner in innerExceptions)
                {
                    // Act
                    var ex = new BadRequestException(message, inner);

                    // Assert
                    Assert.IsNotNull(ex, "Constructor returned null reference.");
                    Assert.AreEqual(message, ex.Message, "Message was not preserved.");
                    Assert.AreSame(inner, ex.InnerException, "InnerException reference was not preserved.");
                    Assert.IsInstanceOfType(ex, typeof(BaseException), "Exception is not a BaseException.");
                    Assert.IsInstanceOfType(ex, typeof(Exception), "Exception is not an Exception.");
                    Assert.AreEqual("BAD_REQUEST", ex.ErrorCode, "ErrorCode was not set to BAD_REQUEST.");
                    Assert.AreEqual(400, ex.StatusCode, "StatusCode was not set to 400.");
                }
            }
        }

        /// <summary>
        /// Verifies that when an inner exception already contains an InnerException chain,
        /// the chain is preserved when passed to the BadRequestException constructor.
        /// 
        /// Input: an Exception with its own InnerException (chain of two).
        /// Expected: BadRequestException.InnerException is the provided exception and its InnerException chain is preserved.
        /// </summary>
        [TestMethod]
        public void Ctor_WithInnerExceptionChain_PreservesInnerExceptionChain()
        {
            // Arrange
            var innerMost = new Exception("innermost");
            var middle = new Exception("middle", innerMost);

            // Act
            var ex = new BadRequestException("outer message", middle);

            // Assert
            Assert.IsNotNull(ex);
            Assert.AreSame(middle, ex.InnerException, "Top-level inner exception was not preserved.");
            Assert.AreSame(innerMost, ex.InnerException?.InnerException, "Inner exception chain was not preserved.");
        }
    }
}