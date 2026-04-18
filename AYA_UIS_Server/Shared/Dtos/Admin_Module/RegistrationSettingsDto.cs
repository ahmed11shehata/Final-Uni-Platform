namespace Shared.Dtos.Admin_Module
{
    public class RegistrationSettingsDto
    {
        public bool Open { get; set; }
        public List<int> AllowedYears { get; set; } = new();

        /// <summary>
        /// Maximum credits per student by year (e.g., {"3": 18, "4": 21})
        /// </summary>
        public Dictionary<int, int>? MaxCreditsPerStudent { get; set; }
    }

    public class OpenRegistrationDto
    {
        public List<int> AllowedYears { get; set; } = new();
        public Dictionary<int, int>? MaxCreditsPerStudent { get; set; }
    }
}
