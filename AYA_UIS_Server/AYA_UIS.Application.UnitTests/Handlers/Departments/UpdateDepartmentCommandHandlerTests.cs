using System;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Departments;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Departments;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Shared;
using AYA_UIS.Shared.Exceptions;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.DepartmentDtos;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Departments.UnitTests
{
    /// <summary>
    /// Tests for UpdateDepartmentCommandHandler constructor behavior.
    /// </summary>
    [TestClass]
    public class UpdateDepartmentCommandHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor creates a non-null instance and the instance implements
        /// IRequestHandler&lt;UpdateDeparmentCommand, Response&lt;DepartmentDto&gt;&gt; when valid dependencies are provided.
        /// Input conditions: valid (non-null) mocks for IUnitOfWork and IMapper are supplied.
        /// Expected result: an instance is created and typed as the expected MediatR handler interface.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstanceAndImplementsInterface()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mapperMock = new Mock<IMapper>();

            // Act
            var handler = new UpdateDepartmentCommandHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Assert
            Assert.IsNotNull(handler, "Handler instance should not be null when constructed with valid dependencies.");
            Assert.IsInstanceOfType(
                handler,
                typeof(IRequestHandler<UpdateDeparmentCommand, Response<DepartmentDto>>),
                "Handler should implement IRequestHandler<UpdateDeparmentCommand, Response<DepartmentDto>>.");
        }

        /// <summary>
        /// Ensures the constructor does not interact with provided dependencies during construction.
        /// Input conditions: fresh mocks for IUnitOfWork and IMapper with no setups or expectations.
        /// Expected result: no calls are made on the mocks by the constructor (VerifyNoOtherCalls passes).
        /// </summary>
        [TestMethod]
        public void Constructor_DoesNotCallDependencies_OnConstruction()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            // Act
            var handler = new UpdateDepartmentCommandHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Assert
            // Instance created successfully
            Assert.IsNotNull(handler, "Handler should be constructed successfully even when mocks are strict.");

            // The constructor should not have invoked any members on the provided dependencies.
            unitOfWorkMock.VerifyNoOtherCalls();
            mapperMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Ensures that when a department exists it is updated with values from the request,
        /// repository.Update and uow.SaveChangesAsync are called, mapper is invoked, and a successful Response is returned.
        /// Tests multiple representative string edge cases and numeric id extremes in a single test to avoid redundancy.
        /// </summary>
        [TestMethod]
        public async Task Handle_DepartmentExists_UpdatesProperties_CallsUpdateAndSaveAndReturnsMappedResponse()
        {
            // Arrange - test cases combining id extremes and string edge cases
            var cases = new[]
            {
                new { Id = int.MinValue, Name = "", Code = "C1", Description = (string?)null },
                new { Id = 0, Name = "   ", Code = "C2", Description = "Contains\nControl\tChars" },
                new { Id = int.MaxValue, Name = new string('x', 1024), Code = "Code$%^", Description = new string('d', 512) }
            };

            foreach (var c in cases)
            {
                var repoMock = new Mock<IDepartmentRepository>();
                var uowMock = new Mock<IUnitOfWork>();
                var mapperMock = new Mock<IMapper>();

                // Original department returned from repository (before update)
                var originalDepartment = new Department
                {
                    Name = "OldName",
                    Code = "OldCode",
                    Description = "OldDesc"
                };

                // Ensure Departments property returns our repository mock
                repoMock.Setup(r => r.GetByIdAsync(c.Id)).ReturnsAsync(originalDepartment);
                // Capture the department passed to Update
                Department? capturedUpdated = null;
                repoMock.Setup(r => r.Update(It.IsAny<Department>()))
                        .Callback<Department>(d => capturedUpdated = d)
                        .Returns(Task.CompletedTask);

                uowMock.Setup(u => u.Departments).Returns(repoMock.Object);
                uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

                // Mapper should return a DepartmentDto built from the passed department instance
                mapperMock.Setup(m => m.Map<DepartmentDto>(It.IsAny<object>()))
                          .Returns((object src) =>
                          {
                              var srcDept = src as Department;
                              return new DepartmentDto
                              {
                                  Id = c.Id,
                                  Name = srcDept?.Name ?? string.Empty,
                                  Code = srcDept?.Code ?? string.Empty,
                                  Description = srcDept?.Description
                              };
                          });

                var handler = new UpdateDepartmentCommandHandler(uowMock.Object, mapperMock.Object);

                var updateDto = new UpdateDepartmentDto
                {
                    Name = c.Name,
                    Code = c.Code,
                    Description = c.Description
                };

                var request = new UpdateDeparmentCommand(c.Id, updateDto);

                // Act
                var response = await handler.Handle(request, CancellationToken.None);

                // Assert - repository.Update was called with the modified department
                repoMock.Verify(r => r.GetByIdAsync(c.Id), Times.Once);
                repoMock.Verify(r => r.Update(It.IsAny<Department>()), Times.Once);
                uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
                mapperMock.Verify(m => m.Map<DepartmentDto>(It.IsAny<object>()), Times.Once);

                Assert.IsNotNull(capturedUpdated, "Expected repository.Update to be invoked and capture the updated department instance.");
                Assert.AreEqual(c.Name, capturedUpdated!.Name, "Department.Name should be updated from request");
                Assert.AreEqual(c.Code, capturedUpdated.Code, "Department.Code should be updated from request");
                Assert.AreEqual(c.Description, capturedUpdated.Description, "Department.Description should be updated from request");

                Assert.IsNotNull(response, "Handler must return a response");
                Assert.IsTrue(response.Success, "Response should indicate success for existing department");
                Assert.IsNotNull(response.Data, "Response.Data should contain the mapped DepartmentDto");
                Assert.AreEqual(c.Id, response.Data!.Id, "Mapped DTO Id should match the requested Id");
                Assert.AreEqual(c.Name, response.Data.Name, "Mapped DTO Name should reflect updated value");
                Assert.AreEqual(c.Code, response.Data.Code, "Mapped DTO Code should reflect updated value");
                Assert.AreEqual(c.Description, response.Data.Description, "Mapped DTO Description should reflect updated value");
            }
        }
    }
}