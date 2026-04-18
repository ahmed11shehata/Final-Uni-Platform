using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.UserStudyYears;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.UserStudyYears;
using AYA_UIS.Application.UnitTests;
using AYA_UIS.Core.Domain;
using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Domain;
using Domain.Contracts;
using Microsoft;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.UserStudyYearDtos;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.UserStudyYears.UnitTests
{
    [TestClass]
    public class GetUserStudyYearTimelineQueryHandlerTests
    {
        #region Async Queryable helpers (inner classes)

        internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }

            public TestAsyncEnumerable(Expression expression) : base(expression) { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

        }

        internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

            public T Current => _inner.Current;

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return default;
            }

            public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());
        }

        #endregion

        /// <summary>
        /// Verifies that the constructor creates a non-null instance when all dependencies are provided.
        /// Input conditions: Valid mocked IUnitOfWork, UserManager{User}, and IMapper instances.
        /// Expected result: Constructor returns an instance that implements the expected IRequestHandler interface and does not throw.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockMapper = new Mock<IMapper>();
            var mockUserStore = new Mock<IUserStore<User>>();

            // UserManager requires a store plus several other optional parameters in its constructor.
            var userManagerMock = new Mock<UserManager<User>>(
                mockUserStore.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            // Act
            var handler = new GetUserStudyYearTimelineQueryHandler(mockUnitOfWork.Object, userManagerMock.Object, mockMapper.Object);

            // Assert
            Assert.IsNotNull(handler, "Constructor should produce a non-null instance when dependencies are provided.");
            Assert.IsInstanceOfType(handler, typeof(MediatR.IRequestHandler<GetUserStudyYearTimelineQuery, Response<UserStudyYearTimelineDto>>),
                "The created instance should implement IRequestHandler<GetUserStudyYearTimelineQuery, Response<UserStudyYearTimelineDto>>.");
        }

        /// <summary>
        /// Ensures multiple distinct instances can be constructed with different dependency instances.
        /// Input conditions: Two different sets of mocked dependencies.
        /// Expected result: Two handler instances are created and are distinct objects.
        /// </summary>
        [TestMethod]
        public void Constructor_MultipleInvocations_CreateDistinctInstances()
        {
            // Arrange - first set
            var mockUnitOfWork1 = new Mock<IUnitOfWork>();
            var mockMapper1 = new Mock<IMapper>();
            var mockUserStore1 = new Mock<IUserStore<User>>();
            var userManagerMock1 = new Mock<UserManager<User>>(
                mockUserStore1.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            // Arrange - second set
            var mockUnitOfWork2 = new Mock<IUnitOfWork>();
            var mockMapper2 = new Mock<IMapper>();
            var mockUserStore2 = new Mock<IUserStore<User>>();
            var userManagerMock2 = new Mock<UserManager<User>>(
                mockUserStore2.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            // Act
            var handler1 = new GetUserStudyYearTimelineQueryHandler(mockUnitOfWork1.Object, userManagerMock1.Object, mockMapper1.Object);
            var handler2 = new GetUserStudyYearTimelineQueryHandler(mockUnitOfWork2.Object, userManagerMock2.Object, mockMapper2.Object);

            // Assert
            Assert.IsNotNull(handler1);
            Assert.IsNotNull(handler2);
            Assert.AreNotSame(handler1, handler2, "Separate constructor invocations should produce distinct instances.");
        }
    }
}