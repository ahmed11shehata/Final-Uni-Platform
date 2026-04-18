using AYA_UIS.Application.Commands.AdminCourseLock;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/admin/course-lock")]
    [Authorize(Roles = "Admin")]
    public class AdminCourseLockController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminCourseLockController(IMediator mediator)
            => _mediator = mediator;

        /// <summary>Prevent a student from registering a specific course.</summary>
        [HttpPost("{academicCode}/course/{courseId:int}")]
        public async Task<IActionResult> Lock(string academicCode, int courseId)
        {
            if (string.IsNullOrWhiteSpace(academicCode))
                return BadRequest(new { errors = "Academic code is required." });

            if (courseId <= 0)
                return BadRequest(new { errors = "Invalid course ID." });

            var result = await _mediator.Send(new LockCourseCommand(academicCode, courseId));
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Remove the lock from a student's course.</summary>
        [HttpDelete("{academicCode}/course/{courseId:int}")]
        public async Task<IActionResult> Unlock(string academicCode, int courseId)
        {
            if (string.IsNullOrWhiteSpace(academicCode))
                return BadRequest(new { errors = "Academic code is required." });

            if (courseId <= 0)
                return BadRequest(new { errors = "Invalid course ID." });

            var result = await _mediator.Send(new UnlockCourseCommand(academicCode, courseId));
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
