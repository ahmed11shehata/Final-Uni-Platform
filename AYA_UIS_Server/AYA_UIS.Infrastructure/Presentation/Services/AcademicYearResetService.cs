using System.Text.Json;
using Abstraction.Contracts;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using AYA_UIS.Shared.Exceptions;
using Domain.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Presistence;
using Shared.Dtos.Admin_Module;

namespace Presentation.Services
{
    /// <summary>
    /// Implements the Academic Year Reset feature.
    /// Preview is read-only. Execute runs in a single DbContext transaction
    /// and rolls back on any failure. Grades feed into the shared FinalGrade
    /// source-of-truth used by Student Grades; archived registrations carry
    /// frozen NumericTotal/Grade so transcript history stays correct.
    /// </summary>
    public class AcademicYearResetService : IAcademicYearResetService
    {
        private readonly IUnitOfWork           _uow;
        private readonly UniversityDbContext   _ctx;
        private readonly UserManager<User>     _userManager;
        private readonly IConfiguration        _config;
        private readonly INotificationService  _notifications;

        // Defaults match acceptance-criteria text — overridable via appsettings.
        private const string DEFAULT_PASSWORD     = "RESET@123#";
        private const string DEFAULT_CONFIRMATION = "RESET";

        public AcademicYearResetService(
            IUnitOfWork           uow,
            UniversityDbContext   ctx,
            UserManager<User>     userManager,
            IConfiguration        config,
            INotificationService  notifications)
        {
            _uow            = uow;
            _ctx            = ctx;
            _userManager    = userManager;
            _config         = config;
            _notifications  = notifications;
        }

        // ═════════════════════════════════════════════════════════════
        // PREVIEW
        // ═════════════════════════════════════════════════════════════
        public async Task<AcademicYearResetPreviewResponseDto> PreviewAsync(
            AcademicYearResetPreviewRequestDto request)
        {
            var students = await ResolveStudentsAsync(request.StudentIds, request.SelectAll);

            var (sourceStudyYearId, sourceSemesterId) = await ResolveCurrentTermAsync();
            var reviewByStudent = sourceStudyYearId > 0 && sourceSemesterId > 0
                ? (await _uow.FinalGradeReviews.GetByTermAsync(sourceStudyYearId, sourceSemesterId))
                    .ToDictionary(r => r.StudentId, r => NormalizeStatus(r.Status))
                : new Dictionary<string, string>();

            var resp     = new AcademicYearResetPreviewResponseDto { SelectedCount = students.Count };
            var allRegs  = await _ctx.Registrations.AsNoTracking().ToListAsync();
            var regsByU  = allRegs.GroupBy(r => r.UserId)
                                  .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var u in students)
            {
                var perStu = await BuildStudentPreviewAsync(u, regsByU, reviewByStudent);
                resp.PerStudent.Add(perStu);

                resp.Totals.RegisteredCourses += perStu.RegisteredCount;
                resp.Totals.PassedCourses     += perStu.PassedCount;
                resp.Totals.FailedCourses     += perStu.FailedCount;
                resp.Totals.UnassignedGrades  += perStu.UnassignedCount;

                if (perStu.ReviewStatus != "completed") resp.Totals.NotCompletedReviewCount++;
                if (perStu.AlreadyReset)                resp.Totals.AlreadyResetCount++;
            }

            // Roll up warnings to the top-level
            if (resp.Totals.NotCompletedReviewCount > 0)
                resp.Warnings.Add($"{resp.Totals.NotCompletedReviewCount} selected student(s) are NOT marked Completed in Final Grade Review.");
            if (resp.Totals.UnassignedGrades > 0)
                resp.Warnings.Add($"{resp.Totals.UnassignedGrades} registered course(s) have no grade — they will be treated as Failed if force reset is used.");
            if (resp.Totals.AlreadyResetCount > 0)
                resp.Warnings.Add($"{resp.Totals.AlreadyResetCount} student(s) were already reset for their current source year/semester.");

            resp.RequiresForceReset = resp.Totals.NotCompletedReviewCount > 0 ||
                                      resp.Totals.UnassignedGrades       > 0;

