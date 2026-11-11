using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Entities;
using FootballPitchManagement.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBusinessPolicyService _policyService;
        private readonly IPricingService _pricingService;
        private readonly IAvailabilityChecker _availabilityChecker;
        private readonly INotificationService _notificationService;

        public BookingService(
            ApplicationDbContext context,
            IBusinessPolicyService policyService,
            IPricingService pricingService,
            IAvailabilityChecker availabilityChecker,
            INotificationService notificationService)
        {
            _context = context;
            _policyService = policyService;
            _pricingService = pricingService;
            _availabilityChecker = availabilityChecker;
            _notificationService = notificationService;
        }

        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            return await _context.Bookings
                .Include(b => b.Field)
                .Include(b => b.TimeSlot)
                .Include(b => b.Account)
                .Include(b => b.Payments)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Field)
                .Include(b => b.TimeSlot)
                .Include(b => b.Account)
                .Include(b => b.Payments)
                .Include(b => b.RefundRequests)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public async Task<Booking?> GetBookingByCodeAsync(string code)
        {
            return await _context.Bookings
                .Include(b => b.Field)
                .Include(b => b.TimeSlot)
                .Include(b => b.Account)
                .Include(b => b.Payments)
                .Include(b => b.RefundRequests)
                .FirstOrDefaultAsync(b => b.BookingCode.ToLower() == code.Trim().ToLower());
        }

        public async Task<List<Booking>> GetCustomerBookingsAsync(int accountId)
        {
            return await _context.Bookings
                .Include(b => b.Field)
                .Include(b => b.TimeSlot)
                .Include(b => b.Payments)
                .Include(b => b.RefundRequests)
                .Where(b => b.AccountId == accountId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsByDateAsync(DateTime date)
        {
            var target = date.Date;
            var nextTarget = target.AddDays(1);
            return await _context.Bookings
                .Include(b => b.Field)
                .Include(b => b.TimeSlot)
                .Include(b => b.Account)
                .Include(b => b.Payments)
                .Where(b => b.BookingDate >= target && b.BookingDate < nextTarget)
                .OrderBy(b => b.TimeSlot != null ? b.TimeSlot.StartTime : TimeSpan.Zero)
                .ToListAsync();
        }

        // Đặt sân trực tuyến từ phía khách hàng (UC10)
        public async Task<(bool Success, string Message, Booking? Booking)> CreateOnlineBookingAsync(
            int fieldId,
            int slotId,
            DateTime bookingDate,
            int accountId)
        {
            var targetDate = bookingDate.Date;
            var today = DateTime.Today;

            if (targetDate < today)
            {
                return (false, "Không thể đặt lịch thi đấu cho ngày trong quá khứ.", null);
            }

            var horizonDays = await _policyService.GetBookingHorizonDaysAsync();
            if (targetDate > today.AddDays(horizonDays))
            {
                return (false, $"Hệ thống chỉ nhận đặt trước tối đa trong vòng {horizonDays} ngày.", null);
            }

            var maxPending = await _policyService.GetMaxPendingPerCustomerAsync();
            var currentPending = await _context.Bookings.CountAsync(b =>
                b.AccountId == accountId &&
                b.Status == BookingStatus.PENDING_PAYMENT &&
                b.HoldExpiresAt > DateTime.UtcNow);

            if (currentPending >= maxPending)
            {
                return (false, $"Bạn đang có {currentPending} đơn giữ chỗ chưa thanh toán (tối đa {maxPending} đơn). Vui lòng hoàn tất hoặc hủy các đơn cũ trước khi đặt tiếp.", null);
            }

            var isAvailable = await _availabilityChecker.IsSlotAvailableAsync(fieldId, slotId, targetDate);
            if (!isAvailable)
            {
                return (false, "Khung giờ này vừa có người đặt hoặc đang trong thời gian giữ chỗ. Vui lòng chọn khung giờ khác.", null);
            }

            var price = await _pricingService.CalculatePriceAsync(fieldId, slotId, targetDate);
            var depositPercent = await _policyService.GetDepositPercentAsync();
            var requiredPrepayment = Math.Round(price * depositPercent / 100m, 0);

            var holdMinutes = await _policyService.GetHoldMinutesAsync();
            var now = DateTime.UtcNow;

            var booking = new Booking
            {
                BookingCode = "BK-" + targetDate.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N")[..4].ToUpper(),
                BookingDate = targetDate,
                FieldId = fieldId,
                SlotId = slotId,
                AccountId = accountId,
                AgreedPrice = price,
                RequiredPrepayment = requiredPrepayment,
                PaidAmount = 0,
                BalanceDue = price,
                SettlementStatus = SettlementStatus.UNPAID,
                Status = BookingStatus.PENDING_PAYMENT,
                HoldExpiresAt = now.AddMinutes(holdMinutes),
                CreatedAt = now
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                accountId,
                "GIỮ_CHỖ_THÀNH_CÔNG",
                $"Đơn đặt sân {booking.BookingCode} đã được tạo. Sân được tạm giữ cho bạn trong {holdMinutes} phút. Vui lòng thanh toán cọc {requiredPrepayment:N0} đ để hoàn tất xác nhận.");

            return (true, "Tạo đơn giữ chỗ thành công.", booking);
        }

        // Đặt sân cho khách tại quầy hoặc qua điện thoại (UC37)
        public async Task<(bool Success, string Message, Booking? Booking)> CreateWalkInBookingAsync(
            int fieldId,
            int slotId,
            DateTime bookingDate,
            string guestName,
            string guestPhone,
            decimal? directPaymentAmount,
            PaymentMethod? method,
            string? receiptRef,
            int? staffAccountId)
        {
            var targetDate = bookingDate.Date;
            var isAvailable = await _availabilityChecker.IsSlotAvailableAsync(fieldId, slotId, targetDate);
            if (!isAvailable)
            {
                return (false, "Khung giờ được chọn hiện không còn trống.", null);
            }

            var price = await _pricingService.CalculatePriceAsync(fieldId, slotId, targetDate);
            var depositPercent = await _policyService.GetDepositPercentAsync();
            var requiredPrepayment = Math.Round(price * depositPercent / 100m, 0);
            var holdMinutes = await _policyService.GetHoldMinutesAsync();
            var now = DateTime.UtcNow;

            var paid = directPaymentAmount ?? 0m;
            var balanceDue = Math.Max(0, price - paid);

            var isConfirmed = paid >= requiredPrepayment;
            var booking = new Booking
            {
                BookingCode = "VL-" + targetDate.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N")[..4].ToUpper(),
                BookingDate = targetDate,
                FieldId = fieldId,
                SlotId = slotId,
                AccountId = null,
                GuestName = guestName,
                GuestPhone = guestPhone,
                AgreedPrice = price,
                RequiredPrepayment = requiredPrepayment,
                PaidAmount = paid,
                BalanceDue = balanceDue,
                SettlementStatus = (balanceDue == 0 && paid > 0) ? SettlementStatus.PAID : (paid > 0 ? SettlementStatus.PARTIALLY_PAID : SettlementStatus.UNPAID),
                Status = isConfirmed ? BookingStatus.CONFIRMED : BookingStatus.PENDING_PAYMENT,
                HoldExpiresAt = now.AddMinutes(holdMinutes),
                CreatedAt = now
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            if (paid > 0)
            {
                var payment = new Payment
                {
                    BookingId = booking.BookingId,
                    TransactionRef = "QUAY-" + DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N")[..6].ToUpper(),
                    Amount = paid,
                    Method = method ?? PaymentMethod.CASH,
                    Channel = PaymentChannel.DIRECT,
                    Status = PaymentStatus.SUCCESS,
                    ReceiptRef = receiptRef,
                    PaidAt = now
                };
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
            }

            return (true, isConfirmed ? "Đặt sân tại quầy và xác nhận thành công." : "Tạo đơn đặt sân tại quầy thành công (Chờ thu cọc).", booking);
        }

        // Khách hàng tự hủy đặt sân (UC13)
        public async Task<(bool Success, string Message)> CancelByCustomerAsync(
            int bookingId,
            int accountId,
            string reason)
        {
            var booking = await _context.Bookings
                .Include(b => b.TimeSlot)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.AccountId == accountId);

            if (booking == null)
            {
                return (false, "Không tìm thấy thông tin đơn đặt sân.");
            }

            if (booking.Status == BookingStatus.CANCELLED ||
                booking.Status == BookingStatus.COMPLETED ||
                booking.Status == BookingStatus.CHECKED_IN ||
                booking.Status == BookingStatus.NO_SHOW)
            {
                return (false, $"Không thể hủy đơn đặt sân đang ở trạng thái '{booking.Status}'.");
            }

            var now = DateTime.UtcNow;

            // Nếu đơn chưa thanh toán cọc hoặc thanh toán thất bại: hủy và giải phóng chỗ ngay lập tức
            if (booking.Status == BookingStatus.PENDING_PAYMENT || booking.Status == BookingStatus.PAYMENT_FAILED)
            {
                booking.Status = BookingStatus.CANCELLED;
                booking.CancellationType = CancellationType.CUSTOMER;
                booking.CancelReason = reason;
                await _context.SaveChangesAsync();
                return (true, "Đã hủy đơn giữ chỗ thành công.");
            }

            // Nếu đơn đã xác nhận (đã có tiền cọc), kiểm tra mốc thời gian tối thiểu 24h
            if (booking.Status == BookingStatus.CONFIRMED && booking.TimeSlot != null)
            {
                var slotDateTime = booking.BookingDate.Date.Add(booking.TimeSlot.StartTime);
                var cutoffHours = await _policyService.GetCancelCutoffHoursAsync();
                var cutoffTime = slotDateTime.AddHours(-cutoffHours);

                if (DateTime.Now > cutoffTime)
                {
                    return (false, $"Từ chối hủy đơn: Khách chỉ được hủy trước giờ đá tối thiểu {cutoffHours} tiếng theo quy định chung.");
                }

                booking.Status = BookingStatus.CANCELLED;
                booking.CancellationType = CancellationType.CUSTOMER;
                booking.CancelReason = reason;

                // Tự động tạo yêu cầu hoàn tiền nếu khách đã trả tiền
                if (booking.PaidAmount > 0)
                {
                    var refundPercent = await _policyService.GetRefundPercentAsync();
                    var refundAmount = Math.Round(booking.PaidAmount * refundPercent / 100m, 0);

                    var refund = new RefundRequest
                    {
                        RefundCode = "REF-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                        BookingId = booking.BookingId,
                        Amount = refundAmount,
                        Reason = string.IsNullOrWhiteSpace(reason) ? "Khách hủy sân đúng hạn" : reason,
                        RefundType = RefundType.CUSTOMER_CANCEL,
                        Status = RefundStatus.PENDING,
                        CreatedAt = now
                    };
                    _context.RefundRequests.Add(refund);

                    await _notificationService.SendNotificationAsync(
                        accountId,
                        "YÊU_CẦU_HOÀN_TIỀN",
                        $"Đơn {booking.BookingCode} đã được hủy. Yêu cầu hoàn lại {refundAmount:N0} đ ({refundPercent}%) đã được chuyển đến ban quản lý xử lý.");
                }

                await _context.SaveChangesAsync();
                return (true, "Hủy đặt sân thành công. Yêu cầu hoàn tiền đã được đưa vào hàng đợi xử lý.");
            }

            return (false, "Không thể hủy đơn đặt sân.");
        }

        // Ban quản lý hủy sân do sự cố thời tiết, bảo trì đột xuất (UC39)
        public async Task<(bool Success, string Message)> CancelByFacilityAsync(
            int bookingId,
            int managerAccountId,
            string reason)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                return (false, "Không tìm thấy đơn đặt sân.");
            }

            if (booking.Status == BookingStatus.CANCELLED || booking.Status == BookingStatus.COMPLETED)
            {
                return (false, $"Không thể hủy đơn đặt sân đang ở trạng thái {booking.Status}.");
            }

            var now = DateTime.UtcNow;
            booking.Status = BookingStatus.CANCELLED;
            booking.CancellationType = CancellationType.FACILITY;
            booking.CancelReason = reason;

            // Hủy do sự cố từ phía sân: luôn hoàn trả 100% toàn bộ số tiền khách đã thanh toán!
            if (booking.PaidAmount > 0)
            {
                var refund = new RefundRequest
                {
                    RefundCode = "REF-SAN-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    BookingId = booking.BookingId,
                    Amount = booking.PaidAmount,
                    Reason = $"Sân bóng hủy lịch do sự cố/thời tiết: {reason}",
                    RefundType = RefundType.FACILITY_CANCEL,
                    Status = RefundStatus.PENDING,
                    CreatedAt = now
                };
                _context.RefundRequests.Add(refund);
            }

            await _context.SaveChangesAsync();

            if (booking.AccountId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    booking.AccountId.Value,
                    "HỦY_LỊCH_DO_SỰ_CỐ_SÂN",
                    $"Rất tiếc! Đơn đặt sân {booking.BookingCode} phải hủy do: {reason}. Toàn bộ số tiền {booking.PaidAmount:N0} đ sẽ được hoàn trả 100% cho bạn.");
            }

            return (true, "Hủy lịch thành công do sự cố sân bãi. Hệ thống đã tạo yêu cầu hoàn tiền 100%.");
        }

        // Check-in đón khách vào sân (UC19)
        public async Task<(bool Success, string Message)> CheckInCustomerAsync(
            int bookingId,
            int staffAccountId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Field)
                .Include(b => b.TimeSlot)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return (false, "Không tìm thấy đơn đặt sân.");
            }

            if (booking.Status != BookingStatus.CONFIRMED)
            {
                return (false, $"Không thể check-in. Đơn đặt sân đang ở trạng thái '{booking.Status}' (yêu cầu phải là Đã xác nhận).");
            }

            if (booking.Field == null || booking.Field.Status != FieldStatus.AVAILABLE)
            {
                return (false, $"Sân '{booking.Field?.Name}' hiện đang ở trạng thái {booking.Field?.Status}. Không thể nhận khách vào sân.");
            }

            booking.Status = BookingStatus.CHECKED_IN;
            booking.CheckedInAt = DateTime.UtcNow;

            // Cập nhật trạng thái sân sang: ĐANG SỬ DỤNG
            booking.Field.Status = FieldStatus.OCCUPIED;

            await _context.SaveChangesAsync();

            if (booking.AccountId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    booking.AccountId.Value,
                    "CHECK_IN_THÀNH_CÔNG",
                    $"Bạn đã check-in thành công tại {booking.Field.Name}. Chúc đội bạn có một trận đấu tuyệt vời!");
            }

            return (true, $"Khách đã check-in thành công. Sân '{booking.Field.Name}' chuyển sang trạng thái ĐANG THI ĐẤU.");
        }

        // Check-out nghiệm thu sân và thanh quyết toán (UC20)
        public async Task<(bool Success, string Message)> CheckOutCustomerAsync(
            int bookingId,
            bool isDamaged,
            string? damageNote,
            decimal directPaymentCollected,
            bool markAsUnsettledDebt,
            int staffAccountId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Field)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return (false, "Không tìm thấy đơn đặt sân.");
            }

            if (booking.Status != BookingStatus.CHECKED_IN)
            {
                return (false, $"Đơn đặt sân phải đang ở trạng thái Đang sử dụng mới được check-out (hiện tại: {booking.Status}).");
            }

            var now = DateTime.UtcNow;

            if (directPaymentCollected > 0)
            {
                var payment = new Payment
                {
                    BookingId = booking.BookingId,
                    TransactionRef = "CHECKOUT-" + DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N")[..6].ToUpper(),
                    Amount = directPaymentCollected,
                    Method = PaymentMethod.CASH,
                    Channel = PaymentChannel.DIRECT,
                    Status = PaymentStatus.SUCCESS,
                    ReceiptRef = "Thanh toán phần tiền còn lại khi trả sân",
                    PaidAt = now
                };
                _context.Payments.Add(payment);

                booking.PaidAmount += directPaymentCollected;
                booking.BalanceDue = Math.Max(0, booking.AgreedPrice - booking.PaidAmount);
            }

            if (booking.BalanceDue == 0)
            {
                booking.SettlementStatus = SettlementStatus.PAID;
            }
            else if (markAsUnsettledDebt)
            {
                booking.SettlementStatus = SettlementStatus.UNSETTLED;
            }

            booking.Status = BookingStatus.COMPLETED;
            booking.CheckedOutAt = now;

            // Nghiệm thu hiện trạng mặt sân
            if (booking.Field != null)
            {
                if (isDamaged)
                {
                    booking.Field.Status = FieldStatus.MAINTENANCE;
                    booking.Field.Description += $" [Ghi nhận hư hỏng sau trận lúc {now:dd/MM/yyyy HH:mm}: {damageNote}]";
                }
                else
                {
                    booking.Field.Status = FieldStatus.AVAILABLE;
                }
            }

            await _context.SaveChangesAsync();

            if (booking.AccountId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    booking.AccountId.Value,
                    "HOÀN_TẤT_TRẬN_ĐẤU",
                    $"Đã hoàn tất thủ tục trả sân cho đơn {booking.BookingCode}. Cảm ơn bạn đã lựa chọn Sport Arena!");
            }

            return (true, "Hoàn tất thủ tục check-out và thanh quyết toán thành công.");
        }

        // Ghi nhận khách không đến (No-Show - UC35)
        public async Task<(bool Success, string Message)> RecordNoShowAsync(
            int bookingId,
            int staffAccountId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Field)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return (false, "Không tìm thấy đơn đặt sân.");
            }

            if (booking.Status != BookingStatus.CONFIRMED)
            {
                return (false, $"Không thể ghi nhận vắng mặt cho đơn đang ở trạng thái '{booking.Status}'.");
            }

            var now = DateTime.UtcNow;
            booking.Status = BookingStatus.NO_SHOW;
            booking.NoShowAt = now;

            // Giữ lại tiền cọc theo điều khoản chính sách, giải phóng sân về trạng thái TRỐNG
            if (booking.Field != null && booking.Field.Status == FieldStatus.OCCUPIED)
            {
                booking.Field.Status = FieldStatus.AVAILABLE;
            }

            await _context.SaveChangesAsync();

            if (booking.AccountId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    booking.AccountId.Value,
                    "VẮNG_MẶT_NO_SHOW",
                    $"Đơn {booking.BookingCode} đã được ghi nhận No-Show do quá thời gian vào sân. Tiền cọc được giữ lại theo quy định.");
            }

            return (true, "Đã ghi nhận khách vắng mặt (No-Show). Tiền đặt cọc được giữ lại theo quy chế.");
        }
    }
}
