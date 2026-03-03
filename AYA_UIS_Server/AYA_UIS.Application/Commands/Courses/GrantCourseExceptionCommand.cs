using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Shared.Dtos.Info_Module.CourseDtos;

namespace AYA_UIS.Application.Commands.Courses
{
    public record GrantCourseExceptionCommand(
    GrantCourseExceptionDto Dto)
    : IRequest<Unit>;
}
