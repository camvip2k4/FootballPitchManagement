using System.Security.Claims;
using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Enums;
using FootballPitchManagement.Models.ViewModels;
using FootballPitchManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IPaymentService _paymentService;
        private readonly IBusinessPolicyService _policyService;
        private readonly INotificationService _notificationService;
        private readonly ApplicationDbContext _context;

        public BookingController(
            IBookingService bookingService,
            IPaymentService paymentService,
            IBusinessPolicyService policyService,
            INotificationService notificationService,
            ApplicationDbContext context)
        {
            _bookingService = bookingService;
            _paymentService = paymentService;
            _policyService = policyService;
            _notificationService = notificationService;
            _context = context;
        }

        // GET: /Booking or /Booking/Index (redirects to MyBookings if logged in, or Search)
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(nameof(MyBookings));
            }
            return RedirectToAction("Index", "Field");
        }

        // GET: /Booking/Create?fieldId=1&slotId=2&date=2026-11-10 (UC10)
        [Authorize]
        public async Task<IActionResult> Create(int fieldId, int slotId, DateTime? date)
        {
            var bookingDate = (date ?? DateTime.Today).Date;
            var field = await _context.FootballFields.FindAsync(fieldId);
            var slot = await _context.TimeSlots.FindAsync(slotId);

            if (field == null || slot == null)
            {
                TempData["ErrorMessage"] = "Invalid field or time slot.";
                return RedirectToAction("Index", "Field");
            }

            var accountId = GetCurrentAccountId();
            var maxPending = await _policyService.GetMaxPendingPerCustomerAsync();
            var currentPending = await _context.Bookings.CountAsync(b =>
                b.AccountId == accountId &&
                b.Status == BookingStatus.PENDING_PAYMENT &&
                b.HoldExpiresAt > DateTime.UtcNow);

            if (currentPending >= maxPending)
            {
                TempData["ErrorMessage"] = $"You already have {currentPending} pending booking holds (max {maxPending}). Please complete or cancel them before booking more.";
                return RedirectToAction(nameof(MyBookings));
            }

            var pricingService = HttpContext.RequestServices.GetRequiredService<IPricingService>();
            var price = await pricingService.CalculatePriceAsync(fieldId, slotId, bookingDate);
            var depositPercent = await _policyService.GetDepositPercentAsync();
            var requiredPrepayment = Math.Round(price * depositPercent / 100m, 0);

            ViewBag.Field = field;
            ViewBag.Slot = slot;
            ViewBag.BookingDate = bookingDate;
            ViewBag.Price = price;
            ViewBag.DepositPercent = depositPercent;
            ViewBag.RequiredPrepayment = requiredPrepayment;
            ViewBag.HoldMinutes = await _policyService.GetHoldMinutesAsync();

            return View();
        }

        // POST: /Booking/Create (UC10)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int fieldId, int slotId, DateTime bookingDate)
        {
            var accountId = GetCurrentAccountId();
            var result = await _bookingService.CreateOnlineBookingAsync(fieldId, slotId, bookingDate, accountId);

            if (!result.Success || result.Booking == null)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Details", "Field", new { id = fieldId, date = bookingDate });
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Payment), new { id = result.Booking.BookingId });
        }

        // GET: /Booking/Payment/5 (UC14: Make Payment Screen)
        [Authorize]
        public async Task<IActionResult> Payment(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            var accountId = GetCurrentAccountId();
            if (booking.AccountId != accountId && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                return Forbid();
            }

            var vm = new MakePaymentVM
            {
                BookingId = booking.BookingId,
                BookingCode = booking.BookingCode,
                FieldName = booking.Field?.Name ?? "Field",
                SlotName = booking.TimeSlot?.SlotName ?? "Slot",
                BookingDate = booking.BookingDate,
                AgreedPrice = booking.AgreedPrice,
                RequiredPrepayment = booking.RequiredPrepayment,
                PaidAmount = booking.PaidAmount,
                BalanceDue = booking.BalanceDue,
                HoldExpiresAt = booking.HoldExpiresAt,
                BookingStatus = booking.Status,
                PaymentAmount = booking.PaidAmount == 0 ? booking.RequiredPrepayment : booking.BalanceDue,
                Method = PaymentMethod.ONLINE_GATEWAY,
                SimulatedResult = PaymentStatus.SUCCESS,
                SimulateLatePayment = false
            };

            return View(vm);
        }

        // POST: /Booking/Payment (UC14: Make Payment Execution)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(MakePaymentVM model)
        {
            var result = await _paymentService.ProcessOnlinePaymentAsync(
                model.BookingId,
                model.PaymentAmount,
                model.Method,
                model.SimulatedResult,
                model.SimulateLatePayment);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(MyBookings));
        }

        // GET: /Booking/MyBookings
        [Authorize]
        public async Task<IActionResult> MyBookings()
        {
            var accountId = GetCurrentAccountId();
            var bookings = await _bookingService.GetCustomerBookingsAsync(accountId);
            return View(bookings);
        }

        // GET: /Booking/Cancel/5 (UC13: Customer Cancellation Dialog)
        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            var accountId = GetCurrentAccountId();
            if (booking.AccountId != accountId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var vm = new CancelBookingVM
            {
                BookingId = booking.BookingId,
                BookingCode = booking.BookingCode,
                PaidAmount = booking.PaidAmount
            };

            ViewBag.Booking = booking;
            ViewBag.CutoffHours = await _policyService.GetCancelCutoffHoursAsync();
            ViewBag.RefundPercent = await _policyService.GetRefundPercentAsync();

            return View(vm);
        }

        // POST: /Booking/Cancel (UC13)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(CancelBookingVM model)
        {
            var accountId = GetCurrentAccountId();
            var result = await _bookingService.CancelByCustomerAsync(model.BookingId, accountId, model.Reason);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(MyBookings));
        }

        // GET: /Booking/History?phone=0987654321
        public async Task<IActionResult> History(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                if (User.Identity?.IsAuthenticated == true)
                {
                    return RedirectToAction(nameof(MyBookings));
                }
                return View(new List<FootballPitchManagement.Models.Entities.Booking>());
            }

            var bookings = await _context.Bookings
                .Include(b => b.Field)
                .Include(b => b.TimeSlot)
                .Include(b => b.Payments)
                .Where(b => (b.GuestPhone != null && b.GuestPhone.Contains(phone.Trim())) ||
                            (b.Account != null && b.Account.Phone.Contains(phone.Trim())))
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            ViewBag.Phone = phone;
            return View(bookings);
        }

        // GET: /Booking/Notifications
        [Authorize]
        public async Task<IActionResult> Notifications()
        {
            var accountId = GetCurrentAccountId();
            var notifs = await _notificationService.GetUserNotificationsAsync(accountId);
            return View(notifs);
        }

        // POST: /Booking/MarkNotificationRead
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return RedirectToAction(nameof(Notifications));
        }

        private int GetCurrentAccountId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }
    }
}
