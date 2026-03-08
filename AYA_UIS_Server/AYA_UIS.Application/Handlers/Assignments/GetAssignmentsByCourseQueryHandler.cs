using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AYA_UIS.Application.Queries.Assignments;
using Domain.Contracts;
using MediatR;
using Shared.Dtos.Info_Module.AssignmentDto;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Assignments
{
    public class GetAssignmentsByCourseQueryHandler
 : IRequestHandler<GetAssignmentsByCourseQuery,
     Response<IEnumerable<AssignmentDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAssignmentsByCourseQueryHandler(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<IEnumerable<AssignmentDto>>> Handle(
            GetAssignmentsByCourseQuery request,
            CancellationToken cancellationToken)
        {
            var assignments = await _unitOfWork.Assignments
                .GetAssignmentsByCourseIdAsync(request.CourseId);
            var result = assignments.Select(a => new AssignmentDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Points = a.Points,
                Deadline = a.Deadline,
                FileUrl = a.FileUrl,
                CourseId = a.CourseId,
                CourseName = a.Course?.Name,
                InstructorName = a.CreatedBy?.UserName
            }).ToList();

            return Response<IEnumerable<AssignmentDto>>
                .SuccessResponse(result);
        }
    }
}
