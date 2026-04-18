namespace Shared.Dtos.Auth_Module
{
    /// <summary>
    /// Wraps the login response in the shape the frontend expects:
    /// { success: true, data: { user: {...}, token: "..." } }
    /// </summary>
    public class FrontendLoginResponseDto
    {
        public bool Success { get; set; } = true;
        public FrontendLoginDataDto Data { get; set; } = new();

        public static FrontendLoginResponseDto FromUserResult(UserResultDto dto) => new()
        {
            Success = true,
            Data = new FrontendLoginDataDto
            {
                Token = dto.Token ?? string.Empty,
                User = new FrontendUserDto
                {
                    Id             = dto.AcademicCode ?? dto.Id,
                    Name           = dto.DisplayName ?? string.Empty,
                    Email          = dto.Email ?? string.Empty,
                    Role           = (dto.Role ?? "student").ToLower(),
                    AcademicCode   = dto.AcademicCode,
                    UserName       = dto.UserName,
                    Gender         = dto.Gender.ToString().ToLower(),
                    Department     = dto.DepartmentName,
                    DepartmentId   = dto.DepartmentId,
                    Level          = dto.Level?.ToString()?.Replace("_", " "),
                    Year           = dto.Level?.ToString()?.Replace("_", " "),
                    EntryYear      = dto.EntryYear,
                    Gpa            = dto.TotalGPA,
                    TotalCredits   = dto.TotalCredits,
                    AllowedCredits = dto.AllowedCredits,
                    Specialization = dto.Specialization,
                    Avatar         = string.IsNullOrEmpty(dto.ProfilePicture) ? null : dto.ProfilePicture,
                    Phone          = dto.PhoneNumber,
                    Address        = dto.Address,
                    Dob            = dto.DateOfBirth,
                    CurrentStudyYearId = dto.CurrentStudyYearId,
                    CurrentSemesterId  = dto.CurrentSemesterId,
                    MustChangePassword = dto.MustChangePassword,
                }
            }
        };
    }

    public class FrontendLoginDataDto
    {
        public FrontendUserDto User { get; set; } = new();
        public string Token { get; set; } = string.Empty;
    }

    public class FrontendUserDto
    {
        public string   Id             { get; set; } = string.Empty;
        public string   Name           { get; set; } = string.Empty;
        public string   Email          { get; set; } = string.Empty;
        public string   Role           { get; set; } = string.Empty;
        public string?  AcademicCode   { get; set; }
        public string?  UserName       { get; set; }
        public string?  Gender         { get; set; }
        public string?  Department     { get; set; }
        public int?     DepartmentId   { get; set; }
        public string?  Level          { get; set; }
        public string?  Year           { get; set; }
        public string?  EntryYear      { get; set; }
        public decimal? Gpa            { get; set; }
        public int?     TotalCredits   { get; set; }
        public int?     AllowedCredits { get; set; }
        public string?  Specialization { get; set; }
        public string?  Avatar         { get; set; }
        public string?  Phone          { get; set; }
        public string?  Address        { get; set; }
        public string?  Dob            { get; set; }
        public int?     CurrentStudyYearId { get; set; }
        public int?     CurrentSemesterId  { get; set; }
        public bool     MustChangePassword { get; set; } = false;
    }
}
