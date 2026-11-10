using System.Security.Claims;
using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Enums;
using FootballPitchManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Manager,Admin")]
    public class FacilityCancelController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookingService _bookingService;

        public FacilityCancelController(ApplicationDbContext context, IBookingService bookingService)
        {
            _context = context;
            _bookingService = bookingService;
        }

        // GET: /Admin/FacilityCancel (UC39)
        public async Task<IActionResult> Index()
        {
            var activeBookings = await _context.Bookings
                .Include(b => b.Field)
                .Include(b => b.TimeSlot)
                .Include(b => b.Account)
                .Where(b => b.Status == BookingStatus.CONFIRMED || b.Status == BookingStatus.PENDING_PAYMENT)
                .OrderBy(b => b.BookingDate)
                .ToListAsync();

            return View(activeBookings);
        }

        // POST: /Admin/FacilityCancel/Cancel (UC39)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int bookingId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "Mandatory cancellation reason must be provided (UC39).";
                return RedirectToAction(nameof(Index));
            }

            var managerId = GetCurrentAccountId();
            var result = await _bookingService.CancelByFacilityAsync(bookingId, managerId, reason);

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

        private int GetCurrentAccountId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }
    }
}

