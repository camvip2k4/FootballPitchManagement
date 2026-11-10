namespace FootballPitchManagement.Models.ViewModels
{
    public class PitchDetailVM
    {
        public int PitchId { get; set; }
        public string PitchName { get; set; } = string.Empty;
        public string PitchTypeName { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
    }
}

