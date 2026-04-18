#nullable enable
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.AcademicSchedules;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared.Dtos.Info_Module;
using System;
using System.Collections;
using System.Threading;


namespace AYA_UIS.Application.Queries.AcademicSchedules.UnitTests
{
    [TestClass]
    public class GetAcademicScheduleByIdQueryTests
    {
        /// <summary>
        /// Verifies that the constructor assigns the provided id to the Id property for a variety of integer inputs,
        /// including boundary and typical values.
        /// Inputs: int.MinValue, -1, 0, 1, int.MaxValue
        /// Expected: The constructed instance's Id equals the provided input and no exception is thrown.
        /// </summary>
        [TestMethod]
        public void GetAcademicScheduleByIdQuery_Constructor_SetsId_ForVariousIntegers()
        {
            // Arrange
            int[] testIds = new[]
            {
                int.MinValue,
                -1,
                0,
                1,
                int.MaxValue
            };

            foreach (int id in testIds)
            {
                // Act
                GetAcademicScheduleByIdQuery query = new GetAcademicScheduleByIdQuery(id);

                // Assert
                Assert.AreEqual(id, query.Id, $"Constructor should set Id to provided value. Failed for id={id}.");
            }
        }

        /// <summary>
        /// Ensures that using the record 'with' expression produces a new instance with the modified Id,
        /// while the original instance remains unchanged.
        /// Input: original id = 42, modified id = 100
        /// Expected: original.Id remains 42, newInstance.Id equals 100, and the two instances are not equal.
        /// </summary>
        [TestMethod]
        public void GetAcademicScheduleByIdQuery_WithExpression_CreatesNewInstanceWithModifiedId()
        {
            // Arrange
            int originalId = 42;
            int modifiedId = 100;
            GetAcademicScheduleByIdQuery original = new GetAcademicScheduleByIdQuery(originalId);

            // Act
            GetAcademicScheduleByIdQuery modified = original with { Id = modifiedId };

            // Assert
            Assert.AreEqual(originalId, original.Id, "Original instance Id should remain unchanged after 'with' expression.");
            Assert.AreEqual(modifiedId, modified.Id, "Modified instance should have the new Id value.");
            Assert.AreNotEqual(original, modified, "Original and modified instances should not be equal when Id differs.");
        }
    }
}