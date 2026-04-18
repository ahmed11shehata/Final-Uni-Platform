using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Quiz;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Quiz;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.QuizDto;

namespace AYA_UIS.Application.Handlers.Quiz.UnitTests
{
    [TestClass]
    public class GetQuizQueryHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor creates an instance when provided a valid IUnitOfWork.
        /// Input: a non-null Mock&lt;IUnitOfWork&gt; object.
        /// Expected: instance is created successfully and implements IRequestHandler&lt;GetQuizQuery, FrontendQuizDto&gt;.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidUnitOfWork_InstanceCreatedAndImplementsInterface()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            var handler = new GetQuizQueryHandler(unitOfWorkMock.Object);

            // Assert
            Assert.IsNotNull(handler, "Handler instance should not be null when constructed with a valid IUnitOfWork.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetQuizQuery, FrontendQuizDto>), "Handler should implement IRequestHandler<GetQuizQuery, FrontendQuizDto>.");
        }

        /// <summary>
        /// Ensures the constructor does not interact with the IUnitOfWork dependency during construction.
        /// Input: a Mock&lt;IUnitOfWork&gt; prepared to track calls.
        /// Expected: no calls or interactions on the mock after construction.
        /// This guards against side-effects in the constructor.
        /// </summary>
        [TestMethod]
        public void Constructor_WithUnitOfWork_DoesNotCallUnitOfWorkMembers()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            var handler = new GetQuizQueryHandler(unitOfWorkMock.Object);

            // Assert
            // Verify that constructor did not invoke any member on the IUnitOfWork
            unitOfWorkMock.VerifyNoOtherCalls();

            // Additionally assert instance is usable (non-null)
            Assert.IsNotNull(handler);
        }

    }
}