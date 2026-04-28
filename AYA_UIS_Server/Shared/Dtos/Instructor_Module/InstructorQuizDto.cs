namespace Shared.Dtos.Instructor_Module
{
    public class InstructorQuizDto
    {
        public string Id { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int Questions { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int Submissions { get; set; }
        public decimal? AvgScore { get; set; }
        /// <summary>True when at least one student has already attempted the quiz.</summary>
        public bool HasAttempts { get; set; }
        /// <summary>Total points if a per-question grade was supplied at create time. Null otherwise.</summary>
        public decimal? TotalPoints { get; set; }
    }

    /// <summary>
    /// Payload for PUT /api/instructor/courses/{courseId}/quizzes/{quizId}.
    /// If <see cref="Questions"/> is null the question structure is left intact (always
    /// the case when student attempts already exist). When supplied, all existing questions
    /// + options are replaced with the new ones.
    /// </summary>
    public class UpdateInstructorQuizDto
    {
        public string? Title { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<QuizQuestionInputDto>? Questions { get; set; }
        /// <summary>Optional new points-per-question. Locked once any student has attempted the quiz.</summary>
        public decimal? GradePerQ { get; set; }
    }

    public class CreateInstructorQuizDto
    {
        public string CourseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Duration { get; set; }
        public List<QuizQuestionInputDto> Questions { get; set; } = new();
        public decimal GradePerQ { get; set; }
        /// <summary>ISO 8601 — when quiz opens</summary>
        public DateTime StartTime { get; set; }
        /// <summary>ISO 8601 — when quiz closes</summary>
        public DateTime EndTime { get; set; }
    }

    public class QuizQuestionInputDto
    {
        public string Text { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public List<AnswerOptionDto> Answers { get; set; } = new();
        public int Correct { get; set; }
    }

    public class AnswerOptionDto
    {
        public string Text { get; set; } = string.Empty;
    }

    public class QuizSubmissionDto
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string SubmittedAt { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public decimal Max { get; set; }
        public List<int> Answers { get; set; } = new();
    }
}
