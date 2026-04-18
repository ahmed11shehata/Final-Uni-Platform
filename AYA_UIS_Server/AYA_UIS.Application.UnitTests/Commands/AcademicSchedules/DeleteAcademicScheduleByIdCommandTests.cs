#nullable enable
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.AcademicSchedules;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Threading;


namespace AYA_UIS.Application.Commands.AcademicSchedules.UnitTests
{
    /// <summary>
    /// Unit tests for AYA_UIS.Application.Commands.AcademicSchedules.DeleteAcademicScheduleByIdCommand
    /// </summary>
    [TestClass]
    public class DeleteAcademicScheduleByIdCommandTests
    {
        /// <summary>
        /// Verifies that the constructor correctly assigns the provided id to the Id property.
        /// Tests a set of numeric edge values including int.MinValue, negative, zero, positive, and int.MaxValue.
        /// Expected: The constructed instance's Id equals the provided id for all tested values and no exception is thrown.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidAndBoundaryIds_InitializesId()
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
                DeleteAcademicScheduleByIdCommand command = new DeleteAcademicScheduleByIdCommand(id);

                // Assert
                Assert.AreEqual(id, command.Id, $"Constructor should set Id to the provided value. Failed for id={id}.");
            }
        }

        /// <summary>
        /// Verifies that DeleteAcademicScheduleByIdCommand implements MediatR.IRequest&lt;bool&gt;.
        /// Condition: any valid integer id (here 0 is used).
        /// Expected: The instance is assignable to IRequest&lt;bool&gt; and no exception is thrown.
        /// </summary>
        [TestMethod]
        public void Constructor_Instance_ImplementsIRequestOfBool()
        {
            // Arrange
            int id = 0;

            // Act
            DeleteAcademicScheduleByIdCommand command = new DeleteAcademicScheduleByIdCommand(id);

            // Assert
            Assert.IsInstanceOfType(command, typeof(IRequest<bool>), "DeleteAcademicScheduleByIdCommand should implement IRequest<bool>.");
        }
    }
}