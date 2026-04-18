using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AYA_UIS.Application.Commands.Semesters;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain.Contracts;
using MediatR;

namespace AYA_UIS.Application.Handlers.Semesters
{
    public class CreateSemesterCommandHandler : IRequestHandler<CreateSemesterCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateSemesterCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(CreateSemesterCommand request, CancellationToken cancellationToken)
        {
            // Map the CreateSemesterDto to the Semester entity
            var semester = new Semester
            {
                Title = request.SemesterDto.Title,
                StartDate = request.SemesterDto.StartDate,
                EndDate = request.SemesterDto.EndDate,
                StudyYearId = request.StudyYearId
            };

            // Add the new Semester to the repository
            await _unitOfWork.Semesters.AddAsync(semester);

            // Save changes to the database
            await _unitOfWork.SaveChangesAsync();

            // Return the ID of the newly created Semester
            return semester.Id;
        }
    }
}