using System;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Departments;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Departments;
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
    /// Tests for the GetDepartmentByIdQueryHandler constructor.
    /// Focused solely on constructor behavior (assignment and successful instantiation).
    /// </summary>
    [TestClass]
    public class GetDepartmentByIdQueryHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor creates a non-null instance when valid (mocked) dependencies are provided.
        /// Input conditions:
        /// - IUnitOfWork: valid mock instance
        /// - IMapper: valid mock instance
        /// Expected result:
        /// - No exception is thrown.
        /// - Returned instance is not null and implements the IRequestHandler interface for the expected types.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesHandler()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            // Act
            var handler = new GetDepartmentByIdQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

            // Assert
            Assert.IsNotNull(handler, "Handler instance should not be null when valid dependencies are provided.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetDepartmentByIdQuery, Response<DepartmentDto>>), "Handler should implement the expected IRequestHandler interface.");
        }

        /// <summary>
        /// Verifies that multiple constructions with different dependency instances result in distinct handler instances
        /// and that constructors do not share internal state by reference (sanity check using object identity).
        /// Input conditions:
        /// - Two different IUnitOfWork mocks and two different IMapper mocks.
        /// Expected result:
        /// - Two distinct handler instances are created without throwing exceptions.
        /// - Both instances implement the expected IRequestHandler interface.
        /// </summary>
        [TestMethod]
        public void Constructor_WithDifferentDependencies_CreatesIndependentHandlerInstances()
        {
            // Arrange
            var unitOfWorkMock1 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mapperMock1 = new Mock<IMapper>(MockBehavior.Strict);

            var unitOfWorkMock2 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mapperMock2 = new Mock<IMapper>(MockBehavior.Strict);

            // Act
            var handler1 = new GetDepartmentByIdQueryHandler(unitOfWorkMock1.Object, mapperMock1.Object);
            var handler2 = new GetDepartmentByIdQueryHandler(unitOfWorkMock2.Object, mapperMock2.Object);

            // Assert
            Assert.IsNotNull(handler1, "First handler instance should not be null.");
            Assert.IsNotNull(handler2, "Second handler instance should not be null.");
            Assert.AreNotSame(handler1, handler2, "Two handler instances constructed with different dependencies should not be the same object.");
            Assert.IsInstanceOfType(handler1, typeof(IRequestHandler<GetDepartmentByIdQuery, Response<DepartmentDto>>), "First handler should implement the expected IRequestHandler interface.");
            Assert.IsInstanceOfType(handler2, typeof(IRequestHandler<GetDepartmentByIdQuery, Response<DepartmentDto>>), "Second handler should implement the expected IRequestHandler interface.");
        }

        /// <summary>
        /// Tests that Handle returns a successful Response containing the mapped DepartmentDto when a Department is found.
        /// Input conditions: repository returns a Department instance for the specified Id and mapper maps it to DepartmentDto.
        /// Expected result: Response.Success is true, Data is the mapped DepartmentDto, and the standard success Message is present.
        /// </summary>
        [TestMethod]
        public async Task Handle_DepartmentFound_ReturnsSuccessResponseWithMappedDto()
        {
            // Arrange
            var dept = new Department
            {
                Id = 123,
                Name = "Computer Science",
                Code = "CS",
                Description = "Description"
            };

            var expectedDto = new DepartmentDto
            {
                Id = 123,
                Name = "Computer Science",
                Code = "CS",
                Description = "Description"
            };

            var deptRepoMock = new Mock<IDepartmentRepository>(MockBehavior.Strict);
            deptRepoMock
                .Setup(r => r.GetByIdWithDetailsAsync(It.Is<int>(i => i == dept.Id)))
                .ReturnsAsync(dept);

            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            unitOfWorkMock.Setup(u => u.Departments).Returns(deptRepoMock.Object);

            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            mapperMock
                .Setup(m => m.Map<DepartmentDto>(It.Is<object>(o => ReferenceEquals(o, dept))))
                .Returns(expectedDto);

            var handler = new GetDepartmentByIdQueryHandler(unitOfWorkMock.Object, mapperMock.Object);

            var query = new GetDepartmentByIdQuery(dept.Id);

            // Act
            var response = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(response, "Response should not be null");
            Assert.IsTrue(response.Success, "Response.Success should be true for found department");
            Assert.IsNotNull(response.Data, "Response.Data should not be null for found department");
            Assert.AreSame(expectedDto, response.Data, "Response.Data should be the mapped DepartmentDto instance");
            Assert.AreEqual("Operation completed successfully", response.Message, "Unexpected success message");

            // Verify interactions
            deptRepoMock.Verify(r => r.GetByIdWithDetailsAsync(dept.Id), Times.Once);
            unitOfWorkMock.Verify(u => u.Departments, Times.AtLeastOnce);
            mapperMock.Verify(m => m.Map<DepartmentDto>(It.Is<object>(o => ReferenceEquals(o, dept))), Times.Once);
        }
    }
}