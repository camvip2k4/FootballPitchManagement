using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Services
{
    public class AvailabilityChecker : IAvailabilityChecker
    {
        private readonly ApplicationDbContext _context;

        public AvailabilityChecker(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsSlotAvailableAsync(int fieldId, int slotId, DateTime bookingDate)
        {
            var field = await _context.FootballFields.FindAsync(fieldId);
            if (field == null || !field.IsActive || field.Status == FieldStatus.MAINTENANCE)
            {
                return false;
            }

            var slot = await _context.TimeSlots.FindAsync(slotId);
            if (slot == null || !slot.IsActive)
            {
                return false;
            }

            var targetDate = bookingDate.Date;
            var slotStart = targetDate.Add(slot.StartTime);
            var slotEnd = targetDate.Add(slot.EndTime);

            // Check ScheduleBlock
            var hasBlock = await _context.ScheduleBlocks.AnyAsync(sb =>
                sb.FieldId == fieldId &&
                sb.StartTime < slotEnd &&
                sb.EndTime > slotStart);

            if (hasBlock)
            {
                return false;
            }

            // Check existing Bookings on this slot
            var now = DateTime.UtcNow;
            var conflictingBooking = await _context.Bookings.AnyAsync(b =>
                b.FieldId == fieldId &&
                b.SlotId == slotId &&
                b.BookingDate.Date == targetDate &&
                (b.Status == BookingStatus.CONFIRMED ||
                 b.Status == BookingStatus.CHECKED_IN ||
                 (b.Status == BookingStatus.PENDING_PAYMENT && b.HoldExpiresAt > now)));

            return !conflictingBooking;
        }
    }
}

