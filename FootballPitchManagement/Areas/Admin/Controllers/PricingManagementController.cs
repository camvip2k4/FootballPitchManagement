using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Entities;
using FootballPitchManagement.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Manager,Admin")]
    public class PricingManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PricingManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/PricingManagement (UC26)
        public async Task<IActionResult> Index(int? fieldId)
        {
            var query = _context.PricingRules
                .Include(r => r.Field)
                .Include(r => r.Slot)
                .AsQueryable();

            if (fieldId.HasValue)
            {
                query = query.Where(r => r.FieldId == fieldId.Value);
            }

            var rules = await query
                .OrderBy(r => r.FieldId)
                .ThenByDescending(r => r.Priority)
                .ToListAsync();

            ViewBag.Fields = new SelectList(await _context.FootballFields.ToListAsync(), "FieldId", "Name", fieldId);
            ViewBag.SelectedFieldId = fieldId;
            return View(rules);
        }

        // GET: /Admin/PricingManagement/Create
        public async Task<IActionResult> Create(int? fieldId)
        {
            var fields = await _context.FootballFields.ToListAsync();
            ViewBag.Fields = new SelectList(fields, "FieldId", "Name", fieldId);
            return View(new PricingRule
            {
                FieldId = fieldId ?? fields.FirstOrDefault()?.FieldId ?? 0,
                Price = 300000,
                EffectiveFrom = DateTime.Today,
                EffectiveTo = DateTime.Today.AddYears(1),
                Priority = 1
            });
        }

        // POST: /Admin/PricingManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PricingRule rule)
        {
            if (ModelState.IsValid)
            {
                _context.PricingRules.Add(rule);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Pricing rule added successfully.";
                return RedirectToAction(nameof(Index), new { fieldId = rule.FieldId });
            }

            ViewBag.Fields = new SelectList(await _context.FootballFields.ToListAsync(), "FieldId", "Name", rule.FieldId);
            return View(rule);
        }

        // POST: /Admin/PricingManagement/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var rule = await _context.PricingRules.FindAsync(id);
            if (rule != null)
            {
                var fieldId = rule.FieldId;
                _context.PricingRules.Remove(rule);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Pricing rule deleted.";
                return RedirectToAction(nameof(Index), new { fieldId });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

