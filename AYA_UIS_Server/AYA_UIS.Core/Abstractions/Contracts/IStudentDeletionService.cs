using Shared.Dtos.Admin_Module;

namespace Abstraction.Contracts
{
    /// <summary>
    /// Permanent student deletion executed from the Email Manager → Danger Zone.
    /// Distinct from Reset Material (course material cleanup) and Academic Year
    /// Reset (year/term advance). Deletes the student account + every row tied
    /// to that student via UserId/StudentId, plus their physical files. Runs in
    /// a single transaction.
    /// </summary>
    public interface IStudentDeletionService
    {
        Task<StudentDeletionPreviewDto> PreviewAsync(string academicCode);

        Task<StudentDeletionResultDto> ExecuteAsync(
            string adminId,
            StudentDeletionExecuteRequestDto request);
    }
}
