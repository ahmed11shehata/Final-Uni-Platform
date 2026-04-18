using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using AYA_UIS.Shared;
using AYA_UIS.Shared.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AYA_UIS.Shared.Exceptions.UnitTests
{
    /// <summary>
    /// Tests for AYA_UIS.Shared.Exceptions.ValidationException constructors.
    /// </summary>
    [TestClass]
    public partial class ValidationExceptionTests
    {
        /// <summary>
        /// Verifies that the constructor ValidationException(string message, IEnumerable&lt;string&gt; errors)
        /// stores the provided message in Exception.Message and assigns the Errors property to the provided enumerable.
        /// Tests multiple message and errors variations including empty message, whitespace, long message,
        /// empty collection, single-item and multi-item collections with duplicates.
        /// Expected: Message equals input message and Errors enumerates the same sequence (and preserves reference).
        /// </summary>
        [TestMethod]
        public void Ctor_MessageAndErrors_AssignsMessageAndErrors()
        {
            // Arrange - prepare multiple test cases
            var longMessage = new string('x', 1024);
            var messagesToTest = new List<string>
            {
                "Custom validation failed message",
                string.Empty,
                "   ",
                longMessage,
                "Special chars \0 \u001F \u263A"
            };

            var errorCollections = new List<IEnumerable<string>>
            {
                new List<string>(), // empty
                new List<string> { "Error1" }, // single
                new List<string> { "E1", "E2", "E1" }, // duplicates
                new string[] { "A", "B", "C" }, // array
                GetDeferredEnumerable(new List<string> { "D1", "D2" }) // deferred LINQ-like enumerable
            };

            foreach (var msg in messagesToTest)
            {
                foreach (var errors in errorCollections)
                {
                    // Act
                    var ex = new ValidationException(msg, errors);

                    // Assert
                    Assert.IsNotNull(ex, "Constructor returned null.");
                    Assert.AreEqual(msg, ex.Message, "Exception.Message should match the provided message.");
                    // Reference equality when possible: since constructor assigns reference, check that reference equals for mutable collections and arrays.
                    // For deferred enumerable we cannot rely on reference equality, so also check sequence equality.
                    if (ReferenceEquals(errors, ex.Errors))
                    {
                        Assert.AreSame(errors, ex.Errors, "Errors property should preserve reference to provided enumerable when possible.");
                    }

                    CollectionAssert.AreEqual(ToList(errors), ToList(ex.Errors), "Errors sequence should match the provided errors.");
                }
            }
        }

        /// <summary>
        /// Helper to convert an IEnumerable to a List for comparison, handling nulls safely.
        /// </summary>
        private static List<string> ToList(IEnumerable<string>? source)
        {
            var list = new List<string>();
            if (source == null) return list;
            foreach (var s in source) list.Add(s);
            return list;
        }

        /// <summary>
        /// Returns a deferred IEnumerable (iterates when enumerated) to simulate non-materialized sequences.
        /// </summary>
        private static IEnumerable<string> GetDeferredEnumerable(IEnumerable<string> items)
        {
            foreach (var it in items)
            {
                yield return it;
            }
        }

        /// <summary>
        /// Verifies that the constructor sets Message, InnerException, and Errors correctly for several representative inputs.
        /// Inputs include: normal message with multiple errors, empty message with empty errors, very long message and errors,
        /// and a message with special/control characters. Ensures Errors reference is preserved and properties match expected values.
        /// </summary>
        [TestMethod]
        public void ValidationException_WithMessageErrorsAndInnerException_PropertiesAreSet()
        {
            // Arrange - prepare multiple test cases
            var inner = new InvalidOperationException("inner");
            var longString = new string('A', 1024);
            var cases = new List<(string Message, IEnumerable<string> Errors)>
            {
                // normal case with multiple errors
                ("Validation failed for entity", new List<string> { "Name is required", "Age must be >= 0" }),
                // empty message and empty errors
                (string.Empty, new List<string>()),
                // whitespace message and single error
                ("   ", new List<string> { "Whitespace message case" }),
                // very long message and very long error entry
                (longString, new List<string> { longString }),
                // message with special/control characters
                ("Line1\r\nLine2\t\u0001", new List<string> { "Error\u0000WithNullChar", "" })
            };

            foreach (var (message, errors) in cases)
            {
                // Act
                var ex = new ValidationException(message, errors, inner);

                // Assert
                // Message must be the same as provided message
                Assert.AreEqual(message, ex.Message, "Exception.Message does not match the provided message.");

                // InnerException must be the same instance passed in
                Assert.AreSame(inner, ex.InnerException, "InnerException reference should be preserved.");

                // Errors property should reference the same enumerable instance passed in
                Assert.AreSame(errors, ex.Errors, "Errors property should reference the same enumerable instance provided.");

                // Additionally ensure that Errors enumeration yields the same sequence as the provided enumerable
                CollectionAssert.AreEqual(new List<string>(errors), new List<string>(ex.Errors ?? Array.Empty<string>()),
                    "Errors sequence should match the provided sequence.");
            }
        }

        /// <summary>
        /// Ensures that providing a null inner exception is allowed and preserved by the constructor.
        /// This covers the case where callers may pass null for innerException.
        /// Expected: no exception thrown; InnerException is null and Errors/Message set as provided.
        /// </summary>
        [TestMethod]
        public void ValidationException_WithNullInnerException_PreservesNullInner()
        {
            // Arrange
            var message = "Msg";
            var errors = new List<string> { "E1" };
            Exception? inner = null;

            // Act
            var ex = new ValidationException(message, errors, inner!);

            // Assert
            Assert.AreEqual(message, ex.Message);
            Assert.IsNull(ex.InnerException, "InnerException is expected to be null when null was provided.");
            Assert.AreSame(errors, ex.Errors);
        }

        /// <summary>
        /// Verifies behavior when errors collection contains duplicate and null/empty entries.
        /// Expected: constructor preserves the enumerable reference and sequence content unchanged.
        /// </summary>
        [TestMethod]
        public void ValidationException_WithDuplicateAndEmptyErrors_PreservesSequence()
        {
            // Arrange
            var message = "Duplicate test";
            var errors = new List<string?> { "dup", "dup", string.Empty, null };
            var inner = new Exception("inner");

            // Act
            var ex = new ValidationException(message, errors as IEnumerable<string>, inner);

            // Assert
            Assert.AreEqual(message, ex.Message);
            Assert.AreSame(inner, ex.InnerException);
            Assert.AreSame(errors, ex.Errors);
            CollectionAssert.AreEqual(new List<string?>(errors), new List<string?>(ex.Errors ?? Array.Empty<string>()),
                "Errors collection content should be preserved including duplicates and empty/null entries.");
        }

        /// <summary>
        /// Tests that when null is provided for the 'errors' parameter the constructor does not throw
        /// and the Errors property is set to null.
        /// Input: errors = null
        /// Expected: No exception; Errors == null.
        /// </summary>
        [TestMethod]
        public void ValidationException_ErrorsParameterIsNull_ErrorsPropertyIsNull()
        {
            // Arrange
            IEnumerable<string>? errors = null;

            // Act
            ValidationException ex = new ValidationException(errors!);

            // Assert
            Assert.IsNull(ex.Errors, "Errors property should be null when constructed with a null enumerable.");
        }

        /// <summary>
        /// Tests that an empty collection is accepted and assigned to the Errors property.
        /// Input: errors = empty list
        /// Expected: No exception; Errors is not null and is empty.
        /// </summary>
        [TestMethod]
        public void ValidationException_EmptyErrorsCollection_ErrorsPropertyIsEmpty()
        {
            // Arrange
            IEnumerable<string> errors = new List<string>();

            // Act
            ValidationException ex = new ValidationException(errors);

            // Assert
            Assert.IsNotNull(ex.Errors, "Errors property should not be null when constructed with an empty collection.");
            CollectionAssert.AreEqual(new List<string>(), new List<string>(ex.Errors), "Errors should be an empty collection.");
        }

        /// <summary>
        /// Tests that a populated collection is preserved by reference and contents.
        /// Input: errors = list with items (including duplicates and special characters)
        /// Expected: No exception; Errors property references the same instance and contains the same items in order.
        /// </summary>
        [TestMethod]
        public void ValidationException_PopulatedErrorsCollection_ErrorsPropertyPreservesReferenceAndContents()
        {
            // Arrange
            var errors = new List<string>
            {
                "First error",
                "",
                "   ",
                "Error with special chars: \u0000 \u2603",
                new string('x', 1024), // very long string
                "First error" // duplicate
            };

            // Act
            ValidationException ex = new ValidationException(errors);

            // Assert
            // Reference equality: constructor should store the same reference (no defensive copy in implementation)
            Assert.AreSame(errors, ex.Errors, "Errors property should reference the same enumerable instance passed to the constructor.");

            // Content equality and order
            CollectionAssert.AreEqual(errors, new List<string>(ex.Errors), "Errors property should contain the same items in the same order as the provided collection.");
        }

        // Helper inner class to simulate an IEnumerable that throws when enumerated.
        private class ThrowOnEnumerateEnumerable : IEnumerable<string>
        {
            public IEnumerator<string> GetEnumerator()
            {
                throw new InvalidOperationException("Enumeration not supported in this test enumerable.");
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}