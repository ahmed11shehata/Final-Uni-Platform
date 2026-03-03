using System.Security.Claims;
using AYA_UIS.Application.Commands.Courses;
using AYA_UIS.Application.Commands.CourseUploads;
using AYA_UIS.Application.Queries.CoursePrequisites;
using AYA_UIS.Application.Queries.Courses;
using AYA_UIS.Application.Queries.Registrations;
using AYA_UIS.Core.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.Info_Module.CourseDtos;
using Shared.Dtos.Info_Module.CourseUploadDtos;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CourseController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [Authorize(Roles = "Admin")]
        [HttpPatch("{courseId}/status")]
        public async Task<IActionResult> UpdateStatus(int courseId,[FromQuery] CourseStatus status)
        {
            await _mediator.Send(
                new UpdateCourseStatusCommand(courseId, status));

            return NoContent();
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add(CreateCourseDto courseDto)
        {
            var result = await _mediator.Send(
                new CreateCourseCommand(courseDto));

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(
                new GetAllCoursesQuery());

            return Ok(result);
        }

        [HttpGet("{id}/uploads")]
        public async Task<IActionResult> GetCourseUploads(int id)
        {
            var result = await _mediator.Send(
                new GetCourseUploadsQuery(id));

            return Ok(result);
        }

        [HttpGet("{id}/registrations/{yearId}")]
        public async Task<IActionResult> GetCourseYearRegistrations(
            int id,
            int yearId)
        {
            var result = await _mediator.Send(
                new GetCourseYearRegistrationsQuery(id, yearId));

            return Ok(result);
        }

        [Authorize]
        [HttpPost("{courseId}/upload")]
        public async Task<IActionResult> UploadCourseFile(
            int courseId,
            [FromForm] string title,
            [FromForm] string description,
            [FromForm] string type,
            IFormFile file)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var command = new CreateCourseUploadCommand
            {
                CourseUploadDto = new CreateCourseUploadDto
                {
                    Title = title,
                    Description = description,
                    Type = type,
                    CourseId = courseId
                },
                File = file,
                UserId = userId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> DeparmentCourses(int departmentId)
        {
            var result = await _mediator.Send(
                new GetDepartmentCoursesQuery(departmentId));

            return Ok(result);
        }

        [Authorize]
        [HttpGet("prequisites/{courseId}")]
        public async Task<IActionResult> GetCoursePrequisites(int courseId)
        {
            var result = await _mediator.Send(
                new GetCoursePrequisitesQuery(courseId));

            return Ok(result);
        }

        [Authorize]
        [HttpGet("dependencies/{courseId}")]
        public async Task<IActionResult> GetCourseDependencies(int courseId)
        {
            var result = await _mediator.Send(
                new GetCourseDependenciesQuery(courseId));

            return Ok(result);
        }

        [Authorize]
        [HttpGet("open/department/{departmentId}")]
        public async Task<IActionResult> GetDepartmentOpenCourses(
            int departmentId)
        {
            var result = await _mediator.Send(
                new GetDepartmentOpenCoursesQuery(departmentId));

            return Ok(result);
        }
    }
}