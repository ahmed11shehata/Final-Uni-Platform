using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
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
    public class QuizzesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public QuizzesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Instructor creates quiz
        [Authorize(Roles = "Instructor , Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateQuiz(CreateQuizDto dto)
        {
            var result = await _mediator.Send(
                new CreateQuizCommand { QuizDto = dto });

            return Ok(result);
        }

        // Instructor adds question
        [Authorize(Roles = "Instructor , Admin")]
        [HttpPost("{quizId}/questions")]
        public async Task<IActionResult> AddQuestion(
            int quizId,
            CreateQuestionDto dto)
        {
            var result = await _mediator.Send(
                new AddQuestionToQuizCommand
                {
                    QuizId = quizId,
                    Question = dto
                });

            return Ok(result);
        }

        // Student submits quiz
        [Authorize(Roles = "Student , Admin")]
        [HttpPost("{quizId}/submit")]
        public async Task<IActionResult> SubmitQuiz(
            int quizId,
            SubmitQuizDto dto)
        {
            var studentId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            dto.QuizId = quizId;

            var result = await _mediator.Send(
                new SubmitQuizCommand
                {
                    Submission = dto,
                    StudentId = studentId
                });

            return Ok(result);
        }


        [Authorize]
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetCourseQuizzes(int courseId)
        {
            var result = await _mediator.Send(
                new GetCourseQuizzesQuery
                {
                    CourseId = courseId
                });

            return Ok(result);
        }


        [Authorize]
        [HttpGet("{quizId}")]
        public async Task<IActionResult> GetQuiz(int quizId)
        {
            var result = await _mediator.Send(
                new GetQuizQuery
                {
                    QuizId = quizId
                });

            return Ok(result);
        }
    }
}
