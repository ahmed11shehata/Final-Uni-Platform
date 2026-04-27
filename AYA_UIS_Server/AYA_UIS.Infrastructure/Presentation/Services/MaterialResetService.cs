using System.Text.Json;
using Abstraction.Contracts;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Shared.Exceptions;
using Domain.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Presistence;
using Shared.Dtos.Admin_Module;

namespace Presentation.Services
{
    /// <summary>
    /// Admin Reset Material feature.
    /// Preview is read-only and surfaces pending-submission blockers.
    /// Execute archives assignments + quizzes (DB rows preserved for grade integrity),
    /// hard-deletes lecture rows, and removes physical files for all three types.
    /// All DB writes happen in a single transaction. File deletes are best-effort
    /// (missing-file errors do not roll back the DB).
    /// </summary>
    public class MaterialResetService : IMaterialResetService
    {
        private readonly IUnitOfWork          _uow;
        private readonly UniversityDbContext  _ctx;
        private readonly UserManager<User>    _userManager;
        private readonly IConfiguration       _config;
        private readonly INotificationService _notifications;
        private readonly ILocalFileService    _fileService;
        private readonly ILogger<MaterialResetService> _logger;

        // Per spec — fixed strong-confirmation password.
        private const string DEFAULT_PASSWORD = "Material@123#";

        public MaterialResetService(
            IUnitOfWork          uow,
            UniversityDbContext  ctx,
            UserManager<User>    userManager,
            IConfiguration       config,
            INotificationService notifications,
            ILocalFileService    fileService,
            ILogger<MaterialResetService> logger)
        {
            _uow            = uow;
            _ctx            = ctx;
            _userManager    = userManager;
            _config         = config;
            _notifications  = notifications;
            _fileService    = fileService;
            _logger         = logger;
        }

