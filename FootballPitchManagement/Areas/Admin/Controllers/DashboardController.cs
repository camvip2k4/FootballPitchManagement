using FootballPitchManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalPitches = await _context.Pitches.CountAsync();
            ViewBag.TotalBookings = await _context.Bookings.CountAsync();
            ViewBag.TotalRevenue = await _context.Invoices
                .Where(i => i.PaymentStatus == "Paid")
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

            var recentBookings = await _context.Bookings
                .Include(b => b.Pitch)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(recentBookings);
        }
    }
}

