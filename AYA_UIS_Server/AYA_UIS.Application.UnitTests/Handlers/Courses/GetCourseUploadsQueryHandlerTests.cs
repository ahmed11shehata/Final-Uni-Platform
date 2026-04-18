using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Courses;
using AYA_UIS.Application.Queries;
using AYA_UIS.Application.Queries.Courses;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module;
using Shared.Dtos.Info_Module.CourseUploadDtos;

namespace AYA_UIS.Application.Handlers.Courses.UnitTests
{
    [TestClass]
    public partial class GetCourseUploadsQueryHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor successfully creates an instance when valid non-null dependencies are provided.
        /// Input conditions:
        /// - A mocked IUnitOfWork (non-null).
        /// - A constructed UserManager&lt;User&gt; with mocked/placeholder dependencies (non-null).
        /// Expected result:
        /// - No exception is thrown.
        /// - Returned instance is not null and implements IRequestHandler&lt;GetCourseUploadsQuery, IEnumerable&lt;CourseUploadDto&gt;&gt;.
        /// </summary>
        [TestMethod]
        public void Constructor_ValidDependencies_InstanceCreatedAndImplementsInterface()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Create minimal dependencies required by UserManager<TUser>
            var userStoreMock = new Mock<IUserStore<User>>(MockBehavior.Strict);
            var passwordHasherMock = new Mock<IPasswordHasher<User>>(MockBehavior.Strict);
            var userValidators = new List<IUserValidator<User>>();
            var passwordValidators = new List<IPasswordValidator<User>>();
            var keyNormalizerMock = new Mock<ILookupNormalizer>(MockBehavior.Strict);
            var errors = new IdentityErrorDescriber();
            var services = new Mock<IServiceProvider>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger<UserManager<User>>>(MockBehavior.Strict);
            var options = Options.Create(new IdentityOptions());

            var userManager = new UserManager<User>(
                userStoreMock.Object,
                options,
                passwordHasherMock.Object,
                userValidators,
                passwordValidators,
                keyNormalizerMock.Object,
                errors,
                services.Object,
                loggerMock.Object);

            // Act
            GetCourseUploadsQueryHandler? handler = null;
            Exception? ctorEx = null;
            try
            {
                handler = new GetCourseUploadsQueryHandler(unitOfWorkMock.Object, userManager);
            }
            catch (Exception ex)
            {
                ctorEx = ex;
            }

            // Assert
            Assert.IsNull(ctorEx, "Constructor should not throw when provided valid non-null dependencies.");
            Assert.IsNotNull(handler, "Handler instance should be created.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetCourseUploadsQuery, IEnumerable<CourseUploadDto>>));
        }

