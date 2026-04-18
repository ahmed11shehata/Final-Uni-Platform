namespace AYA_UIS.Core.Domain.Entities.Identity
{
    public class PasswordResetOtp
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public int Attempts { get; set; } = 0;
        public bool IsUsed { get; set; } = false;
    }
}
