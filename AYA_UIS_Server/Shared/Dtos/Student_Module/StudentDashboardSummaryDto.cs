using System.Collections.Generic;

namespace Shared.Dtos.Student_Module
{
    /// <summary>
    /// One-shot payload for GET /api/student/dashboard/summary.
    /// Drives the existing StudentDashboard hero card, stat chips, and "My Courses"
    /// list with a single round-trip — no mock data on the frontend.
    /// </summary>
    public class StudentDashboardSummaryDto
    {
        public DashboardStudentDto Student   { get; set; } = new();
        public DashboardAcademicDto Academic { get; set; } = new();
        public DashboardCoursesDto Courses   { get; set; } = new();
        public DashboardCountsDto Counts     { get; set; } = new();
    }

    public class DashboardStudentDto
    {
        public string Id                    { get; set; } = string.Empty;
        public string Name                  { get; set; } = string.Empty;
        public string StudentCode           { get; set; } = string.Empty;
        public string Email                 { get; set; } = string.Empty;
        public string ProfilePicture        { get; set; } = string.Empty;
        public string Level                 { get; set; } = string.Empty;
        public int    CurrentYear           { get; set; }
        public string CurrentYearLabel      { get; set; } = string.Empty;
        public int    CurrentSemester       { get; set; }
        public string CurrentSemesterLabel  { get; set; } = string.Empty;
        public string AcademicYear          { get; set; } = string.Empty;
    }

    public class DashboardAcademicDto
    {
        public decimal Gpa                { get; set; }
        public int     RegisteredCredits  { get; set; }
        public int     AllowedCredits     { get; set; }
        public int     RemainingCredits   { get; set; }
        public string  Standing           { get; set; } = string.Empty;
        public bool    IsNewStudent       { get; set; }
    }

    public class DashboardCoursesDto
    {
        public int RegisteredCount { get; set; }
        public List<DashboardCourseItemDto> Items { get; set; } = new();
    }

    public class DashboardCourseItemDto
    {
        public string Id         { get; set; } = string.Empty;
        public string Code       { get; set; } = string.Empty;
        public string Name       { get; set; } = string.Empty;
        public string Instructor { get; set; } = string.Empty;
        public int    Progress   { get; set; }
        public string Color      { get; set; } = string.Empty;
        public string Icon       { get; set; } = string.Empty;
    }

    public class DashboardCountsDto
    {
        public int AssignmentsDueThisWeek { get; set; }
        public int QuizzesPending         { get; set; }
    }
}
