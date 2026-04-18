using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Departments;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Departments;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
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
    [TestClass]
    public partial class CreateDepartmentCommandHandlerTests
    {
        /// <summary>
        /// Verifies that Handle maps the incoming CreateDepartmentDto to a Department, calls repository AddAsync,
        /// calls SaveChangesAsync and returns a successful Response containing the mapped DepartmentDto.
        /// Input conditions: several representative CreateDepartmentDto variations (empty, whitespace, long, special chars, null description).
        /// Expected result: Response.Success is true, returned Data matches the mapped DepartmentDto, repository and unit-of-work methods are invoked once.
        /// </summary>
        [TestMethod]
        public async Task Handle_ValidRequestVariations_ReturnsSuccessAndPersistsDepartment()
        {
            // Arrange: prepare multiple representative DTO inputs to exercise string edge cases.
            var testCases = new List<CreateDepartmentDto>
            {
                // empty name and code (allowed by DTO non-nullable contract but empty)
                new CreateDepartmentDto { Name = string.Empty, Code = string.Empty, Description = null },
                // whitespace-only
                new CreateDepartmentDto { Name = "   ", Code = "\t", Description = "desc" },
                // long strings
                new CreateDepartmentDto { Name = new string('A', 5000), Code = new string('C', 5000), Description = new string('D', 2000) },
                // special/control characters
                new CreateDepartmentDto { Name = "Name\u0000WithNull", Code = "Code\nWithNewLine", Description = null }
            };

            foreach (var dto in testCases)
            {
                // Arrange per-case
                var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
                var mockDepartmentRepo = new Mock<IDepartmentRepository>(MockBehavior.Strict);
                mockUnitOfWork.Setup(u => u.Departments).Returns(mockDepartmentRepo.Object);
                mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

                var mappedDepartment = new Department
                {
                    Name = dto.Name,
                    Code = dto.Code,
                    Description = dto.Description
                };

                var mappedDto = new DepartmentDto
                {
                    Id = 0,
                    Name = mappedDepartment.Name,
                    Code = mappedDepartment.Code,
                    Description = mappedDepartment.Description
                };

                var mockMapper = new Mock<IMapper>(MockBehavior.Strict);
                // Map from CreateDepartmentDto -> Department
                mockMapper
                    .Setup(m => m.Map<Department>(It.Is<object>(o => ReferenceEquals(o, dto))))
                    .Returns(mappedDepartment);
                // Map from Department -> DepartmentDto
                mockMapper
                    .Setup(m => m.Map<DepartmentDto>(It.Is<object>(o => ReferenceEquals(o, mappedDepartment))))
                    .Returns(mappedDto);

                mockDepartmentRepo.Setup(r => r.AddAsync(It.Is<Department>(d => ReferenceEquals(d, mappedDepartment))))
                                  .Returns(Task.CompletedTask);

                var handler = new CreateDepartmentCommandHandler(mockUnitOfWork.Object, mockMapper.Object);

                var request = new CreateDepartmentCommand { Department = dto };

                // Act
                var result = await handler.Handle(request, CancellationToken.None).ConfigureAwait(false);

                // Assert
                Assert.IsNotNull(result, "Response should not be null.");
                Assert.IsTrue(result.Success, "Response.Success should be true for successful creation.");
                Assert.IsNotNull(result.Data, "Response.Data should not be null on success.");
                Assert.AreEqual(mappedDto.Name, result.Data!.Name, "Mapped Name should be returned in Response.Data.");
                Assert.AreEqual(mappedDto.Code, result.Data.Code, "Mapped Code should be returned in Response.Data.");
                Assert.AreEqual(mappedDto.Description, result.Data.Description, "Mapped Description should be returned in Response.Data.");
                Assert.AreEqual("Operation completed successfully", result.Message, "Success message should match the Response implementation.");

                mockMapper.Verify(m => m.Map<Department>(It.Is<object>(o => ReferenceEquals(o, dto))), Times.Once);
                mockDepartmentRepo.Verify(r => r.AddAsync(It.Is<Department>(d => ReferenceEquals(d, mappedDepartment))), Times.Once);
                mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
                mockMapper.Verify(m => m.Map<DepartmentDto>(It.Is<object>(o => ReferenceEquals(o, mappedDepartment))), Times.Once);

                // Cleanup verifications for next iteration (strict mocks will be disposed after leaving scope)
            }
        }

        /// <summary>
        /// Ensures that even if SaveChangesAsync returns 0 (no rows affected), the handler still returns a successful Response
        /// because the handler does not branch on the save result.
        /// Input conditions: a minimal valid CreateDepartmentDto and SaveChangesAsync => 0.
        /// Expected result: Response.Success is true and repository methods are still called.
        /// </summary>
        [TestMethod]
        public async Task Handle_SaveChangesReturnsZero_StillReturnsSuccess()
        {
            // Arrange
            var dto = new CreateDepartmentDto { Name = "Dept", Code = "DPT", Description = "Desc" };

            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mockDepartmentRepo = new Mock<IDepartmentRepository>(MockBehavior.Strict);
            mockUnitOfWork.Setup(u => u.Departments).Returns(mockDepartmentRepo.Object);
            // Simulate SaveChangesAsync returning 0
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            var mappedDepartment = new Department
            {
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description
            };

            var mappedDto = new DepartmentDto
            {
                Id = 0,
                Name = mappedDepartment.Name,
                Code = mappedDepartment.Code,
                Description = mappedDepartment.Description
            };

            var mockMapper = new Mock<IMapper>(MockBehavior.Strict);
            mockMapper.Setup(m => m.Map<Department>(It.Is<object>(o => ReferenceEquals(o, dto))))
                      .Returns(mappedDepartment);
            mockMapper.Setup(m => m.Map<DepartmentDto>(It.Is<object>(o => ReferenceEquals(o, mappedDepartment))))
                      .Returns(mappedDto);

            mockDepartmentRepo.Setup(r => r.AddAsync(It.Is<Department>(d => ReferenceEquals(d, mappedDepartment))))
                              .Returns(Task.CompletedTask);

            var handler = new CreateDepartmentCommandHandler(mockUnitOfWork.Object, mockMapper.Object);
            var request = new CreateDepartmentCommand { Department = dto };

            // Act
            var result = await handler.Handle(request, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result, "Response should not be null even when SaveChangesAsync returns 0.");
            Assert.IsTrue(result.Success, "Handler should still return success.");
            Assert.IsNotNull(result.Data, "Response.Data should not be null on success.");
            Assert.AreEqual(mappedDto.Name, result.Data!.Name);
            mockDepartmentRepo.Verify(r => r.AddAsync(It.IsAny<Department>()), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        /// <summary>
        /// Verifies that the constructor creates a non-null instance when valid (non-null) dependencies are provided.
        /// Input: mocked IUnitOfWork and IMapper instances.
        /// Expected: an instance of CreateDepartmentCommandHandler is constructed and implements IRequestHandler<CreateDepartmentCommand, Response<DepartmentDto>>.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            // Act
            var handler = new CreateDepartmentCommandHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Assert
            Assert.IsNotNull(handler, "Handler instance should not be null when constructed with valid dependencies.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<CreateDepartmentCommand, Response<DepartmentDto>>), "Handler should implement the expected MediatR request handler interface.");
        }

        /// <summary>
        /// Verifies that multiple constructions with the same dependency instances produce distinct handler instances and do not throw.
        /// Input: the same mocked IUnitOfWork and IMapper instances used twice.
        /// Expected: two distinct CreateDepartmentCommandHandler instances are created successfully.
        /// </summary>
        [TestMethod]
        public void Constructor_SameDependencies_MultipleInstancesAreDistinct()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Loose);
            var mapperMock = new Mock<IMapper>(MockBehavior.Loose);

            // Act
            var first = new CreateDepartmentCommandHandler(unitOfWorkMock.Object, mapperMock.Object);
            var second = new CreateDepartmentCommandHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Assert
            Assert.IsNotNull(first, "First handler instance should not be null.");
            Assert.IsNotNull(second, "Second handler instance should not be null.");
            Assert.AreNotSame(first, second, "Separate constructor calls should produce distinct handler instances.");
        }
    }
}