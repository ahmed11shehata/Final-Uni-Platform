using AYA_UIS.Core.Domain.Enums;

namespace Shared.Dtos.Info_Module.DashboardDtos
{
    // ── Admin ───────────────────────────────────────────────
    public class AdminDashboardDto
    {
        public int    TotalStudents        { get; set; }
        public int    TotalInstructors     { get; set; }
        public int    TotalCourses         { get; set; }
        public int    ActiveRegistrations  { get; set; }
        public bool   RegistrationOpen     { get; set; }
        public CurrentStudyYearDto? CurrentStudyYear { get; set; }
        public List<object> RecentActivity { get; set; } = new();
    }

    public class CurrentStudyYearDto
    {
        public int Id        { get; set; }
        public int StartYear { get; set; }
        public int EndYear   { get; set; }
    }

    public class AdminUserDto
    {
        public string  Id                 { get; set; } = string.Empty;
        public string  Code               { get; set; } = string.Empty;
        public string  Name               { get; set; } = string.Empty;
        public string? Email              { get; set; }
        public string  Role               { get; set; } = string.Empty;
        public string? Gender             { get; set; }
        public string? Dept               { get; set; }
        public decimal Gpa                { get; set; }
        public int     TotalCreditsEarned { get; set; }
        public int     AllowedCredits     { get; set; }
        public string? Phone              { get; set; }
        public string? Avatar             { get; set; }
        public string? Level              { get; set; }
        public bool    Active             { get; set; } = true;
        public List<string> RegisteredCourses  { get; set; } = new();
        public List<CompletedCourseDto> CompletedCourses { get; set; } = new();
        public List<string> FailedCourses      { get; set; } = new();
    }

    public class CompletedCourseDto
    {
        public string  Code     { get; set; } = string.Empty;
        public decimal? Total   { get; set; }
        public int     Year     { get; set; }
        public int     Semester { get; set; }
    }

    // ── Student ─────────────────────────────────────────────
    public class StudentDashboardDto
    {
        public StudentStatsDto Stats                 { get; set; } = new();
        public List<object>    UpcomingEvents        { get; set; } = new();
        public List<object>    EnrolledCourses       { get; set; } = new();
    }

    public class StudentStatsDto
    {
        public decimal Gpa            { get; set; }
        public int     Credits        { get; set; }
        public int     AllowedCredits { get; set; }
        public string  Standing       { get; set; } = string.Empty;
        public string  StandingColor  { get; set; } = string.Empty;
        public int     Year           { get; set; }
        public int     Semester       { get; set; }
        public string  Department     { get; set; } = string.Empty;
    }

    public class StudentTranscriptDto
    {
        public TranscriptStudentDto Student { get; set; } = new();
        public List<TranscriptYearDto> Years { get; set; } = new();
    }

    public class TranscriptStudentDto
    {
        public string Name            { get; set; } = string.Empty;
        public string Id              { get; set; } = string.Empty;
        public string Department      { get; set; } = string.Empty;
        public int    CurrentYear     { get; set; }
        public int    CurrentSemester { get; set; }
    }

    public class TranscriptYearDto
    {
        public int    Year      { get; set; }
        public string Label     { get; set; } = string.Empty;
        public List<TranscriptSemesterDto> Semesters { get; set; } = new();
    }

    public class TranscriptSemesterDto
    {
        public string  Id           { get; set; } = string.Empty;
        public string  Label        { get; set; } = string.Empty;
        public string  Period       { get; set; } = string.Empty;
        public bool    IsCurrent    { get; set; }
        public int     TotalCredits { get; set; }
        public decimal? SemesterGpa { get; set; }
        public List<TranscriptCourseDto> Courses { get; set; } = new();
    }

    public class TranscriptCourseDto
    {
        public string  Code    { get; set; } = string.Empty;
        public string  Name    { get; set; } = string.Empty;
        public int     Credits { get; set; }
        public string  Grade   { get; set; } = string.Empty;
        public decimal Points  { get; set; }
        public string  Total   { get; set; } = string.Empty;
    }

    // ── Timetable ────────────────────────────────────────────
    public class TimetableEventDto
    {
        public int     Id         { get; set; }
        public string  Type       { get; set; } = string.Empty;
        public string  Title      { get; set; } = string.Empty;
        public string  CourseCode { get; set; } = string.Empty;
        public string  Course     { get; set; } = string.Empty;
        public string  Date       { get; set; } = string.Empty;
        public string  Time       { get; set; } = string.Empty;
        public string? Duration   { get; set; }
    }

    // ── Instructor ───────────────────────────────────────────
    public class InstructorDashboardDto
    {
        public List<InstructorCourseDto>    Courses        { get; set; } = new();
        public Dictionary<string, object>   GradeSummary   { get; set; } = new();
        public List<object>                 RecentActivity { get; set; } = new();
        public List<object>                 Upcoming       { get; set; } = new();
    }

    public class InstructorCourseDto
    {
        public string Id       { get; set; } = string.Empty;
        public string Code     { get; set; } = string.Empty;
        public string Name     { get; set; } = string.Empty;
        public string Color    { get; set; } = "#818cf8";
        public string Icon     { get; set; } = "📚";
        public int    Students { get; set; }
        public int    Progress { get; set; }
    }
}
