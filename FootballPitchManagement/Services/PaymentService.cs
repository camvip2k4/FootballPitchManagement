using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Entities;
using FootballPitchManagement.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public PaymentService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<(bool Success, string Message, string ResultStatus)> ProcessOnlinePaymentAsync(
            int bookingId,
            decimal amount,
            PaymentMethod method,
            PaymentStatus simulatedResult,
            bool simulateLatePayment)
        {
            var booking = await _context.Bookings
                .Include(b => b.Field)
                .Include(b => b.TimeSlot)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return (false, "Booking not found.", "NOT_FOUND");
            }

            var now = DateTime.UtcNow;
            var isLate = simulateLatePayment ||
                         booking.Status == BookingStatus.EXPIRED ||
                         booking.Status == BookingStatus.CANCELLED ||
                         booking.HoldExpiresAt < now;

            // Scenario 1: Payment Failed
            if (simulatedResult == PaymentStatus.FAILED)
            {
                var failedPayment = new Payment
                {
                    BookingId = booking.BookingId,
                    TransactionRef = "TXN-FAIL-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    Amount = amount,
                    Method = method,
                    Channel = PaymentChannel.ONLINE,
                    Status = PaymentStatus.FAILED,
                    PaidAt = now
                };
                _context.Payments.Add(failedPayment);

                // If hold still active, keep or transition to PAYMENT_FAILED
                if (booking.HoldExpiresAt > now && booking.Status == BookingStatus.PENDING_PAYMENT)
                {
                    booking.Status = BookingStatus.PAYMENT_FAILED;
                }
                await _context.SaveChangesAsync();

                if (booking.AccountId.HasValue)
                {
                    await _notificationService.SendNotificationAsync(
                        booking.AccountId.Value,
                        "PAYMENT_FAILED",
                        $"Online payment for booking {booking.BookingCode} failed. You can retry before the hold expires.");
                }

                return (false, "Payment failed from the gateway.", "FAILED");
            }

            // Scenario 2: Payment Pending / Unknown / Timeout
            if (simulatedResult == PaymentStatus.PENDING)
            {
                var pendingPayment = new Payment
                {
                    BookingId = booking.BookingId,
                    TransactionRef = "TXN-PEND-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    Amount = amount,
                    Method = method,
                    Channel = PaymentChannel.ONLINE,
                    Status = PaymentStatus.PENDING,
                    PaidAt = now
                };
                _context.Payments.Add(pendingPayment);
                await _context.SaveChangesAsync();

                return (false, "Payment is in pending/unknown status. Please verify status with provider.", "PENDING");
            }

            // Scenario 3: Payment Success
            var successPayment = new Payment
            {
                BookingId = booking.BookingId,
                TransactionRef = "TXN-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                Amount = amount,
                Method = method,
                Channel = PaymentChannel.ONLINE,
                Status = PaymentStatus.SUCCESS,
                PaidAt = now
            };
            _context.Payments.Add(successPayment);

            if (isLate)
            {
                // Late payment handling (UC14):
                // Record payment, create 100% RefundRequest
                var refundRequest = new RefundRequest
                {
                    RefundCode = "REF-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    BookingId = booking.BookingId,
                    Payment = successPayment,
                    Amount = amount,
                    Reason = "Late payment received after hold expired or booking cancelled.",
                    RefundType = RefundType.LATE_PAYMENT,
                    Status = RefundStatus.PENDING,
                    CreatedAt = now
                };
                _context.RefundRequests.Add(refundRequest);
                await _context.SaveChangesAsync();

                if (booking.AccountId.HasValue)
                {
                    await _notificationService.SendNotificationAsync(
                        booking.AccountId.Value,
                        "LATE_PAYMENT_REFUND",
                        $"Your payment for {booking.BookingCode} arrived after hold expiry. The slot was released, and a 100% refund of {amount:N0} VND has been queued.");
                }

                return (true, "Payment arrived late after hold expired. A 100% refund has been queued.", "LATE_PAYMENT");
            }

            // Normal timely payment
            booking.PaidAmount += amount;
            booking.BalanceDue = Math.Max(0, booking.AgreedPrice - booking.PaidAmount);

            if (booking.PaidAmount >= booking.RequiredPrepayment)
            {
                booking.Status = BookingStatus.CONFIRMED;
            }

            booking.SettlementStatus = (booking.BalanceDue == 0)
                ? SettlementStatus.PAID
                : SettlementStatus.PARTIALLY_PAID;

            await _context.SaveChangesAsync();

            if (booking.AccountId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    booking.AccountId.Value,
                    "BOOKING_CONFIRMED",
                    $"Your booking {booking.BookingCode} is confirmed! Paid: {amount:N0} VND. Remaining balance: {booking.BalanceDue:N0} VND.");
            }

            return (true, "Payment successful! Your booking is confirmed.", "SUCCESS");
        }

        public async Task<(bool Success, string Message)> RecordDirectPaymentAsync(
            int bookingId,
            decimal amount,
            PaymentMethod method,
            string receiptRef,
            int? staffAccountId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                return (false, "Booking not found.");
            }

            var payment = new Payment
            {
                BookingId = booking.BookingId,
                TransactionRef = "DIR-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                Amount = amount,
                Method = method,
                Channel = PaymentChannel.DIRECT,
                Status = PaymentStatus.SUCCESS,
                ReceiptRef = receiptRef,
                PaidAt = DateTime.UtcNow
            };
            _context.Payments.Add(payment);

            booking.PaidAmount += amount;
            booking.BalanceDue = Math.Max(0, booking.AgreedPrice - booking.PaidAmount);

            if (booking.Status == BookingStatus.PENDING_PAYMENT && booking.PaidAmount >= booking.RequiredPrepayment)
            {
                booking.Status = BookingStatus.CONFIRMED;
            }

            booking.SettlementStatus = (booking.BalanceDue == 0)
                ? SettlementStatus.PAID
                : SettlementStatus.PARTIALLY_PAID;

            await _context.SaveChangesAsync();

            if (booking.AccountId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    booking.AccountId.Value,
                    "PAYMENT_RECORDED",
                    $"Direct payment of {amount:N0} VND received at counter for {booking.BookingCode}. Balance: {booking.BalanceDue:N0} VND.");
            }

            return (true, "Direct payment recorded successfully.");
        }
    }
}

