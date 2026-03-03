using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Application.Commands.CourseResults;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Shared.Exceptions;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AYA_UIS.Application.Handlers.StudentResults
{
    public class AddStudentResultsCommandHandler
    : IRequestHandler<AddStudentResultsCommand, Unit>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public AddStudentResultsCommandHandler(
            UserManager<User> userManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(AddStudentResultsCommand request, CancellationToken ct)
        {
            var dto = request.Dto;

            // 1️⃣ Get student
            var student = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Academic_Code == dto.AcademicCode);

            if (student == null)
                throw new NotFoundException("Student not found");

            decimal totalPoints = 0;
            int totalCredits = 0;

            foreach (var item in dto.Results)
            {
                var course = await _unitOfWork.Courses.GetByIdAsync(item.CourseId);
                if (course == null)
                    throw new NotFoundException($"Course {item.CourseId} not found");

                var result = new CourseResult
                {
                    UserId = student.Id,
                    CourseId = course.Id,
                    StudyYearId = dto.StudyYearId,
                    IsPassed = item.IsPassed,
                    Grade = item.Grade
                };

                await _unitOfWork.CourseResults.AddAsync(result);

                if (item.IsPassed)
                {
                    totalPoints += item.Grade * course.Credits;
                    totalCredits += course.Credits;
                }
            }

            // 2️⃣ Calculate GPA
            student.TotalGPA = totalCredits == 0
                ? 0
                : Math.Round(totalPoints / totalCredits / 25, 2); // مثال scale

            // 3️⃣ Allowed Credits
            student.AllowedCredits = student.TotalGPA switch
            {
                < 1 => 12,
                < 2 => 15,
                < 3 => 18,
                _ => 21
            };

            await _unitOfWork.SaveChangesAsync();
            await _userManager.UpdateAsync(student);

            return Unit.Value;
        }
    }
}
