using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos;
using Shared.Dtos.ErrorModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;


namespace Shared.Dtos.ErrorModels.UnitTests
{
    /// <summary>
    /// Unit tests for Shared.Dtos.ErrorModels.ErrorDetails.ToString().
    /// Tests focus on correct JSON serialization for a variety of edge-case inputs.
    /// </summary>
    [TestClass]
    public class ErrorDetailsTests
    {
        /// <summary>
        /// Verifies that ErrorDetails.ToString serializes the instance to valid JSON and preserves
        /// property values for a variety of inputs, including numeric extremes, empty and long strings,
        /// null/empty/duplicate collections, and special characters.
        /// Input conditions: multiple representative ErrorDetails instances created inline.
        /// Expected result: Produced JSON parses successfully and contains the same values for
        /// StatusCode, ErrorMessage, and Errors (or null for Errors when set to null).
        /// </summary>
        [TestMethod]
        public void ToString_VariousInputs_SerializesCorrectly()
        {
            // Arrange: prepare a set of diverse test cases exercising numeric extremes, strings, and collections.
            var longString = new string('x', 1000);
            var testCases = new[]
            {
                new
                {
                    Name = "DefaultInstance",
                    StatusCode = 0,
                    ErrorMessage = string.Empty,
                    Errors = (IEnumerable<string>?)null
                },
                new
                {
                    Name = "MinStatus_WithSingleError",
                    StatusCode = int.MinValue,
                    ErrorMessage = "min-value-test",
                    Errors = (IEnumerable<string>?)new List<string> { "single-error" }
                },
                new
                {
                    Name = "MaxStatus_WithEmptyErrors",
                    StatusCode = int.MaxValue,
                    ErrorMessage = "max-value-test",
                    Errors = (IEnumerable<string>?)Array.Empty<string>()
                },
                new
                {
                    Name = "WhitespaceAndSpecialChars",
                    StatusCode = 200,
                    ErrorMessage = " \t\n\"Quote\\Backslash\u2603",
                    Errors = (IEnumerable<string>?)new List<string> { "dup", "dup" }
                },
                new
                {
                    Name = "LongErrorMessage_WithSingleItemErrors",
                    StatusCode = 400,
                    ErrorMessage = longString,
                    Errors = (IEnumerable<string>?)new List<string> { "single" }
                }
            };

            foreach (var tc in testCases)
            {
                // Act: create instance and serialize via ToString()
                var details = new ErrorDetails
                {
                    StatusCode = tc.StatusCode,
                    ErrorMessage = tc.ErrorMessage,
                    Errors = tc.Errors
                };

                string json = null!;
                Exception? caught = null;
                try
                {
                    json = details.ToString();
                }
                catch (Exception ex)
                {
                    caught = ex;
                }

                // Assert: ToString should not throw and should return non-empty JSON
                Assert.IsNull(caught, $"ToString threw for case '{tc.Name}': {caught}");
                Assert.IsFalse(string.IsNullOrEmpty(json), $"Serialized JSON is null or empty for case '{tc.Name}'.");

                // Parse JSON and assert values match expected
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // StatusCode
                Assert.IsTrue(root.TryGetProperty("StatusCode", out var statusProp), $"Missing StatusCode for case '{tc.Name}'.");
                Assert.AreEqual(tc.StatusCode, statusProp.GetInt32(), $"StatusCode mismatch for case '{tc.Name}'.");

                // ErrorMessage
                Assert.IsTrue(root.TryGetProperty("ErrorMessage", out var msgProp), $"Missing ErrorMessage for case '{tc.Name}'.");
                // GetString may return null only if JSON value is null, but ErrorMessage is non-nullable in source; compare safely.
                var serializedMessage = msgProp.ValueKind == JsonValueKind.Null ? null : msgProp.GetString();
                Assert.AreEqual(tc.ErrorMessage, serializedMessage ?? string.Empty, $"ErrorMessage mismatch for case '{tc.Name}'.");

                // Errors: can be null, array (possibly empty), or array with items
                Assert.IsTrue(root.TryGetProperty("Errors", out var errorsProp), $"Missing Errors property for case '{tc.Name}'.");
                if (tc.Errors is null)
                {
                    Assert.AreEqual(JsonValueKind.Null, errorsProp.ValueKind, $"Expected Errors to be null for case '{tc.Name}'.");
                }
                else
                {
                    Assert.AreEqual(JsonValueKind.Array, errorsProp.ValueKind, $"Expected Errors to be an array for case '{tc.Name}'.");

                    // Build expected list for comparison
                    var expectedList = new List<string>(tc.Errors);
                    Assert.AreEqual(expectedList.Count, errorsProp.GetArrayLength(), $"Errors array length mismatch for case '{tc.Name}'.");

                    int index = 0;
                    foreach (var item in errorsProp.EnumerateArray())
                    {
                        // Each element expected to be a string (non-nullable in source)
                        Assert.AreEqual(JsonValueKind.String, item.ValueKind, $"Errors[{index}] not a string for case '{tc.Name}'.");
                        var value = item.GetString();
                        Assert.AreEqual(expectedList[index], value, $"Errors[{index}] mismatch for case '{tc.Name}'.");
                        index++;
                    }
                }
            }
        }

        /// <summary>
        /// Ensures that ToString produces valid JSON even when ErrorMessage contains control characters
        /// such as null character. Input: ErrorMessage with NUL char and Errors with several items.
        /// Expected: No exception thrown and ErrorMessage round-trips correctly via JSON parsing (control chars escaped).
        /// </summary>
        [TestMethod]
        public void ToString_ErrorMessageWithControlChars_DoesNotThrowAndPreservesContent()
        {
            // Arrange
            var controlMessage = "contains\u0000nullchar";
            var details = new ErrorDetails
            {
                StatusCode = 123,
                ErrorMessage = controlMessage,
                Errors = new List<string> { "one", "two" }
            };

            // Act
            string json = null!;
            Exception? ex = null;
            try
            {
                json = details.ToString();
            }
            catch (Exception e)
            {
                ex = e;
            }

            // Assert
            Assert.IsNull(ex, $"ToString threw an unexpected exception: {ex}");
            Assert.IsFalse(string.IsNullOrEmpty(json), "Serialized JSON should not be null or empty.");

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var msg = root.GetProperty("ErrorMessage").GetString();
            Assert.AreEqual(controlMessage, msg, "Control characters in ErrorMessage were not preserved after serialization.");
        }
    }
}