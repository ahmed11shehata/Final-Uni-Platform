using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Fees;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using System;
using System.Collections;
using System.Threading;


namespace AYA_UIS.Application.Queries.Fees.UnitTests
{
    /// <summary>
    /// Tests for AYA_UIS.Application.Queries.Fees.GetFeesOfStudyYearQuery constructor behavior.
    /// </summary>
    [TestClass]
    public class GetFeesOfStudyYearQueryTests
    {
        /// <summary>
        /// Verifies that the constructor assigns the provided studyYearId value to the StudyYearId property
        /// for a variety of integer edge cases (int.MinValue, negative, zero, positive, int.MaxValue).
        /// Expected result: StudyYearId equals the constructor input and no exception is thrown.
        /// </summary>
        [TestMethod]
        public void Constructor_WithVariousStudyYearIds_SetsPropertyCorrectly()
        {
            // Arrange
            int[] testValues = new[]
            {
                int.MinValue,
                -1,
                0,
                1,
                int.MaxValue
            };

            foreach (int input in testValues)
            {
                // Act
                var query = new GetFeesOfStudyYearQuery(input);

                // Assert
                Assert.IsNotNull(query, "Constructor should not return null.");
                Assert.AreEqual(input, query.StudyYearId, $"Constructor should set StudyYearId to the provided value ({input}).");
            }
        }

        /// <summary>
        /// Ensures that the constructor initializes StudyYearId independently and that the property remains writable.
        /// Input condition: initial value provided to constructor.
        /// Expected: initial value is set, and after modifying the property it reflects the new value.
        /// </summary>
        [TestMethod]
        public void Constructor_PropertyMutable_AfterConstructionCanModifyStudyYearId()
        {
            // Arrange
            int initial = 2023;
            int updated = -2023;

            // Act
            var query = new GetFeesOfStudyYearQuery(initial);

            // Assert initial assignment
            Assert.AreEqual(initial, query.StudyYearId, "Constructor must set the initial StudyYearId correctly.");

            // Act: modify property
            query.StudyYearId = updated;

            // Assert modification
            Assert.AreEqual(updated, query.StudyYearId, "StudyYearId property must be writable and reflect updated value.");
        }
    }
}