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
    public class CreateQuizCommand : IRequest<Response<int>>
    {
        public CreateQuizDto QuizDto { get; set; }
    }
}
