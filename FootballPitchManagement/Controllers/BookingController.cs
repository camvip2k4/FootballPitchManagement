using FootballPitchManagement.Data;
using FootballPitchManagement.Models.ViewModels;
using FootballPitchManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ApplicationDbContext _context;

        public BookingController(IBookingService bookingService, ApplicationDbContext context)
        {
            _bookingService = bookingService;
            _context = context;
        }

        // GET: Booking
        public async Task<IActionResult> Index()
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            return View(bookings);
        }

        // GET: Booking/Create
        public async Task<IActionResult> Create(int? pitchId)
        {
            var pitches = await _context.Pitches.Where(p => p.IsActive).ToListAsync();
            ViewBag.Pitches = new SelectList(pitches, "PitchId", "Name", pitchId);

            var model = new BookingRequestVM();
            if (pitchId.HasValue)
            {
                model.PitchId = pitchId.Value;
            }

            return View(model);
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingRequestVM request)
        {
            if (ModelState.IsValid)
            {
                var success = await _bookingService.CreateBookingAsync(request);
                if (success)
                {
                    TempData["SuccessMessage"] = "Pitch booked successfully!";
                    return RedirectToAction(nameof(History), new { phone = request.CustomerPhone });
                }

                ModelState.AddModelError(string.Empty, "The selected pitch is not available for this time range.");
            }

            var pitches = await _context.Pitches.Where(p => p.IsActive).ToListAsync();
            ViewBag.Pitches = new SelectList(pitches, "PitchId", "Name", request.PitchId);
            return View(request);
        }

        // GET: Booking/History
        public async Task<IActionResult> History(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return View(Enumerable.Empty<FootballPitchManagement.Models.Entities.Booking>());
            }

            ViewBag.CustomerPhone = phone;
            var history = await _bookingService.GetBookingHistoryAsync(phone);
            return View(history);
        }
    }
}

