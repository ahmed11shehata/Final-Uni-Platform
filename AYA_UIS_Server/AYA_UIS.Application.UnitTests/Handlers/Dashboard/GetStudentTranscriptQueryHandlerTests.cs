using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Dashboard;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Dashboard;
using AYA_UIS.Core.Domain;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.DashboardDtos;

namespace AYA_UIS.Application.Handlers.Dashboard.UnitTests
{
    /// <summary>
    /// Tests for GetStudentTranscriptQueryHandler constructor behavior.
    /// Focus: verify construction succeeds with valid dependency instances and the produced instance implements expected handler interface.
    /// </summary>
    [TestClass]
    public class GetStudentTranscriptQueryHandlerTests
    {
        /// <summary>
        /// Arrange: valid mocks for IUnitOfWork and UserManager{User}.
        /// Act: construct GetStudentTranscriptQueryHandler.
        /// Assert: instance is created (no exception) and not null.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidDependencies_InstanceCreated()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            // Create a minimal IUserStore mock to satisfy UserManager constructor.
            var userStoreMock = new Mock<IUserStore<User>>();

            // Act
            var userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object,
                null,
                null,
                new List<IUserValidator<User>>(),
                new List<IPasswordValidator<User>>(),
                null,
                null,
                null,
                null);

            var handler = new GetStudentTranscriptQueryHandler(unitOfWorkMock.Object, userManagerMock.Object);

            // Assert
            Assert.IsNotNull(handler);
        }

        /// <summary>
        /// Arrange: valid mocks for IUnitOfWork and UserManager{User}.
        /// Act: construct GetStudentTranscriptQueryHandler.
        /// Assert: created instance implements IRequestHandler{GetStudentTranscriptQuery, StudentTranscriptDto}.
        /// This verifies the constructed object type conforms to expected interface contracts.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidDependencies_ImplementsIRequestHandler()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var userStoreMock = new Mock<IUserStore<User>>();

            var userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object,
                null,
                null,
                new List<IUserValidator<User>>(),
                new List<IPasswordValidator<User>>(),
                null,
                null,
                null,
                null);

            // Act
            var handler = new GetStudentTranscriptQueryHandler(unitOfWorkMock.Object, userManagerMock.Object);

            // Assert
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetStudentTranscriptQuery, StudentTranscriptDto>));
        }

    }
}