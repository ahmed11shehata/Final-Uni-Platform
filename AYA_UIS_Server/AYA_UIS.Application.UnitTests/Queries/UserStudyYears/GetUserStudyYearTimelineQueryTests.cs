#nullable enable
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.UserStudyYears;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared;
using Shared.Dtos.Info_Module;
using System;


namespace AYA_UIS.Application.Queries.UserStudyYears.UnitTests
{
    [TestClass]
    public class GetUserStudyYearTimelineQueryTests
    {
        /// <summary>
        /// Verifies that the constructor stores the provided userId into the UserId property.
        /// Inputs tested:
        /// - typical non-empty id
        /// - empty string
        /// - whitespace-only string
        /// - very long string (stress)
        /// - string with special/control/unicode characters
        /// Expected:
        /// - The UserId property equals the constructor argument for every input and no exception is thrown.
        /// </summary>
        [TestMethod]
        public void Constructor_VariousValidUserIds_SetsUserIdProperty()
        {
            // Arrange
            string[] testUserIds = new[]
            {
                "user-123",                  // typical
                string.Empty,                // empty
                "   \t  ",                   // whitespace-only
                new string('a', 10000),      // very long string boundary
                "special-\n\r\t\u2603-©-Ω"    // special/control/unicode chars
            };

            for (int i = 0; i < testUserIds.Length; i++)
            {
                string userId = testUserIds[i];

                // Act
                GetUserStudyYearTimelineQuery query = new GetUserStudyYearTimelineQuery(userId);

                // Assert
                Assert.IsNotNull(query, $"Query instance should not be null for case index {i}.");
                Assert.AreEqual(userId, query.UserId, $"UserId property mismatch for case index {i}.");
            }
        }

        /// <summary>
        /// Ensures that when the constructor is provided an empty string it results in an empty UserId,
        /// demonstrating that the string default initializer does not prevent assignment by the constructor.
        /// Input: empty string
        /// Expected: UserId equals empty string (not null).
        /// </summary>
        [TestMethod]
        public void Constructor_EmptyString_ResultsInEmptyUserIdNotNull()
        {
            // Arrange
            string userId = string.Empty;

            // Act
            GetUserStudyYearTimelineQuery query = new GetUserStudyYearTimelineQuery(userId);

            // Assert
            Assert.IsNotNull(query.UserId, "UserId should not be null even when an empty string is provided.");
            Assert.AreEqual(string.Empty, query.UserId, "UserId should match the empty string provided to the constructor.");
        }

        /// <summary>
        /// Confirms that constructor preserves leading/trailing whitespace in the provided userId.
        /// Input: whitespace-padded string
        /// Expected: Property preserves whitespace exactly.
        /// </summary>
        [TestMethod]
        public void Constructor_WhitespacePaddedString_PreservesWhitespace()
        {
            // Arrange
            string userId = "  userWithSpaces  ";

            // Act
            GetUserStudyYearTimelineQuery query = new GetUserStudyYearTimelineQuery(userId);

            // Assert
            Assert.AreEqual(userId, query.UserId, "Constructor should preserve leading and trailing whitespace in UserId.");
        }
    }
}