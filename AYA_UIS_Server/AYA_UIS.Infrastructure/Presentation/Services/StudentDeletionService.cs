using Abstraction.Contracts;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using AYA_UIS.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Presistence;
using Shared.Dtos.Admin_Module;

namespace Presentation.Services
{
    /// <summary>
    /// Permanent student deletion (Email Manager → Danger Zone).
    ///
    /// Preview is read-only. Execute requires the fixed password and a
    /// re-typed academic code, then in a single DbContext transaction:
    ///   1. Removes every row tied to the student via UserId/StudentId
    ///   2. Removes the AspNetUsers row (UserManager.DeleteAsync cascades roles/claims/tokens/logins)
    ///   3. Writes an audit row
    /// After commit, best-effort deletes the student's physical submission files.
    /// Shared course/material rows (Courses, Assignments, Quizzes, CourseUploads,
    /// QuizQuestions/Options, RegistrationCourseInstructors) are NEVER touched.
    /// </summary>
    public class StudentDeletionService : IStudentDeletionService
    {
        private readonly UniversityDbContext  _ctx;
        private readonly UserManager<User>    _userManager;
        private readonly IConfiguration       _config;
        private readonly ILocalFileService    _fileService;
        private readonly ILogger<StudentDeletionService> _logger;

        // Per spec — fixed strong-confirmation password (overridable via config).
        private const string DEFAULT_PASSWORD = "StudentDelete@123#";

        public StudentDeletionService(
            UniversityDbContext  ctx,
            UserManager<User>    userManager,
            IConfiguration       config,
            ILocalFileService    fileService,
            ILogger<StudentDeletionService> logger)
        {
            _ctx          = ctx;
            _userManager  = userManager;
            _config       = config;
            _fileService  = fileService;
            _logger       = logger;
        }

        // ═════════════════════════════════════════════════════════════
        // PREVIEW
        // ═════════════════════════════════════════════════════════════
        public async Task<StudentDeletionPreviewDto> PreviewAsync(string academicCode)
        {
            var user = await ResolveStudentAsync(academicCode);

            var registrations = await _ctx.Registrations.AsNoTracking()
                .Where(r => r.UserId == user.Id).ToListAsync();
            int registeredActive = registrations.Count(r =>
                r.Status == RegistrationStatus.Approved || r.Status == RegistrationStatus.Pending);

            int subs = await _ctx.AssignmentSubmissions.AsNoTracking()
                .CountAsync(s => s.StudentId == user.Id);

            int attempts = await _ctx.StudentQuizAttempts.AsNoTracking()
                .CountAsync(a => a.StudentId == user.Id);

            // Year label — best effort from the most recent active registration.
            string? yearLabel = LevelLabel(user.Level);
            string? semesterLabel = null;
            var lastReg = registrations
                .Where(r => r.Status == RegistrationStatus.Approved || r.Status == RegistrationStatus.Pending)
                .OrderByDescending(r => r.SemesterId).FirstOrDefault();
            if (lastReg != null)
            {
                semesterLabel = lastReg.SemesterId % 2 == 0 ? "Semester 2" : "Semester 1";
            }

            return new StudentDeletionPreviewDto
            {
                AcademicCode           = user.Academic_Code ?? string.Empty,
                Name                   = user.DisplayName,
                Email                  = user.Email,
                Year                   = yearLabel,
                Semester               = semesterLabel,
                RegisteredCoursesCount = registeredActive,
                TotalRegistrations     = registrations.Count,
                SubmissionsCount       = subs,
                QuizAttemptsCount      = attempts,
            };
        }

