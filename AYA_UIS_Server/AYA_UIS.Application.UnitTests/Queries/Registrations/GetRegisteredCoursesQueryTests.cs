using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Registrations;
using MediatR;
using Shared.Dtos.Info_Module;
using System;
using System.Collections;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AYA_UIS.Application.Queries.Registrations.UnitTests
{
    [TestClass]
    public partial class GetRegisteredCoursesQueryTests
    {
        /// <summary>
        /// Verifies that the constructor assigns the provided non-null StudentId to the StudentId property.
        /// Test cases include empty string, whitespace-only, very long string, and strings with special/control characters.
        /// Expected: The StudentId property equals the exact string passed to the constructor for each case.
        /// </summary>
        [TestMethod]
        public void GetRegisteredCoursesQuery_Constructor_AssignsStudentId_ForVariousStrings()
        {
            // Arrange
            var testCases = new[]
            {
                string.Empty,
                "   ", // whitespace-only
                new string('A', 5000), // very long string
                "normal-id-123",
                "special-!@#$%^&*()\u0000\u0001" // contains special and control chars
            };

            foreach (var input in testCases)
            {
                // Act
                var query = new GetRegisteredCoursesQuery(input);

                // Assert
                Assert.IsNotNull(query.StudentId, "StudentId should not be null after construction when a non-null value is provided.");
                Assert.AreEqual(input, query.StudentId, "Constructor should assign the StudentId property to the provided value.");
            }
        }

        /// <summary>
        /// Verifies that the constructor overwrites the property's default initializer (empty string) with the provided value.
        /// Condition: provide a non-empty value.
        /// Expected: StudentId is equal to the provided non-empty value and not the default empty string.
        /// </summary>
        [TestMethod]
        public void GetRegisteredCoursesQuery_Constructor_OverwritesDefaultEmptyString()
        {
            // Arrange
            var provided = "student-xyz";

            // Act
            var query = new GetRegisteredCoursesQuery(provided);

            // Assert
            Assert.AreEqual(provided, query.StudentId, "Constructor must overwrite the property's default initializer with the provided value.");
            Assert.AreNotEqual(string.Empty, query.StudentId, "After construction with a non-empty value, StudentId should not remain the default empty string.");
        }
    }
}