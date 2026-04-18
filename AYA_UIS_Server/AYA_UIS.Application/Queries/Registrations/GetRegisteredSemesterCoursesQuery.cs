using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Shared.Dtos.Info_Module.RegistrationDtos;

namespace AYA_UIS.Application.Queries.Registrations
{
    public class GetRegisteredSemesterCoursesQuery : IRequest<List<RegistrationCourseDto>>
    {
        public int StudyYearId { get; set; }
        public int SemesterId { get; set; }
        public string StudentId { get; set; } = string.Empty;

        public GetRegisteredSemesterCoursesQuery(int studyYearId, int semesterId, string studentId)
        {
            StudyYearId = studyYearId;
            SemesterId = semesterId;
            StudentId = studentId;
        }
    }
}