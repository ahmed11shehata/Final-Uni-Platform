namespace Shared.Dtos.Student_Module
{
    public class StudentProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public int EntryYear { get; set; }
        public string Dob { get; set; } = string.Empty;
    }
}
