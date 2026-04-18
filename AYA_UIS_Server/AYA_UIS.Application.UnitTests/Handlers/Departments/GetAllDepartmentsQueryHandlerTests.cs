using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Departments;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Departments;
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
    public class GetAllDepartmentsQueryHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor accepts valid IUnitOfWork and IMapper implementations
        /// and does not invoke any member on the dependencies during construction.
        /// Conditions:
        /// - Dependencies are provided as Moq mocks with varying MockBehavior (Strict and Loose).
        /// Expected:
        /// - No exception is thrown.
        /// - The created instance is not null and implements the expected IRequestHandler interface.
        /// - No calls were made to the mocks during construction.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidDependencies_InstanceCreatedAndNoDependencyCalls()
        {
            // Arrange
            var behaviors = new[] { MockBehavior.Loose, MockBehavior.Strict };

            foreach (var behavior in behaviors)
            {
                var unitOfWorkMock = new Mock<IUnitOfWork>(behavior);
                var mapperMock = new Mock<IMapper>(behavior);

                // Act
                GetAllDepartmentsQueryHandler? handler = null;
                Exception? caught = null;
                try
                {
                    handler = new GetAllDepartmentsQueryHandler(unitOfWorkMock.Object, mapperMock.Object);
                }
                catch (Exception ex)
                {
                    caught = ex;
                }

                // Assert
                Assert.IsNull(caught, $"Constructor threw an exception for MockBehavior {behavior}: {caught?.Message}");
                Assert.IsNotNull(handler, "Handler instance should not be null for valid dependencies.");

                // The handler should implement the expected IRequestHandler interface with correct generic parameters.
                Assert.IsTrue(handler is IRequestHandler<GetAllDepartmentsQuery, Response<IEnumerable<DepartmentDto>>>,
                    "Handler should implement IRequestHandler<GetAllDepartmentsQuery, Response<IEnumerable<DepartmentDto>>>");

                // Ensure constructor did not call any members on the dependencies.
                // This will fail if any member was accessed during construction.
                unitOfWorkMock.VerifyNoOtherCalls();
                mapperMock.VerifyNoOtherCalls();
            }
        }

        /// <summary>
        /// This test documents that constructor does not perform eager validation (no ArgumentNullException)
        /// when given non-null mock instances. It is intentionally separate to make the contract explicit.
        /// Conditions:
        /// - Dependencies are provided as default (Loose) mocks.
        /// Expected:
        /// - Construction succeeds and no calls are made to dependencies.
        /// </summary>
        [TestMethod]
        public void Constructor_WithDefaultMocks_DoesNotThrowAndDoesNotCallDependencies()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Loose);
            var mapperMock = new Mock<IMapper>(MockBehavior.Loose);

            // Act & Assert: Should not throw
            var handler = new GetAllDepartmentsQueryHandler(unitOfWorkMock.Object, mapperMock.Object);
            Assert.IsNotNull(handler);

            // No dependency interactions during construction
            unitOfWorkMock.VerifyNoOtherCalls();
            mapperMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Verifies that when the repository returns departments, the mapper is called with the repository result
        /// and the handler returns a successful Response containing the mapped DepartmentDto collection.
        /// Input: repository returns a single Department, mapper returns a corresponding DepartmentDto list.
        /// Expected: Response.Success == true and Data contains the mapped dto with expected properties.
        /// </summary>
        [TestMethod]
        public async Task Handle_RepositoryReturnsDepartments_MapsAndReturnsSuccessResponse()
        {
            // Arrange
            var department = new AYA_UIS.Core.Domain.Entities.Models.Department { Id = 1, Name = "CS", Code = "CS01" };
            IEnumerable<AYA_UIS.Core.Domain.Entities.Models.Department> departments = new List<AYA_UIS.Core.Domain.Entities.Models.Department> { department };

            var expectedDto = new DepartmentDto { Id = 1, Name = "CS", Code = "CS01" };
            IEnumerable<DepartmentDto> mappedDtos = new List<DepartmentDto> { expectedDto };

            var repoMock = new Mock<IDepartmentRepository>();
            repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(departments);

            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(u => u.Departments).Returns(repoMock.Object);

            var mapperMock = new Mock<IMapper>();
            // Ensure mapper receives the exact object returned from repository
            mapperMock
                .Setup(m => m.Map<IEnumerable<DepartmentDto>>(It.Is<object>(o => ReferenceEquals(o, departments))))
                .Returns(mappedDtos);

            var handler = new GetAllDepartmentsQueryHandler(uowMock.Object, mapperMock.Object);

            // Act
            var response = await handler.Handle(new GetAllDepartmentsQuery(), CancellationToken.None);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success, "Expected Success to be true for successful mapping");
            Assert.IsNotNull(response.Data, "Expected Data to be non-null when mapper returns a collection");
            var dataList = response.Data!.ToList();
            Assert.AreEqual(1, dataList.Count, "Expected exactly one mapped department dto");
            Assert.AreEqual(expectedDto.Id, dataList[0].Id);
            Assert.AreEqual(expectedDto.Name, dataList[0].Name);
            Assert.AreEqual(expectedDto.Code, dataList[0].Code);

            // Verify interactions
            repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            mapperMock.Verify(m => m.Map<IEnumerable<DepartmentDto>>(It.IsAny<object>()), Times.Once);
        }

        /// <summary>
        /// Verifies that when the repository returns an empty collection, the handler returns Success
        /// and the Data is an empty collection (as provided by the mapper).
        /// Input: repository returns empty list, mapper returns empty list.
        /// Expected: Success == true and Data is empty.
        /// </summary>
        [TestMethod]
        public async Task Handle_RepositoryReturnsEmptyCollection_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            IEnumerable<AYA_UIS.Core.Domain.Entities.Models.Department> departments = new List<AYA_UIS.Core.Domain.Entities.Models.Department>();
            IEnumerable<DepartmentDto> mappedDtos = new List<DepartmentDto>();

            var repoMock = new Mock<IDepartmentRepository>();
            repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(departments);

            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(u => u.Departments).Returns(repoMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<IEnumerable<DepartmentDto>>(It.IsAny<object>())).Returns(mappedDtos);

            var handler = new GetAllDepartmentsQueryHandler(uowMock.Object, mapperMock.Object);

            // Act
            var response = await handler.Handle(new GetAllDepartmentsQuery(), CancellationToken.None);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(0, response.Data!.Count());
            repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            mapperMock.Verify(m => m.Map<IEnumerable<DepartmentDto>>(It.IsAny<object>()), Times.Once);
        }

        /// <summary>
        /// Verifies behavior when repository returns null and mapper returns null.
        /// Input: repository returns null, mapper maps null to null.
        /// Expected: Response.Success == true and Data == null (SuccessResponse permits null Data).
        /// </summary>
        [TestMethod]
        public async Task Handle_RepositoryReturnsNull_MapperReturnsNull_ResponseDataIsNull()
        {
            // Arrange
            IEnumerable<AYA_UIS.Core.Domain.Entities.Models.Department>? departments = null;
            IEnumerable<DepartmentDto>? mappedDtos = null;

            var repoMock = new Mock<IDepartmentRepository>();
            repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(departments);

            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(u => u.Departments).Returns(repoMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<IEnumerable<DepartmentDto>>(It.Is<object?>(o => o == null))).Returns(mappedDtos);

            var handler = new GetAllDepartmentsQueryHandler(uowMock.Object, mapperMock.Object);

            // Act
            var response = await handler.Handle(new GetAllDepartmentsQuery(), CancellationToken.None);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.IsNull(response.Data, "Expected Data to be null when mapper returns null");
            repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            mapperMock.Verify(m => m.Map<IEnumerable<DepartmentDto>>(It.IsAny<object?>()), Times.Once);
        }

    }
}