            return resp;
        }

        // ═════════════════════════════════════════════════════════════
        // EXECUTE
        // ═════════════════════════════════════════════════════════════
        public async Task<AcademicYearResetExecuteResponseDto> ExecuteAsync(
            string adminId,
            AcademicYearResetExecuteRequestDto request)
        {
            // 1. Strong confirmation — backend authoritative
            var expectedPwd  = _config["AcademicReset:Password"]         ?? DEFAULT_PASSWORD;
            var expectedTxt  = _config["AcademicReset:ConfirmationText"] ?? DEFAULT_CONFIRMATION;

            if (!string.Equals(request.ConfirmationText?.Trim(),  expectedTxt, StringComparison.Ordinal))
                throw new BadRequestException("Confirmation text is incorrect. Type RESET to confirm.");
            if (!string.Equals(request.ResetPassword,             expectedPwd, StringComparison.Ordinal))
                throw new BadRequestException("Reset password is incorrect.");

            // 2. Resolve students
            var students = await ResolveStudentsAsync(request.StudentIds, request.SelectAll);
            if (students.Count == 0)
                throw new BadRequestException("No students selected for reset.");

            var (sourceStudyYearId, sourceSemesterId) = await ResolveCurrentTermAsync();
            var reviewByStudent = sourceStudyYearId > 0 && sourceSemesterId > 0
                ? (await _uow.FinalGradeReviews.GetByTermAsync(sourceStudyYearId, sourceSemesterId))
                    .ToDictionary(r => r.StudentId, r => NormalizeStatus(r.Status))
                : new Dictionary<string, string>();

            // 3. Pre-flight per-student validation
            var allRegs = await _ctx.Registrations.AsNoTracking().ToListAsync();
            var regsByU = allRegs.GroupBy(r => r.UserId)
                                 .ToDictionary(g => g.Key, g => g.ToList());

            var blockingErrors = new List<string>();
            foreach (var u in students)
            {
                var preview = await BuildStudentPreviewAsync(u, regsByU, reviewByStudent);
                if (preview.AlreadyReset)
                    blockingErrors.Add($"{u.DisplayName} ({u.Academic_Code}): already reset for {preview.CurrentLevel} / Semester {preview.CurrentSemester}.");

                if (!request.ForceReset)
                {
                    if (preview.ReviewStatus != "completed")
                        blockingErrors.Add($"{u.DisplayName} ({u.Academic_Code}): not marked Completed in Final Grade Review.");
                    if (preview.UnassignedCount > 0)
                        blockingErrors.Add($"{u.DisplayName} ({u.Academic_Code}): {preview.UnassignedCount} registered course(s) have unassigned grades.");
                }
            }
            if (blockingErrors.Count > 0)
                throw new BadRequestException("Reset blocked: " + string.Join(" | ", blockingErrors));

            // 4. Persist audit row first so snapshots can FK-reference it
            var sourceTermLabel = await DescribeTermAsync(sourceStudyYearId, sourceSemesterId);
            var resetRow = new AcademicYearReset
            {
                AdminId           = adminId,
                ExecutedAt        = DateTime.UtcNow,
                StudentsCount     = students.Count,
                ForceReset        = request.ForceReset,
                SelectAll         = request.SelectAll,
                SourceStudyYearId = sourceStudyYearId > 0 ? sourceStudyYearId : (int?)null,
                SourceSemesterId  = sourceSemesterId  > 0 ? sourceSemesterId  : (int?)null,
                SourceTerm        = sourceTermLabel,
            };
            _ctx.AcademicYearResets.Add(resetRow);
            await _ctx.SaveChangesAsync();

            // 5. Single transaction: snapshot + archive + purge + advance + notify
            var resp = new AcademicYearResetExecuteResponseDto { ResetId = resetRow.Id };
            await using var tx = await _ctx.Database.BeginTransactionAsync();

            try
            {
                foreach (var user in students)
                {
                    var perResult = await ResetSingleStudentAsync(user, resetRow);

                    resp.StudentsReset++;
                    resp.ArchivedRegistrations += perResult.archived;
                    resp.PassedCount           += perResult.passed;
                    resp.FailedCount           += perResult.failed;
                    resp.UnassignedFailedCount += perResult.unassignedFailed;
                    resp.FinalGradesPurged     += perResult.fgPurged;
                    resp.QuizAttemptsPurged    += perResult.quizPurged;
                    resp.SubmissionsPurged     += perResult.subsPurged;
                    resp.MidtermsPurged        += perResult.midPurged;
                }

                // ── Roll up totals on the audit row ──
                resetRow.ArchivedRegistrations = resp.ArchivedRegistrations;
                resetRow.PassedCount           = resp.PassedCount;
                resetRow.FailedCount           = resp.FailedCount;
                resetRow.UnassignedFailedCount = resp.UnassignedFailedCount;
                resetRow.FinalGradesPurged     = resp.FinalGradesPurged;
                resetRow.QuizAttemptsPurged    = resp.QuizAttemptsPurged;
                resetRow.SubmissionsPurged     = resp.SubmissionsPurged;
                resetRow.MidtermsPurged        = resp.MidtermsPurged;
                resetRow.SummaryJson           = JsonSerializer.Serialize(resp);
                _ctx.AcademicYearResets.Update(resetRow);

                await _ctx.SaveChangesAsync();

                // ── Notifications inside transaction so a hub failure rolls back ──
                var notifs = students.Select(s => new Notification
                {
                    UserId          = s.Id,
                    Type            = "system_alert",
                    Title           = "Academic year updated",
                    Body            = "Your academic year/semester has been updated. Old courses moved to grade history. You can register new available courses when registration opens.",
                    TargetStudentId = s.Id,
                    StudentName     = s.DisplayName,
                    StudentCode     = s.Academic_Code,
                    IsRead          = false,
                }).ToList();

                if (notifs.Count > 0)
                {
                    await _notifications.SendManyAsync(notifs);
                    resp.NotificationsSent      = notifs.Count;
                    resetRow.NotificationsSent  = notifs.Count;
                    _ctx.AcademicYearResets.Update(resetRow);
                    await _ctx.SaveChangesAsync();
                }

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            resp.Limitations.Add("Shared course content (quizzes, assignments, lectures, files) is preserved across all students; only per-student attempts/submissions/midterms/final-grade rows for archived registrations are purged.");
            resp.Limitations.Add("Physical files on disk/storage are not deleted in this phase.");

            return resp;
        }

        // ═════════════════════════════════════════════════════════════
        // PER-STUDENT EXECUTION
        // ═════════════════════════════════════════════════════════════
        private async Task<(int archived, int passed, int failed, int unassignedFailed,
                            int fgPurged, int quizPurged, int subsPurged, int midPurged)>
            ResetSingleStudentAsync(User user, AcademicYearReset resetRow)
        {
            int archived = 0, passed = 0, failed = 0, unassignedFailed = 0;
            int fgPurged = 0, quizPurged = 0, subsPurged = 0, midPurged = 0;

            // Pull tracked active registrations
            var activeRegs = await _ctx.Registrations
                .Where(r => r.UserId == user.Id &&
                            !r.IsArchived &&
                            !r.IsEquivalency &&
                            (r.Status == RegistrationStatus.Approved ||
                             r.Status == RegistrationStatus.Pending))
                .ToListAsync();

            int sourceLevelNum = LevelToYearNum(user.Level);
            int sourceSemNum   = activeRegs.Count > 0
                ? (activeRegs[0].SemesterId % 2 == 0 ? 2 : 1)
                : await ResolveGlobalSemesterNumAsync();

            var (targetLevel, targetSemNum) = ComputeTransition(user.Level, sourceSemNum);

            var courseIds = activeRegs.Select(r => r.CourseId).ToList();

            // ── Snapshot pre-state for this student ──
            var preFinalGrades = await _ctx.FinalGrades
                .Where(f => f.StudentId == user.Id && courseIds.Contains(f.CourseId))
                .ToListAsync();
            var preMidterms = await _ctx.MidtermGrades
                .Where(m => m.StudentId == user.Id && courseIds.Contains(m.CourseId))
                .ToListAsync();
            var preSubs = await _ctx.AssignmentSubmissions
                .Where(s => s.StudentId == user.Id &&
                            _ctx.Assignments.Any(a => a.Id == s.AssignmentId && courseIds.Contains(a.CourseId)))
                .ToListAsync();
            var preAttempts = await _ctx.StudentQuizAttempts
                .Where(a => a.StudentId == user.Id &&
                            _ctx.Quizzes.Any(q => q.Id == a.QuizId && courseIds.Contains(q.CourseId)))
                .ToListAsync();

            var snapshot = new AcademicYearResetSnapshot
            {
                ResetId            = resetRow.Id,
                StudentId          = user.Id,
                CapturedAt         = DateTime.UtcNow,
                SourceLevel        = user.Level?.ToString(),
                SourceSemester     = $"Semester {sourceSemNum}",
                TargetLevel        = targetLevel?.ToString(),
                TargetSemester     = $"Semester {targetSemNum}",
                RegistrationsCount = activeRegs.Count,
                FinalGradesCount   = preFinalGrades.Count,
                SubmissionsCount   = preSubs.Count,
                QuizAttemptsCount  = preAttempts.Count,
                PayloadJson        = JsonSerializer.Serialize(new
                {
                    registrations = activeRegs.Select(r => new {
                        r.Id, r.CourseId, r.Status, r.Progress, r.Grade, r.IsPassed,
                        r.NumericTotal, r.StudyYearId, r.SemesterId, r.TranscriptYear,
                        r.IsEquivalency, r.RegisteredAt
                    }),
                    finalGrades = preFinalGrades.Select(f => new {
                        f.Id, f.CourseId, f.FinalScore, f.Bonus, f.AdminFinalTotal, f.Published
                    }),
                    midterms    = preMidterms.Select(m => new { m.CourseId, m.Grade, m.Max }),
                    submissions = preSubs.Select(s => new {
                        s.Id, s.AssignmentId, s.Grade, s.Status, s.SubmittedAt
                    }),
                    quizAttempts = preAttempts.Select(a => new {
                        a.Id, a.QuizId, a.Score, a.SubmittedAt
                    }),
                }),
            };
            _ctx.AcademicYearResetSnapshots.Add(snapshot);

            // ── Archive each active registration with frozen grade history ──
            var fgByCourse = preFinalGrades.ToDictionary(f => f.CourseId);

            foreach (var reg in activeRegs)
            {
                int totalRaw;
                bool isUnassigned = false;

                if (fgByCourse.TryGetValue(reg.CourseId, out var fg))
                {
                    if (fg.AdminFinalTotal.HasValue)
                    {
                        totalRaw = fg.AdminFinalTotal.Value;
                    }
                    else
                    {
                        var cwTotal = await ComputeCourseworkAsync(user.Id, reg.CourseId, fg.Bonus);
                        totalRaw = (int)Math.Round(cwTotal + fg.FinalScore);
                    }
                }
                else
                {
                    isUnassigned = true; // force-reset path treats this as failed (0)
                    totalRaw = 0;
                }

                int total = Math.Max(0, Math.Min(100, totalRaw));
                bool isPassed = total >= 60 && !isUnassigned;
                var (gradeEnum, _, _) = DeriveGrade(total);

                reg.IsArchived     = true;
                reg.ArchivedAt     = DateTime.UtcNow;
                reg.Progress       = CourseProgress.Completed;
                reg.IsPassed       = isPassed;
                reg.Grade          = isPassed ? gradeEnum : Grads.F;
                reg.NumericTotal   = total;
                reg.TranscriptYear = sourceLevelNum > 0 ? sourceLevelNum : (int?)null;
                _ctx.Registrations.Update(reg);

                archived++;
                if (isUnassigned) unassignedFailed++;
                if (isPassed)     passed++;
                else              failed++;
            }

            // ── Purge per-student records (FinalGrade, Midterm, Submission, QuizAttempt) ──
            if (preFinalGrades.Count > 0)
            {
                _ctx.FinalGrades.RemoveRange(preFinalGrades);
                fgPurged = preFinalGrades.Count;
            }
            if (preMidterms.Count > 0)
            {
                _ctx.MidtermGrades.RemoveRange(preMidterms);
                midPurged = preMidterms.Count;
            }
            if (preSubs.Count > 0)
            {
                _ctx.AssignmentSubmissions.RemoveRange(preSubs);
                subsPurged = preSubs.Count;
            }
            if (preAttempts.Count > 0)
            {
                _ctx.StudentQuizAttempts.RemoveRange(preAttempts);
                quizPurged = preAttempts.Count;
            }

            // ── Advance the student's level only on Sem 2 → Sem 1 transition ──
            if (sourceSemNum == 2 && targetLevel != user.Level)
                user.Level = targetLevel;

            // ── Flush archive/purge before recomputing GPA ──
            // RecalculateStudentGpaAsync reads via AsNoTracking, which would not
            // see the in-memory IsArchived/IsPassed/NumericTotal mutations on the
            // tracked Registration entities. Without this save, newly-passed
            // archived courses would be excluded from the GPA. The save is still
            // inside the open transaction, so atomicity is preserved.
            await _ctx.SaveChangesAsync();

            // ── Recalculate GPA and standing from passed (incl. archived passed) rows ──
            await RecalculateStudentGpaAsync(user);

            await _userManager.UpdateAsync(user);

            return (archived, passed, failed, unassignedFailed, fgPurged, quizPurged, subsPurged, midPurged);
        }

        // ═════════════════════════════════════════════════════════════
        // PER-STUDENT PREVIEW BUILDER
        // ═════════════════════════════════════════════════════════════
        private async Task<AcademicYearResetStudentPreviewDto> BuildStudentPreviewAsync(
            User user,
            Dictionary<string, List<Registration>> regsByU,
            Dictionary<string, string> reviewByStudent)
        {
            regsByU.TryGetValue(user.Id, out var allUserRegs);
            allUserRegs ??= new List<Registration>();

            var active = allUserRegs
                .Where(r => !r.IsArchived &&
                            !r.IsEquivalency &&
                            (r.Status == RegistrationStatus.Approved ||
                             r.Status == RegistrationStatus.Pending))
                .ToList();

            int registered = active.Count;
            int passed = 0, failed = 0, unassigned = 0;

            foreach (var reg in active)
            {
                var fg = await _uow.FinalGrades.GetAsync(user.Id, reg.CourseId);
                if (fg == null) { unassigned++; continue; }

                int total;
                if (fg.AdminFinalTotal.HasValue)
                {
                    total = fg.AdminFinalTotal.Value;
                }
                else
                {
                    var cw = await ComputeCourseworkAsync(user.Id, reg.CourseId, fg.Bonus);
                    total = (int)Math.Round(cw + fg.FinalScore);
                }
                if (total >= 60) passed++;
                else             failed++;
            }

            int curSemNum = active.Count > 0
                ? (active[0].SemesterId % 2 == 0 ? 2 : 1)
                : await ResolveGlobalSemesterNumAsync();
            var (targetLevel, targetSemNum) = ComputeTransition(user.Level, curSemNum);

            // Detect duplicate-reset for the SAME source year+sem
            var levelStr = user.Level?.ToString();
            bool alreadyReset = await _ctx.AcademicYearResetSnapshots.AnyAsync(s =>
                s.StudentId == user.Id &&
                s.SourceLevel == levelStr &&
                s.SourceSemester == "Semester " + curSemNum);

            var status = reviewByStudent.TryGetValue(user.Id, out var st) ? st : "progress";

            var dto = new AcademicYearResetStudentPreviewDto
            {
                StudentId       = user.Id,
                StudentName     = user.DisplayName ?? "",
                AcademicCode    = user.Academic_Code ?? "",
                CurrentLevel    = user.Level?.ToString() ?? "",
                CurrentSemester = curSemNum,
                TargetLevel     = targetLevel?.ToString() ?? "",
                TargetSemester  = targetSemNum,
                RegisteredCount = registered,
                PassedCount     = passed,
                FailedCount     = failed,
                UnassignedCount = unassigned,
                ReviewStatus    = status,
                AlreadyReset    = alreadyReset,
            };

            if (alreadyReset)
                dto.Warnings.Add("This student was already reset for this source year/semester.");
            if (status != "completed")
                dto.Warnings.Add("Final Grade Review status is not Completed.");
            if (unassigned > 0)
                dto.Warnings.Add($"{unassigned} course(s) have no final grade — will be marked Failed under force reset.");

            return dto;
        }

        // ═════════════════════════════════════════════════════════════
        // HELPERS — student / term resolution, transition, coursework
        // ═════════════════════════════════════════════════════════════
        private async Task<List<User>> ResolveStudentsAsync(List<string> ids, bool selectAll)
        {
            if (selectAll)
                return (await _userManager.GetUsersInRoleAsync("Student")).ToList();

            if (ids == null || ids.Count == 0)
                return new List<User>();

            var distinct = ids.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            var result = new List<User>();
            foreach (var id in distinct)
            {
                var u = await _userManager.FindByIdAsync(id);
                if (u != null && (await _userManager.IsInRoleAsync(u, "Student")))
                    result.Add(u);
            }
            return result;
        }

        private async Task<(int studyYearId, int semesterId)> ResolveCurrentTermAsync()
        {
            var settings = await _uow.RegistrationSettings.GetCurrentAsync();

            StudyYear? sy = null;
            if (settings != null && !string.IsNullOrWhiteSpace(settings.AcademicYear))
            {
                var parts = settings.AcademicYear.Split('/', '-');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0].Trim(), out var s) &&
                    int.TryParse(parts[1].Trim(), out var e))
                {
                    var all = await _uow.StudyYears.GetAllAsync();
                    sy = all.FirstOrDefault(y => y.StartYear == s && y.EndYear == e);
                }
            }
            sy ??= await _uow.StudyYears.GetCurrentStudyYearAsync();
            if (sy == null) return (0, 0);

            Semester? sem = null;
            if (settings != null && !string.IsNullOrWhiteSpace(settings.Semester))
            {
                var key  = settings.Semester.Trim().ToLowerInvariant();
                var list = await _uow.Semesters.GetByStudyYearIdAsync(sy.Id);
                sem = key switch
                {
                    "first"  or "first semester"  or "semester 1" or "1" =>
                        list.FirstOrDefault(x => x.Title == SemesterEnum.First_Semester),
                    "second" or "second semester" or "semester 2" or "2" =>
                        list.FirstOrDefault(x => x.Title == SemesterEnum.Second_Semester),
                    _ => list.FirstOrDefault()
                };
            }
            sem ??= await _uow.Semesters.GetActiveSemesterByStudyYearIdAsync(sy.Id);
            if (sem == null) return (sy.Id, 0);
            return (sy.Id, sem.Id);
        }

        private async Task<string> DescribeTermAsync(int studyYearId, int semesterId)
        {
            if (studyYearId == 0 || semesterId == 0) return "(no current term)";
            var sy  = await _uow.StudyYears.GetByIdAsync(studyYearId);
            var sem = await _uow.Semesters.GetByIdAsync(semesterId);
            return $"{sy?.StartYear}-{sy?.EndYear} / {sem?.Title}";
        }

        private async Task<int> ResolveGlobalSemesterNumAsync()
        {
            var settings = await _uow.RegistrationSettings.GetCurrentAsync();
            var key = (settings?.Semester ?? "").Trim().ToLowerInvariant();
            return key switch
            {
                "second" or "second semester" or "semester 2" or "2" => 2,
                _ => 1,
            };
        }

        // Transition rules per spec:
        //   Sem 1 → same year, Sem 2
        //   Sem 2 → next year, Sem 1
        //   Year 4 Sem 2 → Year 4 Sem 1 (do NOT auto-graduate)
        private static (Levels? targetLevel, int targetSemNum) ComputeTransition(Levels? current, int sourceSemNum)
        {
            if (sourceSemNum == 1)
                return (current, 2);

            return current switch
            {
                Levels.Preparatory_Year => (Levels.First_Year,  1),
                Levels.First_Year       => (Levels.Second_Year, 1),
                Levels.Second_Year      => (Levels.Third_Year,  1),
                Levels.Third_Year       => (Levels.Fourth_Year, 1),
                Levels.Fourth_Year      => (Levels.Fourth_Year, 1),
                Levels.Graduate         => (Levels.Graduate,    1),
                _                       => (current, 1),
            };
        }

        private static int LevelToYearNum(Levels? level) => level switch
        {
            Levels.Preparatory_Year => 1,
            Levels.First_Year       => 1,
            Levels.Second_Year      => 2,
            Levels.Third_Year       => 3,
            Levels.Fourth_Year      => 4,
            Levels.Graduate         => 4,
            _                       => 1,
        };

        private async Task<decimal> ComputeCourseworkAsync(string studentId, int courseId, int bonus)
        {
            var midterm  = await _uow.MidtermGrades.GetAsync(studentId, courseId);
            decimal mid  = midterm?.Grade ?? 0;

            decimal quiz = 0;
            foreach (var q in (await _uow.Quizzes.GetQuizzesByCourseId(courseId)).ToList())
            {
                var attempt = await _uow.Quizzes.GetStudentAttemptAsync(q.Id, studentId);
                if (attempt != null) quiz += attempt.Score;
            }

            decimal asn = 0;
            foreach (var a in (await _uow.Assignments.GetAssignmentsByCourseIdAsync(courseId)).ToList())
            {
                var sub = await _uow.Assignments.GetStudentSubmissionAsync(a.Id, studentId);
                if (sub != null && string.Equals(sub.Status, "Accepted", StringComparison.OrdinalIgnoreCase))
                    asn += sub.Grade ?? 0;
            }

            return Math.Min(40m, mid + quiz + asn + bonus);
        }

        private async Task RecalculateStudentGpaAsync(User user)
        {
            var allCourses = (await _uow.Courses.GetAllAsync()).ToList();
            var allRegs    = await _uow.Registrations.GetByUserIdAsync(user.Id);
            var passed     = allRegs.Where(r => r.IsPassed).ToList();

            decimal weighted = 0; int credits = 0;
            foreach (var reg in passed)
            {
                var c = allCourses.FirstOrDefault(x => x.Id == reg.CourseId);
                if (c == null) continue;

                decimal gpa = 0;
                if (reg.Grade.HasValue)
                    gpa = GradeToGpaPoints(reg.Grade.Value);
                else if (reg.NumericTotal.HasValue)
                    gpa = DeriveGrade(reg.NumericTotal.Value).gpaPoints;

                weighted += c.Credits * gpa;
                credits  += c.Credits;
            }
            user.TotalGPA = credits > 0 ? Math.Round(weighted / credits, 2) : 0;
            user.AllowedCredits = ComputeMaxCredits(user.TotalGPA ?? 0);
        }

        private static int ComputeMaxCredits(decimal gpa) => gpa switch
        {
            >= 3.5m => 21,
            >= 3.0m => 18,
            >= 2.5m => 18,
            >= 2.0m => 15,
            >= 1.5m => 12,
            _       => 9,
        };

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
            _     => (Grads.F,       "F",  0.0m),
        };

        private static decimal GradeToGpaPoints(Grads g) => g switch
        {
            Grads.A_Plus  => 4.0m, Grads.A => 4.0m, Grads.A_Minus => 3.7m,
            Grads.B_Plus  => 3.3m, Grads.B => 3.0m, Grads.B_Minus => 2.7m,
            Grads.C_Plus  => 2.3m, Grads.C => 2.0m, Grads.C_Minus => 1.7m,
            Grads.D_Plus  => 1.3m, Grads.D => 1.0m,
            _             => 0.0m,
        };

        private static string NormalizeStatus(string? s) => (s ?? "").Trim().ToLowerInvariant() switch
        {
            "completed"     => "completed",
            "not_completed" or "notcompleted" or "not-completed" => "not_completed",
            _               => "progress",
        };
    }
}
