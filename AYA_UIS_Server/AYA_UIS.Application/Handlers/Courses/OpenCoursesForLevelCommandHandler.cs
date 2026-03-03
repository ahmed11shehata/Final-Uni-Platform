using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Application.Commands.Courses;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Shared.Exceptions;
using Domain.Contracts;
using MediatR;

namespace AYA_UIS.Application.Handlers.Courses
{
    public class OpenCoursesForLevelCommandHandler
     : IRequestHandler<OpenCoursesForLevelCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public OpenCoursesForLevelCommandHandler(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(
            OpenCoursesForLevelCommand request,
            CancellationToken cancellationToken)
        {
            if (!request.Dto.CourseIds.Any())
                throw new BadRequestException("No courses provided.");

            foreach (var courseId in request.Dto.CourseIds)
            {
                var existing =
                    await _unitOfWork.CourseOfferings.GetAsync(
                        courseId,
                        request.Dto.StudyYearId,
                        request.Dto.SemesterId,
                        request.Dto.Level);

                if (existing != null)
                {
                    existing.IsOpen = true;
                }
                else
                {
                    await _unitOfWork.CourseOfferings.AddAsync(
                        new CourseOffering
                        {
                            CourseId = courseId,
                            StudyYearId = request.Dto.StudyYearId,
                            SemesterId = request.Dto.SemesterId,
                            Level = request.Dto.Level,
                            IsOpen = true
                        });
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return Unit.Value;
        }
    }
}
