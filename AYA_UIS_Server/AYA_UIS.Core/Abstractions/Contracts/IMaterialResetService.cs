using Shared.Dtos.Admin_Module;

namespace Abstraction.Contracts
{
    /// <summary>
    /// Orchestrates the Admin "Reset Material" feature.
    /// Preview is read-only and reports blocked pending submissions.
    /// Execute requires the fixed password "Material@123#" and runs in a single
    /// DbContext transaction. Distinct from <c>IAcademicYearResetService</c>
    /// (which resets students/term, not material).
    /// </summary>
    public interface IMaterialResetService
    {
        Task<MaterialResetPreviewResponseDto> PreviewAsync(
            MaterialResetPreviewRequestDto request);

        Task<MaterialResetExecuteResponseDto> ExecuteAsync(
            string adminId,
            MaterialResetExecuteRequestDto request);
    }
}
