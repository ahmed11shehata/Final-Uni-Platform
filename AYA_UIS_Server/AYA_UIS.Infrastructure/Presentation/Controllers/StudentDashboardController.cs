using System.Security.Claims;
using AYA_UIS.Application.Queries.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/student")]
    [Authorize(Roles = "Student,Admin")]
    public class StudentDashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentDashboardController(IMediator mediator) => _mediator = mediator;

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var result = await _mediator.Send(new GetStudentDashboardQuery(CurrentUserId));
            return Ok(new { success = true, data = result });
        }

        [HttpGet("schedule")]
        public IActionResult GetSchedule()
        {
            return Ok(new { success = true, data = new {
                sessions = Array.Empty<object>(),
                midtermExams = Array.Empty<object>(),
                finalExams = Array.Empty<object>()
            }});
        }

        [HttpGet("timetable")]
        public async Task<IActionResult> GetTimetable()
        {
            var result = await _mediator.Send(new GetStudentTimetableQuery(CurrentUserId));
            return Ok(new { success = true, data = result });
        }
    }
}
