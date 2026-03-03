using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AYA_UIS.Application.Commands.UserStudyYears;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions;


namespace AYA_UIS.Application.Handlers.UserStudyYears
{


    public class PromoteAllStudentsCommandHandler : IRequestHandler<PromoteAllStudentsCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public PromoteAllStudentsCommandHandler(
            IUnitOfWork unitOfWork,
            UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<Unit> Handle(PromoteAllStudentsCommand request, CancellationToken cancellationToken)
        {


            var currentStudyYear = await _unitOfWork.StudyYears.GetCurrentStudyYearAsync();
            if (currentStudyYear == null)
                throw new PromotionException("Current study year not found");

            var alreadyPromoted = await _unitOfWork.UserStudyYears
                .ExistsAsync(x => x.StudyYearId == currentStudyYear.Id);

            if (alreadyPromoted)
                throw new PromotionException("Students already promoted for this study year");

            var students = await _userManager.Users
                .Where(u => u.Level != Levels.Graduate)
                .ToListAsync(cancellationToken);

            if (!students.Any())
                throw new PromotionException("No students to promote");

            var newUserStudyYears = new List<UserStudyYear>();

            foreach (var student in students)
            {
                if (student.TotalCredits >= 133)
                {
                    student.Level = Levels.Graduate;
                    continue; 
                }

                if (student.Level != null && student.Level < Levels.Fourth_Year)
                    student.Level += 1;

                var activeRecords = await _unitOfWork.UserStudyYears
                    .GetWhereAsync(x => x.UserId == student.Id && x.IsActive);

                foreach (var record in activeRecords)
                    record.IsActive = false;

                newUserStudyYears.Add(new UserStudyYear
                {
                    UserId = student.Id,
                    StudyYearId = currentStudyYear.Id,
                    Level = student.Level.GetValueOrDefault(),
                    IsActive = true,
                    EnrolledAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.UserStudyYears.AddRangeAsync(newUserStudyYears);
            await _unitOfWork.SaveChangesAsync();

            return Unit.Value;
        }
    }


    //public class PromoteAllStudentsCommandHandler : IRequestHandler<PromoteAllStudentsCommand, Unit>
    //{
    //    private readonly IUnitOfWork _unitOfWork;
    //    private readonly UserManager<User> _userManager;

    //    public PromoteAllStudentsCommandHandler(IUnitOfWork unitOfWork, UserManager<User> userManager)
    //    {
    //        _unitOfWork = unitOfWork;
    //        _userManager = userManager;
    //    }

    //    //public async Task<Unit> Handle(PromoteAllStudentsCommand request, CancellationToken cancellationToken)
    //    //{
    //    //    var allUngraduatedStudents = await _userManager.Users
    //    //        .Where(u => u.Level != Levels.Graduate)
    //    //        .ToListAsync();

    //    //    if (allUngraduatedStudents.Count == 0)
    //    //        throw new Exception("No ungraduated students found");

    //    //    var currentStudyYear = await _unitOfWork.StudyYears.GetCurrentStudyYearAsync();
    //    //    if (currentStudyYear == null)
    //    //        throw new Exception("Current study year not found");

    //    //    var usersStudyYears = new List<UserStudyYear>();
    //    //    foreach (var student in allUngraduatedStudents)
    //    //    {
    //    //        // Re-assign student to the current study year keeping their existing level
    //    //        usersStudyYears.Add(new UserStudyYear
    //    //        {
    //    //            UserId = student.Id,
    //    //            StudyYearId = currentStudyYear.Id,
    //    //            Level = student.Level.GetValueOrDefault(),
    //    //            EnrolledAt = DateTime.UtcNow
    //    //        });
    //    //    }

    //    //    await _unitOfWork.UserStudyYears.AddRangeAsync(usersStudyYears);
    //    //    await _unitOfWork.SaveChangesAsync();

    //    //    return Unit.Value;
    //    //}



    //}
}