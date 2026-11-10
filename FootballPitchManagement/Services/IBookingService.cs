using FootballPitchManagement.Models.Entities;
using FootballPitchManagement.Models.Enums;

namespace FootballPitchManagement.Services
{
    public interface IBookingService
    {
        Task<List<Booking>> GetAllBookingsAsync();
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<Booking?> GetBookingByCodeAsync(string code);
        Task<List<Booking>> GetCustomerBookingsAsync(int accountId);
        Task<List<Booking>> GetBookingsByDateAsync(DateTime date);

        Task<(bool Success, string Message, Booking? Booking)> CreateOnlineBookingAsync(
            int fieldId,
            int slotId,
            DateTime bookingDate,
            int accountId);

        Task<(bool Success, string Message, Booking? Booking)> CreateWalkInBookingAsync(
            int fieldId,
            int slotId,
            DateTime bookingDate,
            string guestName,
            string guestPhone,
            decimal? directPaymentAmount,
            PaymentMethod? method,
            string? receiptRef,
            int? staffAccountId);

        Task<(bool Success, string Message)> CancelByCustomerAsync(
            int bookingId,
            int accountId,
            string reason);

        Task<(bool Success, string Message)> CancelByFacilityAsync(
            int bookingId,
            int managerAccountId,
            string reason);

        Task<(bool Success, string Message)> CheckInCustomerAsync(
            int bookingId,
            int staffAccountId);

        Task<(bool Success, string Message)> CheckOutCustomerAsync(
            int bookingId,
            bool isDamaged,
            string? damageNote,
            decimal directPaymentCollected,
            bool markAsUnsettledDebt,
            int staffAccountId);

        Task<(bool Success, string Message)> RecordNoShowAsync(
            int bookingId,
            int staffAccountId);
    }
}
