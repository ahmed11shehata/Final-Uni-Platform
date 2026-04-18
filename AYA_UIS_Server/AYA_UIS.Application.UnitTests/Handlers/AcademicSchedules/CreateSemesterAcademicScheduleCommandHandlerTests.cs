using System;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application;
using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.AcademicSchedules;
using AYA_UIS.Application.Contracts;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.AcademicSchedules;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Shared;
using AYA_UIS.Shared.Exceptions;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module.AcademicSheduleDtos;

namespace AYA_UIS.Application.Handlers.AcademicSchedules.UnitTests
{
    [TestClass]
    public class CreateSemesterAcademicScheduleCommandHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor succeeds when provided with valid dependency implementations (mocks).
        /// Input conditions: two non-null mocks for IUnitOfWork and ICloudinaryService.
        /// Expected result: an instance is created, not null, and it implements IRequestHandler<CreateSemesterAcademicScheduleCommand, Unit>.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstanceAndImplementsInterface()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var cloudinaryMock = new Mock<ICloudinaryService>(MockBehavior.Strict);

            // Act
            var handler = new CreateSemesterAcademicScheduleCommandHandler(unitOfWorkMock.Object, cloudinaryMock.Object);

            // Assert
            Assert.IsNotNull(handler, "Constructor returned null when valid dependencies were provided.");
            Assert.IsInstanceOfType(handler, typeof(IRequestHandler<CreateSemesterAcademicScheduleCommand, Unit>), "Handler does not implement the expected IRequestHandler interface.");
        }

        /// <summary>
        /// Ensures constructing multiple handlers with different dependency instances yields independent handler objects.
        /// Input conditions: two distinct pairs of mocks for dependencies.
        /// Expected result: both handlers are non-null and are different object instances (no shared identity).
        /// This helps catch accidental static/shared state in the constructor.
        /// </summary>
        [TestMethod]
        public void Constructor_DifferentMockInstances_ProducesDistinctHandlerInstances()
        {
            // Arrange - first set of mocks
            var unitOfWorkMock1 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var cloudinaryMock1 = new Mock<ICloudinaryService>(MockBehavior.Strict);

            // Arrange - second set of mocks
            var unitOfWorkMock2 = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var cloudinaryMock2 = new Mock<ICloudinaryService>(MockBehavior.Strict);

            // Act
            var handler1 = new CreateSemesterAcademicScheduleCommandHandler(unitOfWorkMock1.Object, cloudinaryMock1.Object);
            var handler2 = new CreateSemesterAcademicScheduleCommandHandler(unitOfWorkMock2.Object, cloudinaryMock2.Object);

            // Assert
            Assert.IsNotNull(handler1, "First handler instance should not be null.");
            Assert.IsNotNull(handler2, "Second handler instance should not be null.");
            Assert.AreNotSame(handler1, handler2, "Constructor should produce distinct handler instances for different dependency objects.");
        }

        /// <summary>
        /// Verifies successful creation flow:
        /// - UploadAcademicScheduleAsync is called with the provided file and a generated fileId.
        /// - An AcademicSchedule entity is added with mapped properties (Title, Description, UploadedByUserId, DepartmentId, StudyYearId, SemesterId).
        /// - SaveChangesAsync is invoked.
        /// Input: All repositories return existing entities, cloudinary returns a URL.
        /// Expected: Handler completes and Unit.Value is returned; AddAsync and SaveChangesAsync are invoked; uploaded URL and fileId are propagated to the entity.
        /// </summary>
        [TestMethod]
        public async Task Handle_AllValid_CreatesAcademicScheduleAndCallsDependencies()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            var mockStudyYearRepo = new Mock<IStudyYearRepository>(MockBehavior.Strict);
            var mockDepartmentRepo = new Mock<IDepartmentRepository>(MockBehavior.Strict);
            var mockSemesterRepo = new Mock<ISemesterRepository>(MockBehavior.Strict);
            var mockAcademicScheduleRepo = new Mock<IAcademicScheduleRepository>(MockBehavior.Strict);
            var mockCloudinary = new Mock<ICloudinaryService>(MockBehavior.Strict);

            mockUnitOfWork.Setup(u => u.StudyYears).Returns(mockStudyYearRepo.Object);
            mockUnitOfWork.Setup(u => u.Departments).Returns(mockDepartmentRepo.Object);
            mockUnitOfWork.Setup(u => u.Semesters).Returns(mockSemesterRepo.Object);
            mockUnitOfWork.Setup(u => u.AcademicSchedules).Returns(mockAcademicScheduleRepo.Object);
            mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            mockStudyYearRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new StudyYear());
            mockDepartmentRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Department());
            mockSemesterRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Semester());

            string capturedFileId = null!;
            mockCloudinary
                .Setup(c => c.UploadAcademicScheduleAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://cdn.test/file.pdf")
                .Callback<IFormFile, string, CancellationToken>((f, id, ct) => capturedFileId = id);

            AcademicSchedule? capturedEntity = null;
            mockAcademicScheduleRepo
                .Setup(r => r.AddAsync(It.IsAny<AcademicSchedule>()))
                .Returns(Task.CompletedTask)
                .Callback<AcademicSchedule>(entity => capturedEntity = entity);

            var handler = new CreateSemesterAcademicScheduleCommandHandler(mockUnitOfWork.Object, mockCloudinary.Object);

            var mockFile = new Mock<IFormFile>(MockBehavior.Strict);
            mockFile.SetupGet(f => f.FileName).Returns("schedule.pdf");

            var dto = new CreateSemesterAcademicScheduleDto
            {
                Title = "Semester Schedule",
                File = mockFile.Object,
                Description = "Desc"
            };

            var uploadedBy = "u_user";
            var studyYearId = 10;
            var departmentId = 20;
            var semesterId = 30;

            var command = new CreateSemesterAcademicScheduleCommand(uploadedBy, studyYearId, departmentId, semesterId, dto);
            var ct = CancellationToken.None;

            var beforeCall = DateTime.UtcNow;

            // Act
            var result = await handler.Handle(command, ct);

            var afterCall = DateTime.UtcNow;

            // Assert
            Assert.AreEqual(Unit.Value, result, "Handler should return MediatR.Unit.Value on success.");

            // Verify Upload was called with provided file and that it supplied a fileId
            mockCloudinary.Verify(c => c.UploadAcademicScheduleAsync(It.Is<IFormFile>(f => f == mockFile.Object),
                                                                      It.IsAny<string>(),
                                                                      It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsFalse(string.IsNullOrEmpty(capturedFileId), "A fileId (GUID string) should be generated and passed to the cloud service.");

            // Verify entity captured and mapped correctly
            Assert.IsNotNull(capturedEntity, "AcademicSchedule entity should be created and passed to repository AddAsync.");
            Assert.AreEqual(dto.Title, capturedEntity!.Title);
            Assert.AreEqual(dto.Description, capturedEntity.Description);
            Assert.AreEqual(uploadedBy, capturedEntity.UploadedByUserId);
            Assert.AreEqual(departmentId, capturedEntity.DepartmentId);
            Assert.AreEqual(studyYearId, capturedEntity.StudyYearId);
            Assert.AreEqual(semesterId, capturedEntity.SemesterId);
            Assert.AreEqual(capturedFileId, capturedEntity.FileId, "Entity.FileId must equal the fileId provided to cloudinary.");
            Assert.AreEqual("https://cdn.test/file.pdf", capturedEntity.Url, "Entity.Url must equal the value returned by cloudinary.");

            // ScheduleDate should be set to a recent UTC time between beforeCall and afterCall
            Assert.IsTrue(capturedEntity.ScheduleDate >= beforeCall && capturedEntity.ScheduleDate <= afterCall.AddSeconds(1),
                $"ScheduleDate should be set to a recent UTC time. Actual: {capturedEntity.ScheduleDate:o}, RangeStart: {beforeCall:o}, RangeEnd: {afterCall.AddSeconds(1):o}");

            mockAcademicScheduleRepo.Verify(r => r.AddAsync(It.IsAny<AcademicSchedule>()), Times.Once);
            mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}