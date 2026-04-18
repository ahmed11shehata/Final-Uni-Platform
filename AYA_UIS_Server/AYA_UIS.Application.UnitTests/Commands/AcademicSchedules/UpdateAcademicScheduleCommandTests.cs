#nullable enable
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.AcademicSchedules;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;


namespace AYA_UIS.Application.Commands.AcademicSchedules.UnitTests
{
    /// <summary>
    /// Tests for UpdateAcademicScheduleCommand constructor behavior.
    /// </summary>
    [TestClass]
    public class UpdateAcademicScheduleCommandTests
    {
        /// <summary>
        /// Verifies that the constructor initializes Title, Description and File properties
        /// for a variety of title/description inputs (normal, empty, whitespace, very long, special chars).
        /// Expected: no exception and properties match provided constructor arguments.
        /// </summary>
        [TestMethod]
        public void Constructor_VariousTitleAndDescriptionValues_InitializesProperties()
        {
            // Arrange
            var longString = new string('A', 5000);
            var specialString = "Title\u0000\u0001\n\r\t\u263A";
            var testCases = new List<(string title, string description)>
            {
                ("Normal Title", "Normal Description"),
                (string.Empty, string.Empty),
                ("   ", " \t\n"),
                (longString, longString),
                (specialString, specialString)
            };

            foreach (var (title, description) in testCases)
            {
                // Use a fresh mock for each iteration to ensure identity preservation
                var mockFile = new Mock<IFormFile>(MockBehavior.Strict);
                IFormFile file = mockFile.Object;

                // Act
                var command = new UpdateAcademicScheduleCommand(title, description, file);

                // Assert
                Assert.IsNotNull(command, "Constructor returned null command instance.");
                Assert.AreEqual(title, command.Title, "Title property did not match the constructor argument.");
                Assert.AreEqual(description, command.Description, "Description property did not match the constructor argument.");
                Assert.AreSame(file, command.File, "File property did not preserve the same instance passed to constructor.");
            }
        }

        /// <summary>
        /// Verifies that the constructor preserves the exact IFormFile instance provided.
        /// Condition: a mocked IFormFile instance is passed.
        /// Expected: File property references the same instance (reference equality).
        /// </summary>
        [TestMethod]
        public void Constructor_WithMockFile_AssignsSameInstance()
        {
            // Arrange
            var title = "Sample";
            var description = "Sample desc";
            var mockFile = new Mock<IFormFile>(MockBehavior.Strict);
            IFormFile file = mockFile.Object;

            // Act
            var command = new UpdateAcademicScheduleCommand(title, description, file);

            // Assert
            Assert.IsNotNull(command);
            Assert.AreSame(file, command.File, "Constructor should assign the provided IFormFile instance to the File property.");
            Assert.AreEqual(title, command.Title);
            Assert.AreEqual(description, command.Description);
        }
    }
}