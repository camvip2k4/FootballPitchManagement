using System.ComponentModel.DataAnnotations;

namespace FootballPitchManagement.Models.Entities
{
    public class TimeSlot
    {
        [Key]
        public int SlotId { get; set; }

        public int FieldId { get; set; }
        public FootballField? Field { get; set; }

        [Required]
        [MaxLength(50)]
        public string SlotName { get; set; } = string.Empty;

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<PricingRule> PricingRules { get; set; } = new List<PricingRule>();
    }
}

