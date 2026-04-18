#nullable enable
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.AcademicSchedules;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using System;
using System.Text;


namespace AYA_UIS.Application.Queries.AcademicSchedules.UnitTests
{
    /// <summary>
    /// Tests for <see cref="GetAcademicScheduleByTitleQuery"/>.
    /// </summary>
    [TestClass]
    public class GetAcademicScheduleByTitleQueryTests
    {
        /// <summary>
        /// Verifies that the constructor assigns the ScheduleTitle property exactly as provided.
        /// Tests a variety of valid string inputs: normal text, empty string, whitespace-only,
        /// a very long string, and strings containing control/unicode characters.
        /// Expected result: the ScheduleTitle property equals the original input and is non-null.
        /// </summary>
        [TestMethod]
        public void GetAcademicScheduleByTitleQuery_Constructor_AssignsScheduleTitle_ForVariousValidInputs()
        {
            // Arrange
            string normal = "Fall 2026 Schedule";
            string empty = string.Empty;
            string whitespace = "   \t  ";
            string veryLong = new string('A', 10000); // boundary: very long input
            string special = "Title\tWith\nControl\u0001AndUnicode\u2764";

            string[] inputs = new[] { normal, empty, whitespace, veryLong, special };

            // Act & Assert
            foreach (string input in inputs)
            {
                // Act
                var query = new GetAcademicScheduleByTitleQuery(input);

                // Assert - property should be assigned exactly the provided value
                Assert.IsNotNull(query.ScheduleTitle, "ScheduleTitle must not be null for valid (non-nullable) constructor input.");
                Assert.AreEqual(input, query.ScheduleTitle, "Constructor should assign ScheduleTitle exactly as provided.");
            }
        }

        /// <summary>
        /// Ensures that providing an empty string to the constructor results in ScheduleTitle being empty (not null).
        /// This verifies that the class does not convert empty input to null and preserves the value.
        /// Expected result: ScheduleTitle == string.Empty.
        /// </summary>
        [TestMethod]
        public void GetAcademicScheduleByTitleQuery_Constructor_PreservesEmptyString()
        {
            // Arrange
            string input = string.Empty;

            // Act
            var query = new GetAcademicScheduleByTitleQuery(input);

            // Assert
            Assert.IsNotNull(query.ScheduleTitle, "ScheduleTitle must not be null.");
            Assert.AreEqual(string.Empty, query.ScheduleTitle, "Empty input should result in an empty ScheduleTitle.");
        }
    }
}