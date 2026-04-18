using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared;
using Shared.Respones;

namespace Shared.Respones.UnitTests
{
    /// <summary>
    /// Tests for Shared.Respones.Response&lt;T&gt; focusing on SuccessResponse(T data).
    /// </summary>
    [TestClass]
    public class ResponseTests
    {
        /// <summary>
        /// Verifies SuccessResponse for several string variants including empty, whitespace,
        /// long string, and special/control characters. The method ensures Data preserves the provided string,
        /// Success is true, Errors is null, and the success message is set.
        /// Inputs tested: "", "   ", very long string, string with control characters.
        /// Expected: Data equals input, Success true, Errors null, Message expected.
        /// </summary>
        [TestMethod]
        public void SuccessResponse_StringVariants_ReturnsExpectedResponse()
        {
            // Arrange
            string longString = new string('a', 10000);
            string specialChars = "line1\nline2\t\u0000\u0001";
            string[] inputs = new[] { string.Empty, "   ", longString, specialChars };

            foreach (string input in inputs)
            {
                // Act
                Response<string> result = Response<string>.SuccessResponse(input);

                // Assert
                Assert.IsNotNull(result, "Result should not be null for input (may be empty or whitespace).");
                Assert.IsTrue(result.Success, "Success must be true for successful response.");
                Assert.AreEqual(input, result.Data, "Data must equal the provided string input.");
                Assert.IsNull(result.Errors, "Errors should be null for a successful response.");
                Assert.AreEqual("Operation completed successfully", result.Message, "Unexpected success message.");
            }
        }

        /// <summary>
        /// Verifies that SuccessResponse preserves reference identity for reference-type T (List&lt;int&gt;).
        /// Input: a List&lt;int&gt; instance
        /// Expected: Data references the same instance, Success=true, Errors=null, and Message is the expected text.
        /// </summary>
        [TestMethod]
        public void SuccessResponse_ReferenceType_PreservesReferenceAndMetadata()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3 };

            // Act
            Response<List<int>> result = Response<List<int>>.SuccessResponse(list);

            // Assert
            Assert.IsNotNull(result, "Result should not be null for a reference-type input.");
            Assert.IsTrue(result.Success, "Success must be true for successful response.");
            Assert.AreSame(list, result.Data, "Data should reference the same list instance provided as input.");
            Assert.IsNull(result.Errors, "Errors should be null for a successful response.");
            Assert.AreEqual("Operation completed successfully", result.Message, "Unexpected success message.");
        }

        /// <summary>
        /// Verifies that ErrorResponse for reference generic parameter (string):
        /// - preserves the provided error string (including empty, whitespace, long, and special-character inputs),
        /// - sets Success to false,
        /// - sets Data to default(T) which is null for reference types,
        /// - sets Message to "Operation failed".
        /// This test exercises multiple representative string edge cases in a single test method.
        /// Expected: no exceptions; properties set as described.
        /// </summary>
        [TestMethod]
        public void ErrorResponse_ReferenceErrorInputs_ReturnsFailureResponseWithExpectedProperties()
        {
            // Arrange
            string[] testErrors =
            {
                string.Empty,                          // empty string
                "   ",                                 // whitespace-only
                new string('A', 1024),                 // long string
                "special\u0000chars\n\t\r",            // control characters
                "Emoji-😀-and-其它"                     // unicode characters
            };

            foreach (string error in testErrors)
            {
                // Act
                Response<string> response = Response<string>.ErrorResponse(error);

                // Assert
                Assert.IsNotNull(response, "Response instance should not be null.");
                Assert.IsFalse(response.Success, "Success should be false for an error response.");
                // For T = string (reference), Data should be default(T) => null
                string? data = response.Data;
                Assert.IsNull(data, "Data should be null (default of reference T).");
                Assert.AreEqual(error, response.Errors, "Errors should equal the provided error string.");
                Assert.AreEqual("Operation failed", response.Message, "Message should be the fixed failure message.");
            }
        }

        /// <summary>
        /// Verifies that ErrorResponse for a value-type generic parameter (int):
        /// - preserves the provided error string,
        /// - sets Success to false,
        /// - sets Data to default(T) which is 0 for int,
        /// - sets Message to "Operation failed".
        /// Expected: no exceptions; Data equals 0 and other properties set as described.
        /// </summary>
        [TestMethod]
        public void ErrorResponse_ValueType_DataIsDefaultAndPropertiesSet()
        {
            // Arrange
            string error = "ValueType error example";

            // Act
            Response<int> response = Response<int>.ErrorResponse(error);

            // Assert
            Assert.IsNotNull(response, "Response instance should not be null.");
            Assert.IsFalse(response.Success, "Success should be false for an error response.");
            // For T = int (value type), Data is int? and should equal default(int) => 0
            int? data = response.Data;
            Assert.AreEqual(0, data, "Data should be 0 (default of int).");
            Assert.AreEqual(error, response.Errors, "Errors should equal the provided error string.");
            Assert.AreEqual("Operation failed", response.Message, "Message should be the fixed failure message.");
        }
    }
}