using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Application.Commands.CourseResults;
using AYA_UIS.Application.Queries.StudentResultsQuery;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.Info_Module.CourseResultDtos;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/results")]
    [Authorize(Roles = "Admin")]
    public class ResultsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ResultsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("batch")]
        public async Task<IActionResult> AddStudentResults(
            [FromBody] AddStudentResultsDto dto)
        {
            await _mediator.Send(new AddStudentResultsCommand(dto));
            return Ok("Results added, GPA updated");
        }
    }
}
