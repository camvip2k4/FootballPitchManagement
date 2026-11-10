using System.ComponentModel.DataAnnotations;
using FootballPitchManagement.Models.Enums;

namespace FootballPitchManagement.Models.Entities
{
    public class FootballField
    {
        [Key]
        public int FieldId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public FieldType FieldType { get; set; } = FieldType.FIELD_7;

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        public string? Description { get; set; }

        public TimeSpan OpenTime { get; set; } = new TimeSpan(6, 0, 0);

        public TimeSpan CloseTime { get; set; } = new TimeSpan(23, 0, 0);

        public FieldStatus Status { get; set; } = FieldStatus.AVAILABLE;

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<PricingRule> PricingRules { get; set; } = new List<PricingRule>();
        public ICollection<ScheduleBlock> ScheduleBlocks { get; set; } = new List<ScheduleBlock>();
    }
}

