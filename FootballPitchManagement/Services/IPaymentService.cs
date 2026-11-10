using FootballPitchManagement.Models.Entities;
using FootballPitchManagement.Models.Enums;

namespace FootballPitchManagement.Services
{
    public interface IPaymentService
    {
        Task<(bool Success, string Message, string ResultStatus)> ProcessOnlinePaymentAsync(
            int bookingId,
            decimal amount,
            PaymentMethod method,
            PaymentStatus simulatedResult,
            bool simulateLatePayment);

        Task<(bool Success, string Message)> RecordDirectPaymentAsync(
            int bookingId,
            decimal amount,
            PaymentMethod method,
            string receiptRef,
            int? staffAccountId);
    }
}

