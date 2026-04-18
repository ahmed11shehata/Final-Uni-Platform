using AutoMapper;
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Courses;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Courses;
using AYA_UIS.Core.Domain;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.CourseDtos;
using Shared.Dtos.Info_Module.FeeDtos;
using Shared.Respones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AYA_UIS.Application.Handlers.Courses.UnitTests
{
    [TestClass]
    public class CreateCourseCommandHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor with valid (non-null) IUnitOfWork and IMapper
        /// creates an instance of CreateCourseCommandHandler without throwing and
        /// the produced object is of the expected type.
        /// Input conditions:
        ///  - IUnitOfWork: mocked non-null instance
        ///  - IMapper: mocked non-null instance
        /// Expected result:
        ///  - No exception is thrown and the resulting object is non-null and of the expected type.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesHandlerInstance()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mockMapper = new Mock<IMapper>(MockBehavior.Strict);
            // Act
            CreateCourseCommandHandler? handler = null;
            Exception? thrown = null;
            try
            {
                handler = new CreateCourseCommandHandler(mockUnitOfWork.Object, mockMapper.Object);
            }
            catch (Exception ex)
            {
                thrown = ex;
            }

            // Assert
            Assert.IsNull(thrown, "Constructor should not throw when provided valid non-null dependencies.");
            Assert.IsNotNull(handler, "Handler instance should not be null after construction.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<CreateCourseCommand, Response<CourseDto>>), "Handler should implement the expected IRequestHandler interface.");
        }

        /// <summary>
        /// Verifies that constructing multiple handlers with different valid dependencies
        /// produces independent, non-null instances. This ensures no unexpected static/shared
        /// state or exceptions when creating more than one instance.
        /// Input conditions:
        ///  - Two distinct IUnitOfWork mocks
        ///  - Two distinct IMapper mocks
        /// Expected result:
        ///  - Both constructions succeed, instances are non-null and not the same reference.
        /// </summary>
        [TestMethod]
        public void Constructor_MultipleDistinctDependencies_ProducesDistinctInstances()
        {
            // Arrange
            var mockUnitOfWork1 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mockMapper1 = new Mock<IMapper>(MockBehavior.Strict);
            var mockUnitOfWork2 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mockMapper2 = new Mock<IMapper>(MockBehavior.Strict);
            // Act
            var handler1 = new CreateCourseCommandHandler(mockUnitOfWork1.Object, mockMapper1.Object);
            var handler2 = new CreateCourseCommandHandler(mockUnitOfWork2.Object, mockMapper2.Object);
            // Assert
            Assert.IsNotNull(handler1, "First handler instance should not be null.");
            Assert.IsNotNull(handler2, "Second handler instance should not be null.");
            Assert.AreNotSame(handler1, handler2, "Separate constructions should yield distinct instances.");
        }
    }
}