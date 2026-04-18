using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Shared.Dtos.Info_Module.CourseDtos;
using Shared.Dtos.Info_Module.RegistrationDtos;

namespace AYA_UIS.Application.Queries.Registrations
{
    public class GetRegisteredCoursesQuery : IRequest<List<RegistrationCourseDto>>
    {
        public string StudentId { get; set; } = string.Empty;

        public GetRegisteredCoursesQuery(string studentId)
        {
            StudentId = studentId;
        }
    }
}