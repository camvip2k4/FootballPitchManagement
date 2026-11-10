using System.Diagnostics;
using FootballPitchManagement.Data;
using FootballPitchManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var fields = await _context.FootballFields
                .Include(f => f.TimeSlots)
                .Where(f => f.IsActive)
                .Take(6)
                .ToListAsync();

            ViewBag.TotalFields = await _context.FootballFields.CountAsync(f => f.IsActive);
            ViewBag.TotalBookings = await _context.Bookings.CountAsync();

            return View(fields);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
