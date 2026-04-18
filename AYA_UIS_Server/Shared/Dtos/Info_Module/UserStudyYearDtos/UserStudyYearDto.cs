using AYA_UIS.Core.Domain.Enums;

namespace Shared.Dtos.Info_Module.UserStudyYearDtos
{
    public class UserStudyYearDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int StudyYearId { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public Levels Level { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public bool IsCurrent { get; set; }
        public DateTime EnrolledAt     { get; set; }
        public int?     CurrentSemesterId { get; set; }
    }
}

