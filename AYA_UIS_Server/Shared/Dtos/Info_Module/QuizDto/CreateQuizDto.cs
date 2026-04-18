public class CreateQuizDto
{
    public string Title { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int CourseId { get; set; }
}