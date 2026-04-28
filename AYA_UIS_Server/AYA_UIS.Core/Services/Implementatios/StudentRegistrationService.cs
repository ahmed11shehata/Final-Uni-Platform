using AYA_UIS.Application.Contracts;
using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Domain.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenedCourseEntryInternal = Shared.Dtos.Admin_Module.OpenedCourseEntryInternal;
using Shared.Dtos.Student_Module;
using System.Text.Json;

namespace Services.Implementatios
{
    public class StudentRegistrationService : IStudentRegistrationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        // Stable colors/patterns per course code
        private static readonly string[] Colors = { "#4F46E5", "#059669", "#D97706", "#DC2626", "#7C3AED", "#0891B2", "#6366f1", "#3b82f6", "#22c55e", "#e05c8a", "#f97316", "#14b8a6" };
        private static readonly string[] Patterns = { "mosaic", "waves", "circles", "squares", "diamonds", "dots" };
        private static readonly string[] Shades = { "#EEF2FF", "#ECFDF5", "#FFFBEB", "#FEF2F2", "#F5F3FF", "#ECFEFF" };

        public StudentRegistrationService(
            IUnitOfWork unitOfWork,
            UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        private static int StableIndex(string code) =>
            Math.Abs(code?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0);

        // ═══════════════════════════════════════════════════════════════
        //  GET /api/student/registration/status
        // ═══════════════════════════════════════════════════════════════
        public async Task<RegistrationStatusDto> GetRegistrationStatusAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var settings = await _unitOfWork.RegistrationSettings.GetCurrentAsync();

            int studentYear = LevelToYear(user?.Level);
            var registrations = await _unitOfWork.Registrations.GetByUserIdAsync(userId);
            int maxCredits = ResolveMaxCredits(user, settings, registrations);

            var allCourses = (await _unitOfWork.Courses.GetAllAsync()).ToDictionary(c => c.Id);

            int currentCredits = 0;
            var registeredCourses = new List<RegisteredCourseItemDto>();

            var activeRegistrations = registrations
                .Where(r => (r.Status == RegistrationStatus.Approved ||
                             r.Status == RegistrationStatus.Pending) &&
                            !r.IsEquivalency &&
                            !r.IsArchived)
                .ToList();

            foreach (var reg in activeRegistrations)
            {
                if (allCourses.TryGetValue(reg.CourseId, out var course))
                {
                    currentCredits += course.Credits;
                    registeredCourses.Add(new RegisteredCourseItemDto
                    {
                        Code    = course.Code,
                        Name    = course.Name,
                        Credits = course.Credits,
                    });
                }
            }

            var failedCourses = registrations
                .Where(r => r.Grade.HasValue && !r.IsPassed)
                .Select(r => allCourses.TryGetValue(r.CourseId, out var c) ? c.Code : null)
                .Where(code => code != null)
                .Distinct()
                .ToList()!;

            var locks = await _unitOfWork.AdminCourseLocks.GetLockedCoursesForUserAsync(userId);
            var lockedCourses = locks
                .Select(l => allCourses.TryGetValue(l.CourseId, out var c) ? c.Code : null)
                .Where(code => code != null)
                .ToList()!;

            // Compute opened pool credits for student's bucket
            int openedPoolCredits = 0;
            if (settings != null && settings.IsOpen)
            {
                var openedByYear = ParseOpenedCoursesByYearRich(settings);
                if (openedByYear.TryGetValue(studentYear.ToString(), out var entries))
                {
                    var openedCodes = entries.Select(e => e.CourseCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    openedPoolCredits = allCourses.Values
                        .Where(c => openedCodes.Contains(c.Code))
                        .Sum(c => c.Credits);
                }
            }

            // Derive current semester number (1 or 2) from the first active registration.
            // SemesterId odd = Semester 1, even = Semester 2.
            var firstActive = activeRegistrations.FirstOrDefault();
            int currentSemesterNum = firstActive != null
                ? (firstActive.SemesterId % 2 == 0 ? 2 : 1)
                : 1;

            return new RegistrationStatusDto
            {
                Open = settings?.IsOpen ?? false,
                AllowedYears = ParseOpenYears(settings),
                CurrentCredits = currentCredits,
                MaxCredits = maxCredits,
                OpenedPoolCredits = openedPoolCredits,
                RegisteredCourses = registeredCourses!,
                FailedCourses = failedCourses!,
                LockedCourses = lockedCourses!,
                CurrentYear = studentYear,
                CurrentSemesterNum = currentSemesterNum,
                Semester = settings?.Semester,
                AcademicYear = settings?.AcademicYear
            };
        }

        // ═══════════════════════════════════════════════════════════════
        //  GET /api/student/registration/courses
        // ═══════════════════════════════════════════════════════════════
        public async Task<RegistrationCoursesDto> GetAvailableCoursesAsync(string userId)
        {
            var settings = await _unitOfWork.RegistrationSettings.GetCurrentAsync();

            // ── 1. Registration closed => empty list ──
            if (settings == null || !settings.IsOpen)
                return new RegistrationCoursesDto
                {
                    YearCounts = new(),
                    Courses = new(),
                    Message = "Registration is closed"
                };

            // ── 2. Parse per-year opened courses from JSON ──
            var openedByYear = ParseOpenedCoursesByYearRich(settings);
            if (openedByYear.Count == 0)
                return new RegistrationCoursesDto { YearCounts = new(), Courses = new() };

            // ── 3. Get student year from Level ──
            var user = await _userManager.FindByIdAsync(userId);
            int studentYear = LevelToYear(user?.Level);
            // Pre-fetch registrations once so ResolveMaxCredits can see history
            var allUserRegsForResolve = await _unitOfWork.Registrations.GetByUserIdAsync(userId);
            int maxCredits = ResolveMaxCredits(user, settings, allUserRegsForResolve);
            bool skipPrereqs = ShouldSkipPrerequisites(user, settings);
            string studentYearKey = studentYear.ToString();

            // ── 4. Build opened codes for THIS student's bucket ONLY ──
            var bucketEntries = new List<OpenedCourseEntryInternal>();
            if (openedByYear.TryGetValue(studentYearKey, out var entries))
                bucketEntries = entries;

            var openedCodes = bucketEntries
                .Select(e => e.CourseCode)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // ── 5. Exception courses for this student ──
            var exceptions = await _unitOfWork.StudentCourseExceptions.GetForUserAsync(userId);
            var exceptionCourseIds = exceptions.Select(e => e.CourseId).ToHashSet();

            // ── 6. Fetch course entities ──
            var allCoursesRaw = await _unitOfWork.Courses.GetAllAsync();
            var allCourses = allCoursesRaw.ToDictionary(c => c.Id);
            var allCoursesByCode = allCoursesRaw.ToDictionary(c => c.Code, c => c, StringComparer.OrdinalIgnoreCase);

            // ── 6b. Active non-equivalency registrations (admin-added must surface too) ──
            var allUserRegs = await _unitOfWork.Registrations.GetByUserIdAsync(userId);
            var activeUserRegs = allUserRegs
                .Where(r => (r.Status == RegistrationStatus.Approved ||
                             r.Status == RegistrationStatus.Pending) &&
                            !r.IsEquivalency &&
                            !r.IsArchived)
                .ToList();

            // Build visible set: bucket opened codes + exception course codes
            // + any course the student is already actively registered for (including
            // admin-added registrations that fall outside the current opened bucket).
            var visibleCodes = new HashSet<string>(openedCodes, StringComparer.OrdinalIgnoreCase);
            foreach (var excId in exceptionCourseIds)
            {
                if (allCourses.TryGetValue(excId, out var excCourse))
                    visibleCodes.Add(excCourse.Code);
            }
            foreach (var reg in activeUserRegs)
            {
                if (allCourses.TryGetValue(reg.CourseId, out var rCourse))
                    visibleCodes.Add(rCourse.Code);
            }

            if (visibleCodes.Count == 0)
                return new RegistrationCoursesDto { YearCounts = new(), Courses = new() };

            var visibleCourseEntities = await _unitOfWork.Courses.GetByCodesAsync(visibleCodes.ToList());

            // ── 7. Student's registrations, completions, locks ──
            var registrations = await _unitOfWork.Registrations.GetByUserIdAsync(userId);

            var activeRegs = registrations
                .Where(r => (r.Status == RegistrationStatus.Approved ||
                             r.Status == RegistrationStatus.Pending) &&
                            !r.IsEquivalency &&
                            !r.IsArchived)
                .ToList();

            var registeredCourseIds = activeRegs
                .Select(r => r.CourseId)
                .ToHashSet();

            // PASS-ONLY prereq: passed means IsPassed == true
            var passedCourseIds = registrations
                .Where(r => r.IsPassed)
                .Select(r => r.CourseId)
                .ToHashSet();

            // Failed = has a grade but NOT passed, and NOT later passed
            var failedCourseIds = registrations
                .Where(r => r.Grade.HasValue && !r.IsPassed)
                .Select(r => r.CourseId)
                .Where(id => !passedCourseIds.Contains(id))
                .ToHashSet();

            var locks = await _unitOfWork.AdminCourseLocks.GetLockedCoursesForUserAsync(userId);
            var locksByCourseid = locks.ToDictionary(l => l.CourseId);

            int currentCredits = activeRegs
                .Sum(r => allCourses.TryGetValue(r.CourseId, out var c) ? c.Credits : 0);

            // ── 8. Seat enrollment counts by bucket ──
            // Count registrations per course for students in the same year bucket
            var allStudents = await _userManager.Users.ToListAsync();
            var studentsByYear = allStudents
                .Where(u => LevelToYear(u.Level) == studentYear)
                .Select(u => u.Id)
                .ToHashSet();

            // Get all registrations for courses in this bucket
            var enrollmentCounts = new Dictionary<int, int>();
            foreach (var entry in bucketEntries)
            {
                if (allCoursesByCode.TryGetValue(entry.CourseCode, out var course))
                {
                    var courseRegs = await _unitOfWork.Registrations.GetByCourseIdAsync(course.Id);
                    var bucketCount = courseRegs
                        .Where(r => (r.Status == RegistrationStatus.Approved ||
                                     r.Status == RegistrationStatus.Pending) &&
                                    !r.IsEquivalency &&
                                    !r.IsArchived &&
                                    studentsByYear.Contains(r.UserId))
                        .Count();
                    enrollmentCounts[course.Id] = bucketCount;
                }
            }

            // Build seat lookup from bucket entries
            var seatLookup = bucketEntries.ToDictionary(
                e => e.CourseCode,
                e => e,
                StringComparer.OrdinalIgnoreCase);

            // Opened pool credits
            int openedPoolCredits = visibleCourseEntities
                .Where(c => openedCodes.Contains(c.Code))
                .Sum(c => c.Credits);

            // ── 9. Build course list with proper status ──
            var courseDtos = new List<RegistrationCourseDto>();
            var yearCounts = new Dictionary<string, int>();

            foreach (var course in visibleCourseEntities)
            {
                var prereqs = await _unitOfWork.Courses.GetCoursePrerequisitesAsync(course.Id);
                var prereqCodes = prereqs.Select(p => p.Code).ToList();

                bool isException = exceptionCourseIds.Contains(course.Id);
                bool isInBucket = openedCodes.Contains(course.Code);

                // Seat info
                seatLookup.TryGetValue(course.Code, out var seatEntry);
                bool isUnlimited = seatEntry?.IsUnlimitedSeats ?? true;
                int? totalSeats = seatEntry?.AvailableSeats;
                enrollmentCounts.TryGetValue(course.Id, out var enrolled);
                int? remaining = isUnlimited ? null : (totalSeats ?? 0) - enrolled;

                // ── Status computation (section 11 order) ──
                string status;
                string? reason = null;
                List<string>? missingPrereqs = null;
                bool retake = false;
                bool canRegister = false;
                bool isAdminLocked = false;

                // a) Already passed
                if (passedCourseIds.Contains(course.Id))
                {
                    status = "completed";
                }
                // b) Already registered
                else if (registeredCourseIds.Contains(course.Id))
                {
                    status = "registered";
                }
                // c) Admin locked
                else if (locksByCourseid.TryGetValue(course.Id, out var lockEntry))
                {
                    status = "locked";
                    reason = string.IsNullOrWhiteSpace(lockEntry.Reason)
                        ? "Locked by admin"
                        : lockEntry.Reason;
                    isAdminLocked = true;
                }
                // d) Prerequisite not passed (PASS-ONLY)
                //    Skipped entirely for First Year / Semester 1 students per spec.
                else
                {
                    var unmetPrereqs = skipPrereqs
                        ? new List<Course>()
                        : prereqs.Where(p => !passedCourseIds.Contains(p.Id)).ToList();

                    if (unmetPrereqs.Any())
                    {
                        status = "unavailable";
                        missingPrereqs = unmetPrereqs.Select(p => p.Name).ToList();
                        if (unmetPrereqs.Count == 1)
                            reason = $"You must pass {unmetPrereqs[0].Name} first";
                        else
                            reason = $"You must pass {string.Join(", ", unmetPrereqs.Select(p => p.Name))} first";
                    }
                    // e) Seats full
                    else if (!isUnlimited && remaining.HasValue && remaining.Value <= 0)
                    {
                        status = "full";
                        reason = "Course is full";
                    }
                    // f) Previously failed
                    else if (failedCourseIds.Contains(course.Id))
                    {
                        status = "available";
                        retake = true;
                        canRegister = true;
                    }
                    // g) Normal available
                    else
                    {
                        status = "available";
                        canRegister = true;
                    }
                }

                // Credit check for canRegister items
                bool wouldExceed = false;
                if (canRegister)
                {
                    if (currentCredits + course.Credits > maxCredits)
                    {
                        wouldExceed = true;
                        canRegister = false;
                    }
                }

                var yearKey = studentYear.ToString();
                yearCounts.TryAdd(yearKey, 0);
                yearCounts[yearKey]++;

                var idx = StableIndex(course.Code);
                courseDtos.Add(new RegistrationCourseDto
                {
                    Id = course.Id.ToString(),
                    CourseId = course.Id,
                    Code = course.Code,
                    Name = course.Name,
                    Credits = course.Credits,
                    Instructor = "",
                    Schedule = "",

                    Capacity = isUnlimited ? null : totalSeats,
                    Enrolled = enrolled,
                    RemainingSeats = remaining,
                    AvailableSeats = isUnlimited ? (object)"unlimited" : totalSeats,
                    IsUnlimitedSeats = isUnlimited,

                    Status = status,
                    Prereqs = prereqCodes,
                    MissingPrerequisites = missingPrereqs,
                    Reason = reason,
                    LockReason = reason,       // backward compat

                    Color = Colors[idx % Colors.Length],
                    Pattern = Patterns[idx % Patterns.Length],

                    Retake = retake,
                    CanRegister = canRegister,
                    IsExceptionCourse = isException,
                    IsAdminLocked = isAdminLocked,

                    CurrentCredits = currentCredits,
                    CurrentMaxCredits = maxCredits,
                    WouldExceedCredits = wouldExceed,

                    Year = studentYear,
                    CourseYear = null  // we don't have a catalog year field on Course entity
                });
            }

            return new RegistrationCoursesDto
            {
                YearCounts = yearCounts,
                Courses = courseDtos
            };
        }

        // ═══════════════════════════════════════════════════════════════
        //  POST /api/student/registration/courses
        // ═══════════════════════════════════════════════════════════════
        public async Task<RegistrationResponseDto> RegisterCourseAsync(string userId, string courseCode)
        {
            // ── 1. Registration open? ──
            var settings = await _unitOfWork.RegistrationSettings.GetCurrentAsync();
            if (settings == null || !settings.IsOpen)
                throw new AYA_UIS.Shared.Exceptions.ForbiddenException("Registration is not currently open.");

            // ── 2. Course exists? ──
            var courses = await _unitOfWork.Courses.GetByCodesAsync(new[] { courseCode });
            var course = courses.FirstOrDefault();
            if (course == null)
                throw new AYA_UIS.Shared.Exceptions.NotFoundException($"Course '{courseCode}' not found.");

            // ── 3. Course in student's bucket or exception? ──
            var user = await _userManager.FindByIdAsync(userId);
            int studentYear = LevelToYear(user?.Level);
            var openedByYear = ParseOpenedCoursesByYearRich(settings);

            var bucketEntries = new List<OpenedCourseEntryInternal>();
            if (openedByYear.TryGetValue(studentYear.ToString(), out var entries))
                bucketEntries = entries;

            var bucketCodes = bucketEntries
                .Select(e => e.CourseCode)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var exceptions = await _unitOfWork.StudentCourseExceptions.GetForUserAsync(userId);
            var exceptionCourseIds = exceptions.Select(e => e.CourseId).ToHashSet();
            bool isException = exceptionCourseIds.Contains(course.Id);

            if (!bucketCodes.Contains(courseCode) && !isException)
                throw new AYA_UIS.Shared.Exceptions.UnprocessableEntityException(
                    $"Course '{courseCode}' is not available for registration in your current academic year.",
                    "NOT_IN_OPENED_POOL");

            // ── 4. Already completed? ──
            var registrations = await _unitOfWork.Registrations.GetByUserIdAsync(userId);
            var passedIds = registrations.Where(r => r.IsPassed).Select(r => r.CourseId).ToHashSet();
            if (passedIds.Contains(course.Id))
                throw new AYA_UIS.Shared.Exceptions.UnprocessableEntityException(
                    $"You have already completed '{course.Name}'.",
                    "ALREADY_COMPLETED");

            // ── 5. Already registered? (409) ──
            var alreadyRegistered = await _unitOfWork.Registrations.IsUserRegisteredInCourseAsync(userId, course.Id);
            if (alreadyRegistered)
                throw new AYA_UIS.Shared.Exceptions.ConflictException($"Already registered in {course.Code}.");

            // ── 6. Admin lock? ──
            var lockEntry = await _unitOfWork.AdminCourseLocks.GetAsync(userId, course.Id);
            if (lockEntry != null)
            {
                var lockReason = string.IsNullOrWhiteSpace(lockEntry.Reason)
                    ? "Locked by admin"
                    : lockEntry.Reason;
                throw new AYA_UIS.Shared.Exceptions.ForbiddenException(lockReason);
            }

            // ── 7. Prerequisites met? (PASS-ONLY) ──
            //    Skipped for First Year / Semester 1 students per spec.
            if (!ShouldSkipPrerequisites(user, settings))
            {
                var prereqs = await _unitOfWork.Courses.GetCoursePrerequisitesAsync(course.Id);
                if (prereqs.Any())
                {
                    var unmetPrereqs = prereqs
                        .Where(p => !passedIds.Contains(p.Id))
                        .ToList();

                    if (unmetPrereqs.Any())
                    {
                        var names = unmetPrereqs.Select(p => p.Name).ToList();
                        var msg = names.Count == 1
                            ? $"You must pass {names[0]} first"
                            : $"You must pass {string.Join(", ", names)} first";
                        throw new AYA_UIS.Shared.Exceptions.UnprocessableEntityException(msg, "PREREQ_NOT_MET");
                    }
                }
            }

            // ── 8. Seats available? ──
            var seatEntry = bucketEntries.FirstOrDefault(e =>
                e.CourseCode.Equals(courseCode, StringComparison.OrdinalIgnoreCase));
            if (seatEntry != null && !seatEntry.IsUnlimitedSeats)
            {
                int totalSeats = seatEntry.AvailableSeats ?? 0;
                // Count current enrollments in this bucket
                var allStudents = await _userManager.Users.ToListAsync();
                var bucketStudentIds = allStudents
                    .Where(u => LevelToYear(u.Level) == studentYear)
                    .Select(u => u.Id)
                    .ToHashSet();

                var courseRegs = await _unitOfWork.Registrations.GetByCourseIdAsync(course.Id);
                int enrolled = courseRegs
                    .Count(r => (r.Status == RegistrationStatus.Approved ||
                                 r.Status == RegistrationStatus.Pending) &&
                                !r.IsEquivalency &&
                                !r.IsArchived &&
                                bucketStudentIds.Contains(r.UserId));

                if (enrolled >= totalSeats)
                    throw new AYA_UIS.Shared.Exceptions.UnprocessableEntityException(
                        "Course is full", "COURSE_FULL");
            }

            // ── 9. Credit limit? ──
            int maxCredits = ResolveMaxCredits(user, settings, registrations);
            var allCourses = (await _unitOfWork.Courses.GetAllAsync()).ToDictionary(c => c.Id);
            int currentCredits = registrations
                .Where(r => (r.Status == RegistrationStatus.Approved ||
                             r.Status == RegistrationStatus.Pending) &&
                            !r.IsEquivalency &&
                            !r.IsArchived)
                .Sum(r => allCourses.TryGetValue(r.CourseId, out var c) ? c.Credits : 0);

            if (currentCredits + course.Credits > maxCredits)
                throw new AYA_UIS.Shared.Exceptions.UnprocessableEntityException(
                    $"Adding '{course.Name}' ({course.Credits} credits) would exceed the maximum of {maxCredits} credits. Current: {currentCredits}.",
                    "CREDIT_LIMIT_EXCEEDED");

            // ── 10. Resolve StudyYear and Semester ──
            StudyYear? resolvedStudyYear = null;
            Semester? resolvedSemester = null;

            if (!string.IsNullOrWhiteSpace(settings.AcademicYear))
            {
                var yearParts = settings.AcademicYear.Split('/', '-');
                if (yearParts.Length == 2 &&
                    int.TryParse(yearParts[0].Trim(), out var startYr) &&
                    int.TryParse(yearParts[1].Trim(), out var endYr))
                {
                    var allYears = await _unitOfWork.StudyYears.GetAllAsync();
                    resolvedStudyYear = allYears.FirstOrDefault(y => y.StartYear == startYr && y.EndYear == endYr);
                }
            }
            resolvedStudyYear ??= await _unitOfWork.StudyYears.GetCurrentStudyYearAsync();
            if (resolvedStudyYear == null)
                throw new AYA_UIS.Shared.Exceptions.BadRequestException("No matching study year found.");

            if (!string.IsNullOrWhiteSpace(settings.Semester))
            {
                var semKey = settings.Semester.Trim().ToLowerInvariant();
                var allSemesters = await _unitOfWork.Semesters.GetByStudyYearIdAsync(resolvedStudyYear.Id);
                resolvedSemester = semKey switch
                {
                    "first" or "first semester" or "semester 1" or "1" =>
                        allSemesters.FirstOrDefault(s => s.Title == SemesterEnum.First_Semester),
                    "second" or "second semester" or "semester 2" or "2" =>
                        allSemesters.FirstOrDefault(s => s.Title == SemesterEnum.Second_Semester),
                    "summer" or "summer semester" or "3" =>
                        allSemesters.FirstOrDefault(s => s.Title == SemesterEnum.Summer),
                    _ => allSemesters.FirstOrDefault()
                };
            }
            resolvedSemester ??= await _unitOfWork.Semesters.GetActiveSemesterByStudyYearIdAsync(resolvedStudyYear.Id);
            if (resolvedSemester == null)
                throw new AYA_UIS.Shared.Exceptions.BadRequestException("No matching semester found.");

            // ── 11. Create registration ──
            var registration = new Registration
            {
                UserId = userId,
                CourseId = course.Id,
                StudyYearId = resolvedStudyYear.Id,
                SemesterId = resolvedSemester.Id,
                Status = RegistrationStatus.Pending,
                Progress = CourseProgress.NotStarted,
                IsPassed = false,
                RegisteredAt = DateTime.UtcNow
            };
            await _unitOfWork.Registrations.AddAsync(registration);
            await _unitOfWork.SaveChangesAsync();

            // ── 12. Response ──
            // Re-compute currentCredits after this registration (excluding equivalency)
            int postCredits = (currentCredits + course.Credits);

            var prereqsList = await _unitOfWork.Courses.GetCoursePrerequisitesAsync(course.Id);
            var prereqCodes = prereqsList.Select(p => p.Code).ToList();

            // Seat info
            var seatEntryResp = bucketEntries.FirstOrDefault(e =>
                e.CourseCode.Equals(courseCode, StringComparison.OrdinalIgnoreCase));
            bool isUnlimited = seatEntryResp?.IsUnlimitedSeats ?? true;

            var idx = StableIndex(course.Code);
            return new RegistrationResponseDto
            {
                Message = $"Successfully registered for {course.Name} ({course.Code}).",
                Course = new RegistrationCourseDto
                {
                    Id = course.Id.ToString(),
                    CourseId = course.Id,
                    Code = course.Code,
                    Name = course.Name,
                    Credits = course.Credits,
                    Instructor = "",
                    Schedule = "",
                    Year = studentYear,
                    Status = "registered",
                    CanRegister = false,
                    Prereqs = prereqCodes,
                    IsUnlimitedSeats = isUnlimited,
                    CurrentCredits = postCredits,
                    CurrentMaxCredits = maxCredits,
                    Color = Colors[idx % Colors.Length],
                    Pattern = Patterns[idx % Patterns.Length]
                }
            };
        }

        // ═══════════════════════════════════════════════════════════════
        //  DELETE /api/student/registration/courses/{courseCode}
        // ═══════════════════════════════════════════════════════════════
        public async Task DropCourseAsync(string userId, string courseCode)
        {
            // Must be open to drop
            var settings = await _unitOfWork.RegistrationSettings.GetCurrentAsync();
            if (settings == null || !settings.IsOpen)
                throw new AYA_UIS.Shared.Exceptions.ForbiddenException("Registration is not currently open. Cannot drop courses.");

            var allCourses = await _unitOfWork.Courses.GetAllAsync();
            var course = allCourses.FirstOrDefault(c =>
                c.Code.Equals(courseCode, StringComparison.OrdinalIgnoreCase));
            if (course == null)
                throw new AYA_UIS.Shared.Exceptions.NotFoundException($"Course '{courseCode}' not found.");

            var registrations = await _unitOfWork.Registrations.GetByUserIdAsync(userId);
            var matchedReg = registrations.FirstOrDefault(r =>
                r.CourseId == course.Id &&
                !r.IsEquivalency &&
                !r.IsArchived &&
                (r.Status == RegistrationStatus.Approved ||
                 r.Status == RegistrationStatus.Pending));

            if (matchedReg == null)
                throw new AYA_UIS.Shared.Exceptions.BadRequestException($"You are not registered for '{courseCode}'.");

            var trackedRegistration = await _unitOfWork.Registrations.GetByIdAsync(matchedReg.Id);
            if (trackedRegistration == null)
                throw new AYA_UIS.Shared.Exceptions.BadRequestException("Registration record not found.");

            await _unitOfWork.Registrations.Delete(trackedRegistration);
            await _unitOfWork.SaveChangesAsync();
            // Seats are computed dynamically from registration count,
            // so dropping automatically "frees" a seat.
        }

        // ═══════════════════════════════════════════════════════════════
        //  GET /api/student/courses  (enrolled)
        // ═══════════════════════════════════════════════════════════════
        public async Task<List<StudentCourseDto>> GetEnrolledCoursesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            int studentYear = LevelToYear(user?.Level);

            var registrations = await _unitOfWork.Registrations.GetByUserIdAsync(userId);
            var activeRegs = registrations
                .Where(r => (r.Status == RegistrationStatus.Approved ||
                             r.Status == RegistrationStatus.Pending) &&
                            !r.IsEquivalency &&
                            !r.IsArchived)
                .ToList();

            // Batch-load all instructor assignments for enrolled courses in one query
            var courseIds = activeRegs
                .Where(r => r.Course != null)
                .Select(r => r.CourseId)
                .Distinct()
                .ToList();

            var instructorAssignments = await _unitOfWork.RegistrationCourseInstructors
                .GetByCourseIdsAsync(courseIds);

            var instructorByCourseid = instructorAssignments
                .GroupBy(x => x.CourseId)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().Instructor?.DisplayName ?? "");