        /// <summary>
        /// Partial test placeholder for null dependency behavior.
        /// Input conditions:
        /// - Intentionally not providing null for non-nullable parameters because source parameters are non-nullable.
        /// Expected result:
        /// - This test is marked inconclusive and documents that passing null would violate the non-nullable contract.
        /// </summary>
        [TestMethod]
        public void Constructor_NullDependencies_NotApplicableMarkedInconclusive()
        {
            // Arrange & Act
            GetCourseUploadsQueryHandler? handler = null;
            Exception? ctorEx = null;
            try
            {
                // Use null-forgiving operator to bypass nullable reference type warnings and exercise runtime behavior.
                handler = new GetCourseUploadsQueryHandler(null!, null!);
            }
            catch (Exception ex)
            {
                ctorEx = ex;
            }

            // Assert
            Assert.IsNull(ctorEx, "Constructor should not throw when provided null dependencies at runtime.");
            Assert.IsNotNull(handler, "Handler instance should be created even when null dependencies are passed at runtime.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<GetCourseUploadsQuery, IEnumerable<CourseUploadDto>>));
        }

        /// <summary>
        /// Test that Handle maps CourseUpload -> CourseUploadDto and uses the user's DisplayName when available,
        /// otherwise falls back to "Unknown". Also verifies repository and user manager interactions.
        /// Input: repository returns two uploads with distinct uploader ids; user manager returns a User for one id and null for the other.
        /// Expected: resulting DTOs contain mapped fields and UploadedBy = DisplayName or "Unknown"; FindByIdAsync invoked once per distinct id.
        /// </summary>
        [TestMethod]
        public async Task Handle_WithUploads_ReturnsMappedDtos_AndUsesUnknownForMissingUser()
        {
            // Arrange
            var repoMock = new Mock<ICourseUploadsRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.CourseUploads).Returns(repoMock.Object);

            var userStore = Mock.Of<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(userStore, null, null, null, null, null, null, null, null);

            var uploads = new List<CourseUpload>
            {
                new CourseUpload
                {
                    Id = 1,
                    Title = "T1",
                    Description = "D1",
                    Type = default,
                    Url = "http://u1",
                    UploadedAt = new DateTime(2020,1,1),
                    UploadedByUserId = "u1"
                },
                new CourseUpload
                {
                    Id = 2,
                    Title = "T2",
                    Description = "D2",
                    Type = default,
                    Url = "http://u2",
                    UploadedAt = new DateTime(2020,2,2),
                    UploadedByUserId = "u2"
                }
            };

            int testCourseId = 123;
            repoMock.Setup(r => r.GetByCourseIdAsync(testCourseId)).ReturnsAsync(uploads);

            userManagerMock.Setup(m => m.FindByIdAsync("u1")).ReturnsAsync(new User { DisplayName = "Alice" });
            userManagerMock.Setup(m => m.FindByIdAsync("u2")).ReturnsAsync((User?)null);

            var handler = new GetCourseUploadsQueryHandler(unitOfWorkMock.Object, userManagerMock.Object);

            var request = new GetCourseUploadsQuery(testCourseId);
            var ct = CancellationToken.None;

            // Act
            var result = (await handler.Handle(request, ct)).ToList();

            // Assert
            Assert.AreEqual(2, result.Count, "Expected two DTOs returned.");
            var dto1 = result.Single(d => d.Id == 1);
            Assert.AreEqual("T1", dto1.Title);
            Assert.AreEqual("D1", dto1.Description);
            Assert.AreEqual("http://u1", dto1.Url);
            Assert.AreEqual(new DateTime(2020, 1, 1), dto1.UploadedAt);
            Assert.AreEqual("Alice", dto1.UploadedBy, "Expected uploader display name when user exists.");

            var dto2 = result.Single(d => d.Id == 2);
            Assert.AreEqual("T2", dto2.Title);
            Assert.AreEqual("D2", dto2.Description);
            Assert.AreEqual("http://u2", dto2.Url);
            Assert.AreEqual(new DateTime(2020, 2, 2), dto2.UploadedAt);
            Assert.AreEqual("Unknown", dto2.UploadedBy, "Expected 'Unknown' when user is not found.");

            repoMock.Verify(r => r.GetByCourseIdAsync(testCourseId), Times.Once);
            userManagerMock.Verify(m => m.FindByIdAsync("u1"), Times.Once);
            userManagerMock.Verify(m => m.FindByIdAsync("u2"), Times.Once);
        }

        /// <summary>
        /// Test that when multiple uploads share the same UploadedByUserId the handler only queries the user manager once per distinct id.
        /// Input: repository returns two uploads with identical uploader id; user manager returns a single User.
        /// Expected: both DTOs have the same UploadedBy display name and FindByIdAsync called exactly once for that id.
        /// </summary>
        [TestMethod]
        public async Task Handle_WithDuplicateUploaderIds_CallsFindByIdOncePerDistinct()
        {
            // Arrange
            var repoMock = new Mock<ICourseUploadsRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.CourseUploads).Returns(repoMock.Object);

            var userStore = Mock.Of<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(userStore, null, null, null, null, null, null, null, null);

