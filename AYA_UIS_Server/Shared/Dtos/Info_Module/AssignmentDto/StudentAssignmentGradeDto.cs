namespace Shared.Dtos.Info_Module.AssignmentDto
{
    public class StudentAssignmentGradeDto
    {
        public int       AssignmentId    { get; set; }
        public string    AssignmentTitle { get; set; } = string.Empty;
        public int       Points          { get; set; }
        public DateTime  Deadline        { get; set; }
        public DateTime? SubmittedAt     { get; set; }
        public int?      Grade           { get; set; }
        public string?   Feedback        { get; set; }
        public string?   FileUrl         { get; set; }
    }
}
