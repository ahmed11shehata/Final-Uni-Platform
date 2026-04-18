namespace Shared.Dtos.Info_Module.AdminCourseLockDtos
{
    public class LockCourseDto
    {
        public string AcademicCode { get; set; } = string.Empty;
        public int    CourseId     { get; set; }
    }

    public class AdminCourseLockResultDto
    {
        public bool   Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
