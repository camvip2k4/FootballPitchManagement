using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Enums;
using FootballPitchManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "MANAGER,Manager,ADMIN,Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var vm = new DashboardVM
            {
                TotalFields = await _context.FootballFields.CountAsync(),
                ActiveFields = await _context.FootballFields.CountAsync(f => f.IsActive),
                TodayBookings = await _context.Bookings.CountAsync(b => b.BookingDate.Date == today),
                PendingRefunds = await _context.RefundRequests.CountAsync(r => r.Status == RefundStatus.PENDING),
                TotalRevenue = await _context.Payments.Where(p => p.Status == PaymentStatus.SUCCESS).SumAsync(p => (decimal?)p.Amount) ?? 0,
                TodayRevenue = await _context.Payments.Where(p => p.Status == PaymentStatus.SUCCESS && p.PaidAt.Date == today).SumAsync(p => (decimal?)p.Amount) ?? 0,
                RecentBookings = await _context.Bookings
                    .Include(b => b.Field)
                    .Include(b => b.TimeSlot)
                    .Include(b => b.Account)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(8)
                    .ToListAsync(),
                PendingRefundRequests = await _context.RefundRequests
                    .Include(r => r.Booking)
                        .ThenInclude(b => b!.Field)
                    .Where(r => r.Status == RefundStatus.PENDING)
                    .OrderBy(r => r.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}
