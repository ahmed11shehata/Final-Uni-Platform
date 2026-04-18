namespace Shared.Dtos.Student_Module
{
    public class CourseMidtermDto
    {
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public bool Published { get; set; }
        public int? Grade { get; set; }
        public int Max { get; set; }
    }
}
