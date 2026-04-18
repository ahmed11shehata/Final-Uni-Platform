namespace Shared.Dtos.Admin_Module
{
    public class SaveScheduleDto
    {
        public List<ScheduleSessionDto> Sessions { get; set; } = new();
        public List<ExamScheduleDto> Exams { get; set; } = new();
    }

    public class SaveScheduleResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int Saved { get; set; }
    }
}
