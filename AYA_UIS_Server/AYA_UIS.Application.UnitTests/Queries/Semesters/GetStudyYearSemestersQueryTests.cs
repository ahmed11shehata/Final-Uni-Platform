using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Semesters;
using MediatR;
using Shared.Dtos.Info_Module;
using System;
using System.Collections;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AYA_UIS.Application.Queries.Semesters.UnitTests
{
    /// <summary>
    /// Tests for AYA_UIS.Application.Queries.Semesters.GetStudyYearSemestersQuery
    /// </summary>
    [TestClass]
    public partial class GetStudyYearSemestersQueryTests
    {
        /// <summary>
        /// Verifies that two instances constructed with the same StudyYearId have equal StudyYearId properties
        /// and that different values produce different StudyYearId values.
        /// Inputs: sameValue = 42, differentValue = -42.
        /// Expected: matching StudyYearId for same inputs; non-matching for different inputs.
        /// </summary>
        [TestMethod]
        public void Constructor_SameAndDifferentValues_StudyYearIdReflectsInput()
        {
            // Arrange
            int sameValue = 42;
            int differentValue = -42;

            // Act
            var q1 = new GetStudyYearSemestersQuery(sameValue);
            var q2 = new GetStudyYearSemestersQuery(sameValue);
            var q3 = new GetStudyYearSemestersQuery(differentValue);

            // Assert
            // same inputs => same property values
            Assert.AreEqual(q1.StudyYearId, q2.StudyYearId, "Instances created with the same constructor value should have equal StudyYearId.");
            Assert.AreEqual(sameValue, q1.StudyYearId, "StudyYearId should equal the provided value.");

            // different inputs => different property values
            Assert.AreNotEqual(q1.StudyYearId, q3.StudyYearId, "Instances created with different constructor values should have different StudyYearId.");
            Assert.AreEqual(differentValue, q3.StudyYearId, "StudyYearId should equal the provided different value.");
        }
    }
}