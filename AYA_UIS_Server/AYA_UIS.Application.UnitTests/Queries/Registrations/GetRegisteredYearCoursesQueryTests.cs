using System;
using System.Collections;
using System.Threading;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Registrations;
using AYA_UIS.Core.Domain.Entities;
using MediatR;
using Shared.Dtos.Info_Module;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AYA_UIS.Application.Queries.Registrations.UnitTests
{
    /// <summary>
    /// Tests for AYA_UIS.Application.Queries.Registrations.GetRegisteredYearCoursesQuery
    /// </summary>
    [TestClass]
    public partial class GetRegisteredYearCoursesQueryTests
    {
        /// <summary>
        /// Verifies that constructor assigns StudentId exactly as provided for various representative string inputs.
        /// Tests empty, whitespace, typical, long, and special-character-containing strings.
        /// Expected: StudentId property equals the provided input and no exception is thrown.
        /// </summary>
        [TestMethod]
        public void Constructor_VariousStudentIdInputs_AssignsStudentIdExactly()
        {
            // Arrange
            var year = 2023;
            var testInputs = new string[]
            {
                string.Empty,
                " ",
                "student-123",
                new string('a', 1024), // very long string
                "sp\u0000ecial\t\nchars" // includes control chars
            };

            foreach (var input in testInputs)
            {
                // Act
                var ex = RecordException(() => new GetRegisteredYearCoursesQuery(input, year));

                // Assert
                Assert.IsNull(ex, $"Constructor threw for input: [{input}] with exception: {ex}");
                var query = new GetRegisteredYearCoursesQuery(input, year);
                Assert.AreEqual(input, query.StudentId, "StudentId should match the provided value exactly.");
                Assert.AreEqual(year, query.Year, "Year should match the provided value.");
            }
        }

        /// <summary>
        /// Verifies that constructor assigns Year exactly as provided for boundary and special integer values.
        /// Tests int.MinValue, -1, 0, 1, int.MaxValue.
        /// Expected: Year property equals the provided value and no exception is thrown.
        /// </summary>
        [TestMethod]
        public void Constructor_BoundaryYearValues_AssignsYearExactly()
        {
            // Arrange
            var studentId = "s-1";
            var years = new int[]
            {
                int.MinValue,
                -1,
                0,
                1,
                int.MaxValue
            };

            foreach (var y in years)
            {
                // Act
                var ex = RecordException(() => new GetRegisteredYearCoursesQuery(studentId, y));

                // Assert no exception thrown
                Assert.IsNull(ex, $"Constructor threw for year: {y} with exception: {ex}");
                var query = new GetRegisteredYearCoursesQuery(studentId, y);
                Assert.AreEqual(studentId, query.StudentId, "StudentId should match the provided value.");
                Assert.AreEqual(y, query.Year, "Year should match the provided value exactly.");
            }
        }

        /// <summary>
        /// Helper to capture any exception thrown by an action. Keeps tests concise without using DataTestMethod.
        /// Returns the exception instance or null if no exception was thrown.
        /// </summary>
        private static Exception? RecordException(Action action)
        {
            try
            {
                action();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}