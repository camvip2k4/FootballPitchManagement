namespace FootballPitchManagement.Models.Entities
{
    public class PitchType
    {
        public int PitchTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public ICollection<Pitch> Pitches { get; set; } = new List<Pitch>();
    }
}

