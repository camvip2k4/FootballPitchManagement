using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Services
{
    public class PricingService : IPricingService
    {
        private readonly ApplicationDbContext _context;

        public PricingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalculatePriceAsync(int fieldId, int slotId, DateTime bookingDate)
        {
            var slot = await _context.TimeSlots.FindAsync(slotId);
            var field = await _context.FootballFields.FindAsync(fieldId);

            if (slot == null || field == null)
            {
                return 300000m;
            }

            // Determine DayType
            DayType dayType = (bookingDate.DayOfWeek == DayOfWeek.Saturday || bookingDate.DayOfWeek == DayOfWeek.Sunday)
                ? DayType.WEEKEND
                : DayType.WEEKDAY;

            // Determine TimeTier (Peak is >= 17:00 / 5 PM)
            TimeTier timeTier = slot.StartTime >= new TimeSpan(17, 0, 0)
                ? TimeTier.PEAK
                : TimeTier.NORMAL;

            var targetDate = bookingDate.Date;

            // Find matching PricingRule
            var rules = await _context.PricingRules
                .Where(r => r.FieldId == fieldId &&
                            r.DayType == dayType &&
                            r.TimeTier == timeTier &&
                            r.EffectiveFrom.Date <= targetDate &&
                            r.EffectiveTo.Date >= targetDate &&
                            (r.SlotId == null || r.SlotId == slotId))
                .OrderByDescending(r => r.SlotId.HasValue ? 100 + r.Priority : r.Priority)
                .ToListAsync();

            if (rules.Any())
            {
                return rules.First().Price;
            }

            // Fallback base rate
            return field.FieldType switch
            {
                FieldType.FIELD_5 => 200000m,
                FieldType.FIELD_7 => 350000m,
                _ => 700000m
            };
        }
    }
}

