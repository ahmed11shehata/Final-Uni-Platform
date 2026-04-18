using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Dashboard;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Dashboard;
using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.DashboardDtos;

namespace AYA_UIS.Application.Handlers.Dashboard.UnitTests
{
    [TestClass]
    public partial class GetAdminDashboardQueryHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor creates an instance when valid (mocked) dependencies are provided.
        /// Inputs: a mocked IUnitOfWork and a mocked UserManager&lt;User&gt;.
        /// Expected: an instance of GetAdminDashboardQueryHandler is created and implements the IRequestHandler interface.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            // UserManager requires an IUserStore&lt;User&gt; in its constructor.
            var userStoreMock = new Mock<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            // Act
            var handler = new GetAdminDashboardQueryHandler(unitOfWorkMock.Object, userManagerMock.Object);

            // Assert
            Assert.IsNotNull(handler, "Constructor should produce a non-null instance when given valid dependencies.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetAdminDashboardQuery, AdminDashboardDto>), "Instance should implement the expected IRequestHandler interface.");
        }

        /// <summary>
        /// Ensures the constructor does not throw when null is supplied for one or both reference-type dependencies.
        /// Inputs: combinations where IUnitOfWork and/or UserManager&lt;User&gt; are null.
        /// Expected: constructor completes without exception and returns a non-null handler instance.
        /// Note: variables that may be null are declared nullable to respect nullable annotations.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullDependencies_DoesNotThrow_ForAllNullCombinations()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var userStoreMock = new Mock<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            IUnitOfWork? maybeNullUow = null;
            UserManager<User>? maybeNullUserManager = null;

            var cases = new List<(IUnitOfWork? uow, UserManager<User>? um, string description)>
            {
                (maybeNullUow, maybeNullUserManager, "both null"),
                (unitOfWorkMock.Object, maybeNullUserManager, "null userManager"),
                (maybeNullUow, userManagerMock.Object, "null unitOfWork")
            };

            foreach (var (uow, um, description) in cases)
            {
                // Act & Assert
                try
                {
                    // Use null-forgiving operator when passing nullable locals to parameters that are not nullable in the SUT signature.
                    var handler = new GetAdminDashboardQueryHandler(uow!, um!);
                    Assert.IsNotNull(handler, $"Constructor should produce an instance when {description}.");
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Constructor threw an unexpected exception when {description}: {ex.GetType().FullName} - {ex.Message}");
                }
            }
        }

    }
}