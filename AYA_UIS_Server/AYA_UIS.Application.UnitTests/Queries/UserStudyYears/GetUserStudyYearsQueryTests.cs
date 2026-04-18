using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.UserStudyYears;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared;
using Shared.Dtos.Info_Module;
using System;
using System.Collections.Generic;


#nullable enable

namespace AYA_UIS.Application.Queries.UserStudyYears.UnitTests
{
    [TestClass]
    public class GetUserStudyYearsQueryTests
    {
        /// <summary>
        /// Verifies that the constructor assigns the provided userId value exactly to the UserId property.
        /// Test inputs include normal, empty, whitespace-only, very long, and special/control-character containing strings.
        /// Expected: After construction, UserId equals the provided input and is not null.
        /// </summary>
        [TestMethod]
        public void Constructor_WithVariousUserIdValues_SetsUserIdExactly()
        {
            // Arrange - a set of representative edge and normal cases for the non-nullable string userId parameter.
            var longString = new string('a', 10000);
            var unicodeString = new string('Ω', 1024);
            var testCases = new List<string>
            {
                "normal-user-id",
                string.Empty,
                "   ",                        // whitespace-only
                longString,                   // very long string
                unicodeString,                // unicode repeated
                "special-chars-!@#$%^&*()\n\t" // includes control characters (newline and tab)
            };

            foreach (var input in testCases)
            {
                // Act
                var query = new GetUserStudyYearsQuery(input);

                // Assert
                Assert.IsNotNull(query.UserId, "UserId should not be null after construction.");
                Assert.AreEqual(input, query.UserId, "Constructor should assign the provided userId exactly.");
            }
        }

        /// <summary>
        /// Ensures that passing an explicitly empty string results in UserId being an empty string (not null),
        /// verifying that the property default (string.Empty) and constructor assignment behave consistently.
        /// Input: empty string.
        /// Expected: UserId equals string.Empty and is not null.
        /// </summary>
        [TestMethod]
        public void Constructor_WithEmptyString_SetsUserIdToEmptyString()
        {
            // Arrange
            var input = string.Empty;

            // Act
            var query = new GetUserStudyYearsQuery(input);

            // Assert
            Assert.IsNotNull(query.UserId);
            Assert.AreEqual(string.Empty, query.UserId);
        }

        // Note:
        // The constructor parameter 'userId' is declared as non-nullable (string).
        // Per nullable reference type rules and project constraints, tests must not pass null to that parameter.
        // Attempting to call the constructor with null would violate the method contract and is therefore not tested here.
    }
}