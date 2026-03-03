using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Application.Commands.Courses;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Shared.Exceptions;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AYA_UIS.Application.Handlers.Courses
{
    public class GrantCourseExceptionCommandHandler
    : IRequestHandler<GrantCourseExceptionCommand, Unit>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public GrantCourseExceptionCommandHandler(
            UserManager<User> userManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(
            GrantCourseExceptionCommand request,
            CancellationToken cancellationToken)
        {
            var student = await _userManager.Users
                .FirstOrDefaultAsync(x =>
                    x.Academic_Code ==
                    request.Dto.AcademicCode);

            if (student is null)
                throw new NotFoundException("Student not found.");

            await _unitOfWork
                .GetRepository<StudentCourseException, int>()
                .AddAsync(new StudentCourseException
                {
                    UserId = student.Id,
                    CourseId = request.Dto.CourseId,
                    StudyYearId = request.Dto.StudyYearId,
                    SemesterId = request.Dto.SemesterId
                });

            await _unitOfWork.SaveChangesAsync();
            return Unit.Value;
        }
    }
}