        // ═════════════════════════════════════════════════════════════
        // EXECUTE
        // ═════════════════════════════════════════════════════════════
        public async Task<StudentDeletionResultDto> ExecuteAsync(
            string adminId,
            StudentDeletionExecuteRequestDto request)
        {
            if (request == null) throw new BadRequestException("Body required.");
            if (string.IsNullOrWhiteSpace(request.AcademicCode))
                throw new BadRequestException("Academic code is required.");

            // 1. Backend re-validation of every confirmation knob.
            if (!string.Equals(request.AcademicCode, request.ConfirmAcademicCode, StringComparison.Ordinal))
                throw new BadRequestException("Confirmation academic code does not match.");

            var expectedPwd = _config["StudentDelete:Password"] ?? DEFAULT_PASSWORD;
            if (!string.Equals(request.Password, expectedPwd, StringComparison.Ordinal))
                throw new BadRequestException("Reset password is incorrect.");

            // 2. Resolve & guard.
            var user = await ResolveStudentAsync(request.AcademicCode);
            if (string.Equals(user.Id, adminId, StringComparison.Ordinal))
                throw new ForbiddenException("Cannot delete your own account.");

            // Snapshot identifiers before the row vanishes.
            var snap = new
            {
                Id           = user.Id,
                Email        = user.Email,
                Name         = user.DisplayName,
                AcademicCode = user.Academic_Code ?? string.Empty,
            };
            var submissionFileUrls = await _ctx.AssignmentSubmissions.AsNoTracking()
                .Where(s => s.StudentId == snap.Id && !string.IsNullOrEmpty(s.FileUrl))
                .Select(s => s.FileUrl)
                .ToListAsync();

            var counts = new StudentDeletionCountsDto();

            await using var tx = await _ctx.Database.BeginTransactionAsync();
            try
            {
                // ── 3. Delete child rows in safe order ──

                // Quiz answers → attempts
                var attemptIds = await _ctx.StudentQuizAttempts
                    .Where(a => a.StudentId == snap.Id)
                    .Select(a => a.Id).ToListAsync();
                if (attemptIds.Count > 0)
                {
                    var answers = await _ctx.StudentAnswers
                        .Where(sa => attemptIds.Contains(sa.AttemptId)).ToListAsync();
                    if (answers.Count > 0) _ctx.StudentAnswers.RemoveRange(answers);
                    counts.QuizAnswersRemoved = answers.Count;
                }
                var attempts = await _ctx.StudentQuizAttempts
                    .Where(a => a.StudentId == snap.Id).ToListAsync();
                if (attempts.Count > 0) _ctx.StudentQuizAttempts.RemoveRange(attempts);
                counts.QuizAttemptsRemoved = attempts.Count;

                // Assignment submissions (rows; files purged after commit)
                var subs = await _ctx.AssignmentSubmissions
                    .Where(s => s.StudentId == snap.Id).ToListAsync();
                if (subs.Count > 0) _ctx.AssignmentSubmissions.RemoveRange(subs);
                counts.AssignmentSubmissionsRemoved = subs.Count;

                // Final grade review classifications
                var fgr = await _ctx.FinalGradeReviews
                    .Where(r => r.StudentId == snap.Id).ToListAsync();
                if (fgr.Count > 0) _ctx.FinalGradeReviews.RemoveRange(fgr);
                counts.FinalGradeReviewsRemoved = fgr.Count;

                // Final grades
                var fg = await _ctx.FinalGrades
                    .Where(g => g.StudentId == snap.Id).ToListAsync();
                if (fg.Count > 0) _ctx.FinalGrades.RemoveRange(fg);
                counts.FinalGradesRemoved = fg.Count;

                // Midterm grades
                var mg = await _ctx.MidtermGrades
                    .Where(g => g.StudentId == snap.Id).ToListAsync();
                if (mg.Count > 0) _ctx.MidtermGrades.RemoveRange(mg);
                counts.MidtermGradesRemoved = mg.Count;

                // Course results
                var cr = await _ctx.CourseResults
                    .Where(c => c.UserId == snap.Id).ToListAsync();
                if (cr.Count > 0) _ctx.CourseResults.RemoveRange(cr);
                counts.CourseResultsRemoved = cr.Count;

                // Semester GPAs
                var sg = await _ctx.SemesterGPAs
                    .Where(g => g.UserId == snap.Id).ToListAsync();
                if (sg.Count > 0) _ctx.SemesterGPAs.RemoveRange(sg);
                counts.SemesterGpasRemoved = sg.Count;

                // User study years
                var usy = await _ctx.UserStudyYears
                    .Where(u => u.UserId == snap.Id).ToListAsync();
                if (usy.Count > 0) _ctx.UserStudyYears.RemoveRange(usy);
                counts.UserStudyYearsRemoved = usy.Count;

                // Student course exceptions
                var sce = await _ctx.StudentCourseExceptions
                    .Where(e => e.UserId == snap.Id).ToListAsync();
                if (sce.Count > 0) _ctx.StudentCourseExceptions.RemoveRange(sce);
                counts.CourseExceptionsRemoved = sce.Count;

                // Admin per-student course locks
                var locks = await _ctx.AdminCourseLocks
                    .Where(l => l.UserId == snap.Id).ToListAsync();
                if (locks.Count > 0) _ctx.AdminCourseLocks.RemoveRange(locks);
                counts.AdminCourseLocksRemoved = locks.Count;

                // Registrations
                var regs = await _ctx.Registrations
                    .Where(r => r.UserId == snap.Id).ToListAsync();
                if (regs.Count > 0) _ctx.Registrations.RemoveRange(regs);
                counts.RegistrationsRemoved = regs.Count;

                // Academic Year Reset per-student snapshots
                var snaps = await _ctx.AcademicYearResetSnapshots
                    .Where(r => r.StudentId == snap.Id).ToListAsync();
                if (snaps.Count > 0) _ctx.AcademicYearResetSnapshots.RemoveRange(snaps);
                counts.ResetSnapshotsRemoved = snaps.Count;

                // Notifications
                var notifs = await _ctx.Notifications
                    .Where(n => n.UserId == snap.Id).ToListAsync();
                if (notifs.Count > 0) _ctx.Notifications.RemoveRange(notifs);
                counts.NotificationsRemoved = notifs.Count;

                // Password-reset OTPs (keyed by email)
                if (!string.IsNullOrWhiteSpace(snap.Email))
                {
                    var otps = await _ctx.PasswordResetOtps
                        .Where(o => o.Email == snap.Email).ToListAsync();
                    if (otps.Count > 0) _ctx.PasswordResetOtps.RemoveRange(otps);
                    counts.OtpRowsRemoved = otps.Count;
                }

                // Persist all dependent deletions before removing the AspNetUsers row.
                await _ctx.SaveChangesAsync();

                // ── 4. Remove the AspNetUsers row (cascades roles/claims/tokens/logins) ──
                var idResult = await _userManager.DeleteAsync(user);
                if (!idResult.Succeeded)
                {
                    var msg = string.Join(", ", idResult.Errors.Select(e => e.Description));
                    throw new InternalServerErrorException("Identity deletion failed: " + msg);
                }

                // ── 5. Audit row inside the same tx ──
                var audit = new StudentDeletionAudit
                {
                    DeletedAt                    = DateTime.UtcNow,
                    DeletedByAdminId             = adminId,
                    DeletedStudentCode           = snap.AcademicCode,
                    DeletedStudentName           = snap.Name,
                    DeletedStudentEmail          = snap.Email,
                    RegistrationsRemoved         = counts.RegistrationsRemoved,
                    FinalGradesRemoved           = counts.FinalGradesRemoved,
                    MidtermGradesRemoved         = counts.MidtermGradesRemoved,
                    FinalGradeReviewsRemoved     = counts.FinalGradeReviewsRemoved,
                    AssignmentSubmissionsRemoved = counts.AssignmentSubmissionsRemoved,
                    QuizAttemptsRemoved          = counts.QuizAttemptsRemoved,
                    QuizAnswersRemoved           = counts.QuizAnswersRemoved,
                    NotificationsRemoved         = counts.NotificationsRemoved,
                    CourseResultsRemoved         = counts.CourseResultsRemoved,
                    SemesterGpasRemoved          = counts.SemesterGpasRemoved,
                    UserStudyYearsRemoved        = counts.UserStudyYearsRemoved,
                    CourseExceptionsRemoved      = counts.CourseExceptionsRemoved,
                    AdminCourseLocksRemoved      = counts.AdminCourseLocksRemoved,
                    ResetSnapshotsRemoved        = counts.ResetSnapshotsRemoved,
                    OtpRowsRemoved               = counts.OtpRowsRemoved,
                    Status                       = "completed",
                };
                _ctx.StudentDeletionAudits.Add(audit);
                await _ctx.SaveChangesAsync();
                await tx.CommitAsync();

                // ── 6. Best-effort physical-file cleanup (after commit) ──
                int filesRemoved = 0;
                foreach (var url in submissionFileUrls)
                {
                    try
                    {
                        await _fileService.DeleteFileByUrlAsync(url);
                        filesRemoved++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Failed to delete student submission file {Url} during permanent delete of {Code}",
                            url, snap.AcademicCode);
                    }
                }
                counts.SubmissionFilesRemoved = filesRemoved;

                // Update the audit row's file count outside the original tx.
                try
                {
                    var trackedAudit = await _ctx.StudentDeletionAudits.FirstOrDefaultAsync(a => a.Id == audit.Id);
                    if (trackedAudit != null)
                    {
                        trackedAudit.SubmissionFilesRemoved = filesRemoved;
                        await _ctx.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update file count on student deletion audit.");
                }

                return new StudentDeletionResultDto
                {
                    AuditId            = audit.Id,
                    DeletedStudentCode = snap.AcademicCode,
                    DeletedStudentName = snap.Name,
                    Counts             = counts,
                };
            }
            catch (BadRequestException) { await tx.RollbackAsync(); throw; }
            catch (ForbiddenException)  { await tx.RollbackAsync(); throw; }
            catch (NotFoundException)   { await tx.RollbackAsync(); throw; }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Permanent student delete failed for {Code}", request?.AcademicCode);

                // Best-effort failure-audit row outside the rolled-back transaction.
                try
                {
                    _ctx.StudentDeletionAudits.Add(new StudentDeletionAudit
                    {
                        DeletedAt           = DateTime.UtcNow,
                        DeletedByAdminId    = adminId,
                        DeletedStudentCode  = request?.AcademicCode ?? "",
                        DeletedStudentName  = "",
                        Status              = "failed",
                        ErrorMessage        = ex.Message,
                    });
                    await _ctx.SaveChangesAsync();
                }
                catch { /* swallow */ }

                throw new InternalServerErrorException("Permanent delete failed: " + ex.Message);
            }
        }

        // ═════════════════════════════════════════════════════════════
        // helpers
        // ═════════════════════════════════════════════════════════════
        private async Task<User> ResolveStudentAsync(string academicCode)
        {
            if (string.IsNullOrWhiteSpace(academicCode))
                throw new BadRequestException("Academic code is required.");

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Academic_Code == academicCode);

            // Allow lookup by GUID id as a fallback (defensive — frontend should send code).
            if (user == null)
                user = await _userManager.FindByIdAsync(academicCode);

            if (user == null)
                throw new NotFoundException("Student not found.");

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any(r => string.Equals(r, "Student", StringComparison.OrdinalIgnoreCase)))
                throw new BadRequestException("This account is not a student. Permanent delete from Danger Zone is restricted to student accounts.");

            return user;
        }

        private static string? LevelLabel(Levels? level) => level switch
        {
            Levels.Preparatory_Year => "Preparatory Year",
            Levels.First_Year       => "First Year",
            Levels.Second_Year      => "Second Year",
            Levels.Third_Year       => "Third Year",
            Levels.Fourth_Year      => "Fourth Year",
            Levels.Graduate         => "Graduate",
            _                       => null,
        };
    }
}
