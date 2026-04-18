using AYA_UIS.Application.Queries.Dashboard;
using AYA_UIS.Core.Domain.Entities.Identity;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Info_Module.DashboardDtos;

namespace AYA_UIS.Application.Handlers.Dashboard
{
    public class GetAdminUsersQueryHandler
        : IRequestHandler<GetAdminUsersQuery, IEnumerable<AdminUserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public GetAdminUsersQueryHandler(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork  = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IEnumerable<AdminUserDto>> Handle(GetAdminUsersQuery request, CancellationToken ct)
        {
            var allUsers = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.Trim().ToLower();
                allUsers = allUsers.Where(u =>
                    u.Academic_Code.ToLower().Contains(s) ||
                    u.DisplayName.ToLower().Contains(s) ||
                    (u.Email != null && u.Email.ToLower().Contains(s)));
            }

            var users  = await allUsers.ToListAsync(ct);
            var result = new List<AdminUserDto>();
            var depts  = await _unitOfWork.Departments.GetAllAsync();
            var deptMap = depts.ToDictionary(d => d.Id, d => d.Name);

            foreach (var u in users)
            {
                var roles    = await _userManager.GetRolesAsync(u);
                var userRole = roles.FirstOrDefault() ?? "Student";

                if (!string.IsNullOrWhiteSpace(request.Role) &&
                    !string.Equals(userRole, request.Role, StringComparison.OrdinalIgnoreCase))
                    continue;

                var regs = await _unitOfWork.Registrations.GetByUserAsync(u.Id);
                var registered = regs.Where(r => !r.IsPassed).Select(r => r.Course?.Code ?? "").Where(c => c != "").ToList();
                var completed  = regs.Where(r => r.IsPassed).Select(r => new CompletedCourseDto
                {
                    Code     = r.Course?.Code ?? "",
                    Total    = r.Grade.HasValue ? (decimal)r.Grade.Value * 10 : null,
                    Year     = r.StudyYear?.StartYear ?? 0,
                    Semester = (int)(r.Semester?.Title ?? 0) + 1
                }).ToList();

                result.Add(new AdminUserDto
                {
                    Id                 = u.Id,
                    Code               = u.Academic_Code,
                    Name               = u.DisplayName,
                    Email              = u.Email,
                    Role               = userRole.ToLower(),
                    Gender             = u.Gender.ToString().ToLower(),
                    Dept               = u.DepartmentId.HasValue && deptMap.ContainsKey(u.DepartmentId.Value)
                                         ? deptMap[u.DepartmentId.Value] : null,
                    Gpa                = u.TotalGPA ?? 0,
                    TotalCreditsEarned = u.TotalCredits ?? 0,
                    AllowedCredits     = u.AllowedCredits ?? 21,
                    Phone              = u.PhoneNumber,
                    Avatar             = u.ProfilePicture,
                    Level              = u.Level?.ToString()?.Replace("_", " "),
                    Active             = true,
                    RegisteredCourses  = registered,
                    CompletedCourses   = completed,
                });
            }
            return result;
        }
    }

    public class GetAdminUserByCodeQueryHandler
        : IRequestHandler<GetAdminUserByCodeQuery, AdminUserDto?>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public GetAdminUserByCodeQueryHandler(UserManager<User> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork  = unitOfWork;
        }

        public async Task<AdminUserDto?> Handle(GetAdminUserByCodeQuery request, CancellationToken ct)
        {
            var u = await _userManager.Users
                .FirstOrDefaultAsync(x => x.Academic_Code == request.AcademicCode, ct);
            if (u == null) return null;

            var roles    = await _userManager.GetRolesAsync(u);
            var userRole = roles.FirstOrDefault() ?? "Student";
            var regs     = await _unitOfWork.Registrations.GetByUserAsync(u.Id);
            var depts    = await _unitOfWork.Departments.GetAllAsync();
            var dept     = u.DepartmentId.HasValue ? depts.FirstOrDefault(d => d.Id == u.DepartmentId.Value) : null;

            return new AdminUserDto
            {
                Id                 = u.Id,
                Code               = u.Academic_Code,
                Name               = u.DisplayName,
                Email              = u.Email,
                Role               = userRole.ToLower(),
                Gender             = u.Gender.ToString().ToLower(),
                Dept               = dept?.Name,
                Gpa                = u.TotalGPA ?? 0,
                TotalCreditsEarned = u.TotalCredits ?? 0,
                AllowedCredits     = u.AllowedCredits ?? 21,
                Phone              = u.PhoneNumber,
                Avatar             = u.ProfilePicture,
                Level              = u.Level?.ToString()?.Replace("_", " "),
                Active             = true,
                RegisteredCourses  = regs.Where(r => !r.IsPassed).Select(r => r.Course?.Code ?? "").Where(c => c != "").ToList(),
                CompletedCourses   = regs.Where(r => r.IsPassed).Select(r => new CompletedCourseDto
                {
                    Code     = r.Course?.Code ?? "",
                    Year     = r.StudyYear?.StartYear ?? 0,
                    Semester = (int)(r.Semester?.Title ?? 0) + 1
                }).ToList(),
            };
        }
    }
}
