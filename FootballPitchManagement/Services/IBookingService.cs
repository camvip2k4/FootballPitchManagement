using FootballPitchManagement.Models.Entities;
using FootballPitchManagement.Models.ViewModels;

namespace FootballPitchManagement.Services
{
    public interface IBookingService
    {
        Task<IEnumerable<Booking>> GetAllBookingsAsync();
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<IEnumerable<Booking>> GetBookingHistoryAsync(string customerPhone);
        Task<bool> CreateBookingAsync(BookingRequestVM request);
        Task<bool> CancelBookingAsync(int id);
        Task<bool> IsPitchAvailableAsync(int pitchId, DateTime startTime, DateTime endTime);
    }
}

