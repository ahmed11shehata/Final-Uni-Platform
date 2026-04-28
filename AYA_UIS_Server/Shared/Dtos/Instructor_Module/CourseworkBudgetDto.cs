using System.Text.Json.Serialization;

namespace Shared.Dtos.Instructor_Module
{
    /// <summary>
    /// Snapshot of a course's coursework budget. Used by the frontend to render
    /// "Used X / 40", and by every backend validator before mutations.
    /// </summary>
    public class CourseworkBudgetDto
    {
        [JsonPropertyName("courseId")]         public int CourseId        { get; set; }
        [JsonPropertyName("budget")]           public int Budget          { get; set; } = 40;
        [JsonPropertyName("assignmentMax")]    public int AssignmentMax   { get; set; }
        [JsonPropertyName("quizMax")]          public int QuizMax         { get; set; }
        [JsonPropertyName("midtermMax")]       public int MidtermMax      { get; set; }
        [JsonPropertyName("used")]             public int Used            { get; set; }
        [JsonPropertyName("remaining")]        public int Remaining       { get; set; }
    }
}
