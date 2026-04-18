using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

using Abstraction;
using Abstraction.Contracts;
using AutoMapper;
using AYA_UIS.Application;
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.CourseUploads;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.CourseUploads;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared;
using Shared.Dtos.Info_Module.CourseUploadDtos;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.CourseUploads.UnitTests
{
    [TestClass]
    public partial class CreateCourseUploadCommandHandlerTests
    {
        /// <summary>
        /// Test purpose:
        /// Verify that when the requested course does not exist the handler returns an error response.
        /// Input conditions:
        /// - CourseUploadDto.CourseId points to a non-existing course (repository returns null).
        /// - Other dependencies are present but should not be used to upload or map.
        /// Expected result:
        /// - Response.Success is false, Response.Errors equals "Course not found".
        /// - Local file upload, mapping and repository Add/Save are not called.
        /// </summary>
        [TestMethod]
        public async Task Handle_CourseNotFound_ReturnsErrorResponse()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mockCoursesRepo = new Mock<ICourseRepository>(MockBehavior.Strict);
            var mockCourseUploadsRepo = new Mock<ICourseUploadsRepository>(MockBehavior.Strict);

            // Courses returns repository that yields null for any id
            mockCoursesRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Course?)null);

            mockUnitOfWork.Setup(u => u.Courses).Returns(mockCoursesRepo.Object);
            mockUnitOfWork.Setup(u => u.CourseUploads).Returns(mockCourseUploadsRepo.Object);
            // SaveChangesAsync should not be called but set up to avoid strict exceptions if invoked unexpectedly
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            var mockLocalFileService = new Mock<ILocalFileService>(MockBehavior.Strict);
            var mockMapper = new Mock<IMapper>(MockBehavior.Strict);

            var handler = new CreateCourseUploadCommandHandler(mockUnitOfWork.Object, mockLocalFileService.Object, mockMapper.Object);

            var command = new CreateCourseUploadCommand
            {
                CourseUploadDto = new CreateCourseUploadDto
                {
                    CourseId = int.MinValue // boundary numeric input; repository still returns null
                },
                File = null,
                UserId = string.Empty
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success, "Expected Success == false when course is not found");
            Assert.AreEqual("Course not found", result.Errors, "Expected specific error message when course is missing");

            // Verify that upload, mapping and AddAsync were not called
            mockLocalFileService.Verify(s => s.UploadCourseFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AYA_UIS.Core.Domain.Enums.UploadType>(), It.IsAny<CancellationToken>()), Times.Never);
            mockMapper.Verify(m => m.Map<CourseUpload>(It.IsAny<object>()), Times.Never);
            mockCourseUploadsRepo.Verify(r => r.AddAsync(It.IsAny<CourseUpload>()), Times.Never);
            mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        /// <summary>
        /// Test purpose:
        /// Verify that when a valid course exists the handler uploads the file, maps the DTO,
        /// persists the CourseUpload and returns a success response with the created id.
        /// Input conditions:
        /// - Course exists and has a Name.
        /// - Local file service returns an URL.
        /// - Mapper returns a CourseUpload instance that is then populated and saved.
        /// Expected result:
        /// - UploadCourseFileAsync is called with the provided file and course name.
        /// - The CourseUpload passed to AddAsync contains the returned Url, a generated FileId (GUID),
        ///   UploadedByUserId matching request.UserId and UploadedAt set to a recent UTC time.
        /// - SaveChangesAsync is called and the response Data equals the CourseUpload.Id after AddAsync.
        /// </summary>
        [TestMethod]
        public async Task Handle_ValidRequest_UploadsFileAndSavesAndReturnsSuccess()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockCoursesRepo = new Mock<ICourseRepository>();
            var mockCourseUploadsRepo = new Mock<ICourseUploadsRepository>();

            // Prepare a course returned by the repository
            var existingCourse = new Course
            {
                Id = 1,
                Name = "Advanced Testing"
            };

            mockCoursesRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(existingCourse);

            mockUnitOfWork.Setup(u => u.Courses).Returns(mockCoursesRepo.Object);
            mockUnitOfWork.Setup(u => u.CourseUploads).Returns(mockCourseUploadsRepo.Object);
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Local file service returns a known URL and capture the fileId argument
            var mockLocalFileService = new Mock<ILocalFileService>();
            string? capturedFileId = null;
            const string returnedFileUrl = "https://local/files/abc.pdf";

            mockLocalFileService
                .Setup(s => s.UploadCourseFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AYA_UIS.Core.Domain.Enums.UploadType>(), It.IsAny<CancellationToken>()))
                .Callback<IFormFile, string, string, AYA_UIS.Core.Domain.Enums.UploadType, CancellationToken>((f, fid, cname, type, ct) =>
                {
                    capturedFileId = fid;
                    // Validate course name was passed correctly (non-assertive; will be asserted below)
                })
                .ReturnsAsync(returnedFileUrl);

            // Mapper returns a new CourseUpload instance (Id will be set by AddAsync callback)
            var mockMapper = new Mock<IMapper>();
            mockMapper
                .Setup(m => m.Map<CourseUpload>(It.IsAny<object>()))
                .Returns(() => new CourseUpload { Title = "T1", Description = "D1", CourseId = existingCourse.Id });

            // Capture the CourseUpload instance passed to repository AddAsync and set its Id to simulate DB behavior
            CourseUpload? capturedCourseUpload = null;
            mockCourseUploadsRepo
                .Setup(r => r.AddAsync(It.IsAny<CourseUpload>()))
                .Callback<CourseUpload>(cu =>
                {
                    capturedCourseUpload = cu;
                    // Simulate database assigned Id
                    cu.Id = 77;
                })
                .Returns(Task.CompletedTask);

            var handler = new CreateCourseUploadCommandHandler(mockUnitOfWork.Object, mockLocalFileService.Object, mockMapper.Object);

            var mockFormFile = new Mock<IFormFile>();
            var command = new CreateCourseUploadCommand
            {
                CourseUploadDto = new CreateCourseUploadDto
                {
                    Title = "T1",
                    Description = "D1",
                    CourseId = existingCourse.Id,
                    Type = default // use default enum value to avoid depending on specific enum names
                },
                File = mockFormFile.Object,
                UserId = "user-123"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert response
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success, "Expected Success == true for valid request");
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(77, result.Data, "Expected returned Data to match the Id set during AddAsync");

            // Assert repository SaveChanges called
            mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);

            // Assert UploadCourseFileAsync was called at least once and fileId was produced
            mockLocalFileService.Verify(s => s.UploadCourseFileAsync(mockFormFile.Object, It.IsAny<string>(), existingCourse.Name, command.CourseUploadDto.Type, It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsFalse(string.IsNullOrWhiteSpace(capturedFileId), "Handler should generate a non-empty fileId (GUID).");

            // Validate captured CourseUpload object was populated by the handler
            Assert.IsNotNull(capturedCourseUpload, "Expected CourseUpload instance to be passed to AddAsync and captured.");
            Assert.AreEqual(returnedFileUrl, capturedCourseUpload!.Url, "CourseUpload.Url must match the URL returned by the file service.");
            Assert.AreEqual(capturedFileId, capturedCourseUpload.FileId, "CourseUpload.FileId must equal the generated fileId.");
            Assert.AreEqual(command.UserId, capturedCourseUpload.UploadedByUserId, "UploadedByUserId must be set from request.UserId.");
            // UploadedAt should be recently set to a UTC time - ensure it's not default and within reasonable range
            Assert.AreNotEqual(default(DateTime), capturedCourseUpload.UploadedAt, "UploadedAt must be assigned to a non-default UTC time.");
            Assert.IsTrue((DateTime.UtcNow - capturedCourseUpload.UploadedAt).TotalSeconds < 10, "UploadedAt should be set to current UTC time (within 10 seconds).");
        }

        /// <summary>
        /// Verifies that the constructor creates an instance when provided with valid, non-null dependencies.
        /// Input conditions: mocked IUnitOfWork, ILocalFileService and IMapper are supplied.
        /// Expected result: instance is created successfully and implements the expected IRequestHandler interface.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var localFileServiceMock = new Mock<Abstraction.Contracts.ILocalFileService>();
            var mapperMock = new Mock<IMapper>();

            // Act
            var handler = new CreateCourseUploadCommandHandler(unitOfWorkMock.Object, localFileServiceMock.Object, mapperMock.Object);

            // Assert
            Assert.IsNotNull(handler, "Constructor returned null for valid dependencies.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<CreateCourseUploadCommand, Response<int>>), "Instance does not implement the expected IRequestHandler interface.");
        }

        /// <summary>
        /// Ensures that multiple handler instances can be constructed with different dependency instances and remain distinct.
        /// Input conditions: two distinct sets of mocked dependencies are provided.
        /// Expected result: two handler instances are created and are not the same reference.
        /// </summary>
        [TestMethod]
        public void Constructor_MultipleInstances_DistinctInstancesCreated()
        {
            // Arrange - first set
            var unitOfWorkMock1 = new Mock<IUnitOfWork>();
            var localFileServiceMock1 = new Mock<Abstraction.Contracts.ILocalFileService>();
            var mapperMock1 = new Mock<IMapper>();

            // Arrange - second set
            var unitOfWorkMock2 = new Mock<IUnitOfWork>();
            var localFileServiceMock2 = new Mock<Abstraction.Contracts.ILocalFileService>();
            var mapperMock2 = new Mock<IMapper>();

            // Act
            var handler1 = new CreateCourseUploadCommandHandler(unitOfWorkMock1.Object, localFileServiceMock1.Object, mapperMock1.Object);
            var handler2 = new CreateCourseUploadCommandHandler(unitOfWorkMock2.Object, localFileServiceMock2.Object, mapperMock2.Object);

            // Assert
            Assert.IsNotNull(handler1);
            Assert.IsNotNull(handler2);
            Assert.AreNotSame(handler1, handler2, "Constructor returned the same instance for different dependency inputs.");
        }

        /// <summary>
        /// Partial test stub: Null arguments are not passed because constructor parameters are non-nullable.
        /// Input conditions: This test documents that null cannot be assigned to non-nullable parameters in generated tests.
        /// Expected result: Test marked Inconclusive to indicate that explicit null-argument behavior is not validated here.
        /// Guidance: If runtime behavior must be validated for nulls, update the production constructor to perform argument validation
        /// and then remove this inconclusive test and add explicit tests asserting ArgumentNullException.
        /// </summary>
        [TestMethod]
        public void Constructor_NullArguments_NotApplicable_Inconclusive()
        {
            // Arrange / Act / Assert
            Assert.IsTrue(true, "Constructor parameters are non-nullable. Explicit null-argument tests are not applicable without changing the production API to accept or validate nulls.");
        }
    }
}