using AYA_UIS.Application.Queries.Dashboard;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Enums;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Info_Module.DashboardDtos;

namespace AYA_UIS.Application.Handlers.Dashboard
{
    public class GetStudentTranscriptQueryHandler
        : IRequestHandler<GetStudentTranscriptQuery, StudentTranscriptDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public GetStudentTranscriptQueryHandler(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork  = unitOfWork;
            _userManager = userManager;
        }

        public async Task<StudentTranscriptDto> Handle(GetStudentTranscriptQuery request, CancellationToken ct)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
            if (user == null) return new StudentTranscriptDto();

            var depts = await _unitOfWork.Departments.GetAllAsync();
            var dept  = user.DepartmentId.HasValue
                ? depts.FirstOrDefault(d => d.Id == user.DepartmentId.Value) : null;

            var userStudyYears = await _unitOfWork.UserStudyYears.GetStudyYearsByUserIdAsync(request.UserId);
            var regs           = await _unitOfWork.Registrations.GetByUserAsync(request.UserId);

            var yearDtos  = new List<TranscriptYearDto>();
            int yearNum   = 0;

            foreach (var usy in userStudyYears.OrderBy(u => u.StudyYear?.StartYear))
            {
                yearNum++;
                var semesters = await _unitOfWork.Semesters.GetByStudyYearIdAsync(usy.StudyYearId);
                var semDtos   = new List<TranscriptSemesterDto>();

                foreach (var sem in semesters.OrderBy(s => s.Title))
                {
                    var semRegs = regs.Where(r => r.StudyYearId == usy.StudyYearId && r.SemesterId == sem.Id).ToList();
                    if (!semRegs.Any()) continue;

                    var courseDtos = semRegs.Select(r =>
                    {
                        var (grade, pts) = GradeFromGrads(r.Grade);
                        return new TranscriptCourseDto
                        {
                            Code    = r.Course?.Code ?? "",
                            Name    = r.Course?.Name ?? "",
                            Credits = r.Course?.Credits ?? 0,
                            Grade   = grade,
                            Points  = pts,
                            Total   = r.Grade.HasValue ? $"{(int)r.Grade.Value * 10}/100" : "—"
                        };
                    }).ToList();

                    var graded  = semRegs.Where(r => r.Grade.HasValue).ToList();
                    decimal gpa = graded.Any()
                        ? Math.Round(graded.Average(r => (decimal)r.Grade!.Value * 10 / 100 * 4), 2)
                        : 0;
                    bool isCurrent = usy.StudyYear?.IsCurrent == true && sem.EndDate >= DateTime.UtcNow;

                    semDtos.Add(new TranscriptSemesterDto
                    {
                        Id           = $"y{yearNum}s{(int)sem.Title + 1}",
                        Label        = sem.Title == SemesterEnum.First_Semester ? "Semester 1" : "Semester 2",
                        Period       = $"{(sem.Title == SemesterEnum.First_Semester ? "Fall" : "Spring")} {sem.StartDate.Year}",
                        IsCurrent    = isCurrent,
                        TotalCredits = semRegs.Sum(r => r.Course?.Credits ?? 0),
                        SemesterGpa  = isCurrent ? null : gpa,
                        Courses      = courseDtos
                    });
                }

                yearDtos.Add(new TranscriptYearDto
                {
                    Year      = yearNum,
                    Label     = usy.Level.ToString().Replace("_", " "),
                    Semesters = semDtos
                });
            }

            return new StudentTranscriptDto
            {
                Student = new TranscriptStudentDto
                {
                    Name            = user.DisplayName,
                    Id              = user.Academic_Code,
                    Department      = dept?.Name ?? "Computer Science",
                    CurrentYear     = yearNum,
                    CurrentSemester = 1
                },
                Years = yearDtos
            };
        }

        private static (string grade, decimal points) GradeFromGrads(Grads? g) =>
            g switch
            {
                Grads.A_Plus  => ("A+", 4.0m),
                Grads.A       => ("A",  4.0m),
                Grads.A_Minus => ("A-", 3.7m),
                Grads.B_Plus  => ("B+", 3.3m),
                Grads.B       => ("B",  3.0m),
                Grads.B_Minus => ("B-", 2.7m),
                Grads.C_Plus  => ("C+", 2.3m),
                Grads.C       => ("C",  2.0m),
                Grads.C_Minus => ("C-", 1.7m),
                Grads.D_Plus  => ("D+", 1.3m),
                Grads.D       => ("D",  1.0m),
                _             => ("F",  0.0m)
            };
    }
}
