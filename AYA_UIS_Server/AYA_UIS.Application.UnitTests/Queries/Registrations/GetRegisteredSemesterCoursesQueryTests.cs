using AYA_UIS.Application.Queries.Registrations;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared.Dtos.Info_Module;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;


namespace AYA_UIS.Application.Queries.Registrations.UnitTests
{
    [TestClass]
    public class GetRegisteredSemesterCoursesQueryTests
    {
        /// <summary>
        /// Verifies that the constructor correctly assigns StudyYearId, SemesterId and StudentId
        /// for a variety of boundary and special-case inputs. This test iterates multiple
        /// input tuples (studyYearId, semesterId, studentId) including int.MinValue,
        /// int.MaxValue, zero, negative, whitespace and very long strings. Expected result:
        /// properties on the created instance match the constructor arguments exactly.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidParameters_PropertiesSet()
        {
            // Arrange
            string veryLong = new string('A', 10_000); // very long string case
            var specialChars = "ID\n\t\u0000#©✓"; // includes newline, tab, null char, and symbols

            var testCases = new List<(int studyYearId, int semesterId, string studentId)>
            {
                (int.MinValue, int.MinValue, "S0"),
                (0, 0, string.Empty),
                (1, 2, "   "), // whitespace-only
                (int.MaxValue, int.MaxValue, veryLong),
                (-1, 999, specialChars)
            };

            foreach (var (studyYearId, semesterId, studentId) in testCases)
            {
                // Act
                var query = new GetRegisteredSemesterCoursesQuery(studyYearId, semesterId, studentId);

                // Assert
                Assert.AreEqual(studyYearId, query.StudyYearId, "StudyYearId should match the constructor input.");
                Assert.AreEqual(semesterId, query.SemesterId, "SemesterId should match the constructor input.");
                Assert.AreEqual(studentId, query.StudentId, "StudentId should match the constructor input.");
            }
        }

        /// <summary>
        /// Ensures that creating multiple instances with different inputs does not share state.
        /// Purpose: detect accidental static/shared state or mutation across instances.
        /// Inputs: two distinct constructor parameter sets.
        /// Expected: each instance preserves its own independent property values.
        /// </summary>
        [TestMethod]
        public void Constructor_MultipleInstances_NoSharedState()
        {
            // Arrange
            var firstId = "FirstStudent";
            var secondId = "SecondStudent";
            var first = new GetRegisteredSemesterCoursesQuery(1, 1, firstId);
            var second = new GetRegisteredSemesterCoursesQuery(2, 2, secondId);

            // Act & Assert
            Assert.AreEqual(1, first.StudyYearId, "First instance StudyYearId must be 1.");
            Assert.AreEqual(1, first.SemesterId, "First instance SemesterId must be 1.");
            Assert.AreEqual(firstId, first.StudentId, "First instance StudentId must match.");

            Assert.AreEqual(2, second.StudyYearId, "Second instance StudyYearId must be 2.");
            Assert.AreEqual(2, second.SemesterId, "Second instance SemesterId must be 2.");
            Assert.AreEqual(secondId, second.StudentId, "Second instance StudentId must match.");
        }

        /// <summary>
        /// Validates that the constructor accepts boundary integer values without throwing exceptions.
        /// Inputs: int.MinValue and int.MaxValue combined with a normal StudentId.
        /// Expected: no exception thrown and properties set to the provided extreme values.
        /// </summary>
        [TestMethod]
        public void Constructor_ExtremeIntegerValues_NoExceptionAndPropertiesSet()
        {
            // Arrange
            var normalStudentId = "BoundaryStudent";

            // Act
            var minQuery = new GetRegisteredSemesterCoursesQuery(int.MinValue, int.MinValue, normalStudentId);
            var maxQuery = new GetRegisteredSemesterCoursesQuery(int.MaxValue, int.MaxValue, normalStudentId);

            // Assert
            Assert.AreEqual(int.MinValue, minQuery.StudyYearId);
            Assert.AreEqual(int.MinValue, minQuery.SemesterId);
            Assert.AreEqual(normalStudentId, minQuery.StudentId);

            Assert.AreEqual(int.MaxValue, maxQuery.StudyYearId);
            Assert.AreEqual(int.MaxValue, maxQuery.SemesterId);
            Assert.AreEqual(normalStudentId, maxQuery.StudentId);
        }
    }
}