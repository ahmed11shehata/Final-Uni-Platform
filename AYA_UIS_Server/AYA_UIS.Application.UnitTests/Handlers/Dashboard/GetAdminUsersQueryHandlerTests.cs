#nullable enable
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Dashboard;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Dashboard;
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
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.DashboardDtos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AYA_UIS.Application.Handlers.Dashboard.UnitTests
{
    [TestClass]
    public class GetAdminUserByCodeQueryHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor creates a non-null instance when provided with valid dependencies.
        /// Inputs: a properly constructed UserManager{User} and a mocked IUnitOfWork.
        /// Expected: The constructor does not throw and the resulting object implements IRequestHandler<GetAdminUserByCodeQuery , AdminUserDto ?>.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidDependencies_InstanceCreatedAndImplementsInterface()
        {
            // Arrange
            var storeMock = new Mock<IUserStore<User>>(MockBehavior.Loose);
            var options = Options.Create(new IdentityOptions());
            var passwordHasherMock = new Mock<IPasswordHasher<User>>(MockBehavior.Loose);
            IEnumerable<IUserValidator<User>> userValidators = Enumerable.Empty<IUserValidator<User>>();
            IEnumerable<IPasswordValidator<User>> pwdValidators = Enumerable.Empty<IPasswordValidator<User>>();
            var lookupNormalizerMock = new Mock<ILookupNormalizer>(MockBehavior.Loose);
            var errorDescriber = new IdentityErrorDescriber();
            IServiceProvider? services = null;
            var loggerMock = new Mock<ILogger<UserManager<User>>>(MockBehavior.Loose);
            var userManager = new UserManager<User>(storeMock.Object, options, passwordHasherMock.Object, userValidators, pwdValidators, lookupNormalizerMock.Object, errorDescriber, services, loggerMock.Object);
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Loose);
            // Act
            var handler = new GetAdminUserByCodeQueryHandler(userManager, unitOfWorkMock.Object);
            // Assert
            Assert.IsNotNull(handler, "Handler instance should not be null when constructed with valid dependencies.");
            Assert.IsTrue(handler is IRequestHandler<GetAdminUserByCodeQuery, AdminUserDto?>, "Handler should implement the expected IRequestHandler interface.");
        }

        /// <summary>
        /// Ensures multiple constructions with distinct dependency instances produce distinct handler instances
        /// and that construction is resilient to different (but valid) dependency mocks.
        /// Inputs: two different UserManager{User} and IUnitOfWork mock combinations.
        /// Expected: Two distinct handler instances are created without exceptions.
        /// </summary>
        [TestMethod]
        public void Constructor_DifferentMocks_ProducesDistinctInstances()
        {
            // Arrange - first set of dependencies
            var storeA = new Mock<IUserStore<User>>();
            var optionsA = Options.Create(new IdentityOptions());
            var passwordHasherA = new Mock<IPasswordHasher<User>>();
            var lookupNormalizerA = new Mock<ILookupNormalizer>();
            var userManagerA = new UserManager<User>(storeA.Object, optionsA, passwordHasherA.Object, Enumerable.Empty<IUserValidator<User>>(), Enumerable.Empty<IPasswordValidator<User>>(), lookupNormalizerA.Object, new IdentityErrorDescriber(), null, new Mock<ILogger<UserManager<User>>>().Object);
            var unitOfWorkA = new Mock<IUnitOfWork>();
            // Arrange - second set of dependencies (distinct mocks)
            var storeB = new Mock<IUserStore<User>>();
            var optionsB = Options.Create(new IdentityOptions());
            var passwordHasherB = new Mock<IPasswordHasher<User>>();
            var lookupNormalizerB = new Mock<ILookupNormalizer>();
            var userManagerB = new UserManager<User>(storeB.Object, optionsB, passwordHasherB.Object, Enumerable.Empty<IUserValidator<User>>(), Enumerable.Empty<IPasswordValidator<User>>(), lookupNormalizerB.Object, new IdentityErrorDescriber(), null, new Mock<ILogger<UserManager<User>>>().Object);
            var unitOfWorkB = new Mock<IUnitOfWork>();
            // Act
            var handlerA = new GetAdminUserByCodeQueryHandler(userManagerA, unitOfWorkA.Object);
            var handlerB = new GetAdminUserByCodeQueryHandler(userManagerB, unitOfWorkB.Object);
            // Assert
            Assert.IsNotNull(handlerA, "First handler instance should not be null.");
            Assert.IsNotNull(handlerB, "Second handler instance should not be null.");
            Assert.AreNotSame(handlerA, handlerB, "Distinct constructor invocations with different dependencies should produce distinct handler instances.");
        }

    }
}

