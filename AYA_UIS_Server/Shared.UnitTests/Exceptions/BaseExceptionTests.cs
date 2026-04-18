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
    /// Tests for AYA_UIS.Shared.Exceptions.BaseException.
    /// Focused on the protected constructor BaseException(string message, string errorCode, int statusCode, Exception innerException)
    /// (lines 24-29 in the provided source).
    /// </summary>
    [TestClass]
    public class BaseExceptionTests
    {
        /// <summary>
        /// Verifies that the protected constructor which accepts an inner exception correctly assigns
        /// Message, ErrorCode, StatusCode and InnerException for a variety of representative inputs.
        /// Inputs tested:
        /// - message: normal, empty, whitespace-only, very long string
        /// - errorCode: normal, empty, whitespace-only, special characters
        /// - statusCode: int.MinValue, 0, int.MaxValue
        /// - innerException: different non-null Exception instances
        /// Expected result: Created BaseException instance exposes the same values via its public properties.
        /// </summary>
        [TestMethod]
        public void Constructor_WithInnerException_AssignsProperties_ForVariousInputs()
        {
            // Arrange: define a set of test cases covering required edge cases.
            var longMessage = new string('m', 1024);
            var longErrorCode = new string('e', 512);

            var testCases = new (string Message, string ErrorCode, int StatusCode, Exception Inner)[]
            {
                // typical values
                ("Standard message", "E001", 200, new InvalidOperationException("inner1")),
                // empty strings and zero status
                (string.Empty, string.Empty, 0, new Exception("inner2")),
                // whitespace-only strings and min status
                ("   ", "   ", int.MinValue, new ArgumentException("inner3")),
                // very long strings and max status, special characters in error code
                (longMessage, longErrorCode + "\n\t\u2603", int.MaxValue, new Exception()),
            };

            foreach (var (message, errorCode, statusCode, inner) in testCases)
            {
                // Act: create a Moq instance of the abstract BaseException by invoking the protected constructor.
                // Moq will call the base constructor using the provided arguments.
                var mock = new Mock<BaseException>(message, errorCode, statusCode, inner) { CallBase = true };
                BaseException? sut = mock.Object;

                // Assert: ensure public properties and base properties reflect the constructor inputs.
                Assert.IsNotNull(sut, "Mocked BaseException instance should not be null.");
                Assert.AreEqual(message, sut.Message, "Message should match the constructor value.");
                Assert.AreEqual(errorCode, sut.ErrorCode, "ErrorCode should match the constructor value.");
                Assert.AreEqual(statusCode, sut.StatusCode, "StatusCode should match the constructor value.");
                Assert.AreSame(inner, sut.InnerException, "InnerException reference should be the same instance passed to the constructor.");
            }
        }

        /// <summary>
        /// Ensures that no exception is thrown when constructing the abstract BaseException via Moq
        /// for several numeric boundary statusCode values and that statusCode is preserved.
        /// Inputs tested: int.MinValue, -1, 0, 1, int.MaxValue
        /// Expected result: No exception during construction and StatusCode equals the provided value.
        /// </summary>
        [TestMethod]
        public void Constructor_WithInnerException_DoesNotThrow_ForNumericBoundaries()
        {
            // Arrange
            var message = "Boundary test";
            var errorCode = "BOUND";
            var boundaries = new int[] { int.MinValue, -1, 0, 1, int.MaxValue };
            var inner = new Exception("inner-boundary");

            foreach (var status in boundaries)
            {
                // Act & Assert: Ensure construction succeeds and status is preserved.
                var mock = new Mock<BaseException>(message, errorCode, status, inner) { CallBase = true };
                BaseException? sut = mock.Object;

                Assert.IsNotNull(sut, $"Instance should be created for status {status}.");
                Assert.AreEqual(status, sut.StatusCode, $"StatusCode should equal provided boundary value {status}.");
                Assert.AreEqual(errorCode, sut.ErrorCode, "ErrorCode should match provided value.");
                Assert.AreSame(inner, sut.InnerException, "InnerException should be the same instance provided.");
            }
        }

        /// <summary>
        /// Verifies that the protected constructor assigns Message, ErrorCode and StatusCode correctly for a variety of string inputs.
        /// Input conditions:
        /// - message: empty, whitespace-only, long string, special characters
        /// - errorCode: empty, whitespace-only, long string, special characters
        /// - statusCode: typical success code (200)
        /// Expected:
        /// - No exception is thrown.
        /// - Exception.Message equals provided message.
        /// - ErrorCode equals provided errorCode.
        /// - StatusCode equals provided status code.
        /// </summary>
        [TestMethod]
        public void Constructor_WithVariousStringInputs_SetsPropertiesCorrectly()
        {
            // Arrange
            var messages = new string[] {
                string.Empty,
                "   ",
                new string('A', 1024),
                "msg-with-special-chars:\t\n\r\u2603"
            };

            var errorCodes = new string[] {
                string.Empty,
                "   ",
                new string('E', 512),
                "ERR-©-£-漢字"
            };

            const int statusCode = 200;

            foreach (var msg in messages)
            {
                foreach (var ec in errorCodes)
                {
                    // Act
                    Mock<BaseException> mock = new Mock<BaseException>(msg, ec, statusCode) { CallBase = true };
                    BaseException instance = mock.Object;

                    // Assert
                    Assert.AreEqual(msg, instance.Message, "Exception.Message should match the provided message");
                    Assert.AreEqual(ec, instance.ErrorCode, "ErrorCode should match the provided errorCode");
                    Assert.AreEqual(statusCode, instance.StatusCode, "StatusCode should match the provided statusCode");
                    Assert.IsNull(instance.InnerException, "InnerException should be null for this constructor overload");
                }
            }
        }

        /// <summary>
        /// Verifies that the protected constructor correctly handles boundary numeric status codes.
        /// Input conditions:
        /// - statusCode: int.MinValue, -1, 0, 1, int.MaxValue
        /// - message and errorCode: representative non-null strings
        /// Expected:
        /// - No exception is thrown.
        /// - StatusCode property equals the provided statusCode for each tested value.
        /// - Message and ErrorCode are preserved.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNumericBoundaries_SetsStatusCodeExactly()
        {
            // Arrange
            var message = "boundary-test-message";
            var errorCode = "BOUNDARY_ERR";
            var statusCodes = new int[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (var code in statusCodes)
            {
                // Act
                Mock<BaseException> mock = new Mock<BaseException>(message, errorCode, code) { CallBase = true };
                BaseException instance = mock.Object;

                // Assert
                Assert.AreEqual(message, instance.Message, "Message should be preserved");
                Assert.AreEqual(errorCode, instance.ErrorCode, "ErrorCode should be preserved");
                Assert.AreEqual(code, instance.StatusCode, "StatusCode should equal the input status code");
            }
        }

        /// <summary>
        /// Ensures that constructing the mock-derived BaseException does not inadvertently alter object identity semantics.
        /// Input:
        /// - typical non-empty message and error code and a typical status code.
        /// Expected:
        /// - The created object's type derives from BaseException.
        /// - Properties are set as provided.
        /// </summary>
        [TestMethod]
        public void Constructor_CreatedMockObject_DerivesFromBaseExceptionAndHasExpectedValues()
        {
            // Arrange
            var message = "identity-check";
            var errorCode = "ID_ERR";
            var statusCode = 418; // arbitrary

            // Act
            Mock<BaseException> mock = new Mock<BaseException>(message, errorCode, statusCode) { CallBase = true };
            BaseException instance = mock.Object;

            // Assert
            Assert.IsInstanceOfType(instance, typeof(BaseException), "Mock object should be assignable to BaseException");
            Assert.AreEqual(message, instance.Message);
            Assert.AreEqual(errorCode, instance.ErrorCode);
            Assert.AreEqual(statusCode, instance.StatusCode);
        }
    }
}