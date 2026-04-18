#nullable enable
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.CoursePrequisites;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using System;
using System.Collections;
using System.Threading;


namespace AYA_UIS.Application.Queries.CoursePrequisites.UnitTests
{
    /// <summary>
    /// Tests for GetCoursePrequisitesQuery constructor and CourseId property behavior.
    /// </summary>
    [TestClass]
    public class GetCoursePrequisitesQueryTests
    {
        /// <summary>
        /// Verifies that the constructor assigns the CourseId property for a variety of integer inputs,
        /// including boundary values. This ensures no validation or exceptions occur and the value is stored as provided.
        /// Inputs tested: int.MinValue, -1, 0, 1, int.MaxValue.
        /// Expected: The CourseId property equals the constructor argument for each case.
        /// </summary>
        [TestMethod]
        public void GetCoursePrequisitesQuery_Constructor_VariousIntegers_SetsCourseId()
        {
            // Arrange
            int[] testValues = new int[] { int.MinValue, -1, 0, 1, int.MaxValue };

            foreach (int input in testValues)
            {
                // Act
                var query = new GetCoursePrequisitesQuery(input);

                // Assert
                Assert.AreEqual(input, query.CourseId, $"Constructor should set CourseId to {input}");
            }
        }

        /// <summary>
        /// Verifies that the CourseId property is mutable after construction.
        /// Condition: construct with an initial value and assign a new value via setter.
        /// Expected: CourseId reflects the updated value and no exception is thrown.
        /// </summary>
        [TestMethod]
        public void CourseId_Setter_AfterConstruction_UpdatesValue()
        {
            // Arrange
            int initial = 42;
            int updated = 100;
            var query = new GetCoursePrequisitesQuery(initial);

            // Act
            query.CourseId = updated;

            // Assert
            Assert.AreEqual(updated, query.CourseId, "Setter should update the CourseId value after construction.");
        }
    }
}