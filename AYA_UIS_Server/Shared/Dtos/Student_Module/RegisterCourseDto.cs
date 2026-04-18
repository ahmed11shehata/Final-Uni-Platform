namespace Shared.Dtos.Student_Module
{
    public class RegisterCourseDto
    {
        public string CourseCode { get; set; } = string.Empty;
    }

    public class RegistrationResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public RegistrationCourseDto? Course { get; set; }
    }
}
