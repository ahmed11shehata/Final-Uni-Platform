using System.Security.Claims;
using AYA_UIS.Application.Queries.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/instructor")]
    [Authorize(Roles = "Instructor,Admin")]
    public class InstructorDashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InstructorDashboardController(IMediator mediator) => _mediator = mediator;

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var result = await _mediator.Send(new GetInstructorDashboardQuery(CurrentUserId));
            return Ok(new { success = true, data = result });
        }

        [HttpGet("courses")]
        public async Task<IActionResult> GetCourses()
        {
            var dash = await _mediator.Send(new GetInstructorDashboardQuery(CurrentUserId));
            return Ok(new { success = true, data = dash.Courses });
        }

        [HttpGet("grades/{courseId}")]
        public IActionResult GetGrades(int courseId)
        {
            return Ok(new { success = true, data = new {
                submissions = Array.Empty<object>(),
                students = Array.Empty<object>(),
                examSchedule = new {
                    midterm = new { date = (string?)null, published = false },
                    final_ = new { date = (string?)null, published = false }
                },
                studentMeta = new Dictionary<string, object>()
            }});
        }
    }
}
