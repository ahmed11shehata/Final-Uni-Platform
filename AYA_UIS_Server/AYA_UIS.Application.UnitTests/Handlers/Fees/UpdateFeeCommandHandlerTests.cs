using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Fees;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Fees;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Shared;
using AYA_UIS.Shared.Exceptions;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module.FeeDtos;

namespace AYA_UIS.Application.Handlers.Fees.UnitTests
{
    [TestClass]
    public partial class UpdateFeeCommandHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor succeeds when provided a valid IUnitOfWork.
        /// Input conditions: a non-null mocked IUnitOfWork is supplied.
        /// Expected result: no exception is thrown and a non-null handler instance is returned.
        /// </summary>
        [TestMethod]
        public void UpdateFeeCommandHandler_Constructor_WithValidUnitOfWork_DoesNotThrow()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            UpdateFeeCommandHandler? handler = null;
            Exception? caught = null;
            try
            {
                handler = new UpdateFeeCommandHandler(mockUnitOfWork.Object);
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            // Assert
            Assert.IsNull(caught, $"Constructor threw an unexpected exception: {caught?.GetType().Name} {caught?.Message}");
            Assert.IsNotNull(handler, "Handler instance should not be null when a valid IUnitOfWork is provided.");
        }

    }
}