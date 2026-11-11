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
            var tomorrow = today.AddDays(1);

            // Tải danh sách giao dịch thành công để tính tổng doanh thu an toàn cho cả SqlServer và SQLite
            var successPayments = await _context.Payments
                .Where(p => p.Status == PaymentStatus.SUCCESS)
                .Select(p => new { p.Amount, p.PaidAt })
                .ToListAsync();

            var totalRevenue = successPayments.Sum(p => p.Amount);
            var todayRevenue = successPayments.Where(p => p.PaidAt >= today && p.PaidAt < tomorrow).Sum(p => p.Amount);

            var vm = new DashboardVM
            {
                TotalFields = await _context.FootballFields.CountAsync(),
                ActiveFields = await _context.FootballFields.CountAsync(f => f.IsActive),
                TodayBookings = await _context.Bookings.CountAsync(b => b.BookingDate >= today && b.BookingDate < tomorrow),
                PendingRefunds = await _context.RefundRequests.CountAsync(r => r.Status == RefundStatus.PENDING),
                TotalRevenue = totalRevenue,
                TodayRevenue = todayRevenue,
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