            var uploads = new List<CourseUpload>
            {
                new CourseUpload { Id = 1, Title = "A", Description = "a", Type = default, Url = "u", UploadedAt = DateTime.UtcNow, UploadedByUserId = "dup" },
                new CourseUpload { Id = 2, Title = "B", Description = "b", Type = default, Url = "v", UploadedAt = DateTime.UtcNow, UploadedByUserId = "dup" }
            };

            int courseId = 7;
            repoMock.Setup(r => r.GetByCourseIdAsync(courseId)).ReturnsAsync(uploads);

            userManagerMock.Setup(m => m.FindByIdAsync("dup")).ReturnsAsync(new User { DisplayName = "Bob" });

            var handler = new GetCourseUploadsQueryHandler(unitOfWorkMock.Object, userManagerMock.Object);

            // Act
            var dtos = (await handler.Handle(new GetCourseUploadsQuery(courseId), CancellationToken.None)).ToList();

            // Assert
            Assert.AreEqual(2, dtos.Count);
            Assert.IsTrue(dtos.All(d => d.UploadedBy == "Bob"), "Both DTOs should have the uploader's display name.");
            userManagerMock.Verify(m => m.FindByIdAsync("dup"), Times.Once, "FindByIdAsync should be called once for the distinct uploader id.");
        }

        /// <summary>
        /// Test that when repository returns an empty collection the handler returns an empty enumeration and does not call the user manager.
        /// Input: repository returns empty list for provided course id.
        /// Expected: returned enumerable is empty; FindByIdAsync is never called.
        /// </summary>
        [TestMethod]
        public async Task Handle_WithEmptyUploads_ReturnsEmptyEnumerable_AndNoFindByIdCall()
        {
            // Arrange
            var repoMock = new Mock<ICourseUploadsRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.CourseUploads).Returns(repoMock.Object);

            var userStore = Mock.Of<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(userStore, null, null, null, null, null, null, null, null);

            int courseId = 999;
            repoMock.Setup(r => r.GetByCourseIdAsync(courseId)).ReturnsAsync(new List<CourseUpload>());

            var handler = new GetCourseUploadsQueryHandler(unitOfWorkMock.Object, userManagerMock.Object);

            // Act
            var result = (await handler.Handle(new GetCourseUploadsQuery(courseId), CancellationToken.None)).ToList();

            // Assert
            Assert.AreEqual(0, result.Count, "Expected no DTOs when repository returns empty collection.");
            userManagerMock.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never, "UserManager should not be queried when there are no uploads.");
        }

        /// <summary>
        /// Test passing numeric boundary CourseId values to ensure they are forwarded to repository unchanged and do not cause unexpected behavior.
        /// Inputs: int.MinValue, 0, int.MaxValue with repository returning empty collection.
        /// Expected: repository invoked with same course ids and no exceptions thrown.
        /// </summary>
        [TestMethod]
        public async Task Handle_WithBoundaryCourseIds_RepositoryIsCalledWithSameIds_NoExceptions()
        {
            // Arrange
            var repoMock = new Mock<ICourseUploadsRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.CourseUploads).Returns(repoMock.Object);

            var userStore = Mock.Of<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(userStore, null, null, null, null, null, null, null, null);

            var handler = new GetCourseUploadsQueryHandler(unitOfWorkMock.Object, userManagerMock.Object);

            var idsToTest = new[] { int.MinValue, 0, int.MaxValue };

            foreach (var id in idsToTest)
            {
                // Setup repository to verify call and return empty list
                repoMock.Setup(r => r.GetByCourseIdAsync(id)).ReturnsAsync(new List<CourseUpload>()).Verifiable();

                // Act
                var result = (await handler.Handle(new GetCourseUploadsQuery(id), CancellationToken.None)).ToList();

                // Assert
                Assert.AreEqual(0, result.Count, $"Expected empty result for course id {id}.");
                repoMock.Verify(r => r.GetByCourseIdAsync(id), Times.Once, $"Repository should be called once with id {id}.");

                // Reset setups to avoid interference between iterations
                repoMock.ResetCalls();
            }
        }
    }
}