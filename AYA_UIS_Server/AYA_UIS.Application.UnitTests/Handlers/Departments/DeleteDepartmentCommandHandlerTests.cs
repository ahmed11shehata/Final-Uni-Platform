using System;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
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

namespace AYA_UIS.Application.Handlers.Departments.UnitTests
{
    [TestClass]
    public class DeleteDepartmentCommandHandlerTests
    {
        /// <summary>
        /// Verifies that providing a valid IUnitOfWork to the constructor produces a non-null instance
        /// and that the created instance implements IRequestHandler&lt;DeleteDepartmentCommand, Unit&gt;.
        /// Input conditions: a mocked IUnitOfWork (non-null) is provided.
        /// Expected result: the constructor returns a non-null DeleteDepartmentCommandHandler instance
        /// which implements the expected MediatR interface.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidUnitOfWork_InstanceCreated()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Act
            var handler = new DeleteDepartmentCommandHandler(mockUnitOfWork.Object);

            // Assert
            Assert.IsNotNull(handler, "Handler instance should not be null when constructed with a valid IUnitOfWork.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<DeleteDepartmentCommand, Unit>), "Handler should implement IRequestHandler<DeleteDepartmentCommand, Unit>.");
        }

        /// <summary>
        /// Verifies that constructing multiple handlers with different IUnitOfWork instances
        /// yields distinct handler instances. This ensures simple constructor wiring behaves
        /// deterministically and does not return shared singleton instances unexpectedly.
        /// Input conditions: two distinct mocked IUnitOfWork instances.
        /// Expected result: two distinct DeleteDepartmentCommandHandler instances are created.
        /// </summary>
        [TestMethod]
        public void Constructor_DifferentUnitOfWorkInstances_CreateDistinctHandlers()
        {
            // Arrange
            var mockUnitOfWork1 = new Mock<IUnitOfWork>();
            var mockUnitOfWork2 = new Mock<IUnitOfWork>();

            // Act
            var handler1 = new DeleteDepartmentCommandHandler(mockUnitOfWork1.Object);
            var handler2 = new DeleteDepartmentCommandHandler(mockUnitOfWork2.Object);

            // Assert
            Assert.IsNotNull(handler1, "First handler instance should not be null.");
            Assert.IsNotNull(handler2, "Second handler instance should not be null.");
            Assert.AreNotSame(handler1, handler2, "Distinct constructor invocations should produce distinct handler instances.");
            Assert.IsInstanceOfType(handler1, typeof(DeleteDepartmentCommandHandler));
            Assert.IsInstanceOfType(handler2, typeof(DeleteDepartmentCommandHandler));
        }
    }
}