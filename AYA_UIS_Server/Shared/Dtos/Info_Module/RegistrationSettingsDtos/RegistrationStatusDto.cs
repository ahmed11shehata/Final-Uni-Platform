namespace Shared.Dtos.Info_Module.RegistrationSettingsDtos
{
    public class RegistrationStatusDto
    {
        public bool         IsOpen          { get; set; }
        public string       Semester        { get; set; } = string.Empty;
        public string       AcademicYear    { get; set; } = string.Empty;
        public DateTime?    StartDate       { get; set; }
        public DateTime?    Deadline        { get; set; }
        public List<int>    OpenYears       { get; set; } = new();
        public List<string> EnabledCourses  { get; set; } = new();
        public int          DaysLeft        { get; set; }
    }

    public class OpenRegistrationDto
    {
        public string       Semester        { get; set; } = string.Empty;
        public string       AcademicYear    { get; set; } = string.Empty;
        public DateTime?    StartDate       { get; set; }
        public DateTime     Deadline        { get; set; }
        public List<int>    OpenYears       { get; set; } = new();
        public List<string> EnabledCourses  { get; set; } = new();
    }
}
