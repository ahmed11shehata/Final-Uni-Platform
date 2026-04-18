using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AYA_UIS.Application.Commands.AcademicSchedules
{
    public class UpdateAcademicScheduleCommand : IRequest<Unit>
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile? File { get; set; }

        public UpdateAcademicScheduleCommand(string title, string description, IFormFile file)
        {
            Title = title;
            Description = description;
            File = file;
        }
    }
}