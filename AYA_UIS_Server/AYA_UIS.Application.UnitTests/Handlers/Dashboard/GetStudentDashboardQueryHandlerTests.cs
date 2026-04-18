using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Dashboard;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Dashboard;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.DashboardDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace AYA_UIS.Application.Handlers.Dashboard.UnitTests
{
    /// <summary>
    /// Tests for GetStudentDashboardQueryHandler constructor behavior.
    /// </summary>
    [TestClass]
    public class GetStudentDashboardQueryHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor returns a non-null instance and that the created instance
        /// implements IRequestHandler&lt;GetStudentDashboardQuery, StudentDashboardDto&gt; when
        /// valid mocked dependencies are supplied.
        /// Inputs: Mocked IUnitOfWork and mocked UserManager&lt;User&gt; (non-null).
        /// Expected: No exception and a non-null instance that implements the expected interface.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidMocks_CreatesInstanceAndImplementsInterface()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var userStoreMock = new Mock<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            // Act
            var handler = new GetStudentDashboardQueryHandler(unitOfWorkMock.Object, userManagerMock.Object);
            // Assert
            Assert.IsNotNull(handler, "Constructor returned null for valid dependencies.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetStudentDashboardQuery, StudentDashboardDto>), "Handler does not implement the expected IRequestHandler interface.");
        }

        /// <summary>
        /// Ensures the constructor does not throw across different MockBehavior configurations.
        /// Inputs: combinations of MockBehavior for mocks (Default, Strict).
        /// Expected: Construction succeeds (no exception) and result is non-null for each configuration.
        /// </summary>
        [TestMethod]
        public void Constructor_WithVariousMockBehaviors_DoesNotThrowAndCreatesInstance()
        {
            // Arrange
            var behaviors = new[]
            {
                MockBehavior.Default,
                MockBehavior.Strict
            };
            foreach (var behavior in behaviors)
            {
                // IUnitOfWork mock with specified behavior
                var uowMock = new Mock<IUnitOfWork>(behavior);
                // UserStore/UserManager mocks with specified behavior for UserManager creation
                var userStoreMock = new Mock<IUserStore<User>>(behavior);
                var userManagerMock = new Mock<UserManager<User>>(behavior, userStoreMock.Object, null, null, null, null, null, null, null, null);
                // When using Strict mock behavior, the UserManager constructor may invoke virtual members
                // (e.g. setting Logger). Ensure those invocations are allowed to avoid Moq.Strict failures.
                userManagerMock.SetupSet(um => um.Logger = It.IsAny<ILogger<User>>());
                // Act & Assert - ensure no exception and instance is created
                GetStudentDashboardQueryHandler? handler = null;
                Exception? creationException = null;
                try
                {
                    handler = new GetStudentDashboardQueryHandler(uowMock.Object, userManagerMock.Object);
                }
                catch (Exception ex)
                {
                    creationException = ex;
                }

                Assert.IsNull(creationException, $"Constructor threw an exception for MockBehavior {behavior}: {creationException}");
                Assert.IsNotNull(handler, $"Handler instance was null for MockBehavior {behavior}.");
            }
        }
    }
}