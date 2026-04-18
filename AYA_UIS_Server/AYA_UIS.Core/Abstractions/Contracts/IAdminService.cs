using Shared.Dtos.Admin_Module;

namespace AYA_UIS.Application.Contracts
{
    public interface IAdminService
    {
        // ── 3.1 Dashboard Stats ──────────────────────────────────
        Task<AdminStatsResponseDto> GetStatsAsync();

        // ── 3.2 Email Manager ────────────────────────────────────
        Task<AdminEmailListResponseDto> GetEmailsAsync();
        Task<CreateEmailAccountResponseDto> CreateEmailAccountAsync(CreateEmailAccountDto dto);
        Task<object> ToggleActiveAsync(string userId, string currentUserId);
        Task<ResetPasswordResponseDto> ResetPasswordAsync(string userId, string newPassword);
        Task DeleteAccountAsync(string userId, string currentUserId);
        Task<object> UpdateAccountAsync(string userId, UpdateAccountDto dto);

        // ── 3.3 Schedule Manager ─────────────────────────────────
        Task<SessionResponseDto> CreateSessionAsync(CreateSessionDto dto);
        Task DeleteSessionAsync(int sessionId);
        Task<ExamResponseDto> CreateExamAsync(CreateExamDto dto);
        Task DeleteExamAsync(int examId);
        Task<DateTime> PublishScheduleAsync();
        Task<object> GetScheduleAsync(int? year, string? group, string? view);

        // ── 3.4 Registration Manager ─────────────────────────────
        Task<AdminRegistrationStatusDto> GetRegistrationStatusAsync();
        Task<AdminRegistrationStatusDto> StartRegistrationAsync(StartRegistrationDto dto);
        Task<DateTime> StopRegistrationAsync();
        Task<AdminRegistrationStatusDto> UpdateRegistrationSettingsAsync(StartRegistrationDto dto);

        // ── 3.5 Courses Manager ──────────────────────────────────
        Task<List<AdminCourseListItemDto>> GetCoursesAsync(int? year, string? semester, string? type, string? search);
        Task UpdateCourseSettingsAsync(AdminCourseSettingsDto dto);

        // ── 3.6 Student Control ──────────────────────────────────
        Task<AdminStudentDetailDto> GetStudentAsync(string studentId);
        Task ForceAddCourseAsync(string studentId, AdminAddCourseDto dto);
        Task ForceRemoveCourseAsync(string studentId, string courseCode);
        Task<string> UnlockCourseAsync(string studentId, string courseCode);
        Task LockCourseAsync(string studentId, string courseCode, string? reason = null);
        Task OverrideMaxCreditsAsync(string studentId, AdminMaxCreditsDto dto);

        // ── 3.7 Academic Setup ────────────────────────────────────
        Task<AcademicSetupResponseDto> GetAcademicSetupAsync(string studentId);
        Task<AcademicSetupSaveResultDto> SaveAcademicSetupAsync(string studentId, AcademicSetupSaveRequestDto dto);

        // ── 3.9 Instructor Control ────────────────────────────────
        Task<InstructorControlDto> GetInstructorControlAsync();
        Task AssignInstructorsAsync(int courseId, AssignInstructorsDto dto);

        // ── 3.8 Student Transcript (student self-view) ────────────
        /// <summary>
        /// Returns only courses with real admin-entered grades (equivalency
        /// registrations with IsPassed=true and NumericTotal).
        /// Used by GET /api/student/transcript.
        /// </summary>
        Task<StudentTranscriptResponseDto> GetStudentTranscriptAsync(string studentId);
    }
}
