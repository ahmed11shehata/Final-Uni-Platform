using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Shared.Dtos.Info_Module.QuizDto;
using Shared.Respones;

namespace AYA_UIS.Application.Commands.Quiz
{
    public class SubmitQuizCommand : IRequest<Response<int>>
    {
        public SubmitQuizDto Submission { get; set; }

        public string? StudentId { get; set; } = string.Empty;
    }
}
