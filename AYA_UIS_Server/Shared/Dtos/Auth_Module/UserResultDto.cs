using AYA_UIS.Core.Domain.Enums;

namespace Shared.Dtos.Auth_Module
{
    public class UserResultDto
    {
        public string   Id             { get; set; } = string.Empty;
        public string   DisplayName    { get; set; } = string.Empty;
        public string   Email          { get; set; } = string.Empty;
        public string   Token          { get; set; } = string.Empty;
        public string   AcademicCode   { get; set; } = string.Empty;
        public string?  PhoneNumber    { get; set; }
        public string   Role           { get; set; } = string.Empty;
        public string   UserName       { get; set; } = string.Empty;
        public int?     TotalCredits   { get; set; }
        public int?     AllowedCredits { get; set; }
        public decimal? TotalGPA       { get; set; }
        public string?  Specialization { get; set; }
        public Levels?  Level          { get; set; }
        public string?  DepartmentName { get; set; }
        public int?     DepartmentId   { get; set; }
        public string?  ProfilePicture     { get; set; }
        public Gender   Gender             { get; set; }
        public int?     CurrentStudyYearId { get; set; }
        public int?     CurrentSemesterId  { get; set; }
        public string?  Address        { get; set; }
        public string?  DateOfBirth    { get; set; }
        public string?  EntryYear      { get; set; }
        public string   ThemeId            { get; set; } = "default";
        public bool     MustChangePassword { get; set; } = false;
    }
}