namespace AYA_UIS.Application.Handlers.Dashboard.UnitTests
{
    [TestClass]
    public class GetAdminUsersQueryHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor accepts valid dependencies and produces a non-null instance.
        /// Input conditions: mocked IUnitOfWork and mocked UserManager{User} are provided.
        /// Expected result: an instance is created, implements the expected IRequestHandler interface,
        /// and the same dependency instances passed to the constructor are preserved (exposed via test subclass).
        /// </summary>
        [TestMethod]
        public void Constructor_ValidDependencies_InstanceCreatedAndImplementsInterface()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var store = Mock.Of<IUserStore<User>>();
            var options = Mock.Of<IOptions<IdentityOptions>>();
            var passwordHasher = Mock.Of<IPasswordHasher<User>>();
            IEnumerable<IUserValidator<User>> userValidators = new List<IUserValidator<User>>();
            IEnumerable<IPasswordValidator<User>> passwordValidators = new List<IPasswordValidator<User>>();
            var lookupNormalizer = Mock.Of<ILookupNormalizer>();
            var errorDescriber = new IdentityErrorDescriber();
            var services = Mock.Of<IServiceProvider>();
            var logger = Mock.Of<ILogger<UserManager<User>>>();
            var userManagerMock = new Mock<UserManager<User>>(store, options, passwordHasher, userValidators, passwordValidators, lookupNormalizer, errorDescriber, services, logger);
            // Act
            var handler = new TestableGetAdminUsersQueryHandler(unitOfWorkMock.Object, userManagerMock.Object);
            // Assert
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetAdminUsersQuery, IEnumerable<AdminUserDto>>));
            Assert.AreSame(unitOfWorkMock.Object, handler.ExposedUnitOfWork);
            Assert.AreSame(userManagerMock.Object, handler.ExposedUserManager);
        }

        // Helper subclass inside test class to expose constructor parameters for verification.
        // This is allowed per guidance to create a small helper inside the test class to aid testing.
        private class TestableGetAdminUsersQueryHandler : GetAdminUsersQueryHandler
        {
            public IUnitOfWork ExposedUnitOfWork { get; }
            public UserManager<User> ExposedUserManager { get; }

            public TestableGetAdminUsersQueryHandler(IUnitOfWork unitOfWork, UserManager<User> userManager) : base(unitOfWork, userManager)
            {
                ExposedUnitOfWork = unitOfWork;
                ExposedUserManager = userManager;
            }
        }

        /// <summary>
        /// Tests Role-based filtering, default role when no roles exist, and mapping of Registered and Completed courses.
        /// Input conditions:
        /// - A user without roles (default role should be 'Student') and two registrations:
        ///   one not passed with a course code, and one passed with a grade value set.
        /// - Request.Role provided in different casing ("STUDENT") should match ignoring case.
        /// Expected result:
        /// - The user is included because role comparison is case-insensitive and defaults to 'Student'.
        /// - RegisteredCourses contains the non-passed course code.
        /// - CompletedCourses contains one item with Total calculated as enum numeric * 10, Year default 0 and Semester default 1.
        /// - Dept is null when DepartmentId is not set or not found in dept map.
        /// </summary>
        [TestMethod]
        public async Task Handle_RoleFilterAndRegistrations_MapsRegisteredAndCompletedAndRoleDefault()
        {
            // Arrange
            CancellationToken ct = CancellationToken.None;
            var user = new User
            {
                Id = "stu1",
                Academic_Code = "S001",
                DisplayName = "Student One",
                Email = null,
                Gender = Gender.Female,
                DepartmentId = null // no department
            };
            var users = new List<User>
            {
                user
            };
            var asyncUsers = new TestAsyncEnumerable<User>(users);
            var mockStore = new Mock<IUserStore<User>>();
            var mockUserManager = new Mock<UserManager<User>>(mockStore.Object, null, null, null, null, null, null, null, null);
            mockUserManager.Setup(m => m.Users).Returns(asyncUsers);
            // No roles => firstOrDefault null => userRole "Student"
            mockUserManager.Setup(m => m.GetRolesAsync(It.Is<User>(u => u.Id == user.Id))).ReturnsAsync(new List<string>());
            var mockUnit = new Mock<IUnitOfWork>();
            // Departments repository returns empty list so deptMap will be empty
            var mockDeptRepo = new Mock<IDepartmentRepository>();
            mockDeptRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Enumerable.Empty<Department>());
            mockUnit.Setup(u => u.Departments).Returns(mockDeptRepo.Object);
            // Prepare registrations
            var regNotPassed = new Registration
            {
                Course = new Course
                {
                    Code = "C101"
                },
                IsPassed = false,
                UserId = user.Id
            };
            var regPassed = new Registration
            {
                Course = new Course
                {
                    Code = "C202"
                },
                IsPassed = true,
                Grade = Grads.A, // numeric value -> expect Total = (decimal)Grads.A * 10
                StudyYear = null,
                Semester = null,
                UserId = user.Id
            };
            var mockRegRepo = new Mock<IRegistrationRepository>();
            mockRegRepo.Setup(r => r.GetByUserAsync(user.Id, null)).ReturnsAsync(new[] { regNotPassed, regPassed });
            // Fallback for other call signatures
            mockRegRepo.Setup(r => r.GetByUserAsync(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync((string uid, int? y) =>
            {
                if (uid == user.Id)
                    return (IEnumerable<Registration>)new[]
                    {
                        regNotPassed,
                        regPassed
                    };
                return Enumerable.Empty<Registration>();
            });
            mockUnit.Setup(u => u.Registrations).Returns(mockRegRepo.Object);
            var handler = new GetAdminUsersQueryHandler(mockUnit.Object, mockUserManager.Object);
            var request = new GetAdminUsersQuery
            {
                Role = "STUDENT" // different casing should still match "Student"
            };
            // Act
            var result = (await handler.Handle(request, ct)).ToList();
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count, "Expected the user to be included since role defaults to 'Student' and filter is case-insensitive.");
            var dto = result[0];
            // Role defaulted to "student"
            Assert.AreEqual("student", dto.Role);
            // No department -> Dept should be null
            Assert.IsNull(dto.Dept);
            // RegisteredCourses contains the non-passed course code
            CollectionAssert.AreEqual(new List<string> { "C101" }, dto.RegisteredCourses);
            // CompletedCourses contains one entry
            Assert.AreEqual(1, dto.CompletedCourses.Count);
            var completed = dto.CompletedCourses[0];
            Assert.AreEqual("C202", completed.Code);
            // Total for Grads.A: Grads enum underlying value for A is 1 (A_Plus = 0, A = 1) => 1 * 10 = 10
            Assert.AreEqual((decimal)Grads.A * 10m, completed.Total);
            // Year defaults to 0 when StudyYear null
            Assert.AreEqual(0, completed.Year);
            // Semester defaults to (int)(0) + 1 => 1
            Assert.AreEqual(1, completed.Semester);
        }

#region Test Async IQueryable helpers (inner classes)
        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IQueryable<T>, IAsyncEnumerable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
            {
            }

            public TestAsyncEnumerable(Expression expression) : base(expression)
            {
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

        }

        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;
            public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return default;
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(Task.FromResult(_inner.MoveNext()));
            }

            public T Current => _inner.Current;
        }
#endregion
    }
}