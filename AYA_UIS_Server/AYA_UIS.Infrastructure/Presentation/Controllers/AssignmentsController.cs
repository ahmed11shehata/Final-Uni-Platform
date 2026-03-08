using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AYA_UIS.Application.Commands.Assignment;
using AYA_UIS.Application.Commands.CreateAssignment;
using AYA_UIS.Application.Queries.Assignments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.Info_Module.AssignmentDto;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssignmentsController : ControllerBase
    {


        private readonly IMediator _mediator;

        public AssignmentsController(IMediator mediator)
        {
            _mediator = mediator;
        }





        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateAssignment(
            int courseId,
            [FromForm] string title,
            [FromForm] string description,
            [FromForm] int points,
            [FromForm] DateTime deadline,
            IFormFile file)
        {
            var instructorId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            var command = new CreateAssignmentCommand
            {
                AssignmentDto = new CreateAssignmentDto
                {
                    Title = title,
                    Description = description,
                    Points = points,
                    Deadline = deadline,
                    CourseId = courseId
                },
                File = file,
                InstructorId = instructorId
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }


        [HttpGet("courses/{courseId}/assignments")]
        public async Task<IActionResult> GetAssignments(int courseId)
        {
            var result = await _mediator.Send(
                new GetAssignmentsByCourseQuery
                {
                    CourseId = courseId
                });

            return Ok(result);
        }





        [Authorize(Roles = "Student,Admin")]
        [HttpPost("{assignmentId}/submit")]
        public async Task<IActionResult> SubmitAssignment(
    int assignmentId,
    IFormFile file)
        {
            var academic_Code =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            var command = new SubmitAssignmentCommand
            {
                AssignmentId = assignmentId,
                Academic_Code = academic_Code,
                File = file
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }


        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("{assignmentId}/submissions")]
        public async Task<IActionResult> GetSubmissions(int assignmentId)
        {
            var result = await _mediator.Send(
                new GetAssignmentSubmissionsQuery
                {
                    AssignmentId = assignmentId
                });

            return Ok(result);
        }


        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost("grade")]
        public async Task<IActionResult> GradeSubmission(
    GradeSubmissionCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(result);
        }



        [Authorize(Roles = "Student,Admin")]
        [HttpGet("courses/{courseId}/my-grades")]
        public async Task<IActionResult> GetMyGrades(int courseId)
        {
            var studentId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _mediator.Send(
                new GetStudentAssignmentGradesQuery
                {
                    CourseId = courseId,
                    StudentId = studentId
                });

            return Ok(result);
        }
    }
}
