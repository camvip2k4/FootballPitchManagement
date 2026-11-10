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
                return (false, "Không tìm thấy thông tin đơn đặt sân.", "NOT_FOUND");
            }

            var now = DateTime.UtcNow;
            var isLate = simulateLatePayment ||
                         booking.Status == BookingStatus.EXPIRED ||
                         booking.Status == BookingStatus.CANCELLED ||
                         booking.HoldExpiresAt < now;

            // Kịch bản 1: Thanh toán thất bại từ phía ngân hàng / cổng thanh toán
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

                if (booking.HoldExpiresAt > now && booking.Status == BookingStatus.PENDING_PAYMENT)
                {
                    booking.Status = BookingStatus.PAYMENT_FAILED;
                }
                await _context.SaveChangesAsync();

                if (booking.AccountId.HasValue)
                {
                    await _notificationService.SendNotificationAsync(
                        booking.AccountId.Value,
                        "THANH_TOÁN_THẤT_BẠI",
                        $"Giao dịch thanh toán cho đơn {booking.BookingCode} không thành công. Bạn có thể thử lại trước khi hết thời gian giữ chỗ.");
                }

                return (false, "Giao dịch thanh toán không thành công từ phía ngân hàng / cổng thanh toán.", "FAILED");
            }

            // Kịch bản 2: Giao dịch đang chờ xử lý / Timeout
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

                return (false, "Giao dịch đang chờ xác nhận từ phía ngân hàng. Đơn giữ chỗ chưa được kích hoạt.", "PENDING");
            }

            // Kịch bản 3: Thanh toán thành công
            var successPayment = new Payment
            {
                BookingId = booking.BookingId,
                TransactionRef = "VNPAY-" + DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N")[..6].ToUpper(),
                Amount = amount,
                Method = method,
                Channel = PaymentChannel.ONLINE,
                Status = PaymentStatus.SUCCESS,
                PaidAt = now
            };
            _context.Payments.Add(successPayment);

            if (isLate)
            {
                // Thanh toán muộn khi thời gian giữ chỗ 15p đã hết (UC14)
                var refundRequest = new RefundRequest
                {
                    RefundCode = "REF-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    BookingId = booking.BookingId,
                    Payment = successPayment,
                    Amount = amount,
                    Reason = "Thanh toán nhận được sau khi hết hạn 15 phút giữ chỗ hoặc đơn đã bị hủy.",
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
                        "HOÀN_TIỀN_DO_QUÁ_HẠN",
                        $"Tiền thanh toán cho đơn {booking.BookingCode} được ghi nhận sau khi hết hạn giữ chỗ. Hệ thống đã tự động tạo yêu cầu hoàn tiền 100% ({amount:N0} đ) chuyển về tài khoản của bạn.");
                }

                return (true, "Thanh toán đến sau khi hết thời gian giữ chỗ. Hệ thống đã tự động đưa vào hàng đợi hoàn tiền 100%.", "LATE_PAYMENT");
            }

            // Thanh toán đúng hạn bình thường
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
                    "ĐÃ_XÁC_NHẬN_ĐẶT_SÂN",
                    $"Đơn đặt sân {booking.BookingCode} đã được xác nhận thành công! Số tiền đã cọc: {amount:N0} đ. Số tiền còn lại khi ra sân: {booking.BalanceDue:N0} đ.");
            }

            return (true, "Thanh toán thành công! Đơn đặt sân của bạn đã được xác nhận.", "SUCCESS");
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
                return (false, "Không tìm thấy đơn đặt sân.");
            }

            var payment = new Payment
            {
                BookingId = booking.BookingId,
                TransactionRef = "QUAY-" + DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N")[..6].ToUpper(),
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
                    "PHIẾU_THU_TẠI_QUẦY",
                    $"Đã thu trực tiếp {amount:N0} đ tại quầy lễ tân cho đơn {booking.BookingCode}. Số dư còn lại: {booking.BalanceDue:N0} đ.");
            }

            return (true, "Ghi nhận thanh toán tại quầy thành công.");
        }
    }
}
