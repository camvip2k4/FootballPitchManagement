namespace FootballPitchManagement.Models.Entities
{
    public class Pitch
    {
        public int PitchId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PitchTypeId { get; set; }
        public PitchType? PitchType { get; set; }
        public decimal HourlyRate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}

