using System.Security.Claims;
using AYA_UIS.Application.Commands.Quiz;
using AYA_UIS.Application.Queries.Quiz;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.Info_Module.QuizDto;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/quizzes")]
    [Authorize]
    public class QuizzesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public QuizzesController(IMediator mediator)
            => _mediator = mediator;

        private string? CurrentUserId
            => User.FindFirstValue(ClaimTypes.NameIdentifier);

        /// <summary>Get all quizzes for a course (any authenticated user).</summary>
        [HttpGet("course/{courseId:int}")]
        public async Task<IActionResult> GetCourseQuizzes(int courseId)
        {
            if (courseId <= 0)
                return BadRequest(new { errors = "Invalid course ID." });

            var result = await _mediator.Send(new GetCourseQuizzesQuery { CourseId = courseId });
            return Ok(result);
        }

        /// <summary>Get a specific quiz with questions.</summary>
        [HttpGet("{quizId:int}")]
        public async Task<IActionResult> GetQuiz(int quizId)
        {
            if (quizId <= 0)
                return BadRequest(new { errors = "Invalid quiz ID." });

            var result = await _mediator.Send(new GetQuizQuery
            {
                QuizId    = quizId,
                StudentId = CurrentUserId
            });
            if (result is null)
                return NotFound(new { errors = "Quiz not found." });
            return Ok(result);
        }

        /// <summary>Submit quiz answers. Students and Admins only.</summary>
        [HttpPost("{quizId:int}/submit")]
        [Authorize(Roles = "Student,Admin")]
        public async Task<IActionResult> SubmitQuiz(int quizId, [FromBody] SubmitQuizDto dto)
        {
            if (quizId <= 0)
                return BadRequest(new { errors = "Invalid quiz ID." });

            if (dto is null)
                return BadRequest(new { errors = "Request body is required." });

            if (string.IsNullOrWhiteSpace(CurrentUserId))
                return Unauthorized(new { errors = "User identity not found." });

            dto.QuizId = quizId;
            var result = await _mediator.Send(new SubmitQuizCommand
            {
                Submission = dto,
                StudentId  = CurrentUserId
            });
            return Ok(result);
        }

        /// <summary>Create a new quiz. Instructors and Admins only.</summary>
        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage) });

            if (dto is null)
                return BadRequest(new { errors = "Request body is required." });

            var result = await _mediator.Send(new CreateQuizCommand { QuizDto = dto });
            return Ok(result);
        }

        /// <summary>Add a question to an existing quiz. Instructors and Admins only.</summary>
        [HttpPost("{quizId:int}/questions")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> AddQuestion(int quizId, [FromBody] CreateQuestionDto dto)
        {
            if (quizId <= 0)
                return BadRequest(new { errors = "Invalid quiz ID." });

            if (dto is null)
                return BadRequest(new { errors = "Question data is required." });

            if (string.IsNullOrWhiteSpace(dto.QuestionText))
                return BadRequest(new { errors = "Question text is required." });

            var result = await _mediator.Send(new AddQuestionToQuizCommand
            {
                QuizId   = quizId,
                Question = dto
            });
            return Ok(result);
        }

        /// <summary>Get all student attempts for a quiz. Instructors and Admins only.</summary>
        [HttpGet("{quizId:int}/attempts")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> GetAttempts(int quizId)
        {
            if (quizId <= 0)
                return BadRequest(new { errors = "Invalid quiz ID." });

            var result = await _mediator.Send(new GetQuizAttemptsQuery(quizId));
            return Ok(result);
        }
    }
}

