using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.RegistrationSettings;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.RegistrationSettings;
using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.AssignmentDto;
using Shared.Dtos.Info_Module.CourseDtos;
using Shared.Dtos.Info_Module.DashboardDtos;
using Shared.Dtos.Info_Module.RegistrationSettingsDtos;
using Shared.Dtos.Info_Module.SemesterDtos;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AYA_UIS.Application.Handlers.RegistrationSettings.UnitTests
{
    [TestClass]
    public class OpenRegistrationCommandHandlerTests
    {
        /// <summary>
        /// Ensures that when a valid IUnitOfWork dependency is provided the constructor
        /// creates a non-null instance that implements the expected MediatR request handler interface.
        /// Input conditions: a mocked IUnitOfWork instance (non-null).
        /// Expected result: instance is created and implements IRequestHandler&lt;OpenRegistrationCommand, RegistrationStatusDto&gt;.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidUnitOfWork_InstanceCreatedAndImplementsInterface()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            // Act
            var handler = new OpenRegistrationCommandHandler(mockUow.Object);
            // Assert
            Assert.IsNotNull(handler, "Constructor returned null for a valid IUnitOfWork dependency.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<OpenRegistrationCommand, RegistrationStatusDto>), "Constructed instance does not implement the expected IRequestHandler<OpenRegistrationCommand, RegistrationStatusDto> interface.");
        }

        /// <summary>
        /// Ensures that providing different IUnitOfWork instances to the constructor
        /// results in distinct handler instances and does not throw.
        /// Input conditions: two different mocked IUnitOfWork instances.
        /// Expected result: two distinct handler instances are created successfully.
        /// </summary>
        [TestMethod]
        public void Constructor_DifferentUnitOfWorkInstances_CreateDistinctHandlers()
        {
            // Arrange
            var mockUow1 = new Mock<IUnitOfWork>();
            var mockUow2 = new Mock<IUnitOfWork>();
            // Act
            var handler1 = new OpenRegistrationCommandHandler(mockUow1.Object);
            var handler2 = new OpenRegistrationCommandHandler(mockUow2.Object);
            // Assert
            Assert.IsNotNull(handler1, "First handler instance should not be null.");
            Assert.IsNotNull(handler2, "Second handler instance should not be null.");
            Assert.IsFalse(ReferenceEquals(handler1, handler2), "Handlers constructed with different dependencies should not be the same instance.");
            Assert.IsInstanceOfType(handler1, typeof(IRequestHandler<OpenRegistrationCommand, RegistrationStatusDto>));
            Assert.IsInstanceOfType(handler2, typeof(IRequestHandler<OpenRegistrationCommand, RegistrationStatusDto>));
        }
    }
}