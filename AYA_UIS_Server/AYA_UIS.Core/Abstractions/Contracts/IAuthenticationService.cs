using Shared.Dtos.Auth_Module;

namespace AYA_UIS.Application.Contracts
{
    public interface IAuthenticationService
    {
        Task<UserResultDto> LoginAsync(LoginDto loginDto);
        Task<UserResultDto> RegisterAsync(RegisterDto registerDto, string role = "Student");
        Task<UserResultDto> RegisterStudentAsync(int departmentId, RegisterStudentDto registerStudentDto);
        Task<string> ResetPasswordAsync(string email, string newPassword);
        Task<string> UpdateRoleByEmailAsync(string email, string newRole);

        /// <summary>
        /// Gets user info from a JWT token (for GET /api/auth/me).
        /// </summary>
        Task<UserResultDto> GetCurrentUserAsync(string userId);

        /// <summary>
        /// Invalidates a JWT token server-side (for POST /api/auth/logout).
        /// </summary>
        Task LogoutAsync(string token);
    }
}
