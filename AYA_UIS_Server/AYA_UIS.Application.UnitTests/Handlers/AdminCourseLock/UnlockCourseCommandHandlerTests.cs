using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.AdminCourseLock;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.AdminCourseLock;
using AYA_UIS.Core.Domain.Contracts;
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
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.AdminCourseLockDtos;

namespace AYA_UIS.Application.Handlers.AdminCourseLock.UnitTests
{
    [TestClass]
    public class UnlockCourseCommandHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor creates a non-null instance when provided with valid dependency instances.
        /// Input conditions: a mocked IUnitOfWork and a concrete UserManager<User> constructed with minimal required collaborators.
        /// Expected result: an instance is created and it implements IRequestHandler&lt;UnlockCourseCommand, AdminCourseLockResultDto&gt;.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesHandlerInstance()
        {
            // Arrange
            var uowMock = new Mock<IUnitOfWork>();
            var store = Mock.Of<IUserStore<User>>();
            var options = Options.Create(new IdentityOptions());
            var hasher = Mock.Of<IPasswordHasher<User>>();
            var userValidators = new List<IUserValidator<User>>();
            var passwordValidators = new List<IPasswordValidator<User>>();
            var lookupNormalizer = Mock.Of<ILookupNormalizer>();
            var errorDescriber = new IdentityErrorDescriber();
            var services = Mock.Of<IServiceProvider>();
            var logger = Mock.Of<ILogger<UserManager<User>>>();

            var userManager = new UserManager<User>(
                store,
                options,
                hasher,
                userValidators,
                passwordValidators,
                lookupNormalizer,
                errorDescriber,
                services,
                logger);

            // Act
            var handler = new UnlockCourseCommandHandler(uowMock.Object, userManager);

            // Assert
            Assert.IsNotNull(handler, "Constructor should return a non-null instance.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<UnlockCourseCommand, AdminCourseLockResultDto>), "Instance should implement the expected IRequestHandler interface.");
        }

        /// <summary>
        /// Ensures the constructor succeeds when provided with a mocked UserManager instance (using Moq) and a mocked IUnitOfWork.
        /// Input conditions: Mock<IUnitOfWork>.Object and Mock<UserManager<User>>.Object constructed with valid constructor parameters.
        /// Expected result: no exception is thrown and the returned instance implements IRequestHandler&lt;UnlockCourseCommand, AdminCourseLockResultDto&gt;.
        /// </summary>
        [TestMethod]
        public void Constructor_WithMockedUserManagerAndUow_ProducesValidHandler()
        {
            // Arrange
            var uowMock = new Mock<IUnitOfWork>();

            var store = Mock.Of<IUserStore<User>>();
            var options = Options.Create(new IdentityOptions());
            var hasher = Mock.Of<IPasswordHasher<User>>();
            var userValidators = new List<IUserValidator<User>>();
            var passwordValidators = new List<IPasswordValidator<User>>();
            var lookupNormalizer = Mock.Of<ILookupNormalizer>();
            var errorDescriber = new IdentityErrorDescriber();
            var services = Mock.Of<IServiceProvider>();
            var logger = Mock.Of<ILogger<UserManager<User>>>();

            // Create a Mock<UserManager<User>> by supplying required constructor arguments.
            var userManagerMock = new Mock<UserManager<User>>(
                store,
                options,
                hasher,
                userValidators,
                passwordValidators,
                lookupNormalizer,
                errorDescriber,
                services,
                logger);

            // Act
            UnlockCourseCommandHandler? handler = null;
            Exception? ctorEx = null;
            try
            {
                handler = new UnlockCourseCommandHandler(uowMock.Object, userManagerMock.Object);
            }
            catch (Exception ex)
            {
                ctorEx = ex;
            }

            // Assert
            Assert.IsNull(ctorEx, "Constructor should not throw when provided with mocked dependencies.");
            Assert.IsNotNull(handler, "Handler instance should not be null when constructed with mocks.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<UnlockCourseCommand, AdminCourseLockResultDto>), "Handler should implement the expected interface.");
        }
    }
}