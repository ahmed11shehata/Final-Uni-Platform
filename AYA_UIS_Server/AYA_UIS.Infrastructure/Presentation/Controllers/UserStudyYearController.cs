using System.Security.Claims;
using AYA_UIS.Application.Commands.UserStudyYears;
using AYA_UIS.Application.Queries.UserStudyYears;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.Info_Module.UserStudyYearDtos;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserStudyYearController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserStudyYearController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Assign a study year to a user (Admin/Instructor only).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> CreateUserStudyYear(CreateUserStudyYearDto dto)
        {
            var command = new CreateUserStudyYearCommand(dto);
            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Update a user's study year record (e.g., promote level, set current).
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> UpdateUserStudyYear(int id, UpdateUserStudyYearDto dto)
        {
            var command = new UpdateUserStudyYearCommand(id, dto);
            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get all study years for the currently authenticated user.
        /// </summary>
        [HttpGet("my-study-years")]
        public async Task<IActionResult> GetMyStudyYears()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var query = new GetUserStudyYearsQuery(userId);
            var result = await _mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get the full study year timeline for the authenticated user (from enrollment to graduation).
        /// </summary>
        [HttpGet("my-timeline")]
        public async Task<IActionResult> GetMyTimeline()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var query = new GetUserStudyYearTimelineQuery(userId);
            var result = await _mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get the current active study year for the authenticated user.
        /// </summary>
        [HttpGet("my-current")]
        public async Task<IActionResult> GetMyCurrentStudyYear()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var query = new GetCurrentUserStudyYearQuery(userId);
            var result = await _mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get all study years for a specific user (Admin/Instructor only).
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetUserStudyYears(string userId)
        {
            var query = new GetUserStudyYearsQuery(userId);
            var result = await _mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get timeline for a specific user (Admin/Instructor only).
        /// </summary>
        [HttpGet("user/{userId}/timeline")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetUserTimeline(string userId)
        {
            var query = new GetUserStudyYearTimelineQuery(userId);
            var result = await _mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // Assign all user that are students that are not graduated yet from first year to second year, from second year to third year, and from third year to fourth year (Admin only).
        [Authorize(Roles = "Admin")]
        [HttpPost("promote-all")]
        public async Task<IActionResult> PromoteAllStudents()
        {
            var command = new PromoteAllStudentsCommand();
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        


    }
}
