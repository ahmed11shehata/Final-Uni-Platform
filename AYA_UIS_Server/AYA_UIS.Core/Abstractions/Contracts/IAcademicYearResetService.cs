using Shared.Dtos.Admin_Module;

namespace Abstraction.Contracts
{
    /// <summary>
    /// Orchestrates the Academic Year Reset feature: preview impact and execute
    /// (archive current registrations, freeze grade history, advance term,
    /// purge per-student attempt data, audit/snapshot the operation, and notify).
    /// </summary>
    public interface IAcademicYearResetService
    {
        Task<AcademicYearResetPreviewResponseDto> PreviewAsync(
            AcademicYearResetPreviewRequestDto request);

        Task<AcademicYearResetExecuteResponseDto> ExecuteAsync(
            string adminId,
            AcademicYearResetExecuteRequestDto request);
    }
}
