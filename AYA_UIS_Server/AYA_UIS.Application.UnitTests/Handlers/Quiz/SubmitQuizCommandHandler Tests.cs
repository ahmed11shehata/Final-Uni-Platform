#nullable enable
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Quiz;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Quiz;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared;
using Shared.Dtos.Info_Module.QuizDto;
using Shared.Respones;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace AYA_UIS.Application.Handlers.Quiz.UnitTests
{
    [TestClass]
    public class SubmitQuizCommandHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor does not throw and returns a non-null handler instance
        /// when provided with a valid IUnitOfWork implementation.
        /// Input conditions: a simple Mock<IUnitOfWork> instance is provided (non-null).
        /// Expected result: handler instance is created, is non-null, and implements the expected MediatR interface.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidUnitOfWork_InstanceCreatedAndImplementsInterface()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Act
            var handler = new SubmitQuizCommandHandler(mockUnitOfWork.Object);

            // Assert
            Assert.IsNotNull(handler, "Constructor should create a non-null instance when a valid IUnitOfWork is provided.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<SubmitQuizCommand, Response<decimal>>),
                "Handler should implement IRequestHandler<SubmitQuizCommand, Response<int>>.");
        }

        /// <summary>
        /// Ensures the constructor consistently constructs handler instances for different valid IUnitOfWork implementations.
        /// Input conditions: two different IUnitOfWork implementations produced by Moq (default and strict).
        /// Expected result: no exceptions; distinct handler instances are returned for different inputs and are of correct type.
        /// </summary>
        [TestMethod]
        public void Constructor_WithVariousIUnitOfWorkImplementations_DoesNotThrowAndCreatesDistinctInstances()
        {
            // Arrange
            var unitOfWorkImplementations = new IUnitOfWork[]
            {
                new Mock<IUnitOfWork>().Object,
                new Mock<IUnitOfWork>(MockBehavior.Strict).Object
            };

            var constructedHandlers = new List<SubmitQuizCommandHandler>();

            // Act
            foreach (var uow in unitOfWorkImplementations)
            {
                // Creating handler should not throw for any valid IUnitOfWork implementation
                var handler = new SubmitQuizCommandHandler(uow);
                constructedHandlers.Add(handler);
            }

            // Assert
            Assert.AreEqual(unitOfWorkImplementations.Length, constructedHandlers.Count, "Should have constructed a handler for each provided IUnitOfWork.");
            Assert.AreNotSame(constructedHandlers[0], constructedHandlers[1], "Each constructor call should produce a distinct instance.");
            foreach (var handler in constructedHandlers)
            {
                Assert.IsInstanceOfType(handler, typeof(SubmitQuizCommandHandler));
            }
        }
    }
}