using FootballPitchManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "MANAGER,Manager,ADMIN,Admin")]
    public class PolicyManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PolicyManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/PolicyManagement
        public async Task<IActionResult> Index()
        {
            var policies = await _context.BusinessPolicies.OrderBy(p => p.PolicyCode).ToListAsync();
            return View(policies);
        }

        // POST: /Admin/PolicyManagement/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int policyId, string paramValue)
        {
            var policy = await _context.BusinessPolicies.FindAsync(policyId);
            if (policy != null && !string.IsNullOrWhiteSpace(paramValue))
            {
                policy.ParamValue = paramValue.Trim();
                policy.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Chính sách {policy.PolicyCode} ({policy.Description}) đã cập nhật thành công: {policy.ParamValue} {policy.Unit}.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
