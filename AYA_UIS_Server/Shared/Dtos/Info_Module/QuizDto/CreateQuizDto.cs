public class CreateQuizDto
{
    public string Title { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int CourseId { get; set; }
}