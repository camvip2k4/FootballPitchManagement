using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AccountManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/AccountManagement
        public async Task<IActionResult> Index()
        {
            var accounts = await _context.Accounts.OrderBy(a => a.AccountId).ToListAsync();
            return View(accounts);
        }

        // POST: /Admin/AccountManagement/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                account.Status = account.Status == AccountStatus.ACTIVE ? AccountStatus.INACTIVE : AccountStatus.ACTIVE;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Account '{account.Username}' status changed to {account.Status}.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/AccountManagement/UpdateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(int id, RoleType role)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                account.Role = role;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Account '{account.Username}' role updated to {role}.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/AccountManagement/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                account.PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Password for '{account.Username}' has been reset to: 123456";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

