using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Courses;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Courses;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using AYA_UIS.Shared;
using AYA_UIS.Shared.Exceptions;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.CourseDtos;

namespace AYA_UIS.Application.Handlers.Courses.UnitTests
{
    /// <summary>
    /// Tests for the GetDepartmentOpenCoursesQueryHandler constructor behavior.
    /// Focused on ensuring the constructor accepts and stores the dependency without invoking it.
    /// </summary>
    [TestClass]
    public class GetDepartmentOpenCoursesQueryHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor creates a non-null handler instance when supplied with a valid IUnitOfWork.
        /// Input: a mocked IUnitOfWork instance.
        /// Expected: the constructor completes without throwing and returns a non-null handler that implements the expected IRequestHandler interface.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidUnitOfWork_InstanceCreated()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            // Act
            var handler = new GetDepartmentOpenCoursesQueryHandler(mockUnitOfWork.Object);

            // Assert
            Assert.IsNotNull(handler, "Handler instance should not be null when constructed with a valid IUnitOfWork.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetDepartmentOpenCoursesQuery, IEnumerable<FrontendCourseDto>>),
                "Handler should implement IRequestHandler<GetDepartmentOpenCoursesQuery, IEnumerable<FrontendCourseDto>>.");
        }

        /// <summary>
        /// Ensures the constructor does not call or interact with the provided IUnitOfWork during construction.
        /// Input: a mocked IUnitOfWork instance.
        /// Expected: no invocations on the mock after constructing the handler.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidUnitOfWork_DoesNotInvokeDependencies()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            var handler = new GetDepartmentOpenCoursesQueryHandler(mockUnitOfWork.Object);

            // Assert
            // Verify that constructor did not trigger any calls on the dependency.
            mockUnitOfWork.VerifyNoOtherCalls();

            // Additional sanity: handler should be available for later use.
            Assert.IsNotNull(handler);
        }

        // Helper replicating GetAllCoursesQueryHandler.ExtractYear logic used by handler.
        private static int ExpectedYear(string code)
        {
            if (string.IsNullOrEmpty(code)) return 1;
            foreach (char ch in code)
            {
                if (char.IsDigit(ch) && ch >= '1' && ch <= '4')
                    return ch - '0';
            }
            return 1;
        }
    }
}