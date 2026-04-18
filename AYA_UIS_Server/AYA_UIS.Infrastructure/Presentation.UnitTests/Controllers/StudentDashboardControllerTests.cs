using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Dashboard;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Presentation;
using Presentation.Controllers;
using Shared.Dtos.Info_Module.DashboardDtos;
using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;


namespace Presentation.Controllers.UnitTests
{
    [TestClass]
    public class StudentDashboardControllerTests
    {
        /// <summary>
        /// Verifies that the constructor creates a non-null StudentDashboardController when a valid IMediator is provided.
        /// Input: a mocked IMediator instance.
        /// Expected: constructor returns a non-null controller instance and the instance is derived from ControllerBase.
        /// </summary>
        [TestMethod]
        public void StudentDashboardController_Constructor_WithValidMediator_CreatesInstance()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();

            // Act
            var controller = new StudentDashboardController(mediatorMock.Object);

            // Assert
            Assert.IsNotNull(controller, "Constructor returned null when provided a valid IMediator.");
            Assert.IsInstanceOfType(controller, typeof(ControllerBase), "Controller should derive from ControllerBase.");
        }

        /// <summary>
        /// Verifies that the constructor does not throw and still creates an instance when null is passed for IMediator.
        /// Input: null IMediator (nullable annotation used per test constraints).
        /// Expected: constructor completes and returns a non-null controller instance (field may be null internally).
        /// </summary>
        [TestMethod]
        public void StudentDashboardController_Constructor_WithNullMediator_DoesNotThrowAndCreatesInstance()
        {
            // Arrange
            IMediator? mediator = null;

            // Act
            var controller = new StudentDashboardController(mediator);

            // Assert
            Assert.IsNotNull(controller, "Constructor returned null when provided a null IMediator reference.");
            Assert.IsInstanceOfType(controller, typeof(ControllerBase), "Controller should derive from ControllerBase even if mediator is null.");
        }
    }
}