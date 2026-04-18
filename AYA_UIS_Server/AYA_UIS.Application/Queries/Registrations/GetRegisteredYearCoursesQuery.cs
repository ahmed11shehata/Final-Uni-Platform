using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AYA_UIS.Core.Domain.Entities.Models;
using MediatR;
using Shared.Dtos.Info_Module.RegistrationDtos;

namespace AYA_UIS.Application.Queries.Registrations
{
    public class GetRegisteredYearCoursesQuery : IRequest<List<RegistrationCourseDto>>
    {
        public string StudentId { get; set; } = string.Empty;
        public int Year { get; set; }

        public GetRegisteredYearCoursesQuery(string studentId, int year)
        {
            StudentId = studentId;
            Year = year;
        }
    }
}