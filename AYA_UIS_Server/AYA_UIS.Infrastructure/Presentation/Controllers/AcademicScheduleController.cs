using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Application.Commands.AcademicSchedules;
using AYA_UIS.Application.Queries.AcademicSchedules;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.Info_Module.AcademicSheduleDtos;

namespace Presentation.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AcademicScheduleController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AcademicScheduleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllAcademicSchedulesQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetAcademicScheduleByIdQuery(id));
            return Ok(result);
        }

        [HttpGet("by-title/{title}")]
        public async Task<IActionResult> GetByTitle([FromRoute]string title)
        {
            var result = await _mediator.Send(new GetAcademicScheduleByTitleQuery(title));
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("study-year/{studyYearId}/department/{departmentId}/semester/{semesterId}")]
        public async Task<IActionResult> Create(int studyYearId, int departmentId, int semesterId, CreateSemesterAcademicScheduleDto createAcademicScheduleDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))                
                return Unauthorized();
            await _mediator.Send(new CreateSemesterAcademicScheduleCommand(userId, studyYearId, departmentId, semesterId, createAcademicScheduleDto));
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(UpdateAcademicScheduleCommand command)
        {

            await _mediator.Send(command);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute]DeleteAcademicScheduleByIdCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("semester/{semesterId}")]
        public async Task<IActionResult> GetSemesterAcademicSchedules(int semesterId)
        {
            var result = await _mediator.Send(new GetAcademicSchedulesBySemesterIdQuery(semesterId));
            return Ok(result);
        }
    }
}
