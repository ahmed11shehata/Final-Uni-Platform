using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.CoursePrequisites;
using MediatR;
using Shared.Dtos.Info_Module;
using System;
using System.Collections;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AYA_UIS.Application.Queries.CoursePrequisites.UnitTests
{
    /// <summary>
    /// Tests for GetCourseDependenciesQuery constructor and basic behavior.
    /// </summary>
    [TestClass]
    public partial class GetCourseDependenciesQueryTests
    {
        /// <summary>
        /// Verifies that the constructor assigns the provided CourseId value to the CourseId property.
        /// Tests a variety of integer edge cases (int.MinValue, int.MaxValue, negative, zero, positive).
        /// Expected: Property equals the value passed to the constructor and no exception is thrown.
        /// </summary>
        [TestMethod]
        public void GetCourseDependenciesQuery_Constructor_AssignsCourseId_ForVariousIntegers()
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
                GetCourseDependenciesQuery query = new GetCourseDependenciesQuery(input);

                // Assert
                Assert.IsNotNull(query, "Constructor returned null instance.");
                Assert.AreEqual(input, query.CourseId, $"CourseId should be set to the constructor input ({input}).");
            }
        }

    }
}