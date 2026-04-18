using AYA_UIS.Application.Commands.AdminCourseLock;
using AYA_UIS.Core.Domain.Entities.Identity;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Info_Module.AdminCourseLockDtos;

namespace AYA_UIS.Application.Handlers.AdminCourseLock
{
    public class UnlockCourseCommandHandler
        : IRequestHandler<UnlockCourseCommand, AdminCourseLockResultDto>
    {
        private readonly IUnitOfWork      _uow;
        private readonly UserManager<User> _userManager;

        public UnlockCourseCommandHandler(
            IUnitOfWork      uow,
            UserManager<User> userManager)
        {
            _uow         = uow;
            _userManager = userManager;
        }

        public async Task<AdminCourseLockResultDto> Handle(
            UnlockCourseCommand request,
            CancellationToken   ct)
        {
            if (string.IsNullOrWhiteSpace(request.AcademicCode))
                return new AdminCourseLockResultDto { Success = false, Message = "Academic code is required." };

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Academic_Code == request.AcademicCode, ct);

            if (user is null)
                return new AdminCourseLockResultDto { Success = false, Message = $"Student '{request.AcademicCode}' not found." };

            var lockEntry = await _uow.AdminCourseLocks.GetAsync(user.Id, request.CourseId);
            if (lockEntry is null)
                return new AdminCourseLockResultDto { Success = true, Message = "Course was not locked." };

            await _uow.AdminCourseLocks.RemoveAsync(lockEntry);
            await _uow.SaveChangesAsync();
            return new AdminCourseLockResultDto { Success = true, Message = "Course unlocked." };
        }
    }
}
