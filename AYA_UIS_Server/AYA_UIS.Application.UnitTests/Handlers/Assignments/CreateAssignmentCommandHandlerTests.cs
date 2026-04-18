using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using Abstraction;
using Abstraction.Contracts;
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.CreateAssignment;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Assignments;
using AYA_UIS.Core.Domain;
using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared;
using Shared.Dtos.Info_Module.AssignmentDto;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Assignments.UnitTests
{
    [TestClass]
    public class CreateAssignmentCommandHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor succeeds when provided with valid, non-null dependencies.
        /// Input conditions: mocked IUnitOfWork and ILocalFileService are provided (non-null).
        /// Expected result: an instance of CreateAssignmentCommandHandler is created and implements the expected IRequestHandler interface.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstanceAndImplementsInterface()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var fileServiceMock = new Mock<ILocalFileService>();

            // Act
            var handler = new CreateAssignmentCommandHandler(unitOfWorkMock.Object, fileServiceMock.Object);

            // Assert
            Assert.IsNotNull(handler, "Constructor returned null when provided valid dependencies.");
            Assert.IsInstanceOfType(
                handler,
                typeof(IRequestHandler<CreateAssignmentCommand, Response<int>>),
                "Handler does not implement IRequestHandler<CreateAssignmentCommand, Response<int>> as expected.");
        }

        /// <summary>
        /// Ensures multiple constructions with different dependency instances produce distinct handler instances.
        /// Input conditions: two separate mocked dependency instances are supplied for two constructions.
        /// Expected result: two distinct CreateAssignmentCommandHandler instances (not the same reference).
        /// </summary>
        [TestMethod]
        public void Constructor_DifferentDependencyInstances_CreatesDistinctHandlerInstances()
        {
            // Arrange
            var unitOfWorkMock1 = new Mock<IUnitOfWork>();
            var fileServiceMock1 = new Mock<ILocalFileService>();

            var unitOfWorkMock2 = new Mock<IUnitOfWork>();
            var fileServiceMock2 = new Mock<ILocalFileService>();

            // Act
            var handler1 = new CreateAssignmentCommandHandler(unitOfWorkMock1.Object, fileServiceMock1.Object);
            var handler2 = new CreateAssignmentCommandHandler(unitOfWorkMock2.Object, fileServiceMock2.Object);

            // Assert
            Assert.IsNotNull(handler1, "First constructed handler should not be null.");
            Assert.IsNotNull(handler2, "Second constructed handler should not be null.");
            Assert.AreNotSame(handler1, handler2, "Two handlers constructed with different dependency instances should not reference the same object.");
        }

        /// <summary>
        /// Tests that when the incoming IFormFile is null (nullable property), the handler still calls the upload service
        /// and uses the returned file URL (here an empty string) as the Assignment.FileUrl. Expected: operation succeeds
        /// and returned Response contains the repository-assigned Id and assignment.FileUrl equals the returned value.
        /// </summary>
        [TestMethod]
        public async Task Handle_NullFile_UsesReturnedFileUrlAndReturnsSuccess()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var assignmentRepoMock = new Mock<AYA_UIS.Core.Domain.Contracts.IAssignmentRepository>();
            unitOfWorkMock.SetupGet(u => u.Assignments).Returns(assignmentRepoMock.Object);
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            Assignment? capturedAssignment = null;
            assignmentRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Assignment>()))
                .Returns<Assignment>(a =>
                {
                    capturedAssignment = a;
                    a.Id = 7;
                    return Task.CompletedTask;
                });

            var fileServiceMock = new Mock<ILocalFileService>();
            // Simulate service accepting a null file and returning empty string as file URL
            fileServiceMock
                .Setup(fs => fs.UploadAssignmentFileAsync(null, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(string.Empty);

            var handler = new CreateAssignmentCommandHandler(unitOfWorkMock.Object, fileServiceMock.Object);

            var dto = new CreateAssignmentDto
            {
                Title = string.Empty, // empty string edge case
                Description = "  ", // whitespace-only description
                Points = 0, // boundary
                Deadline = DateTime.MinValue,
                CourseId = 0
            };

            var command = new CreateAssignmentCommand
            {
                AssignmentDto = dto,
                File = null,
                InstructorId = string.Empty // empty instructor id
            };

            // Act
            var response = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsTrue(response.Success, "Operation should succeed even if file is null as long as upload service handles it.");
            Assert.AreEqual(7, response.Data, "Returned Data should equal repository-assigned Id.");
            Assert.IsNotNull(capturedAssignment, "Repository AddAsync should have been called.");
            Assert.AreEqual(string.Empty, capturedAssignment!.FileUrl, "FileUrl should be exactly the value returned by the file service (empty string).");
        }

    }
}