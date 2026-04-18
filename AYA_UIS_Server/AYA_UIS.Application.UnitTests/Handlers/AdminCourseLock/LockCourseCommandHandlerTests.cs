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
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.AdminCourseLockDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AYA_UIS.Application.Handlers.AdminCourseLock.UnitTests
{
    [TestClass]
    public class LockCourseCommandHandlerTests
    {
        /// <summary>
        /// Verifies that constructing LockCourseCommandHandler with valid (mocked) dependencies
        /// succeeds without throwing and produces a non-null instance.
        /// Inputs: a mocked IUnitOfWork and a mocked UserManager&lt;User&gt; (via Moq constructor).
        /// Expected result: an instance of LockCourseCommandHandler is created (no exception).
        /// </summary>
        [TestMethod]
        public void LockCourseCommandHandler_Constructor_ValidDependencies_DoesNotThrow()
        {
            // Arrange
            var uowMock = new Mock<IUnitOfWork>();

            // Build a UserManager<User> mock by supplying required constructor args.
            var store = Mock.Of<IUserStore<User>>();
            var userValidators = new List<IUserValidator<User>>();
            var pwdValidators = new List<IPasswordValidator<User>>();
            var mockUserManager = new Mock<UserManager<User>>(store, null, null, userValidators, pwdValidators, null, null, null, null);

            // Act
            var handler = new LockCourseCommandHandler(uowMock.Object, mockUserManager.Object);

            // Assert
            Assert.IsNotNull(handler, "Handler should be instantiated when valid dependencies are provided.");
        }

        /// <summary>
        /// Verifies constructor behavior when dependencies are null in various combinations.
        /// Inputs tested:
        ///   1) null IUnitOfWork, valid UserManager
        ///   2) valid IUnitOfWork, null UserManager
        ///   3) both null
        /// Expected result: constructor does not throw and returns a non-null handler instance for each combination.
        /// Note: This test focuses on constructor behavior only and does not invoke handler methods.
        /// </summary>
        [TestMethod]
        public void LockCourseCommandHandler_Constructor_NullDependencyCombinations_AllowsConstruction()
        {
            // Arrange
            var validUowMock = new Mock<IUnitOfWork>();
            var validUow = validUowMock.Object;

            var store = Mock.Of<IUserStore<User>>();
            var mockUserManager = new Mock<UserManager<User>>(store, null, null, new List<IUserValidator<User>>(), new List<IPasswordValidator<User>>(), null, null, null, null);
            var validUserManager = mockUserManager.Object;

            var cases = new (IUnitOfWork? uow, UserManager<User>? userManager)[]
            {
                (null, validUserManager),
                (validUow, null),
                (null, null)
            };

            foreach (var (uow, userManager) in cases)
            {
                // Act
                LockCourseCommandHandler? handler = null;
                Exception? thrown = null;
                try
                {
                    handler = new LockCourseCommandHandler(uow!, userManager!);
                }
                catch (Exception ex)
                {
                    thrown = ex;
                }

                // Assert
                Assert.IsNull(thrown, $"Constructor should not throw when invoked with uow={(uow is null ? "null" : "valid")} and userManager={(userManager is null ? "null" : "valid")}.");
                Assert.IsNotNull(handler, "Handler instance should be created even when dependencies are null.");
            }
        }
    }
}