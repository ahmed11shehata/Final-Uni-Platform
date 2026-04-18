using System.Security.Claims;
using System.Text.Json;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Presistence;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/schedule")]
    public class ScheduleController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly UniversityDbContext _ctx;

        public ScheduleController(IUnitOfWork uow, UniversityDbContext ctx)
        {
            _uow = uow;
            _ctx = ctx;
        }

        // ══════════ ADMIN — Sessions (draft) ══════════

        /// <summary>GET /api/schedule/admin/sessions?year=&group=</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/sessions")]
        public async Task<IActionResult> GetAdminSessions([FromQuery] int? year, [FromQuery] string? group)
        {
            var list = await _uow.ScheduleSessions.GetByFiltersAsync(year, group);
            var result = list.Select(s => new
            {
                id = s.Id, year = s.Year, group = s.Group, day = s.Day,
                startTime = s.StartTime,
                endTime = s.EndTime,
                courseId = s.CourseId,
                code = s.Course?.Code,
                name = s.Course?.Name,
                type = s.Type, instructor = s.Instructor, room = s.Room,
                color = GetCourseColor(s.Course?.Code)
            });
            return Ok(new { data = result });
        }

        /// <summary>POST /api/schedule/admin/sessions</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/sessions")]
        public async Task<IActionResult> AddSession([FromBody] AddSessionDto dto)
        {
            var courses = await _uow.Courses.GetByCodesAsync(new[] { dto.CourseCode ?? "" });
            var course = courses.FirstOrDefault();
            if (course == null)
                return BadRequest(new { error = $"Course '{dto.CourseCode}' not found." });

            var session = new ScheduleSession
            {
                Year = dto.Year,
                Group = dto.Group ?? "A",
                Day = dto.Day ?? "Saturday",
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                CourseId = course.Id,
                Type = dto.Type ?? "Lecture",
                Instructor = dto.Instructor ?? "",
                Room = dto.Room ?? ""
            };
            await _uow.ScheduleSessions.AddAsync(session);
            await _uow.SaveChangesAsync();
            return Ok(new { data = new { session.Id } });
        }

        /// <summary>DELETE /api/schedule/admin/sessions/{id}</summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/sessions/{id}")]
        public async Task<IActionResult> RemoveSession(int id)
        {
            var s = await _uow.ScheduleSessions.GetByIdAsync(id);
            if (s == null) return NotFound();
            await _uow.ScheduleSessions.RemoveAsync(s);
            await _uow.SaveChangesAsync();
            return Ok();
        }

        // ══════════ ADMIN — Exams (draft) ══════════

        /// <summary>GET /api/schedule/admin/exams?year=&type=</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/exams")]
        public async Task<IActionResult> GetAdminExams([FromQuery] int? year, [FromQuery] string? type)
        {
            var list = await _uow.ExamSchedules.GetByFiltersAsync(year);
            if (!string.IsNullOrEmpty(type))
                list = list.Where(e => e.Type == type);
            var result = list.Select(e => new
            {
                id = e.Id, year = e.Year, type = e.Type,
                courseId = e.CourseId,
                code = e.Course?.Code,
                name = e.Course?.Name,
                date = e.Date.ToString("yyyy-MM-dd"),
                startTime = e.StartTime,
                duration = e.Duration,
                location = e.Location,
                color = GetCourseColor(e.Course?.Code)
            });
            return Ok(new { data = result });
        }

        /// <summary>POST /api/schedule/admin/exams</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/exams")]
        public async Task<IActionResult> AddExam([FromBody] AddExamDto dto)
        {
            var courses = await _uow.Courses.GetByCodesAsync(new[] { dto.CourseCode ?? "" });
            var course = courses.FirstOrDefault();
            if (course == null)
                return BadRequest(new { error = $"Course '{dto.CourseCode}' not found." });

            var exam = new ExamScheduleEntry
            {
                CourseId = course.Id,
                Type = dto.Type ?? "midterm",
                Date = dto.Date,
                StartTime = dto.StartTime,
                Duration = dto.Duration,
                Year = dto.Year,
                Location = dto.Location ?? ""
            };
            await _uow.ExamSchedules.AddAsync(exam);
            await _uow.SaveChangesAsync();
            return Ok(new { data = new { exam.Id } });
        }

        /// <summary>DELETE /api/schedule/admin/exams/{id}</summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/exams/{id}")]
        public async Task<IActionResult> RemoveExam(int id)
        {
            var e = await _uow.ExamSchedules.GetByIdAsync(id);
            if (e == null) return NotFound();
            await _uow.ExamSchedules.RemoveAsync(e);
            await _uow.SaveChangesAsync();
            return Ok();
        }

        // ══════════ ADMIN — Publish ══════════

        /// <summary>POST /api/schedule/admin/publish?year=&type=</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/publish")]
        public async Task<IActionResult> Publish([FromQuery] int year, [FromQuery] string type = "weekly")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "admin";

            object snapshotData;
            if (type == "weekly")
            {
                var sessions = await _uow.ScheduleSessions.GetByFiltersAsync(year);
                snapshotData = sessions.Select(s => new
                {
                    id = s.Id, year = s.Year, group = s.Group, day = s.Day,
                    startTime = s.StartTime,
                    endTime = s.EndTime,
                    code = s.Course?.Code,
                    name = s.Course?.Name,
                    type = s.Type, instructor = s.Instructor, room = s.Room,
                    color = GetCourseColor(s.Course?.Code)
                });
            }
            else
            {
                var exams = (await _uow.ExamSchedules.GetByFiltersAsync(year))
                    .Where(e => e.Type == type);
                snapshotData = exams.Select(e => new
                {
                    id = e.Id, year = e.Year, type = e.Type,
                    code = e.Course?.Code,
                    name = e.Course?.Name,
                    date = e.Date.ToString("yyyy-MM-dd"),
                    startTime = e.StartTime,
                    duration = e.Duration,
                    location = e.Location,
                    color = GetCourseColor(e.Course?.Code)
                });
            }

            var json = JsonSerializer.Serialize(snapshotData);

            var existing = await _ctx.SchedulePublishes
                .FirstOrDefaultAsync(p => p.Year == year && p.Type == type);

            if (existing != null)
            {
                existing.PublishedData = json;
                existing.PublishedAt = DateTime.UtcNow;
                existing.PublishedBy = userId;
            }
            else
            {
                _ctx.SchedulePublishes.Add(new SchedulePublish
                {
                    Year = year,
                    Type = type,
                    PublishedData = json,
                    PublishedAt = DateTime.UtcNow,
                    PublishedBy = userId
                });
            }

            await _ctx.SaveChangesAsync();
            return Ok(new { message = $"{type} schedule for Year {year} published.", publishedAt = DateTime.UtcNow });
        }

        // ══════════ PUBLIC — Published schedule (students/instructors) ══════════

        /// <summary>GET /api/schedule/published?year=&type=</summary>
        [Authorize]
        [HttpGet("published")]
        public async Task<IActionResult> GetPublished([FromQuery] int year, [FromQuery] string type = "weekly")
        {
            var pub = await _ctx.SchedulePublishes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Year == year && p.Type == type);

            if (pub == null)
                return Ok(new { data = Array.Empty<object>(), publishedAt = (DateTime?)null });

            return Ok(new
            {
                data = JsonSerializer.Deserialize<object>(pub.PublishedData),
                publishedAt = pub.PublishedAt
            });
        }

        /// <summary>GET /api/schedule/publish-info?year= — freshness metadata</summary>
        [Authorize]
        [HttpGet("publish-info")]
        public async Task<IActionResult> GetPublishInfo([FromQuery] int? year)
        {
            var query = _ctx.SchedulePublishes.AsNoTracking().AsQueryable();
            if (year.HasValue) query = query.Where(p => p.Year == year.Value);

            var info = await query.Select(p => new
            {
                p.Year,
                p.Type,
                p.PublishedAt,
                p.PublishedBy
            }).ToListAsync();

            return Ok(new { data = info });
        }

        // ── Helpers ──
        private static string GetCourseColor(string? code)
        {
            if (string.IsNullOrEmpty(code)) return "#818cf8";
            var hash = code.GetHashCode();
            string[] palette = { "#e8a838", "#7c6fc4", "#78909c", "#e05c8a", "#5b9fb5", "#3d8fe0", "#6366f1", "#8b5cf6", "#22c55e", "#ef4444" };
            return palette[Math.Abs(hash) % palette.Length];
        }
    }

    // ── DTOs ──
    public class AddSessionDto
    {
        public int Year { get; set; }
        public string? Group { get; set; }
        public string? Day { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public string? CourseCode { get; set; }
        public string? Type { get; set; }
        public string? Instructor { get; set; }
        public string? Room { get; set; }
    }

    public class AddExamDto
    {
        public string? CourseCode { get; set; }
        public string? Type { get; set; }
        public DateTime Date { get; set; }
        public double StartTime { get; set; }
        public double Duration { get; set; }
        public int Year { get; set; }
        public string? Location { get; set; }
    }
}
