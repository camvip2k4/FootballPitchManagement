using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Entities;
using FootballPitchManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;

        public BookingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            return await _context.Bookings
                .Include(b => b.Pitch)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Pitch)
                .Include(b => b.Invoices)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public async Task<IEnumerable<Booking>> GetBookingHistoryAsync(string customerPhone)
        {
            return await _context.Bookings
                .Include(b => b.Pitch)
                .Where(b => b.CustomerPhone == customerPhone)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<bool> CreateBookingAsync(BookingRequestVM request)
        {
            var startDateTime = request.BookingDate.Date.Add(request.StartTime);
            var endDateTime = request.BookingDate.Date.Add(request.EndTime);

            var isAvailable = await IsPitchAvailableAsync(request.PitchId, startDateTime, endDateTime);
            if (!isAvailable)
            {
                return false;
            }

            var pitch = await _context.Pitches.FindAsync(request.PitchId);
            if (pitch == null)
            {
                return false;
            }

            var durationHours = (decimal)(endDateTime - startDateTime).TotalHours;
            var totalAmount = durationHours * pitch.HourlyRate;

            var booking = new Booking
            {
                PitchId = request.PitchId,
                CustomerName = request.CustomerName,
                CustomerPhone = request.CustomerPhone,
                CustomerEmail = request.CustomerEmail,
                StartTime = startDateTime,
                EndTime = endDateTime,
                TotalAmount = totalAmount,
                Status = "Confirmed",
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelBookingAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return false;
            }

            booking.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsPitchAvailableAsync(int pitchId, DateTime startTime, DateTime endTime)
        {
            return !await _context.Bookings.AnyAsync(b =>
                b.PitchId == pitchId &&
                b.Status != "Cancelled" &&
                b.StartTime < endTime &&
                b.EndTime > startTime);
        }
    }
}

