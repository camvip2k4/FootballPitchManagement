using System.ComponentModel.DataAnnotations;
using FootballPitchManagement.Models.Enums;

namespace FootballPitchManagement.Models.Entities
{
    public class PricingRule
    {
        [Key]
        public int RuleId { get; set; }

        public int FieldId { get; set; }
        public FootballField? Field { get; set; }

        public int? SlotId { get; set; }
        public TimeSlot? Slot { get; set; }

        public decimal Price { get; set; }

        public DayType DayType { get; set; } = DayType.WEEKDAY;

        public TimeTier TimeTier { get; set; } = TimeTier.NORMAL;

        public int Priority { get; set; } = 1;

        public DateTime EffectiveFrom { get; set; } = DateTime.MinValue;

        public DateTime EffectiveTo { get; set; } = DateTime.MaxValue;
    }
}

