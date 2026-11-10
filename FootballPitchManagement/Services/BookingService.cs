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
            return await _context.Bookings
                .Include(b => b.Field)
                .Include(b => b.TimeSlot)
                .Include(b => b.Account)
                .Include(b => b.Payments)
                .Where(b => b.BookingDate.Date == target)
                .OrderBy(b => b.TimeSlot != null ? b.TimeSlot.StartTime : TimeSpan.Zero)
                .ToListAsync();
        }

        // UC10: Book Football Field (Online by Customer)
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
                return (false, "Cannot book a date in the past.", null);
            }

            var horizonDays = await _policyService.GetBookingHorizonDaysAsync();
            if (targetDate > today.AddDays(horizonDays))
            {
                return (false, $"Cannot book more than {horizonDays} days in advance (POL-05).", null);
            }

            var maxPending = await _policyService.GetMaxPendingPerCustomerAsync();
            var currentPending = await _context.Bookings.CountAsync(b =>
                b.AccountId == accountId &&
                b.Status == BookingStatus.PENDING_PAYMENT &&
                b.HoldExpiresAt > DateTime.UtcNow);

            if (currentPending >= maxPending)
            {
                return (false, $"You have reached the maximum allowed pending bookings ({maxPending}). Please complete or cancel pending holds first (BR-32).", null);
            }

            var isAvailable = await _availabilityChecker.IsSlotAvailableAsync(fieldId, slotId, targetDate);
            if (!isAvailable)
            {
                return (false, "The selected slot is no longer available. Please choose another slot.", null);
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
                "HOLD_STARTED",
                $"Booking {booking.BookingCode} created. Slot is held for {holdMinutes} minutes. Please complete prepayment of {requiredPrepayment:N0} VND.");

            return (true, "Booking hold created successfully.", booking);
        }

        // UC37: Create Booking for Customer (Front Desk Walk-in / Phone)
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
                return (false, "The selected slot is not available.", null);
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
                BookingCode = "WI-" + targetDate.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N")[..4].ToUpper(),
                BookingDate = targetDate,
                FieldId = fieldId,
                SlotId = slotId,
                AccountId = null, // Walk-in customer without account
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
                    TransactionRef = "DIR-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
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

            return (true, isConfirmed ? "Counter booking confirmed with direct payment." : "Counter booking created (Pending payment).", booking);
        }

        // UC13: Cancel Booking by Customer
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
                return (false, "Booking not found.");
            }

            if (booking.Status == BookingStatus.CANCELLED ||
                booking.Status == BookingStatus.COMPLETED ||
                booking.Status == BookingStatus.CHECKED_IN ||
                booking.Status == BookingStatus.NO_SHOW)
            {
                return (false, $"Cannot cancel a booking with status '{booking.Status}'.");
            }

            var now = DateTime.UtcNow;

            // If pending payment or payment failed, simply cancel and release hold
            if (booking.Status == BookingStatus.PENDING_PAYMENT || booking.Status == BookingStatus.PAYMENT_FAILED)
            {
                booking.Status = BookingStatus.CANCELLED;
                booking.CancellationType = CancellationType.CUSTOMER;
                booking.CancelReason = reason;
                await _context.SaveChangesAsync();
                return (true, "Booking hold cancelled successfully.");
            }

            // If confirmed, check cutoff hours
            if (booking.Status == BookingStatus.CONFIRMED && booking.TimeSlot != null)
            {
                var slotDateTime = booking.BookingDate.Date.Add(booking.TimeSlot.StartTime);
                var cutoffHours = await _policyService.GetCancelCutoffHoursAsync();
                var cutoffTime = slotDateTime.AddHours(-cutoffHours);

                if (DateTime.Now > cutoffTime)
                {
                    return (false, $"Cancellation rejected: You can only cancel at least {cutoffHours} hours before the match time (POL-03).");
                }

                booking.Status = BookingStatus.CANCELLED;
                booking.CancellationType = CancellationType.CUSTOMER;
                booking.CancelReason = reason;

                // Create RefundRequest if paid
                if (booking.PaidAmount > 0)
                {
                    var refundPercent = await _policyService.GetRefundPercentAsync();
                    var refundAmount = Math.Round(booking.PaidAmount * refundPercent / 100m, 0);

                    var refund = new RefundRequest
                    {
                        RefundCode = "REF-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                        BookingId = booking.BookingId,
                        Amount = refundAmount,
                        Reason = string.IsNullOrWhiteSpace(reason) ? "Customer cancelled before cutoff" : reason,
                        RefundType = RefundType.CUSTOMER_CANCEL,
                        Status = RefundStatus.PENDING,
                        CreatedAt = now
                    };
                    _context.RefundRequests.Add(refund);

                    await _notificationService.SendNotificationAsync(
                        accountId,
                        "CANCELLATION_REFUND",
                        $"Booking {booking.BookingCode} was cancelled. A refund of {refundAmount:N0} VND has been queued for review.");
                }

                await _context.SaveChangesAsync();
                return (true, "Booking cancelled successfully. Refund request has been queued.");
            }

            return (false, "Unable to cancel booking.");
        }

        // UC39: Cancel Booking by Facility (Manager Emergency / Weather / Maintenance)
        public async Task<(bool Success, string Message)> CancelByFacilityAsync(
            int bookingId,
            int managerAccountId,
            string reason)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                return (false, "Booking not found.");
            }

            if (booking.Status == BookingStatus.CANCELLED || booking.Status == BookingStatus.COMPLETED)
            {
                return (false, $"Cannot cancel booking in status {booking.Status}.");
            }

            var now = DateTime.UtcNow;
            booking.Status = BookingStatus.CANCELLED;
            booking.CancellationType = CancellationType.FACILITY;
            booking.CancelReason = reason;

            // Facility cancellation always grants 100% full refund of all paid money!
            if (booking.PaidAmount > 0)
            {
                var refund = new RefundRequest
                {
                    RefundCode = "REF-FAC-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    BookingId = booking.BookingId,
                    Amount = booking.PaidAmount,
                    Reason = $"Facility cancelled by management: {reason}",
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
                    "FACILITY_CANCELLED",
                    $"Your booking {booking.BookingCode} was cancelled by the facility due to: {reason}. Full refund of {booking.PaidAmount:N0} VND has been queued.");
            }

            return (true, "Booking cancelled by facility. 100% refund queued.");
        }

        // UC19: Check In Customer
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
                return (false, "Booking not found.");
            }

            if (booking.Status != BookingStatus.CONFIRMED)
            {
                return (false, $"Customer cannot check in. Booking status is '{booking.Status}' (must be CONFIRMED).");
            }

            if (booking.Field == null || booking.Field.Status != FieldStatus.AVAILABLE)
            {
                return (false, $"Field '{booking.Field?.Name}' is currently {booking.Field?.Status}. Cannot check in.");
            }

            booking.Status = BookingStatus.CHECKED_IN;
            booking.CheckedInAt = DateTime.UtcNow;

            // Operational status of field becomes OCCUPIED
            booking.Field.Status = FieldStatus.OCCUPIED;

            await _context.SaveChangesAsync();

            if (booking.AccountId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    booking.AccountId.Value,
                    "CHECKED_IN",
                    $"You have checked in for booking {booking.BookingCode} at {booking.Field.Name}. Enjoy your match!");
            }

            return (true, $"Customer checked in successfully. Pitch '{booking.Field.Name}' marked as OCCUPIED.");
        }

        // UC20: Check Out Customer
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
                return (false, "Booking not found.");
            }

            if (booking.Status != BookingStatus.CHECKED_IN)
            {
                return (false, $"Booking must be in CHECKED_IN status to check out (current: {booking.Status}).");
            }

            var now = DateTime.UtcNow;

            if (directPaymentCollected > 0)
            {
                var payment = new Payment
                {
                    BookingId = booking.BookingId,
                    TransactionRef = "CO-PAY-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    Amount = directPaymentCollected,
                    Method = PaymentMethod.CASH,
                    Channel = PaymentChannel.DIRECT,
                    Status = PaymentStatus.SUCCESS,
                    ReceiptRef = "Check-out direct payment",
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

            // Update Field Operational Status
            if (booking.Field != null)
            {
                if (isDamaged)
                {
                    booking.Field.Status = FieldStatus.MAINTENANCE;
                    booking.Field.Description += $" [Damage noted at checkout on {now:yyyy-MM-dd HH:mm}: {damageNote}]";
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
                    "COMPLETED",
                    $"Check-out completed for {booking.BookingCode}. Thank you for playing with us!");
            }

            return (true, "Check-out completed successfully.");
        }

        // UC35: Record No-Show
        public async Task<(bool Success, string Message)> RecordNoShowAsync(
            int bookingId,
            int staffAccountId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Field)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return (false, "Booking not found.");
            }

            if (booking.Status != BookingStatus.CONFIRMED)
            {
                return (false, $"Cannot record no-show for booking with status '{booking.Status}'.");
            }

            var now = DateTime.UtcNow;
            booking.Status = BookingStatus.NO_SHOW;
            booking.NoShowAt = now;

            // Retain paid amount per policy POL-04, no refund created
            if (booking.Field != null && booking.Field.Status == FieldStatus.OCCUPIED)
            {
                booking.Field.Status = FieldStatus.AVAILABLE;
            }

            await _context.SaveChangesAsync();

            if (booking.AccountId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    booking.AccountId.Value,
                    "NO_SHOW",
                    $"Booking {booking.BookingCode} was marked as No-Show due to arrival window expiration. Prepayment retained per policy.");
            }

            return (true, "Booking recorded as No-Show. Paid amount retained.");
        }
    }
}
