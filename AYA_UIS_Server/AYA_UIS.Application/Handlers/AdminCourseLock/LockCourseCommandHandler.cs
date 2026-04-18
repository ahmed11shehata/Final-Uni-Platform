using AYA_UIS.Application.Commands.AdminCourseLock;
using AYA_UIS.Core.Domain.Entities.Identity;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Info_Module.AdminCourseLockDtos;

using AdminLockEntity = AYA_UIS.Core.Domain.Entities.Models.AdminCourseLock;

namespace AYA_UIS.Application.Handlers.AdminCourseLock
{
    public class LockCourseCommandHandler
        : IRequestHandler<LockCourseCommand, AdminCourseLockResultDto>
    {
        private readonly IUnitOfWork      _uow;
        private readonly UserManager<User> _userManager;

        public LockCourseCommandHandler(
            IUnitOfWork      uow,
            UserManager<User> userManager)
        {
            _uow         = uow;
            _userManager = userManager;
        }

        public async Task<AdminCourseLockResultDto> Handle(
            LockCourseCommand request,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.AcademicCode))
                return new AdminCourseLockResultDto { Success = false, Message = "Academic code is required." };

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Academic_Code == request.AcademicCode, ct);

            if (user is null)
                return new AdminCourseLockResultDto { Success = false, Message = $"Student '{request.AcademicCode}' not found." };

            var course = await _uow.Courses.GetByIdAsync(request.CourseId);
            if (course is null)
                return new AdminCourseLockResultDto { Success = false, Message = $"Course {request.CourseId} not found." };

            var alreadyLocked = await _uow.AdminCourseLocks.IsLockedAsync(user.Id, request.CourseId);
            if (alreadyLocked)
                return new AdminCourseLockResultDto { Success = true, Message = $"Course {course.Code} is already locked for {request.AcademicCode}." };

            await _uow.AdminCourseLocks.AddAsync(new AdminLockEntity
            {
                UserId   = user.Id,
                CourseId = request.CourseId,
                LockedAt = DateTime.UtcNow
            });

            await _uow.SaveChangesAsync();
            return new AdminCourseLockResultDto
            {
                Success = true,
                Message = $"Course {course.Code} locked for student {request.AcademicCode}."
            };
        }
    }
}
