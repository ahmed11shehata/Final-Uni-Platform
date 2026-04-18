using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Quiz;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Quiz;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.QuizDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;


namespace AYA_UIS.Application.Handlers.Quiz.UnitTests
{
    [TestClass]
    public partial class GetCourseQuizzesQueryHandlerTests
    {
        /// <summary>
        /// Verifies that constructing GetCourseQuizzesQueryHandler with a valid IUnitOfWork instance
        /// does not throw and returns a usable handler instance.
        /// Input conditions:
        /// - A non-null, mocked IUnitOfWork is provided.
        /// Expected result:
        /// - Constructor completes without throwing and the returned handler is not null.
        /// </summary>
        [TestMethod]
        public void Constructor_WithMockUnitOfWork_DoesNotThrowAndCreatesInstance()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Loose);

            // Act
            GetCourseQuizzesQueryHandler? handler = null;
            Exception? caught = null;
            try
            {
                handler = new GetCourseQuizzesQueryHandler(mockUnitOfWork.Object);
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            // Assert
            Assert.IsNull(caught, "Constructor threw an unexpected exception when provided a valid IUnitOfWork.");
            Assert.IsNotNull(handler, "Constructor returned a null handler instance.");
        }
    }
}