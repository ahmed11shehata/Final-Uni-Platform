namespace Shared.Dtos.Admin_Module
{
    public class AdminUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class CreateAdminUserDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? StudentId { get; set; }
        public string Department { get; set; } = string.Empty;
        public int? Year { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
