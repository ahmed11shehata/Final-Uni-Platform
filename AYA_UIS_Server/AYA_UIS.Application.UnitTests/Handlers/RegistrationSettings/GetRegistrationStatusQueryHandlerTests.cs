using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.RegistrationSettings;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.RegistrationSettings;
using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.RegistrationSettingsDtos;

namespace AYA_UIS.Application.Handlers.RegistrationSettings.UnitTests
{
    [TestClass]
    public class GetRegistrationStatusQueryHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor creates an instance when provided a non-null IUnitOfWork.
        /// Condition: a valid mocked IUnitOfWork is supplied.
        /// Expected: the resulting object is not null and implements the IRequestHandler interface for the expected types.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidIUnitOfWork_InstanceCreatedAndImplementsInterface()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            var handler = new GetRegistrationStatusQueryHandler(mockUow.Object);

            // Assert
            Assert.IsNotNull(handler, "Constructor returned null for a valid IUnitOfWork.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetRegistrationStatusQuery, RegistrationStatusDto>),
                "Handler does not implement the expected IRequestHandler<GetRegistrationStatusQuery, RegistrationStatusDto> interface.");
        }

        /// <summary>
        /// Ensures that multiple instances constructed with different IUnitOfWork references are distinct
        /// and that providing different mock instances does not throw. This helps validate the constructor's simple assignment behavior.
        /// Condition: two distinct mocked IUnitOfWork objects.
        /// Expected: two distinct handler instances are created successfully and are not the same reference.
        /// </summary>
        [TestMethod]
        public void Constructor_DifferentIUnitOfWorkReferences_InstancesAreDistinct()
        {
            // Arrange
            var mockUow1 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mockUow2 = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            var handler1 = new GetRegistrationStatusQueryHandler(mockUow1.Object);
            var handler2 = new GetRegistrationStatusQueryHandler(mockUow2.Object);

            // Assert
            Assert.IsNotNull(handler1);
            Assert.IsNotNull(handler2);
            Assert.AreNotSame(handler1, handler2, "Two handlers constructed with different IUnitOfWork instances should not be the same reference.");
        }

    }
}