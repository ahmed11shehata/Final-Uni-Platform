using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AYA_UIS.Application.Contracts;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using AYA_UIS.Shared.Exceptions;
using Domain.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Admin_Module;

namespace Services.Implementatios
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public AdminService(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        // ═══════════════════════════════════════════════════════════
        // 3.1 GET /api/admin/stats
        // ═══════════════════════════════════════════════════════════
        public async Task<AdminStatsResponseDto> GetStatsAsync()
        {
            var students = await _userManager.GetUsersInRoleAsync("Student");
            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
            var courses = await _unitOfWork.Courses.GetAllAsync();
            var regs = await _unitOfWork.Registrations.GetAllAsync();

            var totalStudents = students.Count;
            var totalRegistered = regs
                .Select(r => r.UserId)
                .Distinct()
                .Count();
            var activeCourses = courses.Count();

            var registrationRate = totalStudents > 0
                ? (int)Math.Round((double)totalRegistered / totalStudents * 100)
                : 0;

            return new AdminStatsResponseDto
            {
                TotalStudents = totalStudents,
                TotalRegistered = totalRegistered,
                TotalInstructors = instructors.Count,
                ActiveCourses = activeCourses,
                RegistrationRate = registrationRate,
                Trends = new AdminTrendsDto
                {
                    Students = "+0%",
                    Registered = "+0%",
                    Instructors = "+0"
                }
            };
        }

        // ═══════════════════════════════════════════════════════════
        // 3.2 Email Manager
        // ═══════════════════════════════════════════════════════════
        public async Task<AdminEmailListResponseDto> GetEmailsAsync()
        {
            var allUsers = await _userManager.Users.ToListAsync();
            var accounts = new List<AdminAccountDto>();

            int studentCount = 0, instructorCount = 0, adminCount = 0, suspendedCount = 0;

            foreach (var u in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var role = (roles.FirstOrDefault() ?? "student").ToLower();

                switch (role)
                {
                    case "student": studentCount++; break;
                    case "instructor": instructorCount++; break;
                    case "admin": adminCount++; break;
                }

                if (!u.Active) suspendedCount++;

                // Split DisplayName into first/last
                var nameParts = (u.DisplayName ?? "").Trim().Split(' ', 2,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var firstName = nameParts.Length > 0 ? nameParts[0] : "";
                var lastName  = nameParts.Length > 1 ? nameParts[1] : "";

                accounts.Add(new AdminAccountDto
                {
                    Id = u.Id,
                    Code = u.Academic_Code ?? string.Empty,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = u.Email ?? string.Empty,
                    Role = role,
                    Active = u.Active,
                    CreatedAt = u.CreatedAt.ToString("yyyy-MM-dd"),
                    Password = "••••••••••••••",
                    SubEmail = u.SubEmail ?? string.Empty
                });
            }

            return new AdminEmailListResponseDto
            {
                Accounts = accounts,
                Counts = new AdminEmailCountsDto
                {
                    Student = studentCount,
                    Instructor = instructorCount,
                    Admin = adminCount,
                    Suspended = suspendedCount,
                    Total = allUsers.Count
                }
            };
        }

        public async Task<CreateEmailAccountResponseDto> CreateEmailAccountAsync(CreateEmailAccountDto dto)
        {
            // Validate role
            var prefix = dto.Role.ToLower() switch
            {
                "student" => "cs",
                "instructor" => "dr",
                "admin" => "adm",
                _ => throw new BadRequestException("Invalid role. Must be 'student', 'instructor', or 'admin'.")
            };

            // Check duplicate code
            var existingByCode = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Academic_Code == dto.Code);
            if (existingByCode != null)
                throw new ConflictException($"An account with code '{dto.Code}' already exists.");

            // Generate email: cs-{code}{FirstName}{LastName}@akhbaracademy.edu.eg
            var firstNameClean = dto.FirstName.Replace(" ", "");
            var lastNameClean = dto.LastName.Replace(" ", "");
            var email = $"{prefix}-{dto.Code}{firstNameClean}{lastNameClean}@akhbaracademy.edu.eg";

            // Check duplicate email
            var existingByEmail = await _userManager.FindByEmailAsync(email);
            if (existingByEmail != null)
                throw new ConflictException($"Email '{email}' already exists.");

            // Use provided password or generate a secure one
            var password = !string.IsNullOrWhiteSpace(dto.Password)
                ? dto.Password
                : GenerateSecurePassword(14);
            var displayName = $"{dto.FirstName} {dto.LastName}";

            var user = new User
            {
                DisplayName = displayName,
                Email = email,
                UserName = email,
                Academic_Code = dto.Code,
                Gender = Gender.Male,
                Active = true,
                MustChangePassword = false,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            if (!string.IsNullOrWhiteSpace(dto.Phone))
                user.PhoneNumber = dto.Phone;

            if (!string.IsNullOrWhiteSpace(dto.Gender) &&
                Enum.TryParse<Gender>(dto.Gender, true, out var genderEnum))
                user.Gender = genderEnum;

            if (!string.IsNullOrWhiteSpace(dto.DateOfBirth) &&
                DateTime.TryParse(dto.DateOfBirth, out var dob))
                user.DateOfBirth = dob;

            if (!string.IsNullOrWhiteSpace(dto.Address))
                user.Address = dto.Address;

            if (!string.IsNullOrWhiteSpace(dto.SubEmail))
                user.SubEmail = dto.SubEmail;

            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                throw new ValidationException(errors);
            }

            // Assign role
            var roleName = char.ToUpper(dto.Role[0]) + dto.Role[1..].ToLower();
            if (await _roleManager.RoleExistsAsync(roleName))
                await _userManager.AddToRoleAsync(user, roleName);

            // Send credentials email
            var emailBody = $@"<!DOCTYPE html>
<html>
<body style='font-family:Sora,Arial,sans-serif;background:#f0f2ff;padding:40px 20px;margin:0;'>
  <div style='max-width:540px;margin:0 auto;'>

    <!-- Header -->
    <div style='background:linear-gradient(135deg,#4338ca 0%,#6366f1 50%,#818cf8 100%);
                border-radius:20px 20px 0 0;padding:40px 32px;text-align:center;'>
      <div style='width:64px;height:64px;background:rgba(255,255,255,0.15);border-radius:16px;
                  margin:0 auto 16px;display:flex;align-items:center;justify-content:center;
                  font-size:28px;line-height:64px;text-align:center;'>🎓</div>
      <h1 style='color:#fff;margin:0 0 6px;font-size:24px;font-weight:900;letter-spacing:-0.5px;'>
        Welcome to Akhbar Alyoum Academy
      </h1>
      <p style='color:rgba(255,255,255,0.75);margin:0;font-size:14px;'>
        University Information System
      </p>
    </div>

    <!-- Body -->
    <div style='background:#fff;padding:36px 32px;'>
      <p style='color:#374151;font-size:15px;margin:0 0 8px;'>
        Hello <strong style='color:#111827;'>{displayName}</strong>,
      </p>
      <p style='color:#6b7280;font-size:14px;margin:0 0 28px;line-height:1.6;'>
        Your university account has been successfully created.
        Use the credentials below to sign in for the first time.
      </p>

      <!-- Credentials Card -->
      <div style='background:#f8f9ff;border:1.5px solid #e0e7ff;border-radius:14px;
                  padding:24px;margin-bottom:20px;'>
        <div style='margin-bottom:20px;'>
          <p style='margin:0 0 6px;color:#6366f1;font-size:11px;font-weight:800;
                    text-transform:uppercase;letter-spacing:1.5px;'>University Email</p>
          <p style='margin:0;color:#111827;font-size:16px;font-weight:700;
                    word-break:break-all;'>{email}</p>
        </div>
        <div style='border-top:1px solid #e0e7ff;padding-top:20px;'>
          <p style='margin:0 0 6px;color:#6366f1;font-size:11px;font-weight:800;
                    text-transform:uppercase;letter-spacing:1.5px;'>Password</p>
          <p style='margin:0;color:#111827;font-size:20px;font-weight:900;
                    letter-spacing:3px;font-family:monospace;'>{password}</p>
        </div>
      </div>

      <!-- Info Banner -->
      <div style='background:#eff6ff;border:1.5px solid #bfdbfe;border-radius:10px;
                  padding:14px 16px;margin-bottom:24px;'>
        <p style='margin:0;color:#1e40af;font-size:13px;line-height:1.6;'>
          ℹ️ Keep this password safe. You can change it anytime from your profile settings
          using the <strong>Password Help</strong> option.
        </p>
      </div>

      <p style='color:#9ca3af;font-size:12px;margin:0;'>
        If you didn't expect this email, please contact the university administration.
      </p>
    </div>

    <!-- Footer -->
    <div style='background:#f9fafb;border-radius:0 0 20px 20px;padding:18px 32px;
                text-align:center;border-top:1px solid #e5e7eb;'>
      <p style='margin:0;color:#9ca3af;font-size:12px;'>
        © 2026 Akhbar Alyoum Academy · University Management System
      </p>
    </div>

  </div>
</body>
</html>";
            try { await _emailService.SendEmailAsync(dto.SubEmail, "Your University Account Credentials", emailBody); }
            catch (Exception emailEx)
            {
                Console.WriteLine($"[EMAIL ERROR] {emailEx.GetType().Name}: {emailEx.Message}");
            }

            return new CreateEmailAccountResponseDto
            {
                Id = user.Id,
                Code = dto.Code,
                Email = email,
                Role = dto.Role.ToLower(),
                Active = true,
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                Password = password
            };
        }

        public async Task<object> ToggleActiveAsync(string userId, string currentUserId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                       ?? await _userManager.Users
                           .FirstOrDefaultAsync(u => u.Academic_Code == userId);
            if (user == null)
                throw new NotFoundException("User not found.");

            user.Active = !user.Active;

            // Set lockout properties directly on the entity to avoid
            // multiple UpdateAsync calls that cause concurrency stamp conflicts
            if (!user.Active)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }
            else
            {
                user.LockoutEnabled = false;
                user.LockoutEnd = null;
            }

            // Single UpdateAsync call — persists Active + lockout atomically
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InternalServerErrorException("Failed to update user.");

            return new { id = user.Id, active = user.Active };
        }

        public async Task<ResetPasswordResponseDto> ResetPasswordAsync(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId)
                       ?? await _userManager.Users
                           .FirstOrDefaultAsync(u => u.Academic_Code == userId);
            if (user == null)
                throw new NotFoundException("User not found.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                throw new ValidationException(errors);
            }

            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);

            if (!string.IsNullOrWhiteSpace(user.SubEmail))
            {
                try
                {
                    var subject = "Your Password Has Been Reset";
                    var body = $@"<!DOCTYPE html>
<html><body style='font-family:Sora,Arial,sans-serif;background:#f4f4f4;padding:32px;'>
  <div style='max-width:520px;margin:0 auto;background:white;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);'>
    <div style='background:linear-gradient(135deg,#6366f1,#4338ca);padding:32px;text-align:center;'>
      <h1 style='color:white;margin:0;font-size:22px;'>Password Reset</h1>
    </div>
    <div style='padding:32px;'>
      <p style='color:#374151;font-size:15px;'>Hello <strong>{user.DisplayName}</strong>,</p>
      <p style='color:#374151;font-size:15px;'>Your university account password has been reset by the administrator.</p>
      <div style='background:#f8f9ff;border:1.5px solid #e0e7ff;border-radius:12px;padding:20px;margin:20px 0;'>
        <p style='margin:0 0 10px;color:#6366f1;font-size:13px;font-weight:700;text-transform:uppercase;letter-spacing:1px;'>Login Email</p>
        <p style='margin:0 0 20px;color:#111827;font-size:16px;font-weight:800;'>{user.Email}</p>
        <p style='margin:0 0 10px;color:#6366f1;font-size:13px;font-weight:700;text-transform:uppercase;letter-spacing:1px;'>New Password</p>
        <p style='margin:0;color:#111827;font-size:16px;font-weight:800;letter-spacing:2px;'>{newPassword}</p>
      </div>
    </div>
    <div style='background:#f9fafb;padding:16px;text-align:center;border-top:1px solid #e5e7eb;'>
      <p style='margin:0;color:#9ca3af;font-size:12px;'>Akhbar Alyoum Academy — University Management System</p>
    </div>
  </div>
</body></html>";
                    await _emailService.SendEmailAsync(user.SubEmail, subject, body);
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"[EMAIL ERROR] {emailEx.GetType().Name}: {emailEx.Message}");
                }
            }

            return new ResetPasswordResponseDto
            {
                Id = user.Id,
                Password = newPassword
            };
        }

        public async Task<object> UpdateAccountAsync(string userId, UpdateAccountDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId)
                       ?? await _userManager.Users
                           .FirstOrDefaultAsync(u => u.Academic_Code == userId);
            if (user == null) throw new NotFoundException("User not found.");

            if (!string.IsNullOrWhiteSpace(dto.FirstName) || !string.IsNullOrWhiteSpace(dto.LastName))
            {
                var parts = (user.DisplayName ?? "").Split(' ', 2);
                var first = dto.FirstName ?? (parts.Length > 0 ? parts[0] : "");
                var last  = dto.LastName  ?? (parts.Length > 1 ? parts[1] : "");
                user.DisplayName = $"{first} {last}".Trim();
            }

            if (dto.Phone != null)
                user.PhoneNumber = dto.Phone;

            if (dto.Address != null)
                user.Address = dto.Address;

            if (dto.SubEmail != null)
                user.SubEmail = dto.SubEmail;

            if (!string.IsNullOrWhiteSpace(dto.Gender) &&
                Enum.TryParse<Gender>(dto.Gender, true, out var genderEnum))
                user.Gender = genderEnum;

            if (!string.IsNullOrWhiteSpace(dto.DateOfBirth) &&
                DateTime.TryParse(dto.DateOfBirth, out var dob))
                user.DateOfBirth = dob;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InternalServerErrorException("Failed to update account.");

            return new
            {
                id = user.Id,
                displayName = user.DisplayName,
                email = user.Email,
                phone = user.PhoneNumber,
                gender = user.Gender.ToString().ToLower(),
                address = user.Address,
                subEmail = user.SubEmail,
                dateOfBirth = user.DateOfBirth?.ToString("yyyy-MM-dd")
            };
        }

        public async Task DeleteAccountAsync(string userId, string currentUserId)
        {
            if (userId == currentUserId)
                throw new ForbiddenException("Cannot delete your own account.");

            var user = await _userManager.FindByIdAsync(userId)
                       ?? await _userManager.Users
                           .FirstOrDefaultAsync(u => u.Academic_Code == userId);
            if (user == null)
                throw new NotFoundException("User not found.");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                throw new InternalServerErrorException("Failed to delete user.");
        }

        // ═══════════════════════════════════════════════════════════
        // 3.3 Schedule Manager
        // ═══════════════════════════════════════════════════════════
        public async Task<SessionResponseDto> CreateSessionAsync(CreateSessionDto dto)
        {
            // Resolve start/end — prefer new field names, fall back to legacy
            double startTime = dto.Start > 0 ? dto.Start : dto.StartTime ?? 0;
            double endTime   = dto.End   > 0 ? dto.End   : dto.EndTime   ?? 0;

            // Validate duration
            var duration = endTime - startTime;
            var allowedDurations = new[] { 0.5, 1.0, 1.5, 2.0, 2.5, 3.0 };
            if (!allowedDurations.Contains(duration))
                throw new BadRequestException($"Duration must be 0.5–3.0 hours. Got {duration}.");

            if (endTime > 18.0)
                throw new BadRequestException("End time cannot exceed 18.0.");

            // Resolve course — prefer code, fall back to courseId
            Course? course = null;
            if (!string.IsNullOrWhiteSpace(dto.Code))
            {
                var matches = await _unitOfWork.Courses.GetByCodesAsync(new[] { dto.Code });
                course = matches.FirstOrDefault();
            }
            course ??= dto.CourseId.HasValue
                ? await _unitOfWork.Courses.GetByIdAsync(dto.CourseId.Value)
                : null;
            if (course == null)
                throw new NotFoundException($"Course '{dto.Code ?? dto.CourseId?.ToString()}' not found.");

            // Check overlap
            var hasOverlap = await _unitOfWork.ScheduleSessions.HasOverlapAsync(
                dto.Year, dto.Group, dto.Day, startTime, endTime);
            if (hasOverlap)
                throw new ConflictException("SCHEDULE_CONFLICT: A session already exists at this time slot.");

            var session = new ScheduleSession
            {
                Year = dto.Year,
                Group = dto.Group,
                Day = dto.Day,
                StartTime = startTime,
                EndTime = endTime,
                CourseId = course.Id,
                Type = dto.Type
            };

            await _unitOfWork.ScheduleSessions.AddAsync(session);
            await _unitOfWork.SaveChangesAsync();

            return MapSession(session, course, dto.Room, dto.Instructor, dto.Color);
        }

        public async Task DeleteSessionAsync(int sessionId)
        {
            var session = await _unitOfWork.ScheduleSessions.GetByIdAsync(sessionId);
            if (session == null)
                throw new NotFoundException($"Session with ID {sessionId} not found.");

            await _unitOfWork.ScheduleSessions.RemoveAsync(session);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ExamResponseDto> CreateExamAsync(CreateExamDto dto)
        {
            if (!DateTime.TryParseExact(dto.Date, "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out var date))
                throw new BadRequestException("Invalid date format. Use YYYY-MM-DD.");

            // Resolve course — prefer code, fall back to courseId
            Course? course = null;
            if (!string.IsNullOrWhiteSpace(dto.Code))
            {
                var matches = await _unitOfWork.Courses.GetByCodesAsync(new[] { dto.Code });
                course = matches.FirstOrDefault();
            }
            course ??= dto.CourseId.HasValue
                ? await _unitOfWork.Courses.GetByIdAsync(dto.CourseId.Value)
                : null;
            if (course == null)
                throw new NotFoundException($"Course '{dto.Code ?? dto.CourseId?.ToString()}' not found.");

            // Resolve start time — prefer dto.Time (string e.g. "09:00"), fall back to dto.StartTime
            double startTime = dto.StartTime ?? 0;
            if (!string.IsNullOrWhiteSpace(dto.Time) && TimeSpan.TryParse(dto.Time, out var ts))
                startTime = ts.TotalHours;

            var exam = new ExamScheduleEntry
            {
                CourseId = course.Id,
                Type = dto.Type.ToLower(),
                Date = date,
                StartTime = startTime,
                Duration = dto.Duration,
                Year = dto.Year,
                Location = dto.Hall ?? dto.Location ?? ""
            };

            await _unitOfWork.ExamSchedules.AddAsync(exam);
            await _unitOfWork.SaveChangesAsync();

            return MapExam(exam, course, dto.Color);
        }

        public async Task DeleteExamAsync(int examId)
        {
            var exam = await _unitOfWork.ExamSchedules.GetByIdAsync(examId);
            if (exam == null)
                throw new NotFoundException($"Exam with ID {examId} not found.");

            await _unitOfWork.ExamSchedules.RemoveAsync(exam);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<DateTime> PublishScheduleAsync()
        {
            var settings = await _unitOfWork.RegistrationSettings.GetCurrentAsync();
            if (settings == null)
            {
                settings = new RegistrationSettings
                {
                    SchedulePublishedAt = DateTime.UtcNow
                };
                await _unitOfWork.RegistrationSettings.AddAsync(settings);
            }
            else
            {
                settings.SchedulePublishedAt = DateTime.UtcNow;
                await _unitOfWork.RegistrationSettings.UpdateAsync(settings);
            }

            await _unitOfWork.SaveChangesAsync();
            return settings.SchedulePublishedAt.Value;
        }

        public async Task<object> GetScheduleAsync(int? year, string? group, string? view)
        {
            var sessions = await _unitOfWork.ScheduleSessions.GetByFiltersAsync(year, group);
            var exams = await _unitOfWork.ExamSchedules.GetByFiltersAsync(year);

            var sessionDtos = new List<SessionResponseDto>();
            foreach (var s in sessions)
            {
                var course = await _unitOfWork.Courses.GetByIdAsync(s.CourseId);
                sessionDtos.Add(MapSession(s, course));
            }

            var examDtos = new List<ExamResponseDto>();
            foreach (var e in exams)
            {
                var course = await _unitOfWork.Courses.GetByIdAsync(e.CourseId);
                examDtos.Add(MapExam(e, course));
            }

            if (view?.ToLower() == "exam")
                return new { exams = examDtos };

            return new { sessions = sessionDtos, exams = examDtos };
        }

        // ── Schedule mapping helpers ──
        private static SessionResponseDto MapSession(ScheduleSession s, Course? course,
            string? room = null, string? instructor = null, string? color = null) => new()
        {
            Id = s.Id,
            Year = s.Year,
            Group = s.Group,
            Day = s.Day,
            Start = s.StartTime,
            End = s.EndTime,
            Code = course?.Code ?? "",
            Name = course?.Name ?? "",
            Type = s.Type,
            Room = room ?? "",
            Instructor = instructor ?? "",
            Color = color ?? ""
        };

        private static ExamResponseDto MapExam(ExamScheduleEntry e, Course? course,
            string? color = null) => new()
        {
            Id = e.Id,
            Year = e.Year,
            Type = e.Type,
            Code = course?.Code ?? "",
            Name = course?.Name ?? "",
            Date = e.Date.ToString("yyyy-MM-dd"),
            Time = TimeSpan.FromHours(e.StartTime).ToString(@"hh\:mm"),
            Hall = e.Location,
            Duration = e.Duration,
            Color = color ?? ""
        };

        // ═══════════════════════════════════════════════════════════
        // 3.4 Registration Manager
        // ═══════════════════════════════════════════════════════════
        public async Task<AdminRegistrationStatusDto> GetRegistrationStatusAsync()
        {
            var s = await _unitOfWork.RegistrationSettings.GetCurrentAsync();
            if (s == null)
                return new AdminRegistrationStatusDto { IsOpen = false };

            return await MapRegistrationSettingsAsync(s);
        }

        public async Task<AdminRegistrationStatusDto> StartRegistrationAsync(StartRegistrationDto dto)
        {
            // Parse dates
            if (!DateTime.TryParseExact(dto.StartDate, "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out var startDate))
                throw new BadRequestException("Invalid startDate format. Use YYYY-MM-DD.");

            if (!DateTime.TryParseExact(dto.Deadline, "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out var deadline))
                throw new BadRequestException("Invalid deadline format. Use YYYY-MM-DD.");

            var existing = await _unitOfWork.RegistrationSettings.GetCurrentAsync();
            if (existing != null && existing.IsOpen)
                throw new ConflictException("Registration is already open. Stop it first.");

            var settings = existing ?? new RegistrationSettings();
            settings.IsOpen = true;
            settings.Semester = dto.Semester;
            settings.AcademicYear = dto.AcademicYear;
            settings.StartDate = startDate;
            settings.Deadline = deadline;
            settings.MaxCredits = dto.MaxCredits;
            settings.OpenedAt = DateTime.UtcNow;
            settings.ClosedAt = null;

            // Persist rich per-year JSON + derive legacy columns for backward compat
            settings.OpenedCoursesByYear = JsonSerializer.Serialize(dto.OpenedCoursesByYear);
            settings.OpenYears = string.Join(",", dto.OpenedCoursesByYear.Keys);
            settings.EnabledCourses = string.Join(",",
                dto.OpenedCoursesByYear.Values
                    .SelectMany(v => v.Select(e => e.CourseCode))
                    .Distinct());

            if (existing == null)
                await _unitOfWork.RegistrationSettings.AddAsync(settings);
            else
                await _unitOfWork.RegistrationSettings.UpdateAsync(settings);

            await _unitOfWork.SaveChangesAsync();

            return await MapRegistrationSettingsAsync(settings);
        }

        public async Task<DateTime> StopRegistrationAsync()
        {
            var settings = await _unitOfWork.RegistrationSettings.GetCurrentAsync();
            if (settings == null || !settings.IsOpen)
                throw new BadRequestException("Registration is not currently open.");

            settings.IsOpen = false;
            settings.ClosedAt = DateTime.UtcNow;
            await _unitOfWork.RegistrationSettings.UpdateAsync(settings);
            await _unitOfWork.SaveChangesAsync();

            return settings.ClosedAt.Value;
        }

        public async Task<AdminRegistrationStatusDto> UpdateRegistrationSettingsAsync(StartRegistrationDto dto)
        {
            var settings = await _unitOfWork.RegistrationSettings.GetCurrentAsync();
            if (settings == null || !settings.IsOpen)
                throw new BadRequestException("Registration is not currently open.");

            if (!DateTime.TryParseExact(dto.StartDate, "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out var startDate))
                throw new BadRequestException("Invalid startDate format. Use YYYY-MM-DD.");
            if (!DateTime.TryParseExact(dto.Deadline, "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out var deadline))
                throw new BadRequestException("Invalid deadline format. Use YYYY-MM-DD.");

            settings.Semester = dto.Semester;
            settings.AcademicYear = dto.AcademicYear;
            settings.StartDate = startDate;
            settings.Deadline = deadline;
            settings.MaxCredits = dto.MaxCredits;

            // Persist rich per-year JSON + derive legacy columns
            settings.OpenedCoursesByYear = JsonSerializer.Serialize(dto.OpenedCoursesByYear);
            settings.OpenYears = string.Join(",", dto.OpenedCoursesByYear.Keys);
            settings.EnabledCourses = string.Join(",",
                dto.OpenedCoursesByYear.Values
                    .SelectMany(v => v.Select(e => e.CourseCode))
                    .Distinct());

            await _unitOfWork.RegistrationSettings.UpdateAsync(settings);
            await _unitOfWork.SaveChangesAsync();

            return await MapRegistrationSettingsAsync(settings);
        }

        // ═══════════════════════════════════════════════════════════
        // 3.5 Courses Manager
        // ═══════════════════════════════════════════════════════════
        public async Task<List<AdminCourseListItemDto>> GetCoursesAsync(
            int? year, string? semester, string? type, string? search)
        {
            var allCourses = await _unitOfWork.Courses.GetAllAsync();
            var list = allCourses.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLower();
                list = list.Where(c =>
                    c.Code.ToLower().Contains(s) ||
                    c.Name.ToLower().Contains(s));
            }

            var result = new List<AdminCourseListItemDto>();
            foreach (var c in list)
            {
                var prereqs = await _unitOfWork.Courses.GetCoursePrerequisitesAsync(c.Id);
                result.Add(new AdminCourseListItemDto
                {
                    Code = c.Code,
                    Name = c.Name,
                    Credits = c.Credits,
                    Prerequisites = prereqs.Select(p => p.Code).ToList()
                });
            }

            return result;
        }

        public async Task UpdateCourseSettingsAsync(AdminCourseSettingsDto dto)
        {
            var settings = await _unitOfWork.RegistrationSettings.GetCurrentAsync();
            if (settings == null)
            {
                settings = new RegistrationSettings
                {
                    OpenYears = string.Join(",", dto.OpenYears),
                    EnabledCourses = string.Join(",", dto.EnabledCourses)
                };
                await _unitOfWork.RegistrationSettings.AddAsync(settings);
            }
            else
            {
                settings.OpenYears = string.Join(",", dto.OpenYears);
                settings.EnabledCourses = string.Join(",", dto.EnabledCourses);
                await _unitOfWork.RegistrationSettings.UpdateAsync(settings);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        // ═══════════════════════════════════════════════════════════
        // 3.6 Student Control
        // ═══════════════════════════════════════════════════════════
        public async Task<AdminStudentDetailDto> GetStudentAsync(string studentId)
        {
            var user = await _userManager.FindByIdAsync(studentId)
                       ?? await _userManager.Users.FirstOrDefaultAsync(u => u.Academic_Code == studentId);

            if (user == null)
                throw new NotFoundException("Student not found.");

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any(r => r.Equals("Student", StringComparison.OrdinalIgnoreCase)))
                throw new BadRequestException("User is not a student.");

            var department = user.DepartmentId.HasValue
                ? await _unitOfWork.Departments.GetByIdAsync(user.DepartmentId.Value)
                : null;

            var standing = ComputeStanding(user.TotalGPA ?? 0);
            if (user.AllowedCredits.HasValue && user.AllowedCredits.Value > 0)
                standing.MaxCredits = user.AllowedCredits.Value;

            // Get registrations
            var registrations = await _unitOfWork.Registrations.GetByUserIdAsync(user.Id);
            var registeredCourses = new List<AdminStudentCourseDto>();
            var completedCourses = new List<AdminStudentCourseDto>();

            foreach (var reg in registrations)
            {
                var course = await _unitOfWork.Courses.GetByIdAsync(reg.CourseId);
                if (course == null) continue;

                var courseDto = new AdminStudentCourseDto
                {
                    Code = course.Code,
                    Name = course.Name,
                    Credits = course.Credits,
                    Status = reg.Progress.ToString().ToLower(),
                    Grade = GradeEnumToLetter(reg.Grade)
                };

                if (reg.IsPassed || reg.Progress == CourseProgress.Completed)
                    completedCourses.Add(courseDto);
                else
                    registeredCourses.Add(courseDto);
            }

            return new AdminStudentDetailDto
            {
                Student = new AdminStudentProfileDto
                {
                    Id = user.Id,
                    Name = user.DisplayName,
                    Email = user.Email,
                    AcademicCode = user.Academic_Code,
                    Department = department?.Name,
                    Year = user.Level?.ToString()?.Replace("_", " "),
                    EntryYear = user.EntryYear,
                    Gpa = user.TotalGPA,
                    Phone = user.PhoneNumber,
                    Avatar = string.IsNullOrEmpty(user.ProfilePicture) ? null : user.ProfilePicture,
                    Active = user.Active
                },
                Standing = standing,
                RegisteredCourses = registeredCourses,
                CompletedCourses = completedCourses
            };
        }

        public async Task ForceAddCourseAsync(string studentId, AdminAddCourseDto dto)
        {
            var user = await ResolveStudent(studentId);

            // Find course by code
            var courses = await _unitOfWork.Courses.GetAllAsync();
            var course = courses.FirstOrDefault(c =>
                c.Code.Equals(dto.CourseCode, StringComparison.OrdinalIgnoreCase));
            if (course == null)
                throw new NotFoundException($"Course '{dto.CourseCode}' not found.");

            // Check if already registered
            var isRegistered = await _unitOfWork.Registrations.IsUserRegisteredInCourseAsync(user.Id, course.Id);
            if (isRegistered)
                throw new ConflictException($"Student is already registered in '{dto.CourseCode}'.");

            // Get current study year and semester
            var currentSY = await _unitOfWork.StudyYears.GetCurrentStudyYearAsync();
            if (currentSY == null)
                throw new BadRequestException("No active study year found.");

            var semesters = await _unitOfWork.Semesters.GetByStudyYearIdAsync(currentSY.Id);
            var activeSem = semesters.FirstOrDefault(s =>
                s.StartDate <= DateTime.UtcNow && s.EndDate >= DateTime.UtcNow)
                ?? semesters.OrderByDescending(s => s.StartDate).FirstOrDefault();

            if (activeSem == null)
                throw new BadRequestException("No active semester found.");

            var registration = new Registration
            {
                UserId = user.Id,
                CourseId = course.Id,
                StudyYearId = currentSY.Id,
                SemesterId = activeSem.Id,
                Status = RegistrationStatus.Approved,
                Progress = CourseProgress.InProgress,
                RegisteredAt = DateTime.UtcNow
            };

            await _unitOfWork.Registrations.AddAsync(registration);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ForceRemoveCourseAsync(string studentId, string courseCode)
        {
            var user = await ResolveStudent(studentId);

            var courses = await _unitOfWork.Courses.GetAllAsync();
            var course = courses.FirstOrDefault(c =>
                c.Code.Equals(courseCode, StringComparison.OrdinalIgnoreCase));
            if (course == null)
                throw new NotFoundException($"Course '{courseCode}' not found.");

            var currentSY = await _unitOfWork.StudyYears.GetCurrentStudyYearAsync();
            if (currentSY == null)
                throw new BadRequestException("No active study year found.");

            var reg = await _unitOfWork.Registrations.GetByUserAndCourseAsync(
                user.Id, course.Id, currentSY.Id);
            if (reg == null)
                throw new NotFoundException($"Student is not registered in '{courseCode}'.");

            await _unitOfWork.Registrations.Delete(reg);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<string> UnlockCourseAsync(string studentId, string courseCode)
        {
            var user = await ResolveStudent(studentId);

            var courses = await _unitOfWork.Courses.GetAllAsync();
            var course = courses.FirstOrDefault(c =>
                c.Code.Equals(courseCode, StringComparison.OrdinalIgnoreCase));
            if (course == null)
                throw new NotFoundException($"Course '{courseCode}' not found.");

            // Remove admin lock if exists
            var lockEntry = await _unitOfWork.AdminCourseLocks.GetAsync(user.Id, course.Id);
            bool wasLocked = lockEntry != null;
            if (lockEntry != null)
            {
                await _unitOfWork.AdminCourseLocks.RemoveAsync(lockEntry);
            }

            // Also create a course exception to bypass prerequisites
            var currentSY = await _unitOfWork.StudyYears.GetCurrentStudyYearAsync();
            if (currentSY != null)
            {
                var semesters = await _unitOfWork.Semesters.GetByStudyYearIdAsync(currentSY.Id);
                var activeSem = semesters.FirstOrDefault(s =>
                    s.StartDate <= DateTime.UtcNow && s.EndDate >= DateTime.UtcNow)
                    ?? semesters.OrderByDescending(s => s.StartDate).FirstOrDefault();

                if (activeSem != null)
                {
                    var existing = await _unitOfWork.StudentCourseExceptions.GetAsync(
                        user.Id, course.Id, currentSY.Id, activeSem.Id);
                    if (existing == null)
                    {
                        await _unitOfWork.StudentCourseExceptions.AddAsync(
                            new StudentCourseException
                            {
                                UserId = user.Id,
                                CourseId = course.Id,
                                StudyYearId = currentSY.Id,
                                SemesterId = activeSem.Id
                            });
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return wasLocked
                ? "Course unlocked."
                : "Course was not locked. Exception grant applied if eligible.";
        }

        public async Task LockCourseAsync(string studentId, string courseCode, string? reason = null)
        {
            var user = await ResolveStudent(studentId);

            var courses = await _unitOfWork.Courses.GetAllAsync();
            var course = courses.FirstOrDefault(c =>
                c.Code.Equals(courseCode, StringComparison.OrdinalIgnoreCase));
            if (course == null)
                throw new NotFoundException($"Course '{courseCode}' not found.");

            // Check if already locked
            var isLocked = await _unitOfWork.AdminCourseLocks.IsLockedAsync(user.Id, course.Id);
            if (isLocked)
                throw new ConflictException($"Course '{courseCode}' is already locked for this student.");

            await _unitOfWork.AdminCourseLocks.AddAsync(new AdminCourseLock
            {
                UserId = user.Id,
                CourseId = course.Id,
                LockedAt = DateTime.UtcNow,
                Reason = reason
            });

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task OverrideMaxCreditsAsync(string studentId, AdminMaxCreditsDto dto)
        {
            var user = await ResolveStudent(studentId);
            user.AllowedCredits = dto.MaxCredits;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InternalServerErrorException("Failed to update max credits.");
        }

        // ═══════════════════════════════════════════════════════════
        // 3.7 Academic Setup
        // ═══════════════════════════════════════════════════════════

        public async Task<AcademicSetupResponseDto> GetAcademicSetupAsync(string studentId)
        {
            var user = await ResolveStudent(studentId);
            int currentYear = LevelToYearNum(user.Level);

            // Load all catalog courses
            var allCourses = (await _unitOfWork.Courses.GetAllAsync()).ToList();

            // Load course offerings to determine year/semester grouping
            var offerings = (await _unitOfWork.CourseOfferings.GetAllAsync()).ToList();
            var courseYearSemMap = BuildCourseYearSemMap(allCourses, offerings);

            // Load student's equivalency registrations
            var allRegs = await _unitOfWork.Registrations.GetByUserIdAsync(user.Id);
            var equivRegs = allRegs
                .Where(r => r.IsEquivalency && r.IsPassed)
                .ToList();
            var equivByCourseId = equivRegs.ToDictionary(r => r.CourseId);

            // Compute current credits earned from ALL passed registrations
            int totalCreditsEarned = allRegs
                .Where(r => r.IsPassed)
                .Sum(r => allCourses.FirstOrDefault(c => c.Id == r.CourseId)?.Credits ?? 0);

            var standing = ComputeStanding(user.TotalGPA ?? 0);

            // Build year/semester structure
            // Include ALL catalog courses: mapped via CourseOffering or fallback to Year 1 / Sem 1
            var coursesById = allCourses.ToDictionary(c => c.Id);

            // Ensure every catalog course has a slot assignment.
            // Courses with offerings are already in courseYearSemMap.
            // Courses WITHOUT offerings: derive year from course-code numeric
            // prefix (e.g. CS2xx → Year 2) and split evenly across semesters.
            var unmappedCourses = allCourses
                .Where(c => !courseYearSemMap.ContainsKey(c.Id))
                .ToList();

            if (unmappedCourses.Count > 0)
            {
                var byYear = unmappedCourses
                    .GroupBy(c => DeriveYearFromCode(c.Code));

                foreach (var grp in byYear)
                {
                    int yr = grp.Key;
                    var sorted = grp.OrderBy(c => c.Code).ToList();
                    int half = (sorted.Count + 1) / 2;
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        int sem = i < half ? 1 : 2;
                        courseYearSemMap[sorted[i].Id] = (yr, sem);
                    }
                }
            }

            // Build override maps for equivalency registrations:
            //   yearOverride: courseId → admin-selected year (1-4) from TranscriptYear
            //   semOverride:  courseId → admin-selected sem (1-2) from SemesterId odd/even
            var equivYearOverride = equivByCourseId
                .Where(kv => kv.Value.TranscriptYear.HasValue)
                .ToDictionary(kv => kv.Key, kv => kv.Value.TranscriptYear!.Value);

            var equivSemOverride = equivByCourseId.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.SemesterId % 2 == 0 ? 2 : 1);

            var yearsDto = new Dictionary<string, AcademicSetupYearDto>();
            for (int yr = 1; yr <= 4; yr++)
            {
                var semDto = new Dictionary<string, List<AcademicSetupCourseDto>>();
                for (int sem = 1; sem <= 2; sem++)
                {
                    // Equivalency courses use admin-stored year+semester (no catalog fallback).
                    // Non-equivalency catalog courses use courseYearSemMap as before.
                    var coursesInSlot = allCourses
                        .Where(c =>
                        {
                            if (equivByCourseId.ContainsKey(c.Id))
                            {
                                // Admin-selected year wins; fall back to catalog only if TranscriptYear was never set
                                int effectiveYear = equivYearOverride.TryGetValue(c.Id, out var ey)
                                    ? ey
                                    : (courseYearSemMap.TryGetValue(c.Id, out var ys0) ? ys0.year : 1);
                                int effectiveSem = equivSemOverride[c.Id];
                                return effectiveYear == yr && effectiveSem == sem;
                            }
                            // Non-equivalency: use catalog placement
                            if (!courseYearSemMap.TryGetValue(c.Id, out var ys)) return false;
                            return ys.year == yr && ys.sem == sem;
                        })
                        .Select(c =>
                        {
                            equivByCourseId.TryGetValue(c.Id, out var eqReg);
                            bool isSelected = eqReg != null;
                            var (_, letter, gpaPoints) = isSelected && eqReg!.NumericTotal.HasValue
                                ? DeriveGrade(eqReg.NumericTotal.Value)
                                : (default(Grads), (string?)null, (decimal?)null);

                            return new AcademicSetupCourseDto
                            {
                                CourseCode = c.Code,
                                Name = c.Name,
                                Credits = c.Credits,
                                Selected = isSelected,
                                Total = eqReg?.NumericTotal,
                                Grade = letter,
                                GpaPoints = gpaPoints,
                                IsEquivalency = isSelected
                            };
                        })
                        .OrderBy(c => c.CourseCode)
                        .ToList();
                    semDto[sem.ToString()] = coursesInSlot;
                }
                yearsDto[yr.ToString()] = new AcademicSetupYearDto { Semesters = semDto };
            }

            return new AcademicSetupResponseDto
            {
                Student = new AcademicSetupStudentDto
                {
                    Id = user.Academic_Code ?? user.Id,
                    Name = user.DisplayName,
                    Email = user.Email,
                    CurrentYear = currentYear,
                    Gpa = user.TotalGPA ?? 0,
                    TotalCreditsEarned = totalCreditsEarned,
                    Standing = standing
                },
                AcademicSetup = new AcademicSetupDataDto
                {
                    CurrentYear = currentYear,
                    Years = yearsDto
                }
            };
        }

        public async Task<StudentTranscriptResponseDto> GetStudentTranscriptAsync(string studentId)
        {
            var user = await ResolveStudent(studentId);

            var allCourses   = (await _unitOfWork.Courses.GetAllAsync()).ToList();
            var coursesById  = allCourses.ToDictionary(c => c.Id);
            var offerings    = (await _unitOfWork.CourseOfferings.GetAllAsync()).ToList();
            var courseYearSemMap = BuildCourseYearSemMap(allCourses, offerings);

            var allRegs = await _unitOfWork.Registrations.GetByUserIdAsync(user.Id);

            // Only include courses with a real admin-entered numeric grade
            var gradedRegs = allRegs
                .Where(r => r.IsEquivalency && r.IsPassed && r.NumericTotal.HasValue)
                .ToList();

            int totalCreditsEarned = gradedRegs
                .Sum(r => coursesById.TryGetValue(r.CourseId, out var c) ? c.Credits : 0);

            var completedCourses = gradedRegs
                .Select(r =>
                {
                    if (!coursesById.TryGetValue(r.CourseId, out var course)) return null;
                    // Admin-selected year wins; fall back to catalog only for legacy records without TranscriptYear
                    int yr = r.TranscriptYear.HasValue
                        ? r.TranscriptYear.Value
                        : (courseYearSemMap.TryGetValue(r.CourseId, out var ys) && ys.year > 0
                            ? ys.year
                            : DeriveYearFromCode(course.Code));
                    // Admin-selected semester: SemesterId odd=1, even=2
                    int sem = r.SemesterId % 2 == 0 ? 2 : 1;
                    var (_, letter, gpaPoints) = DeriveGrade(r.NumericTotal!.Value);
                    return new TranscriptCourseDto
                    {
                        CourseCode = course.Code,
                        Name       = course.Name,
                        Credits    = course.Credits,
                        Year       = yr,
                        Semester   = sem,
                        Total      = r.NumericTotal.Value,
                        Grade      = letter ?? "F",
                        GpaPoints  = gpaPoints
                    };
                })
                .Where(e => e != null)
                .Cast<TranscriptCourseDto>()
                .OrderBy(e => e.Year).ThenBy(e => e.Semester).ThenBy(e => e.CourseCode)
                .ToList();

            return new StudentTranscriptResponseDto
            {
                Student = new AcademicSetupStudentDto
                {
                    Id                 = user.Academic_Code ?? user.Id,
                    Name               = user.DisplayName,
                    Email              = user.Email,
                    CurrentYear        = LevelToYearNum(user.Level),
                    Gpa                = user.TotalGPA ?? 0,
                    TotalCreditsEarned = totalCreditsEarned,
                    Standing           = ComputeStanding(user.TotalGPA ?? 0)
                },
                CompletedCourses = completedCourses
            };
        }

        public async Task<AcademicSetupSaveResultDto> SaveAcademicSetupAsync(
            string studentId, AcademicSetupSaveRequestDto dto)
        {
            var user = await ResolveStudent(studentId);

            // 1. Validate currentYear
            if (dto.CurrentYear < 1 || dto.CurrentYear > 4)
                throw new BadRequestException("Current year must be between 1 and 4.");

            // 2. Collect all courses from request, validate
            var allCourses = (await _unitOfWork.Courses.GetAllAsync()).ToList();
            var courseMap = allCourses.ToDictionary(c => c.Code, c => c, StringComparer.OrdinalIgnoreCase);

            var flatEntries = new List<(string code, int total, int yearKey, int? adminSemester)>();
            var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var (yearKey, yearData) in dto.Years)
            {
                if (!int.TryParse(yearKey, out var yrNum) || yrNum < 1 || yrNum > 4) continue;
                foreach (var entry in yearData.CompletedCourses ?? new())
                {
                    // Validate course exists
                    if (!courseMap.ContainsKey(entry.CourseCode))
                        throw new NotFoundException($"Course '{entry.CourseCode}' was not found.");

                    // Validate total range (must be passing — 60-100)
                    if (entry.Total < 60 || entry.Total > 100)
                        throw new BadRequestException(
                            $"Equivalency total for '{entry.CourseCode}' must be between 60 and 100.");

                    // Detect duplicates
                    if (!seenCodes.Add(entry.CourseCode))
                        throw new BadRequestException(
                            $"Course '{entry.CourseCode}' appears more than once in the academic setup request.");

                    flatEntries.Add((entry.CourseCode, entry.Total, yrNum, entry.Semester));
                }
            }

            // 3. Load course offerings for year/semester derivation
            var offerings = (await _unitOfWork.CourseOfferings.GetAllAsync()).ToList();
            var courseYearSemMap = BuildCourseYearSemMap(allCourses, offerings);

            // 4. Get current study year + semesters for FK references
            var currentSY = await _unitOfWork.StudyYears.GetCurrentStudyYearAsync();
            if (currentSY == null)
                throw new BadRequestException("No active study year found.");

            var semesters = (await _unitOfWork.Semesters.GetByStudyYearIdAsync(currentSY.Id)).ToList();
            var sem1 = semesters.FirstOrDefault(s => s.Title == SemesterEnum.First_Semester);
            var sem2 = semesters.FirstOrDefault(s => s.Title == SemesterEnum.Second_Semester);
            int defaultSemId = sem1?.Id ?? semesters.FirstOrDefault()?.Id ?? 1;

            // 5. Detect courses with active (non-equivalency) registrations — these are current-term.
            // Academic Setup for a currently-registered course writes to FinalGrade instead of
            // creating an equivalency registration, avoiding duplicate rows.
            var allUserRegs = await _unitOfWork.Registrations.GetByUserIdAsync(user.Id);
            var activeNonEquivCourseIds = allUserRegs
                .Where(r => !r.IsEquivalency &&
                            (r.Status == RegistrationStatus.Approved || r.Status == RegistrationStatus.Pending))
                .Select(r => r.CourseId)
                .ToHashSet();

            // Remove old equivalency registrations for this student (tracked query)
            var existingEquivRegs = allUserRegs;
            var existingEquivIds = existingEquivRegs
                .Where(r => r.IsEquivalency)
                .Select(r => r.Id)
                .ToList();

            foreach (var regId in existingEquivIds)
            {
                var tracked = await _unitOfWork.Registrations.GetByIdAsync(regId);
                if (tracked != null)
                    await _unitOfWork.Registrations.Delete(tracked);
            }

            // 6. Insert new equivalency registrations OR upsert FinalGrade for current-term courses
            foreach (var (code, total, yearKey, adminSemester) in flatEntries)
            {
                var course = courseMap[code];
                var (gradeEnum, _, _) = DeriveGrade(total);

                if (activeNonEquivCourseIds.Contains(course.Id))
                {
                    // Current-term course: write AdminFinalTotal to FinalGrade and publish it.
                    // This is the shared grade source that Student Grades reads.
                    var fg = await _unitOfWork.FinalGrades.GetAsync(user.Id, course.Id);
                    if (fg == null)
                    {
                        await _unitOfWork.FinalGrades.AddAsync(new FinalGrade
                        {
                            StudentId      = user.Id,
                            CourseId       = course.Id,
                            AdminFinalTotal = total,
                            Published      = true,
                        });
                    }
                    else
                    {
                        fg.AdminFinalTotal = total;
                        fg.Published       = true;
                        await _unitOfWork.FinalGrades.UpdateAsync(fg);
                    }
                    continue; // No equivalency registration needed
                }

                // Historical course: create equivalency registration (appears in Transcript)
                // Determine semester: admin-supplied value wins; fall back to catalog mapping
                int semId = defaultSemId;
                if (adminSemester.HasValue)
                {
                    semId = adminSemester.Value == 2 ? (sem2?.Id ?? defaultSemId) : defaultSemId;
                }
                else if (courseYearSemMap.TryGetValue(course.Id, out var ys))
                {
                    semId = ys.sem == 2 ? (sem2?.Id ?? defaultSemId) : defaultSemId;
                }

                var reg = new Registration
                {
                    UserId = user.Id,
                    CourseId = course.Id,
                    StudyYearId = currentSY.Id,
                    SemesterId = semId,
                    TranscriptYear = yearKey,   // persist admin-selected year (1-4)
                    Status = RegistrationStatus.Approved,
                    Progress = CourseProgress.Completed,
                    IsPassed = true,
                    Grade = gradeEnum,
                    IsEquivalency = true,
                    NumericTotal = total,
                    RegisteredAt = DateTime.UtcNow
                };

                await _unitOfWork.Registrations.AddAsync(reg);
            }

            // 7. Update student's current year
            user.Level = YearNumToLevel(dto.CurrentYear);

            // 8. Recalculate GPA from ALL passed registrations (real + equivalency)
            // Must save first to persist new registrations, then re-query
            await _unitOfWork.SaveChangesAsync();

            var allRegs = await _unitOfWork.Registrations.GetByUserIdAsync(user.Id);
            var passedRegs = allRegs.Where(r => r.IsPassed).ToList();

            decimal totalWeightedPoints = 0;
            int totalCreditSum = 0;
            foreach (var reg in passedRegs)
            {
                var course = allCourses.FirstOrDefault(c => c.Id == reg.CourseId);
                if (course == null) continue;

                decimal gpaPoints = 0;
                if (reg.Grade.HasValue)
                    gpaPoints = GradeToGpaPoints(reg.Grade.Value);
                else if (reg.NumericTotal.HasValue)
                    gpaPoints = DeriveGrade(reg.NumericTotal.Value).gpaPoints;

                totalWeightedPoints += course.Credits * gpaPoints;
                totalCreditSum += course.Credits;
            }

            decimal newGpa = totalCreditSum > 0
                ? Math.Round(totalWeightedPoints / totalCreditSum, 2)
                : 0;

            user.TotalGPA = newGpa;

            // 9. Update standing-based allowed credits
            var standing = ComputeStanding(newGpa);
            user.AllowedCredits = standing.MaxCredits;

            // 10. Persist user changes
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                throw new InternalServerErrorException("Failed to update student profile.");

            // 11. Build response
            int totalCreditsEarned = passedRegs
                .Sum(r => allCourses.FirstOrDefault(c => c.Id == r.CourseId)?.Credits ?? 0);

            var equivCourseDtos = flatEntries.Select(e =>
            {
                var course = courseMap[e.code];
                var (_, letter, gpaPoints) = DeriveGrade(e.total);
                courseYearSemMap.TryGetValue(course.Id, out var ys);
                int finalSem = e.adminSemester ?? (ys.sem > 0 ? ys.sem : 1);
                return new AcademicSetupCompletedCourseDto
                {
                    Code = course.Code,
                    Total = e.total,
                    Year = ys.year > 0 ? ys.year : e.yearKey,
                    Semester = finalSem,
                    Grade = letter!,
                    GpaPoints = gpaPoints,
                    IsEquivalency = true
                };
            }).ToList();

            return new AcademicSetupSaveResultDto
            {
                StudentId = user.Academic_Code ?? user.Id,
                CurrentYear = dto.CurrentYear,
                Gpa = newGpa,
                TotalCreditsEarned = totalCreditsEarned,
                Standing = standing,
                CompletedCourses = equivCourseDtos
            };
        }

        // ═══════════════════════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════════════════════

        private static AcademicStandingDto ComputeStanding(decimal gpa)
        {
            return gpa switch
            {
                >= 3.5m => new AcademicStandingDto
                {
                    StandingId = "excellent", Gpa = gpa, MaxCredits = 21,
                    MustRetakeFirst = false, CanOnlyRetake = false
                },
                >= 3.0m => new AcademicStandingDto
                {
                    StandingId = "vgood", Gpa = gpa, MaxCredits = 18,
                    MustRetakeFirst = false, CanOnlyRetake = false
                },
                >= 2.5m => new AcademicStandingDto
                {
                    StandingId = "good", Gpa = gpa, MaxCredits = 18,
                    MustRetakeFirst = false, CanOnlyRetake = false
                },
                >= 2.0m => new AcademicStandingDto
                {
                    StandingId = "pass", Gpa = gpa, MaxCredits = 15,
                    MustRetakeFirst = false, CanOnlyRetake = false
                },
                >= 1.5m => new AcademicStandingDto
                {
                    StandingId = "warning", Gpa = gpa, MaxCredits = 12,
                    MustRetakeFirst = true, CanOnlyRetake = false
                },
                _ => new AcademicStandingDto
                {
                    StandingId = "probation", Gpa = gpa, MaxCredits = 9,
                    MustRetakeFirst = true, CanOnlyRetake = true
                }
            };
        }

        private static string? GradeEnumToLetter(Grads? grade) => grade switch
        {
            Grads.A_Plus  => "A+",
            Grads.A       => "A",
            Grads.A_Minus => "A-",
            Grads.B_Plus  => "B+",
            Grads.B       => "B",
            Grads.B_Minus => "B-",
            Grads.C_Plus  => "C+",
            Grads.C       => "C",
            Grads.C_Minus => "C-",
            Grads.D_Plus  => "D+",
            Grads.D       => "D",
            Grads.F       => "F",
            _             => null
        };

        private async Task<User> ResolveStudent(string studentId)
        {
            var user = await _userManager.FindByIdAsync(studentId)
                       ?? await _userManager.Users.FirstOrDefaultAsync(u => u.Academic_Code == studentId);
            if (user == null)
                throw new NotFoundException("Student not found.");

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any(r => r.Equals("Student", StringComparison.OrdinalIgnoreCase)))
                throw new BadRequestException("User is not a student.");

            return user;
        }

        private async Task<AdminRegistrationStatusDto> MapRegistrationSettingsAsync(RegistrationSettings s)
        {
            // Enrich with course catalog data
            var allCourses = (await _unitOfWork.Courses.GetAllAsync())
                .ToDictionary(c => c.Code, c => c, StringComparer.OrdinalIgnoreCase);

            var openedByYearDto = new Dictionary<string, List<OpenedCourseEntryDto>>();

            if (!string.IsNullOrWhiteSpace(s.OpenedCoursesByYear))
            {
                // Try rich format first
                try
                {
                    var rich = JsonSerializer.Deserialize<Dictionary<string, List<OpenedCourseEntryInternal>>>(
                        s.OpenedCoursesByYear,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (rich != null && rich.Count > 0)
                    {
                        foreach (var (k, entries) in rich)
                        {
                            openedByYearDto[k] = entries.Select(e =>
                            {
                                allCourses.TryGetValue(e.CourseCode, out var c);
                                return new OpenedCourseEntryDto
                                {
                                    CourseId = c?.Id ?? 0,
                                    Code = e.CourseCode,
                                    Name = c?.Name ?? e.CourseCode,
                                    AvailableSeats = e.IsUnlimitedSeats ? (object)"unlimited" : e.AvailableSeats,
                                    IsUnlimitedSeats = e.IsUnlimitedSeats
                                };
                            }).ToList();
                        }
                    }
                }
                catch
                {
                    // Try legacy flat format
                    try
                    {
                        var flat = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(s.OpenedCoursesByYear);
                        if (flat != null)
                        {
                            foreach (var (k, codes) in flat)
                            {
                                openedByYearDto[k] = codes.Select(code =>
                                {
                                    allCourses.TryGetValue(code, out var c);
                                    return new OpenedCourseEntryDto
                                    {
                                        CourseId = c?.Id ?? 0,
                                        Code = code,
                                        Name = c?.Name ?? code,
                                        AvailableSeats = (object)"unlimited",
                                        IsUnlimitedSeats = true
                                    };
                                }).ToList();
                            }
                        }
                    }
                    catch { /* malformed */ }
                }
            }
            else if (!string.IsNullOrWhiteSpace(s.EnabledCourses))
            {
                // Backward compat from legacy flat fields
                var codes = s.EnabledCourses
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
                var years = string.IsNullOrWhiteSpace(s.OpenYears)
                    ? new List<int>()
                    : s.OpenYears
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(x => int.TryParse(x, out var n) ? n : 0)
                        .Where(n => n > 0)
                        .ToList();
                var entryList = codes.Select(code =>
                {
                    allCourses.TryGetValue(code, out var c);
                    return new OpenedCourseEntryDto
                    {
                        CourseId = c?.Id ?? 0,
                        Code = code,
                        Name = c?.Name ?? code,
                        AvailableSeats = (object)"unlimited",
                        IsUnlimitedSeats = true
                    };
                }).ToList();
                foreach (var yr in years)
                    openedByYearDto[yr.ToString()] = new List<OpenedCourseEntryDto>(entryList);
                if (years.Count == 0 && codes.Count > 0)
                    openedByYearDto["1"] = entryList;
            }

            return new AdminRegistrationStatusDto
            {
                IsOpen = s.IsOpen,
                Semester = s.Semester,
                AcademicYear = s.AcademicYear,
                StartDate = s.StartDate?.ToString("yyyy-MM-dd"),
                Deadline = s.Deadline?.ToString("yyyy-MM-dd"),
                OpenedCoursesByYear = openedByYearDto,
                MaxCredits = s.MaxCredits
            };
        }

        private static string GenerateSecurePassword(int length)
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*";
            const string allChars = upper + lower + digits + special;

            var password = new char[length];
            var rng = RandomNumberGenerator.Create();

            // Guarantee at least one of each category
            password[0] = PickRandom(upper, rng);
            password[1] = PickRandom(lower, rng);
            password[2] = PickRandom(digits, rng);
            password[3] = PickRandom(special, rng);

            // Fill the rest randomly
            for (int i = 4; i < length; i++)
                password[i] = PickRandom(allChars, rng);

            // Shuffle
            for (int i = password.Length - 1; i > 0; i--)
            {
                var j = GetRandomInt(rng, i + 1);
                (password[i], password[j]) = (password[j], password[i]);
            }

            return new string(password);
        }

        private static char PickRandom(string chars, RandomNumberGenerator rng)
        {
            return chars[GetRandomInt(rng, chars.Length)];
        }

        private static int GetRandomInt(RandomNumberGenerator rng, int max)
        {
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            return (int)(BitConverter.ToUInt32(bytes, 0) % (uint)max);
        }

        // ── Academic Setup Helpers ────────────────────────────────

        /// <summary>Derive grade enum, letter, and GPA points from numeric total (0-100).</summary>
        private static (Grads grade, string letter, decimal gpaPoints) DeriveGrade(int total) => total switch
        {
            >= 97 => (Grads.A_Plus,  "A+", 4.0m),
            >= 93 => (Grads.A,       "A",  4.0m),
            >= 90 => (Grads.A_Minus, "A-", 3.7m),
            >= 87 => (Grads.B_Plus,  "B+", 3.3m),
            >= 83 => (Grads.B,       "B",  3.0m),
            >= 80 => (Grads.B_Minus, "B-", 2.7m),
            >= 77 => (Grads.C_Plus,  "C+", 2.3m),
            >= 73 => (Grads.C,       "C",  2.0m),
            >= 70 => (Grads.C_Minus, "C-", 1.7m),
            >= 67 => (Grads.D_Plus,  "D+", 1.3m),
            >= 60 => (Grads.D,       "D",  1.0m),
            _     => (Grads.F,       "F",  0.0m)
        };

        /// <summary>Convert Grads enum to GPA points for credit-weighted GPA calculation.</summary>
        private static decimal GradeToGpaPoints(Grads grade) => grade switch
        {
            Grads.A_Plus  => 4.0m,
            Grads.A       => 4.0m,
            Grads.A_Minus => 3.7m,
            Grads.B_Plus  => 3.3m,
            Grads.B       => 3.0m,
            Grads.B_Minus => 2.7m,
            Grads.C_Plus  => 2.3m,
            Grads.C       => 2.0m,
            Grads.C_Minus => 1.7m,
            Grads.D_Plus  => 1.3m,
            Grads.D       => 1.0m,
            _             => 0.0m
        };

        /// <summary>Convert Levels enum to numeric year (1-4). Default 1.</summary>
        private static int LevelToYearNum(Levels? level) => level switch
        {
            Levels.First_Year  => 1,
            Levels.Second_Year => 2,
            Levels.Third_Year  => 3,
            Levels.Fourth_Year => 4,
            _ => 1
        };

        /// <summary>Convert numeric year (1-4) to Levels enum.</summary>
        private static Levels YearNumToLevel(int year) => year switch
        {
            1 => Levels.First_Year,
            2 => Levels.Second_Year,
            3 => Levels.Third_Year,
            4 => Levels.Fourth_Year,
            _ => Levels.First_Year
        };

        /// <summary>
        /// Build a mapping of courseId → (year, semester) from CourseOffering data.
        /// Uses the Levels enum ordinal to determine year number, and
        /// SemesterId to determine semester 1 vs 2.
        /// </summary>
        private static Dictionary<int, (int year, int sem)> BuildCourseYearSemMap(
            List<Course> allCourses, List<CourseOffering> offerings)
        {
            var map = new Dictionary<int, (int year, int sem)>();

            // Group offerings by courseId, take one representative per course
            var grouped = offerings
                .GroupBy(o => o.CourseId)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var course in allCourses)
            {
                if (grouped.TryGetValue(course.Id, out var off))
                {
                    int yr = off.Level switch
                    {
                        Levels.First_Year  => 1,
                        Levels.Second_Year => 2,
                        Levels.Third_Year  => 3,
                        Levels.Fourth_Year => 4,
                        _ => 1
                    };
                    // Heuristic: if SemesterId is odd → sem 1, even → sem 2
                    // This works with typical seeding where semester 1 has an odd Id
                    int sem = off.SemesterId % 2 == 0 ? 2 : 1;
                    map[course.Id] = (yr, sem);
                }
            }

            return map;
        }

        /// <summary>
        /// Derive academic year (1-4) from the numeric portion of a course code.
        /// E.g. CS201 → 2, BS101 → 1, CS411 → 4.  Codes with 5xx+ map to Year 4.
        /// </summary>
        private static int DeriveYearFromCode(string code)
        {
            var digits = new string(code.Where(char.IsDigit).ToArray());
            if (digits.Length >= 3)
            {
                int first = digits[0] - '0';
                if (first >= 1 && first <= 4) return first;
                if (first >= 5) return 4; // electives → Year 4
            }
            return 1; // ultimate fallback
        }

        // ═══════════════════════════════════════════════════════════
        // 3.9 Instructor Control
        // Assignments are course-based and persistent. They survive
        // registration open/close cycles and are only cleared when
        // the admin explicitly changes them or an instructor is deleted.
        // ═══════════════════════════════════════════════════════════

        public async Task<InstructorControlDto> GetInstructorControlAsync()
        {
            var settings = await _unitOfWork.RegistrationSettings.GetCurrentAsync();

            // All instructors are always returned (needed for the assign modal)
            var instructorUsers = await _userManager.GetUsersInRoleAsync("Instructor");
            var allInstructors = instructorUsers
                .Select(u => new InstructorItemDto
                {
                    Id    = u.Id,
                    Name  = u.DisplayName ?? u.Email ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    Code  = u.Academic_Code ?? string.Empty
                })
                .OrderBy(i => i.Name)
                .ToList();

            if (settings == null || !settings.IsOpen || string.IsNullOrWhiteSpace(settings.OpenedCoursesByYear))
                return new InstructorControlDto
                {
                    IsOpen         = false,
                    Courses        = [],
                    AllInstructors = allInstructors
                };

            // Determine which courses are open in the current registration window
            var byYear = JsonSerializer.Deserialize<Dictionary<string, List<OpenedCourseEntryInternal>>>(
                settings.OpenedCoursesByYear) ?? [];

            var allCodes    = byYear.Values.SelectMany(list => list.Select(e => e.CourseCode)).ToHashSet();
            var allCoursesDb = await _unitOfWork.Courses.GetAllAsync();
            var courseMap   = allCoursesDb
                .Where(c => allCodes.Contains(c.Code))
                .ToDictionary(c => c.Code, c => c);

            var openCourseIds = courseMap.Values.Select(c => c.Id).ToList();

            // Load persistent assignments for the open courses (not cycle-scoped)
            var assignments = await _unitOfWork.RegistrationCourseInstructors
                .GetByCourseIdsAsync(openCourseIds);

            var assignedMap = assignments
                .GroupBy(a => a.CourseId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(a => new InstructorItemDto
                    {
                        Id    = a.Instructor.Id,
                        Name  = a.Instructor.DisplayName ?? a.Instructor.Email ?? string.Empty,
                        Email = a.Instructor.Email ?? string.Empty,
                        Code  = a.Instructor.Academic_Code ?? string.Empty
                    }).ToList()
                );

            var courses = new List<InstructorCourseItemDto>();
            foreach (var (yearKey, entries) in byYear.OrderBy(k => k.Key))
            {
                int year = int.TryParse(yearKey, out var y) ? y : 0;
                foreach (var entry in entries)
                {
                    if (!courseMap.TryGetValue(entry.CourseCode, out var course)) continue;
                    courses.Add(new InstructorCourseItemDto
                    {
                        Id      = course.Id,
                        Code    = course.Code,
                        Name    = course.Name,
                        Credits = course.Credits,
                        Year    = year,
                        AssignedInstructors = assignedMap.TryGetValue(course.Id, out var list) ? list : []
                    });
                }
            }

            return new InstructorControlDto
            {
                IsOpen         = true,
                Courses        = courses,
                AllInstructors = allInstructors
            };
        }

        public async Task AssignInstructorsAsync(int courseId, AssignInstructorsDto dto)
        {
            // Course must exist
            var allCoursesDb = await _unitOfWork.Courses.GetAllAsync();
            var course = allCoursesDb.FirstOrDefault(c => c.Id == courseId)
                ?? throw new NotFoundException($"Course ID {courseId} not found.");

            // Validate that every supplied ID belongs to an active Instructor account
            var validInstructorIds = new List<string>();
            foreach (var id in dto.InstructorIds.Distinct())
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) continue;
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Instructor")) validInstructorIds.Add(id);
            }

            // Replace-all: remove current assignments for this course, then re-add
            var existing = await _unitOfWork.RegistrationCourseInstructors
                .GetByCourseAsync(courseId);
            if (existing.Count > 0)
                await _unitOfWork.RegistrationCourseInstructors.RemoveRangeAsync(existing);

            // Add new assignments
            if (validInstructorIds.Count > 0)
            {
                var newEntries = validInstructorIds.Select(iid => new RegistrationCourseInstructor
                {
                    CourseId     = courseId,
                    InstructorId = iid,
                    AssignedAt   = DateTime.UtcNow
                });
                await _unitOfWork.RegistrationCourseInstructors.AddRangeAsync(newEntries);
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
