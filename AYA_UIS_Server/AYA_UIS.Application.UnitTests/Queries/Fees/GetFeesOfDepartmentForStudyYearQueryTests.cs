using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Fees;
using MediatR;
using Shared.Dtos.Info_Module;
using System;
using System.Collections;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AYA_UIS.Application.Queries.Fees.UnitTests
{
    /// <summary>
    /// Tests for GetFeesOfDepartmentForStudyYearQuery constructor.
    /// </summary>
    [TestClass]
    public partial class GetFeesOfDepartmentForStudyYearQueryTests
    {
        /// <summary>
        /// Verifies that the constructor assigns DepartmentId and StudyYearId exactly as provided.
        /// Tests multiple edge integer values including int.MinValue, negative, zero, positive, and int.MaxValue.
        /// Expected: properties match inputs without validation or transformation.
        /// </summary>
        [TestMethod]
        public void Constructor_VariousIntegerEdges_AssignsPropertiesExactly()
        {
            // Arrange
            var testCases = new (int departmentId, int studyYearId)[]
            {
                (int.MinValue, int.MinValue),
                (int.MinValue, int.MaxValue),
                (-1, -1),
                (-1, 0),
                (0, 0),
                (0, 1),
                (1, 1),
                (1, int.MaxValue),
                (int.MaxValue, int.MaxValue)
            };

            foreach (var (departmentId, studyYearId) in testCases)
            {
                // Act
                var query = new GetFeesOfDepartmentForStudyYearQuery(departmentId, studyYearId);

                // Assert
                Assert.AreEqual(departmentId, query.DepartmentId, "DepartmentId should be assigned from constructor parameter.");
                Assert.AreEqual(studyYearId, query.StudyYearId, "StudyYearId should be assigned from constructor parameter.");
            }
        }

        /// <summary>
        /// Ensures that when departmentId and studyYearId differ, each property stores its respective value.
        /// Input: departmentId = 42, studyYearId = 7.
        /// Expected: DepartmentId == 42 and StudyYearId == 7.
        /// </summary>
        [TestMethod]
        public void Constructor_DifferentValues_AssignsEachPropertyIndependently()
        {
            // Arrange
            int departmentId = 42;
            int studyYearId = 7;

            // Act
            var query = new GetFeesOfDepartmentForStudyYearQuery(departmentId, studyYearId);

            // Assert
            Assert.AreEqual(42, query.DepartmentId);
            Assert.AreEqual(7, query.StudyYearId);
        }
    }
}