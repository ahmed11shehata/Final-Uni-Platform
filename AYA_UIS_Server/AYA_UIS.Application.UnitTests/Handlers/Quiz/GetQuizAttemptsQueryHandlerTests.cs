using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Quiz;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Quiz;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.QuizDto;

namespace AYA_UIS.Application.Handlers.Quiz.UnitTests
{
    [TestClass]
    public class GetQuizAttemptsQueryHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor successfully creates an instance when a valid IUnitOfWork is provided.
        /// Input: a non-null mocked IUnitOfWork.
        /// Expected: no exception thrown, returned object is not null and implements the expected IRequestHandler interface.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidUnitOfWork_InstanceCreatedAndImplementsInterface()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Loose);
            IUnitOfWork unitOfWork = unitOfWorkMock.Object;

            // Act
            var handler = new GetQuizAttemptsQueryHandler(unitOfWork);

            // Assert
            Assert.IsNotNull(handler, "Handler instance should not be null for a valid IUnitOfWork.");
            Assert.IsInstanceOfType(
                handler,
                typeof(IRequestHandler<GetQuizAttemptsQuery, IEnumerable<QuizAttemptDto>>),
                "Handler should implement IRequestHandler<GetQuizAttemptsQuery, IEnumerable<QuizAttemptDto>>.");
        }

        /// <summary>
        /// Ensures separate constructor calls with different IUnitOfWork instances produce independent handler instances.
        /// Input: two distinct mocked IUnitOfWork objects.
        /// Expected: two distinct handler instances (not the same reference) and both non-null.
        /// This guards against accidental use of singletons or static state in the constructor.
        /// </summary>
        [TestMethod]
        public void Constructor_DifferentUnitOfWorkInstances_ProducesDistinctHandlerInstances()
        {
            // Arrange
            var unitOfWorkMock1 = new Mock<IUnitOfWork>(MockBehavior.Loose);
            var unitOfWorkMock2 = new Mock<IUnitOfWork>(MockBehavior.Loose);
            IUnitOfWork unitOfWork1 = unitOfWorkMock1.Object;
            IUnitOfWork unitOfWork2 = unitOfWorkMock2.Object;

            // Act
            var handler1 = new GetQuizAttemptsQueryHandler(unitOfWork1);
            var handler2 = new GetQuizAttemptsQueryHandler(unitOfWork2);

            // Assert
            Assert.IsNotNull(handler1, "First handler instance should not be null.");
            Assert.IsNotNull(handler2, "Second handler instance should not be null.");
            Assert.AreNotSame(handler1, handler2, "Separate constructor calls should produce distinct handler instances.");
        }
    }
}