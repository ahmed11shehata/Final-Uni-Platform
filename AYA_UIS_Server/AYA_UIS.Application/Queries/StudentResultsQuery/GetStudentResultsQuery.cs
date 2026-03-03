using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Shared.Dtos.Info_Module.CourseResultDtos;

namespace AYA_UIS.Application.Queries.StudentResultsQuery
{
    public record GetStudentResultsQuery(string AcademicCode)
    : IRequest<List<StudentCourseResultDto>>;
}
