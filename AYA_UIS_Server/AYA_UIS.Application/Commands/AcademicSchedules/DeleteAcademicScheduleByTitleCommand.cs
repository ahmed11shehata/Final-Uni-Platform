using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace AYA_UIS.Application.Commands.AcademicSchedules
{
    public class DeleteAcademicScheduleByTitleCommand : IRequest<bool>
    {
        public string ScheduleTitle { get; set; } = string.Empty;

        public DeleteAcademicScheduleByTitleCommand(string scheduleTitle)
        {
            ScheduleTitle = scheduleTitle;
        }
    }
}