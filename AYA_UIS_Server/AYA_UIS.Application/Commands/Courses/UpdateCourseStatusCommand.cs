using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Enums;
using MediatR;

namespace AYA_UIS.Application.Commands.Courses
{

    public record UpdateCourseStatusCommand(
     int CourseId,
     CourseStatus Status
 ) : IRequest<Unit>;
}
