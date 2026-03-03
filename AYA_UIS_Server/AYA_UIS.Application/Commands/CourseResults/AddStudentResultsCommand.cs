using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Shared.Dtos.Info_Module.CourseResultDtos;

namespace AYA_UIS.Application.Commands.CourseResults
{
    public record AddStudentResultsCommand(
     AddStudentResultsDto Dto
    ) : IRequest<Unit>;
}
