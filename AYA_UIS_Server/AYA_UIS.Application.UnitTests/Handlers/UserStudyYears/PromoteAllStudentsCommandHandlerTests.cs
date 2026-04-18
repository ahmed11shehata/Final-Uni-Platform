using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.UserStudyYears;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.UserStudyYears;
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
using Shared;
using Shared.Exceptions;

namespace AYA_UIS.Application.Handlers.UserStudyYears.UnitTests
{
    /// <summary>
    /// Tests for PromoteAllStudentsCommandHandler constructor behavior.
    /// </summary>
    [TestClass]
    public class PromoteAllStudentsCommandHandlerTests
    {
        /// <summary>
        /// Verifies that constructor succeeds when provided with valid (mocked) dependencies.
        /// Input conditions: a mocked IUnitOfWork and a mocked UserManager&lt;User&gt; (constructed via Moq).
        /// Expected result: an instance is created and implements IRequestHandler&lt;PromoteAllStudentsCommand, Unit&gt;; no exceptions thrown.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidDependencies_InstanceCreatedAndImplementsInterface()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            IUnitOfWork unitOfWork = unitOfWorkMock.Object;

            // UserManager requires an IUserStore<User> in constructor; supply a simple mock store.
            var userStore = Mock.Of<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(userStore, null, null, null, null, null, null, null, null);
            UserManager<User> userManager = userManagerMock.Object;

            // Act
            var handler = new PromoteAllStudentsCommandHandler(unitOfWork, userManager);

            // Assert
            Assert.IsNotNull(handler, "Handler instance should not be null when constructed with valid dependencies.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<PromoteAllStudentsCommand, Unit>), "Handler should implement IRequestHandler<PromoteAllStudentsCommand, Unit>.");
        }

        /// <summary>
        /// Ensures constructor can be invoked multiple times with different mocked dependencies without sharing instances.
        /// Input conditions: two different mocked IUnitOfWork and UserManager&lt;User&gt; pairs.
        /// Expected result: two distinct handler instances are created and no exceptions are thrown.
        /// </summary>
        [TestMethod]
        public void Constructor_DifferentDependencies_CreatesDistinctInstances()
        {
            // Arrange - first pair
            var unitOfWorkMock1 = new Mock<IUnitOfWork>();
            IUnitOfWork unitOfWork1 = unitOfWorkMock1.Object;
            var userStore1 = Mock.Of<IUserStore<User>>();
            var userManagerMock1 = new Mock<UserManager<User>>(userStore1, null, null, null, null, null, null, null, null);
            UserManager<User> userManager1 = userManagerMock1.Object;

            // Arrange - second pair
            var unitOfWorkMock2 = new Mock<IUnitOfWork>();
            IUnitOfWork unitOfWork2 = unitOfWorkMock2.Object;
            var userStore2 = Mock.Of<IUserStore<User>>();
            var userManagerMock2 = new Mock<UserManager<User>>(userStore2, null, null, null, null, null, null, null, null);
            UserManager<User> userManager2 = userManagerMock2.Object;

            // Act
            var handler1 = new PromoteAllStudentsCommandHandler(unitOfWork1, userManager1);
            var handler2 = new PromoteAllStudentsCommandHandler(unitOfWork2, userManager2);

            // Assert
            Assert.IsNotNull(handler1, "First handler instance should not be null.");
            Assert.IsNotNull(handler2, "Second handler instance should not be null.");
            Assert.AreNotSame(handler1, handler2, "Two handler instances created with different dependencies should not be the same object instance.");
        }

        #region Test async queryable helpers (inner types)
        // The production code calls ToListAsync on UserManager.Users (IQueryable).
        // Provide an async-capable IQueryable implementation so ToListAsync extension works in tests.
        private class TestAsyncQueryProvider<TEntity> : IQueryProvider
        {
            private readonly IQueryProvider _inner;

            public TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            }

            public IQueryable CreateQuery(Expression expression)
            {
                Type elementType = expression.Type.GetGenericArguments().FirstOrDefault() ?? typeof(TEntity);
                var queryType = typeof(TestAsyncEnumerable<>).MakeGenericType(elementType);
                return (IQueryable)Activator.CreateInstance(queryType, expression)!;
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new TestAsyncEnumerable<TElement>(expression);
            }

            public object? Execute(Expression expression) => _inner.Execute(expression);

            public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);
        }

        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IQueryable<T>, IAsyncEnumerable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
            public TestAsyncEnumerable(Expression expression) : base(expression) { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;
            public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return default;
            }

            public ValueTask<bool> MoveNextAsync()
            {
                try
                {
                    return new ValueTask<bool>(_inner.MoveNext());
                }
                catch (Exception ex)
                {
                    return new ValueTask<bool>(Task.FromException<bool>(ex));
                }
            }

            public T Current => _inner.Current;
        }

        /// <summary>
        /// Extension helper to build an async-capable IQueryable for tests.
        /// </summary>
        private static class TestAsyncQueryableExtensions
        {
        }
        #endregion
    }
}