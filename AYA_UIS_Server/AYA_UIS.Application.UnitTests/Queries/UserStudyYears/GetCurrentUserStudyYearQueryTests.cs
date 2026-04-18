using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.UserStudyYears;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.UserStudyYearDtos;
using Shared.Respones;
using System;

namespace AYA_UIS.Application.Queries.UserStudyYears.UnitTests
{
    [TestClass]
    public partial class GetCurrentUserStudyYearQueryTests
    {
        /// <summary>
        /// Verifies that the constructor assigns the provided non-null userId value to the UserId property.
        /// Tests various representative string inputs including empty, whitespace-only, very long, and strings with special/control characters.
        /// Expected: The UserId property equals the exact input value after construction and the default initializer is overridden.
        /// </summary>
        [TestMethod]
        public void GetCurrentUserStudyYearQuery_Constructor_AssignsUserId_ForVariousInputs()
        {
            // Arrange
            string typical = "user-123";
            string empty = string.Empty;
            string whitespace = "   ";
            string specialChars = "user\n\t\r\u0000!@#$%^&*()";
            string veryLong = new string ('a', 10_000);
            (string input, string description)[] cases = new[]
            {
                (typical, "typical id"),
                (empty, "empty string"),
                (whitespace, "whitespace-only string"),
                (specialChars, "string with special and control characters"),
                (veryLong, "very long string (10k chars)")
            };
            foreach (var(input, description)in cases)
            {
                // Act
                var query = new GetCurrentUserStudyYearQuery(input);
                // Assert
                Assert.IsNotNull(query);
                Assert.IsInstanceOfType(query, typeof(IRequest<Response<UserStudyYearDto>>));
                // UserId is non-nullable; ensure it exactly matches the provided input
                Assert.AreEqual(input, query.UserId, $"Constructor failed to set UserId for case: {description}");
            }
        }

        /// <summary>
        /// Ensures that the default initializer (string.Empty) is overridden by the constructor when a non-empty value is provided.
        /// Input: non-empty identifier string.
        /// Expected: UserId is equal to the provided non-empty value and not the default empty string.
        /// </summary>
        [TestMethod]
        public void GetCurrentUserStudyYearQuery_Constructor_OverridesDefaultInitializer()
        {
            // Arrange
            string provided = "override-me";
            // Act
            var query = new GetCurrentUserStudyYearQuery(provided);
            // Assert
            Assert.AreEqual(provided, query.UserId);
            Assert.AreNotEqual(string.Empty, query.UserId);
        }
    }
}