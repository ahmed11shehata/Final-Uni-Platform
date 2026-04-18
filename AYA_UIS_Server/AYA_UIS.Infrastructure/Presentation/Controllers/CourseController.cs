using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using AYA_UIS.Application.Commands.Courses;
using AYA_UIS.Application.Commands.CourseUploads;
using AYA_UIS.Application.Queries.CoursePrequisites;
using AYA_UIS.Application.Queries.Courses;
using AYA_UIS.Application.Queries.Dashboard;
using AYA_UIS.Core.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

        public CourseController(IMediator mediator) => _mediator = mediator;

        private string? CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        // ── GET /api/Course ───────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllCoursesQuery());
            return Ok(result);
        }

        // ── GET /api/Course/open/department/{id} ──────────────────────────
        [Authorize]
        [HttpGet("open/department/{departmentId}")]
        public async Task<IActionResult> GetDepartmentOpenCourses(int departmentId)
        {
            var result = await _mediator.Send(new GetDepartmentOpenCoursesQuery(departmentId));
            return Ok(result);
        }

        // ── GET /api/Course/department/{id} ───────────────────────────────
        [Authorize]
        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> DepartmentCourses(int departmentId)
        {
            var result = await _mediator.Send(new GetDepartmentCoursesQuery(departmentId));
            return Ok(result);
        }

        // ── GET /api/Course/{courseId}/uploads ────────────────────────────
        [Authorize]
        [HttpGet("{courseId}/uploads")]
        public async Task<IActionResult> GetCourseUploads(int courseId)
        {
            var result = await _mediator.Send(new GetCourseUploadsQuery(courseId));
            return Ok(result);
        }

        // ── GET /api/Course/{id}/registrations/{yearId} ───────────────────
        [HttpGet("{id}/registrations/{yearId}")]
        public async Task<IActionResult> GetCourseYearRegistrations(int id, int yearId)
        {
            var result = await _mediator.Send(new GetCourseYearRegistrationsQuery(id, yearId));
            return Ok(result);
        }

        // ── POST /api/Course/open-level ───────────────────────────────────
        [Authorize(Roles = "Admin")]
        [HttpPost("open-level")]
        public async Task<IActionResult> OpenCoursesForLevel([FromBody] OpenCoursesForLevelDto dto)
        {
            await _mediator.Send(new OpenCoursesForLevelCommand(dto));
            return Ok("Courses opened successfully.");
        }

        // ── POST /api/Course/grant-exception ─────────────────────────────
        [Authorize(Roles = "Admin")]
        [HttpPost("grant-exception")]
        public async Task<IActionResult> GrantException(GrantCourseExceptionDto dto)
        {
            await _mediator.Send(new GrantCourseExceptionCommand(dto));
            return Ok("Exception granted.");
        }

        // ── POST /api/Course ──────────────────────────────────────────────
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add(CreateCourseDto courseDto)
        {
            var result = await _mediator.Send(new CreateCourseCommand(courseDto));
            return Ok(result);
        }

        // ── PATCH /api/Course/{courseId}/status ───────────────────────────
        [Authorize(Roles = "Admin")]
        [HttpPatch("{courseId}/status")]
        public async Task<IActionResult> UpdateStatus(int courseId, [FromQuery] CourseStatus status)
        {
            await _mediator.Send(new UpdateCourseStatusCommand(courseId, status));
            return NoContent();
        }

        // ── POST /api/Course/{courseId}/upload ────────────────────────────
        [Authorize]
        [HttpPost("{courseId}/upload")]
        public async Task<IActionResult> UploadCourseFile(
            int courseId,
            [FromForm] string title,
            [FromForm] string description,
            [FromForm] UploadType type,
            IFormFile file)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var command = new CreateCourseUploadCommand
            {
                CourseUploadDto = new CreateCourseUploadDto
                {
                    Title       = title,
                    Description = description,
                    Type        = type,
                    CourseId    = courseId
                },
                File   = file,
                UserId = userId
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // ── GET /api/Course/prequisites/{courseId} ────────────────────────
        [Authorize]
        [HttpGet("prequisites/{courseId}")]
        public async Task<IActionResult> GetCoursePrequisites(int courseId)
        {
            var result = await _mediator.Send(new GetCoursePrequisitesQuery(courseId));
            return Ok(result);
        }

        // ── GET /api/Course/dependencies/{courseId} ───────────────────────
        [Authorize]
        [HttpGet("dependencies/{courseId}")]
        public async Task<IActionResult> GetCourseDependencies(int courseId)
        {
            var result = await _mediator.Send(new GetCourseDependenciesQuery(courseId));
            return Ok(result);
        }
    }
}