            var result = new List<StudentCourseDto>();
            foreach (var reg in activeRegs)
            {
                var course = reg.Course;
                if (course == null) continue;

                instructorByCourseid.TryGetValue(course.Id, out var instructorName);

                var idx = StableIndex(course.Code);
                result.Add(new StudentCourseDto
                {
                    Id = course.Id.ToString(),
                    Code = course.Code,
                    Name = course.Name,
                    Instructor = instructorName ?? "",
                    Color = Colors[idx % Colors.Length],
                    Shade = Shades[idx % Shades.Length],
                    Pattern = Patterns[idx % Patterns.Length],
                    Credits = course.Credits,
                    Level = studentYear,
                    Semester = reg.StudyYear != null
                        ? $"{reg.StudyYear.StartYear}/{reg.StudyYear.EndYear}"
                        : "",
                    Description = "",
                    Progress = (int)reg.Progress,
                    Students = 0,
                    Icon = "📚"
                });
            }
            return result;
        }

        // ═══════════════════════════════════════════════════════════════
        //  GET /api/student/courses/{courseId}
        // ═══════════════════════════════════════════════════════════════
        public async Task<FullCourseDetailDto> GetCourseDetailAsync(string userId, string courseId)
        {
            if (!int.TryParse(courseId, out var courseIdInt))
                throw new AYA_UIS.Shared.Exceptions.BadRequestException("Invalid course ID.");

            var user = await _userManager.FindByIdAsync(userId);
            int studentYear = LevelToYear(user?.Level);

            var registrations = await _unitOfWork.Registrations.GetByUserIdAsync(userId);
            var reg = registrations.FirstOrDefault(r =>
                r.CourseId == courseIdInt &&
                !r.IsEquivalency &&
                !r.IsArchived &&
                (r.Status == RegistrationStatus.Approved ||
                 r.Status == RegistrationStatus.Pending));

            if (reg == null || reg.Course == null)
                throw new AYA_UIS.Shared.Exceptions.NotFoundException($"You are not enrolled in course '{courseId}'.");

            var course = reg.Course;
            var idx = StableIndex(course.Code);
            var now = DateTime.UtcNow;

            // ── Assignments ──────────────────────────────────────────
            var allAssignments = await _unitOfWork.Assignments.GetAssignmentsByCourseIdAsync(courseIdInt);
            var assignmentDtos = new List<CourseAssignmentDto>();

            foreach (var asn in allAssignments)
            {
                var submission = asn.Submissions.FirstOrDefault(s => s.StudentId == userId);

                string status;
                int? grade = null;
                bool canSubmit = false;
                string? rejectionReason = null;

                if (submission == null || submission.Status == "Cleared")
                {
                    // No active submission (or student soft-cleared their previous one)
                    if (asn.ReleaseDate.HasValue && now < asn.ReleaseDate.Value)
                    {
                        status    = "upcoming";
                        canSubmit = false;
                    }
                    else
                    {
                        canSubmit = now <= asn.Deadline;
                        status    = canSubmit ? "available" : "locked";
                    }
                }
                else if (submission.Status == "Accepted")
                {
                    status = "graded";
                    grade  = submission.Grade;
                }
                else if (submission.Status == "Rejected")
                {
                    status          = "rejected";
                    grade           = 0;
                    rejectionReason = submission.RejectionReason;
                }
                else
                {
                    // Pending — submitted, awaiting review
                    status    = "submitted";
                    canSubmit = now <= asn.Deadline; // allow replace before deadline
                }

                // "upcoming" assignments are visible but not submittable — keep them in the list

                // A "Cleared" submission has no active file — treat it as absent for display purposes
                var activeSubmission = (submission != null && submission.Status != "Cleared") ? submission : null;

                assignmentDtos.Add(new CourseAssignmentDto
                {
                    Id                  = asn.Id.ToString(),
                    Title               = asn.Title,
                    Description         = !string.IsNullOrWhiteSpace(asn.Description) ? asn.Description : null,
                    Deadline            = asn.Deadline.ToString("MMM d, yyyy"),
                    ReleaseDate         = asn.ReleaseDate?.ToString("MMM d, yyyy"),
                    Max                 = asn.Points,
                    Grade               = grade,
                    Status              = status,
                    Types               = new List<string> { "pdf", "doc", "docx", "zip" },
                    File                = activeSubmission?.FileUrl != null ? Path.GetFileName(activeSubmission.FileUrl) : null,
                    AttachmentUrl       = !string.IsNullOrWhiteSpace(asn.FileUrl) ? asn.FileUrl : null,
                    RejectionReason     = rejectionReason,
                    CanSubmit           = canSubmit,
                    SubmissionId        = activeSubmission?.Id,
                    SubmissionFileName  = activeSubmission != null ? Path.GetFileName(activeSubmission.FileUrl) : null,
                    SubmissionFileUrl   = activeSubmission?.FileUrl,
                    SubmissionDate      = activeSubmission?.SubmittedAt.ToString("MMM d, yyyy h:mm tt"),
                    AttemptCount        = submission?.AttemptCount ?? 0,
                });
            }

            // ── Quizzes ──────────────────────────────────────────────
            var allQuizzes  = await _unitOfWork.Quizzes.GetQuizzesByCourseId(courseIdInt);
            var quizDtos    = new List<CourseQuizSummaryDto>();

            foreach (var quiz in allQuizzes)
            {
                var attempt = await _unitOfWork.Quizzes.GetStudentAttemptAsync(quiz.Id, userId);

                string quizStatus;
                decimal? score = null;

                if (attempt != null)
                {
                    quizStatus = "completed";
                    score      = attempt.Score;
                }
                else if (now < quiz.StartTime)
                {
                    quizStatus = "upcoming";
                }
                else if (now <= quiz.EndTime)
                {
                    quizStatus = "available";
                }
                else
                {
                    quizStatus = "completed"; // past deadline, no attempt
                }

                int qCount       = quiz.Questions?.Count ?? 0;
                int durationMins = (int)Math.Round((quiz.EndTime - quiz.StartTime).TotalMinutes);

                quizDtos.Add(new CourseQuizSummaryDto
                {
                    Id              = quiz.Id.ToString(),
                    Title           = quiz.Title,
                    Date            = quiz.StartTime.ToString("MMM d, yyyy"),
                    StartIso        = quiz.StartTime.ToString("yyyy-MM-dd"),
                    StartTime       = quiz.StartTime.ToString("h:mm tt"),
                    Duration        = $"{durationMins} min",
                    Questions       = qCount,
                    Max             = qCount * (quiz.GradePerQuestion <= 0m ? 1m : quiz.GradePerQuestion),
                    Score           = score,
                    Status          = quizStatus,
                    Deadline        = quiz.EndTime.ToString("MMM d, yyyy"),
                    ReviewAvailable = now > quiz.EndTime
                });
            }

            // ── Midterm ──────────────────────────────────────────────
            var midtermGrade = await _unitOfWork.MidtermGrades.GetAsync(userId, courseIdInt);
            CourseMidtermDto? midtermDto = null;

            if (midtermGrade != null)
            {
                // Try to get exam schedule info
                var examEntries = await _unitOfWork.ExamSchedules.GetAllAsync();
                var examEntry   = examEntries.FirstOrDefault(e =>
                    e.CourseId == courseIdInt &&
                    e.Type.Equals("midterm", StringComparison.OrdinalIgnoreCase));

                string dateStr = examEntry?.Date.ToString("MMMM d, yyyy") ?? "";
                string timeStr = "";
                if (examEntry != null)
                {
                    var startH = (int)examEntry.StartTime;
                    var startM = (int)((examEntry.StartTime - startH) * 60);
                    var endDbl = examEntry.StartTime + examEntry.Duration;
                    var endH   = (int)endDbl;
                    var endM   = (int)((endDbl - endH) * 60);
                    timeStr = $"{startH:D2}:{startM:D2} – {endH:D2}:{endM:D2}";
                }

                midtermDto = new CourseMidtermDto
                {
                    Published = midtermGrade.Published,
                    Grade     = midtermGrade.Published ? midtermGrade.Grade : (int?)null,
                    Max       = midtermGrade.Max,
                    Date      = dateStr,
                    Time      = timeStr,
                    Room      = examEntry?.Location ?? ""
                };
            }

            // ── Lectures ─────────────────────────────────────────────
            var allUploads  = await _unitOfWork.CourseUploads.GetByCourseIdAsync(courseIdInt);
            var lectureDtos = allUploads
                .Where(u => u.Type == UploadType.Lecture &&
                            (!u.ReleaseDate.HasValue || u.ReleaseDate.Value <= now))
                .OrderBy(u => u.Week ?? int.MaxValue)
                .ThenBy(u => u.UploadedAt)
                .Select(u => new CourseLectureDto
                {
                    Id       = u.Id.ToString(),
                    Week     = u.Week,
                    Title    = u.Title,
                    Type     = Path.GetExtension(u.Url).TrimStart('.').ToLowerInvariant() is "mp4" or "webm" ? "video" : "pdf",
                    Duration = "",
                    Date     = u.UploadedAt.ToString("MMM d, yyyy"),
                    Size     = "",
                    Watched  = false,
                    Url      = u.Url,
                })
                .ToList();

            // ── Instructor name ──────────────────────────────────────
            var instructorName = "";
            var instructorRecs = await _unitOfWork.RegistrationCourseInstructors
                .GetByCourseAsync(courseIdInt);
            var instructorRec = instructorRecs.FirstOrDefault();
            if (instructorRec?.Instructor != null)
                instructorName = instructorRec.Instructor.DisplayName ?? "";

            return new FullCourseDetailDto
            {
                Meta = new CourseMetaDto
                {
                    Id          = course.Id.ToString(),
                    Code        = course.Code,
                    Name        = course.Name,
                    Instructor  = instructorName,
                    Color       = Colors[idx % Colors.Length],
                    Shade       = Shades[idx % Shades.Length],
                    Light       = Shades[idx % Shades.Length],
                    Credits     = course.Credits,
                    Level       = studentYear,
                    Semester    = reg.StudyYear != null
                        ? $"{reg.StudyYear.StartYear}/{reg.StudyYear.EndYear}"
                        : "",
                    Description = "",
                    Progress    = (int)reg.Progress
                },
                Lectures    = lectureDtos,
                Assignments = assignmentDtos,
                Quizzes     = quizDtos,
                Midterm     = midtermDto
            };
        }

        // ═══════════════════════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Convert Levels enum to int year (1-4).</summary>
        private static int LevelToYear(Levels? level) => level switch
        {
            Levels.Preparatory_Year => 1,
            Levels.First_Year => 1,
            Levels.Second_Year => 2,
            Levels.Third_Year => 3,
            Levels.Fourth_Year => 4,
            Levels.Graduate => 4,
            _ => 4 // default max visibility
        };

        /// <summary>Parse rich OpenedCoursesByYear JSON from settings.</summary>
        private static Dictionary<string, List<OpenedCourseEntryInternal>> ParseOpenedCoursesByYearRich(
            RegistrationSettings? settings)
        {
            if (settings == null) return new();

            // Try new rich format first: {"1":[{"courseCode":"CS101","availableSeats":30,...}], ...}
            if (!string.IsNullOrWhiteSpace(settings.OpenedCoursesByYear))
            {
                try
                {
                    var rich = JsonSerializer.Deserialize<Dictionary<string, List<OpenedCourseEntryInternal>>>(
                        settings.OpenedCoursesByYear,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (rich != null && rich.Count > 0)
                        return rich;
                }
                catch { /* not rich format, try legacy */ }

                // Try legacy flat format: {"1":["CS101","CS102"], ...}
                try
                {
                    var flat = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(
                        settings.OpenedCoursesByYear);
                    if (flat != null && flat.Count > 0)
                    {
                        var result = new Dictionary<string, List<OpenedCourseEntryInternal>>();
                        foreach (var (k, codes) in flat)
                        {
                            result[k] = codes.Select(c => new OpenedCourseEntryInternal
                            {
                                CourseCode = c,
                                AvailableSeats = null,
                                IsUnlimitedSeats = true
                            }).ToList();
                        }
                        return result;
                    }
                }
                catch { /* malformed */ }
            }

            // Legacy fallback: put all enabled codes under each open year
            if (!string.IsNullOrWhiteSpace(settings.EnabledCourses))
            {
                var codes = settings.EnabledCourses
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
                var years = ParseOpenYears(settings);
                var result = new Dictionary<string, List<OpenedCourseEntryInternal>>();
                foreach (var yr in years)
                    result[yr.ToString()] = codes.Select(c => new OpenedCourseEntryInternal
                    {
                        CourseCode = c,
                        AvailableSeats = null,
                        IsUnlimitedSeats = true
                    }).ToList();
                if (years.Count == 0 && codes.Count > 0)
                    result["1"] = codes.Select(c => new OpenedCourseEntryInternal
                    {
                        CourseCode = c,
                        AvailableSeats = null,
                        IsUnlimitedSeats = true
                    }).ToList();
                return result;
            }

            return new();
        }

        private static List<int> ParseOpenYears(RegistrationSettings? settings)
        {
            if (settings == null || string.IsNullOrWhiteSpace(settings.OpenYears))
                return new List<int>();
            return settings.OpenYears
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => int.TryParse(x, out var n) ? n : 0)
                .Where(n => n > 0)
                .ToList();
        }

        /// <summary>
        /// Resolve max credits for the student:
        /// 1. Admin per-student override (AllowedCredits set explicitly by admin) wins.
        /// 2. New students (First Year/Sem 1, or any year with no academic history) → 21.
        /// 3. GPA-based rules per spec:
        ///    >= 3.0 → 21,  2.0–2.99 → 18,  1.0–1.99 → 12,  < 1.0 → 9.
        /// </summary>
        private static int ResolveMaxCredits(
            User? user,
            RegistrationSettings? settings,
            IEnumerable<Registration>? registrations = null)
        {
            // 1. Admin per-student override has highest priority.
            //    AllowedCredits default value is 21, so only treat as override if admin
            //    explicitly changed it to something other than 21.
            if (user?.AllowedCredits != null && user.AllowedCredits != 21)
                return user.AllowedCredits.Value;

            // 2. New-student rule (First Year/Sem 1 OR any year with no history) → 21.
            if (user != null && IsInitialNewStudent(user, registrations ?? Enumerable.Empty<Registration>(), settings))
                return 21;

            // 3. GPA-based rules per spec
            decimal gpa = user?.TotalGPA ?? 0m;
            if (gpa >= 3.0m) return 21;
            if (gpa >= 2.0m) return 18;
            if (gpa >= 1.0m) return 12;
            return 9;
        }

        /// <summary>
        /// New-student detection — kept consistent with AdminService.IsInitialNewStudent.
        /// A student is "new" when:
        ///   1) They are in First Year AND the global registration semester is the first semester, OR
        ///   2) They have no academic history yet (no passed/completed/graded registrations).
        /// </summary>
        private static bool IsInitialNewStudent(
            User user,
            IEnumerable<Registration> registrations,
            RegistrationSettings? settings)
        {
            int year = LevelToYear(user.Level);
            bool isFirstSem = IsFirstSemesterTerm(settings);

            if (year == 1 && isFirstSem) return true;

            bool hasHistory = registrations.Any(r =>
                r.IsPassed
                || r.Progress == CourseProgress.Completed
                || r.Grade.HasValue);
            return !hasHistory;
        }

        /// <summary>
        /// True iff prerequisite enforcement should be SKIPPED for this student.
        /// Per spec: First Year / Semester 1 students are not blocked by prereqs.
        /// </summary>
        private static bool ShouldSkipPrerequisites(User? user, RegistrationSettings? settings)
        {
            if (user == null) return false;
            return LevelToYear(user.Level) == 1 && IsFirstSemesterTerm(settings);
        }

        /// <summary>
        /// True if the global registration semester is the first semester.
        /// Defaults to true when settings/semester is unset (safe for brand-new students).
        /// Handles common stored forms: "First_Semester", "first", "Semester 1", "1", etc.
        /// </summary>
        private static bool IsFirstSemesterTerm(RegistrationSettings? settings)
        {
            if (settings == null || string.IsNullOrWhiteSpace(settings.Semester)) return true;
            var sem = settings.Semester.Trim().ToLowerInvariant().Replace("_", " ");
            return sem.Contains("first") || sem == "1" || sem.Contains("semester 1") || sem.Contains("sem 1");
        }

        /// <summary>Legacy alias — kept for backward compatibility with any older calls.</summary>
        private static bool IsFirstTerm(RegistrationSettings settings) => IsFirstSemesterTerm(settings);
    }
}