        // ═════════════════════════════════════════════════════════════
        // CATALOG — every course + per-course active material counts
        // ═════════════════════════════════════════════════════════════
        public async Task<List<MaterialResetCourseDto>> GetCatalogAsync()
        {
            // Base table is Courses (catalog source of truth). LEFT-style aggregations
            // mean a course with zero material still appears with all counts = 0.
            var courses = await _ctx.Courses.AsNoTracking()
                .Include(c => c.Department)
                .OrderBy(c => c.Code).ThenBy(c => c.Name)
                .ToListAsync();

            if (courses.Count == 0) return new List<MaterialResetCourseDto>();

            var courseIds = courses.Select(c => c.Id).ToList();

            var asnCounts = await _ctx.Assignments.AsNoTracking()
                .Where(a => courseIds.Contains(a.CourseId) && !a.IsArchived)
                .GroupBy(a => a.CourseId)
                .Select(g => new { CourseId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CourseId, x => x.Count);

            var quizCounts = await _ctx.Quizzes.AsNoTracking()
                .Where(q => courseIds.Contains(q.CourseId) && !q.IsArchived)
                .GroupBy(q => q.CourseId)
                .Select(g => new { CourseId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CourseId, x => x.Count);

            var lecCounts = await _ctx.CourseUploads.AsNoTracking()
                .Where(u => courseIds.Contains(u.CourseId))
                .GroupBy(u => u.CourseId)
                .Select(g => new { CourseId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CourseId, x => x.Count);

            return courses.Select(c =>
            {
                int aCount = asnCounts.TryGetValue(c.Id, out var av) ? av : 0;
                int qCount = quizCounts.TryGetValue(c.Id, out var qv) ? qv : 0;
                int lCount = lecCounts.TryGetValue(c.Id, out var lv) ? lv : 0;
                return new MaterialResetCourseDto
                {
                    Id              = c.Id,
                    Code            = c.Code,
                    Name            = c.Name,
                    Credits         = c.Credits,
                    Department      = c.Department?.Name,
                    AssignmentCount = aCount,
                    QuizCount       = qCount,
                    LectureCount    = lCount,
                    HasMaterial     = aCount + qCount + lCount > 0,
                };
            }).ToList();
        }

        // ═════════════════════════════════════════════════════════════
        // PREVIEW
        // ═════════════════════════════════════════════════════════════
        public async Task<MaterialResetPreviewResponseDto> PreviewAsync(
            MaterialResetPreviewRequestDto request)
        {
            var courseIds = await ResolveCourseIdsAsync(request.CourseIds, request.SelectAll);

            var resp = new MaterialResetPreviewResponseDto
            {
                SelectedCourseCount = courseIds.Count,
            };

            if (courseIds.Count == 0) return resp;

            var courses = await _ctx.Courses
                .Where(c => courseIds.Contains(c.Id))
                .AsNoTracking()
                .ToListAsync();

            // Active (not-yet-archived) materials for these courses.
            var assignments = await _ctx.Assignments.AsNoTracking()
                .Where(a => courseIds.Contains(a.CourseId) && !a.IsArchived)
                .ToListAsync();

            var quizzes = await _ctx.Quizzes.AsNoTracking()
                .Where(q => courseIds.Contains(q.CourseId) && !q.IsArchived)
                .ToListAsync();

            var lectures = await _ctx.CourseUploads.AsNoTracking()
                .Where(u => courseIds.Contains(u.CourseId))
                .ToListAsync();

            // Pending submissions = those still with status="Pending" (not Accepted/Rejected/Cleared).
            var assignmentIds = assignments.Select(a => a.Id).ToList();
            var pendingSubs = assignmentIds.Count == 0
                ? new List<AssignmentSubmission>()
                : await _ctx.AssignmentSubmissions.AsNoTracking()
                    .Where(s => assignmentIds.Contains(s.AssignmentId) &&
                                s.Status == "Pending")
                    .ToListAsync();

            // Group pending submissions by assignment for the per-row block message.
            var pendingByAsn = pendingSubs.GroupBy(s => s.AssignmentId)
                                          .ToDictionary(g => g.Key, g => g.ToList());

            // Pre-load student names/codes for the listed pending submissions.
            var pendingStudentIds = pendingSubs.Select(s => s.StudentId).Distinct().ToList();
            var pendingStudents = pendingStudentIds.Count == 0
                ? new List<User>()
                : await _userManager.Users.AsNoTracking()
                    .Where(u => pendingStudentIds.Contains(u.Id))
                    .ToListAsync();
            var studentById = pendingStudents.ToDictionary(u => u.Id, u => u);

            // Per-course rollup
            foreach (var c in courses)
            {
                var cAsn = assignments.Where(a => a.CourseId == c.Id).ToList();
                var cQz  = quizzes.Where(q => q.CourseId == c.Id).ToList();
                var cLec = lectures.Where(u => u.CourseId == c.Id).ToList();
                var cPending = pendingSubs.Count(s => cAsn.Any(a => a.Id == s.AssignmentId));

                resp.PerCourse.Add(new MaterialResetCourseImpactDto
                {
                    CourseId            = c.Id,
                    CourseCode          = c.Code,
                    CourseName          = c.Name,
                    Assignments         = cAsn.Count,
                    Quizzes             = cQz.Count,
                    Lectures            = cLec.Count,
                    PendingSubmissions  = cPending,
                    HasMaterial         = cAsn.Count + cQz.Count + cLec.Count > 0,
                });
            }

            // Pending-submission blocker rows
            foreach (var asn in assignments.Where(a => pendingByAsn.ContainsKey(a.Id)))
            {
                var subs = pendingByAsn[asn.Id];
                var course = courses.FirstOrDefault(c => c.Id == asn.CourseId);
                resp.PendingSubmissions.Add(new MaterialResetPendingSubmissionDto
                {
                    CourseId        = asn.CourseId,
                    CourseCode      = course?.Code ?? "",
                    AssignmentId    = asn.Id,
                    AssignmentTitle = asn.Title,
                    PendingCount    = subs.Count,
                    StudentCodes    = subs.Select(s => studentById.TryGetValue(s.StudentId, out var u) ? (u.Academic_Code ?? "") : "")
                                          .Where(x => !string.IsNullOrEmpty(x)).Take(20).ToList(),
                    StudentNames    = subs.Select(s => studentById.TryGetValue(s.StudentId, out var u) ? u.DisplayName : s.StudentId)
                                          .Take(20).ToList(),
                });
            }

            resp.Totals.Assignments        = assignments.Count;
            resp.Totals.Quizzes            = quizzes.Count;
            resp.Totals.Lectures           = lectures.Count;
            resp.Totals.PendingSubmissions = pendingSubs.Count;
            resp.Totals.InstructorsAffected = await CountInstructorsAffected(courseIds);

            if (resp.Totals.PendingSubmissions > 0)
            {
                resp.Blocked     = true;
                resp.BlockReason = "Reset blocked because some assignment submissions are still pending review. " +
                                   "Instructors must accept or reject all submissions first.";
            }

            return resp;
        }

        // ═════════════════════════════════════════════════════════════
        // EXECUTE
        // ═════════════════════════════════════════════════════════════
        public async Task<MaterialResetExecuteResponseDto> ExecuteAsync(
            string adminId,
            MaterialResetExecuteRequestDto request)
        {
            var expectedPwd = _config["MaterialReset:Password"] ?? DEFAULT_PASSWORD;
            if (!string.Equals(request.Password, expectedPwd, StringComparison.Ordinal))
                throw new BadRequestException("Reset password is incorrect.");

            var courseIds = await ResolveCourseIdsAsync(request.CourseIds, request.SelectAll);
            if (courseIds.Count == 0)
                throw new BadRequestException("No courses selected.");

            // Re-check pending submissions (preview is advisory; server is authoritative).
            var activeAsnIds = await _ctx.Assignments
                .Where(a => courseIds.Contains(a.CourseId) && !a.IsArchived)
                .Select(a => a.Id)
                .ToListAsync();

            var pendingCount = activeAsnIds.Count == 0
                ? 0
                : await _ctx.AssignmentSubmissions
                    .CountAsync(s => activeAsnIds.Contains(s.AssignmentId) && s.Status == "Pending");

            if (pendingCount > 0)
                throw new BadRequestException(
                    "Reset blocked because some assignment submissions are still pending review. " +
                    "Instructors must accept or reject all submissions first.");

            await using var tx = await _ctx.Database.BeginTransactionAsync();

            try
            {
                // 1. Insert audit batch row first so soft-deleted rows can carry its ID.
                var batch = new MaterialReset
                {
                    CreatedById = adminId,
                    CreatedAt   = DateTime.UtcNow,
                    SelectedCourseCount = courseIds.Count,
                    Status = "completed",
                };
                _ctx.MaterialResets.Add(batch);
                await _ctx.SaveChangesAsync();

                var counts = new MaterialResetExecuteCountsDto();

                // 2. Archive assignments + purge files
                var asns = await _ctx.Assignments
                    .Where(a => courseIds.Contains(a.CourseId) && !a.IsArchived)
                    .ToListAsync();

                var asnIds = asns.Select(a => a.Id).ToList();

                foreach (var a in asns)
                {
                    if (!string.IsNullOrWhiteSpace(a.FileUrl))
                    {
                        await _fileService.DeleteFileByUrlAsync(a.FileUrl);
                        counts.AssignmentFilesPurged++;
                    }
                    a.FileUrl       = string.Empty;
                    a.IsArchived    = true;
                    a.DeletedAt     = DateTime.UtcNow;
                    a.DeletedById   = adminId;
                    a.ResetBatchId  = batch.Id;
                    a.FilePurgedAt  = DateTime.UtcNow;
                    a.ContentPurgedAt = DateTime.UtcNow;
                    counts.AssignmentsArchived++;
                }

                // Purge submission files (rows + grade columns are preserved for integrity)
                if (asnIds.Count > 0)
                {
                    var subs = await _ctx.AssignmentSubmissions
                        .Where(s => asnIds.Contains(s.AssignmentId))
                        .ToListAsync();

                    foreach (var s in subs)
                    {
                        if (!string.IsNullOrWhiteSpace(s.FileUrl))
                        {
                            await _fileService.DeleteFileByUrlAsync(s.FileUrl);
                            counts.SubmissionFilesPurged++;
                        }
                        s.FileUrl      = string.Empty;
                        s.FilePurgedAt = DateTime.UtcNow;
                    }
                }

                // 3. Archive quizzes (questions/options/attempts/answers stay in DB)
                var quizzes = await _ctx.Quizzes
                    .Where(q => courseIds.Contains(q.CourseId) && !q.IsArchived)
                    .ToListAsync();

                foreach (var q in quizzes)
                {
                    q.IsArchived   = true;
                    q.DeletedAt    = DateTime.UtcNow;
                    q.DeletedById  = adminId;
                    q.ResetBatchId = batch.Id;
                    q.FilePurgedAt = DateTime.UtcNow;
                    counts.QuizzesArchived++;
                }

                // 4. Hard-delete lecture rows + their physical files
                var lectures = await _ctx.CourseUploads
                    .Where(u => courseIds.Contains(u.CourseId))
                    .ToListAsync();

                foreach (var u in lectures)
                {
                    if (!string.IsNullOrWhiteSpace(u.Url))
                    {
                        await _fileService.DeleteFileByUrlAsync(u.Url);
                        counts.LectureFilesDeleted++;
                    }
                    counts.LecturesDeleted++;
                }
                if (lectures.Count > 0) _ctx.CourseUploads.RemoveRange(lectures);

                // 5. Update audit row with the final counts
                batch.AssignmentCount           = counts.AssignmentsArchived;
                batch.QuizCount                 = counts.QuizzesArchived;
                batch.LectureCount              = counts.LecturesDeleted;
                batch.SubmissionFilePurgedCount = counts.SubmissionFilesPurged;

                await _ctx.SaveChangesAsync();
                await tx.CommitAsync();

                // 6. Notify instructors of affected courses (after commit so we never notify a rolled-back run)
                var instructorIds = await _ctx.RegistrationCourseInstructors
                    .Where(r => courseIds.Contains(r.CourseId))
                    .Select(r => new { r.CourseId, r.InstructorId })
                    .Distinct()
                    .ToListAsync();

                var courseRows = await _ctx.Courses
                    .Where(c => courseIds.Contains(c.Id))
                    .Select(c => new { c.Id, c.Code, c.Name })
                    .ToListAsync();
                var courseById = courseRows.ToDictionary(c => c.Id);

                var notifs = new List<Notification>();
                foreach (var ic in instructorIds)
                {
                    if (string.IsNullOrEmpty(ic.InstructorId)) continue;
                    if (!courseById.TryGetValue(ic.CourseId, out var c)) continue;

                    notifs.Add(new Notification
                    {
                        UserId     = ic.InstructorId,
                        Type       = "material_reset",
                        Title      = "Material reset 🧹",
                        Body       = $"All current materials in {c.Code} — {c.Name} have been archived by an admin.",
                        CourseId   = c.Id,
                        CourseName = c.Name,
                        IsRead     = false,
                    });
                }

                if (notifs.Count > 0)
                {
                    try { await _notifications.SendManyAsync(notifs); }
                    catch (Exception ex) { _logger.LogWarning(ex, "Material reset notifications partially failed."); }
                }
                counts.InstructorsNotified = notifs.Count;

                // Persist the final notify count + summary JSON (best-effort).
                try
                {
                    var trackedBatch = await _ctx.MaterialResets.FirstOrDefaultAsync(b => b.Id == batch.Id);
                    if (trackedBatch != null)
                    {
                        trackedBatch.InstructorsNotified = counts.InstructorsNotified;
                        trackedBatch.SummaryJson = JsonSerializer.Serialize(new
                        {
                            courseIds,
                            counts,
                        });
                        await _ctx.SaveChangesAsync();
                    }
                }
                catch (Exception ex) { _logger.LogWarning(ex, "Material reset audit summary update failed."); }

                // Per-course breakdown (computed BEFORE the rows were archived)
                var perCourse = courseRows.Select(c => new MaterialResetCourseImpactDto
                {
                    CourseId    = c.Id,
                    CourseCode  = c.Code,
                    CourseName  = c.Name,
                    Assignments = asns.Count(a => a.CourseId == c.Id),
                    Quizzes     = quizzes.Count(q => q.CourseId == c.Id),
                    Lectures    = lectures.Count(u => u.CourseId == c.Id),
                    PendingSubmissions = 0,
                    HasMaterial = asns.Any(a => a.CourseId == c.Id) ||
                                  quizzes.Any(q => q.CourseId == c.Id) ||
                                  lectures.Any(u => u.CourseId == c.Id),
                }).ToList();

                var warnings = new List<string>();
                foreach (var c in perCourse)
                    if (!c.HasMaterial)
                        warnings.Add($"{c.CourseCode}: nothing to reset (course had no material).");

                return new MaterialResetExecuteResponseDto
                {
                    BatchId             = batch.Id,
                    SelectedCourseCount = courseIds.Count,
                    Counts              = counts,
                    Warnings            = warnings,
                    PerCourse           = perCourse,
                };
            }
            catch (BadRequestException) { await tx.RollbackAsync(); throw; }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Material reset execution failed.");

                // Best-effort: write a 'failed' audit row outside the rolled-back transaction.
                try
                {
                    _ctx.MaterialResets.Add(new MaterialReset
                    {
                        CreatedById         = adminId,
                        CreatedAt           = DateTime.UtcNow,
                        SelectedCourseCount = courseIds.Count,
                        Status              = "failed",
                        ErrorMessage        = ex.Message,
                    });
                    await _ctx.SaveChangesAsync();
                }
                catch { /* ignore */ }

                throw new InternalServerErrorException("Reset Material failed: " + ex.Message);
            }
        }

        // ═════════════════════════════════════════════════════════════
        // helpers
        // ═════════════════════════════════════════════════════════════
        private async Task<List<int>> ResolveCourseIdsAsync(List<int>? courseIds, bool selectAll)
        {
            if (selectAll)
                return await _ctx.Courses.Select(c => c.Id).ToListAsync();
            return (courseIds ?? new List<int>()).Distinct().ToList();
        }

        private async Task<int> CountInstructorsAffected(List<int> courseIds)
        {
            if (courseIds.Count == 0) return 0;
            return await _ctx.RegistrationCourseInstructors
                .Where(r => courseIds.Contains(r.CourseId))
                .Select(r => r.InstructorId)
                .Distinct()
                .CountAsync();
        }
    }
}
