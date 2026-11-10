using System.Security.Claims;
using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Entities;
using FootballPitchManagement.Models.Enums;
using FootballPitchManagement.Models.ViewModels;
using FootballPitchManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Controllers
{
    [Authorize(Roles = "Staff,Manager,Admin")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookingService _bookingService;
        private readonly IPaymentService _paymentService;
        private readonly IRefundService _refundService;

        public StaffController(
            ApplicationDbContext context,
            IBookingService bookingService,
            IPaymentService paymentService,
            IRefundService refundService)
        {
            _context = context;
            _bookingService = bookingService;
            _paymentService = paymentService;
            _refundService = refundService;
        }

        // GET: /Staff (Bàn làm việc điều độ sân tại quầy lễ tân)
        public async Task<IActionResult> Index(DateTime? date)
        {
            var targetDate = (date ?? DateTime.Today).Date;
            var bookings = await _bookingService.GetBookingsByDateAsync(targetDate);

            var fields = await _context.FootballFields
                .Include(f => f.TimeSlots.Where(s => s.IsActive))
                .Where(f => f.IsActive)
                .ToListAsync();

            ViewBag.TargetDate = targetDate;
            ViewBag.Fields = fields;
            return View(bookings);
        }

        // GET: /Staff/WalkIn (Tiếp nhận khách vãng lai / đặt qua điện thoại)
        public async Task<IActionResult> WalkIn()
        {
            var fields = await _context.FootballFields.Where(f => f.IsActive && f.Status != FieldStatus.MAINTENANCE).ToListAsync();
            ViewBag.Fields = new SelectList(fields, "FieldId", "Name");

            var slots = await _context.TimeSlots.Where(s => s.IsActive).OrderBy(s => s.StartTime).ToListAsync();
            ViewBag.Slots = new SelectList(slots, "SlotId", "SlotName");

            return View(new WalkInBookingVM { BookingDate = DateTime.Today });
        }

        // POST: /Staff/WalkIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WalkIn(WalkInBookingVM model)
        {
            if (!ModelState.IsValid)
            {
                var fields = await _context.FootballFields.Where(f => f.IsActive).ToListAsync();
                ViewBag.Fields = new SelectList(fields, "FieldId", "Name", model.FieldId);
                var slots = await _context.TimeSlots.Where(s => s.IsActive).OrderBy(s => s.StartTime).ToListAsync();
                ViewBag.Slots = new SelectList(slots, "SlotId", "SlotName", model.SlotId);
                return View(model);
            }

            var staffId = GetCurrentAccountId();
            var result = await _bookingService.CreateWalkInBookingAsync(
                model.FieldId,
                model.SlotId,
                model.BookingDate,
                model.GuestName,
                model.GuestPhone,
                model.DirectPaymentAmount,
                model.PaymentMethod,
                model.ReceiptRef,
                staffId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index), new { date = model.BookingDate });
            }

            ModelState.AddModelError(string.Empty, result.Message);
            var fList = await _context.FootballFields.Where(f => f.IsActive).ToListAsync();
            ViewBag.Fields = new SelectList(fList, "FieldId", "Name", model.FieldId);
            var sList = await _context.TimeSlots.Where(s => s.IsActive).OrderBy(s => s.StartTime).ToListAsync();
            ViewBag.Slots = new SelectList(sList, "SlotId", "SlotName", model.SlotId);
            return View(model);
        }

        // POST: /Staff/CheckIn/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id)
        {
            var staffId = GetCurrentAccountId();
            var result = await _bookingService.CheckInCustomerAsync(id, staffId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Staff/CheckOut/5
        public async Task<IActionResult> CheckOut(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            var vm = new CheckOutVM
            {
                BookingId = booking.BookingId,
                BookingCode = booking.BookingCode,
                FieldName = booking.Field?.Name ?? "Sân bóng",
                CustomerInfo = booking.Account?.FullName ?? booking.GuestName ?? "Khách vãng lai",
                AgreedPrice = booking.AgreedPrice,
                PaidAmount = booking.PaidAmount,
                BalanceDue = booking.BalanceDue,
                DirectPaymentCollected = booking.BalanceDue,
                IsDamaged = false,
                MarkAsUnsettledDebt = false
            };

            return View(vm);
        }

        // POST: /Staff/CheckOut
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(CheckOutVM model)
        {
            var staffId = GetCurrentAccountId();
            var result = await _bookingService.CheckOutCustomerAsync(
                model.BookingId,
                model.IsDamaged,
                model.DamageNote,
                model.DirectPaymentCollected,
                model.MarkAsUnsettledDebt,
                staffId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = result.Message;
            return View(model);
        }

        // POST: /Staff/NoShow/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NoShow(int id)
        {
            var staffId = GetCurrentAccountId();
            var result = await _bookingService.RecordNoShowAsync(id, staffId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Staff/RecordPayment/5
        public async Task<IActionResult> RecordPayment(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            var vm = new RecordPaymentVM
            {
                BookingId = booking.BookingId,
                BookingCode = booking.BookingCode,
                BalanceDue = booking.BalanceDue,
                Amount = booking.BalanceDue > 0 ? booking.BalanceDue : 100000,
                Method = PaymentMethod.CASH
            };

            return View(vm);
        }

        // POST: /Staff/RecordPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordPayment(RecordPaymentVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var staffId = GetCurrentAccountId();
            var result = await _paymentService.RecordDirectPaymentAsync(
                model.BookingId,
                model.Amount,
                model.Method,
                model.ReceiptRef ?? "Phiếu thu tiền mặt tại quầy",
                staffId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = result.Message;
            return View(model);
        }

        // GET: /Staff/RefundQueue
        public async Task<IActionResult> RefundQueue()
        {
            var refunds = await _refundService.GetAllRefundRequestsAsync();
            return View(refunds);
        }

        // GET: /Staff/ProcessRefund/5
        public async Task<IActionResult> ProcessRefund(int id)
        {
            var refund = await _context.RefundRequests
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.RefundId == id);

            if (refund == null)
            {
                return NotFound();
            }

            var vm = new ProcessRefundVM
            {
                RefundId = refund.RefundId,
                RefundCode = refund.RefundCode,
                Amount = refund.Amount,
                Reason = refund.Reason,
                RefundType = refund.RefundType,
                EvidenceProof = "Ủy nhiệm chi / Mã giao dịch ngân hàng",
                IsSuccess = true
            };

            return View(vm);
        }

        // POST: /Staff/ProcessRefund
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessRefund(ProcessRefundVM model)
        {
            var staffId = GetCurrentAccountId();
            var result = await _refundService.ProcessRefundAsync(
                model.RefundId,
                model.EvidenceProof,
                model.IsSuccess,
                staffId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(RefundQueue));
            }

            TempData["ErrorMessage"] = result.Message;
            return View(model);
        }

        private int GetCurrentAccountId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }
    }
}
