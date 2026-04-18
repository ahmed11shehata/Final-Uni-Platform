using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Dtos.Instructor_Module;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/instructor")]
    [Authorize(Roles = "Instructor")]
    [EnableRateLimiting("PolicyLimitRate")]
    public class InstructorController : ControllerBase
    {
        /// <summary>
        /// GET /api/instructor/courses/{courseId}/students
        /// </summary>
        [HttpGet("courses/{courseId}/students")]
        public async Task<IActionResult> GetCourseStudents(string courseId)
        {
            // TODO: Implement
            return Ok(new { success = true, data = new List<StudentInCourseDto>() });
        }

        /// <summary>
        /// GET /api/instructor/assignments
        /// </summary>
        [HttpGet("assignments")]
        public async Task<IActionResult> GetAssignments([FromQuery] string? courseId)
        {
            // TODO: Implement
            return Ok(new { success = true, data = new List<InstructorAssignmentDto>() });
        }

        /// <summary>
        /// POST /api/instructor/assignments
        /// </summary>
        [HttpPost("assignments")]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateInstructorAssignmentDto dto)
        {
            // TODO: Implement
            return StatusCode(201, new { success = true, data = new InstructorAssignmentDto() });
        }

        /// <summary>
        /// PUT /api/instructor/assignments/{assignmentId}
        /// </summary>
        [HttpPut("assignments/{assignmentId}")]
        public async Task<IActionResult> UpdateAssignment(
            string assignmentId,
            [FromBody] CreateInstructorAssignmentDto dto)
        {
            // TODO: Implement
            return Ok(new { success = true, data = new InstructorAssignmentDto() });
        }

        /// <summary>
        /// DELETE /api/instructor/assignments/{assignmentId}
        /// </summary>
        [HttpDelete("assignments/{assignmentId}")]
        public async Task<IActionResult> DeleteAssignment(string assignmentId)
        {
            // TODO: Implement
            return Ok(new { success = true, message = "Assignment deleted successfully" });
        }

        /// <summary>
        /// GET /api/instructor/courses/{courseId}/materials
        /// </summary>
        [HttpGet("courses/{courseId}/materials")]
        public async Task<IActionResult> GetCourseMaterials(string courseId)
        {
            // TODO: Implement
            return Ok(new { success = true, data = new List<InstructorMaterialDto>() });
        }

        /// <summary>
        /// POST /api/instructor/courses/{courseId}/materials
        /// </summary>
        [HttpPost("courses/{courseId}/materials")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMaterial(
            string courseId,
            [FromForm] CreateMaterialDto dto)
        {
            // TODO: Implement
            return Ok(new { success = true, data = new InstructorMaterialDto() });
        }
    }
}
