#nullable enable
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.AcademicSchedules;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using System;
using System.Collections;
using System.Threading;


namespace AYA_UIS.Application.Queries.AcademicSchedules.UnitTests
{
    /// <summary>
    /// Tests for GetAcademicSchedulesBySemesterIdQuery constructor behavior.
    /// </summary>
    [TestClass]
    public class GetAcademicSchedulesBySemesterIdQueryTests
    {
        /// <summary>
        /// Verifies that the constructor assigns the SemesterId property correctly for a variety of integer inputs,
        /// including boundary values (int.MinValue, int.MaxValue), zero, negative, and positive values.
        /// Expected: the SemesterId property equals the value passed to the constructor and no exception is thrown.
        /// </summary>
        [TestMethod]
        public void GetAcademicSchedulesBySemesterIdQuery_Constructor_WithVariousInts_SetsSemesterId()
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

            foreach (int value in testValues)
            {
                // Act
                GetAcademicSchedulesBySemesterIdQuery query = new GetAcademicSchedulesBySemesterIdQuery(value);

                // Assert
                Assert.AreEqual(value, query.SemesterId, $"Constructor did not set SemesterId correctly for input {value}.");
            }
        }

        /// <summary>
        /// Ensures that the constructed object allows later mutation of the SemesterId property (public setter).
        /// Input: start value 0, then assign newValue (int.MaxValue).
        /// Expected: after setting, the property reflects the new value.
        /// Note: This test complements constructor verification by asserting the property is writable.
        /// </summary>
        [TestMethod]
        public void GetAcademicSchedulesBySemesterIdQuery_PropertySetter_AllowsMutation()
        {
            // Arrange
            int initial = 0;
            int newValue = int.MaxValue;
            GetAcademicSchedulesBySemesterIdQuery query = new GetAcademicSchedulesBySemesterIdQuery(initial);

            // Act
            query.SemesterId = newValue;

            // Assert
            Assert.AreEqual(newValue, query.SemesterId, "SemesterId setter did not store the assigned value.");
        }
    }
}