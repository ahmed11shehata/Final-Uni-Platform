using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

using AYA_UIS.Application.Commands;
using AYA_UIS.Application.Commands.Semesters;
using AYA_UIS.Application.Handlers;
using AYA_UIS.Application.Handlers.Semesters;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain;
using Domain.Contracts;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shared.Dtos.Info_Module.SemesterDtos;

namespace AYA_UIS.Application.Handlers.Semesters.UnitTests
{
    /// <summary>
    /// Tests for CreateSemesterCommandHandler constructor behavior.
    /// </summary>
    [TestClass]
    public class CreateSemesterCommandHandlerTests
    {
        /// <summary>
        /// Verifies that the constructor successfully creates an instance when a non-null IUnitOfWork is supplied.
        /// Input conditions: a valid mocked IUnitOfWork instance.
        /// Expected result: constructor does not throw and returns a non-null handler instance.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidUnitOfWork_DoesNotThrowAndCreatesInstance()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);

            // Act
            CreateSemesterCommandHandler? handler = null;
            Exception? caught = null;
            try
            {
                handler = new CreateSemesterCommandHandler(mockUnitOfWork.Object);
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            // Assert
            Assert.IsNull(caught, $"Constructor threw an unexpected exception: {caught}");
            Assert.IsNotNull(handler, "Constructor returned null handler instance when provided a valid IUnitOfWork.");
        }

        /// <summary>
        /// Verifies that a valid CreateSemesterCommand results in:
        /// - repository AddAsync being called once with a Semester whose properties match the DTO and StudyYearId,
        /// - SaveChangesAsync being called once,
        /// - the handler returning the Id assigned to the Semester during AddAsync.
        /// Test uses several StudyYearId edge values (int.MinValue, 0, int.MaxValue) and extreme DateTime values.
        /// </summary>
        [TestMethod]
        public async Task Handle_ValidRequest_CallsAddAndSaveAndReturnsAssignedId()
        {
            // Arrange
            // Prepare cases to exercise StudyYearId and DateTime edge values
            var cases = new (int studyYearId, DateTime start, DateTime end, int assignedId)[]
            {
                (int.MinValue, DateTime.MinValue, DateTime.MinValue.AddDays(1), 100),
                (0, new DateTime(2000,1,1), new DateTime(2000,12,31), 200),
                (int.MaxValue, DateTime.MaxValue.AddDays(-1), DateTime.MaxValue, 300)
            };

            foreach (var (studyYearId, start, end, assignedId) in cases)
            {
                // Arrange per-case
                var dto = new CreateSemesterDto
                {
                    // Use numeric cast for enum to avoid relying on specific enum members
                    Title = (AYA_UIS.Core.Domain.Enums.SemesterEnum)0,
                    StartDate = start,
                    EndDate = end
                };

                var command = new CreateSemesterCommand(studyYearId, dto);

                var mockRepo = new Mock<ISemesterRepository>(MockBehavior.Strict);
                Semester? capturedSemester = null;
                mockRepo
                    .Setup(r => r.AddAsync(It.IsAny<Semester>()))
                    .Returns<Semester>(s =>
                    {
                        // simulate repository assigning Id
                        capturedSemester = s;
                        s.Id = assignedId;
                        return Task.CompletedTask;
                    });

                var mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
                mockUnitOfWork.Setup(u => u.Semesters).Returns(mockRepo.Object);
                mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

                var handler = new CreateSemesterCommandHandler(mockUnitOfWork.Object);

                // Act
                var result = await handler.Handle(command, CancellationToken.None);

                // Assert
                Assert.AreEqual(assignedId, result, "Handler should return the Id assigned by repository.");
                Assert.IsNotNull(capturedSemester, "Repository AddAsync should receive a Semester instance.");
                Assert.AreEqual(dto.StartDate, capturedSemester!.StartDate, "StartDate should be mapped from DTO.");
                Assert.AreEqual(dto.EndDate, capturedSemester.EndDate, "EndDate should be mapped from DTO.");
                Assert.AreEqual(dto.Title, capturedSemester.Title, "Title should be mapped from DTO.");
                Assert.AreEqual(studyYearId, capturedSemester.StudyYearId, "StudyYearId should be mapped from command.");
                mockRepo.Verify(r => r.AddAsync(It.IsAny<Semester>()), Times.Once);
                mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);

                // Cleanup setups for next iteration
                mockRepo.VerifyNoOtherCalls();
                mockUnitOfWork.VerifyNoOtherCalls();
            }
        }

    }
}