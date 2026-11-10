using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Entities;
using FootballPitchManagement.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Manager,Admin")]
    public class FieldManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FieldManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/FieldManagement (UC24)
        public async Task<IActionResult> Index()
        {
            var fields = await _context.FootballFields
                .Include(f => f.TimeSlots)
                .OrderBy(f => f.FieldId)
                .ToListAsync();

            return View(fields);
        }

        // GET: /Admin/FieldManagement/Create
        public IActionResult Create()
        {
            return View(new FootballField());
        }

        // POST: /Admin/FieldManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FootballField field)
        {
            if (ModelState.IsValid)
            {
                _context.FootballFields.Add(field);
                await _context.SaveChangesAsync();

                // Generate default slots for this field
                var defaultSlots = new (string name, TimeSpan start, TimeSpan end)[]
                {
                    ("Slot 1 (06:00 - 07:30)", new TimeSpan(6, 0, 0), new TimeSpan(7, 30, 0)),
                    ("Slot 2 (07:30 - 09:00)", new TimeSpan(7, 30, 0), new TimeSpan(9, 0, 0)),
                    ("Slot 3 (15:30 - 17:00)", new TimeSpan(15, 30, 0), new TimeSpan(17, 0, 0)),
                    ("Slot 4 (17:00 - 18:30)", new TimeSpan(17, 0, 0), new TimeSpan(18, 30, 0)),
                    ("Slot 5 (18:30 - 20:00)", new TimeSpan(18, 30, 0), new TimeSpan(20, 0, 0)),
                    ("Slot 6 (20:00 - 21:30)", new TimeSpan(20, 0, 0), new TimeSpan(21, 30, 0))
                };

                foreach (var s in defaultSlots)
                {
                    _context.TimeSlots.Add(new TimeSlot
                    {
                        FieldId = field.FieldId,
                        SlotName = s.name,
                        StartTime = s.start,
                        EndTime = s.end,
                        IsActive = true
                    });
                }
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Field '{field.Name}' created with default time slots.";
                return RedirectToAction(nameof(Index));
            }
            return View(field);
        }

        // GET: /Admin/FieldManagement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var field = await _context.FootballFields.FindAsync(id);
            if (field == null)
            {
                return NotFound();
            }
            return View(field);
        }

        // POST: /Admin/FieldManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FootballField model)
        {
            if (id != model.FieldId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var field = await _context.FootballFields.FindAsync(id);
                if (field == null)
                {
                    return NotFound();
                }

                field.Name = model.Name;
                field.FieldType = model.FieldType;
                field.Location = model.Location;
                field.Description = model.Description;
                field.OpenTime = model.OpenTime;
                field.CloseTime = model.CloseTime;
                field.Status = model.Status;
                field.IsActive = model.IsActive;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Field '{field.Name}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: /Admin/FieldManagement/ToggleActivation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivation(int id)
        {
            var field = await _context.FootballFields.FindAsync(id);
            if (field != null)
            {
                field.IsActive = !field.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Field '{field.Name}' activation set to: {field.IsActive}.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/FieldManagement/UpdateOperationalStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOperationalStatus(int id, FieldStatus status)
        {
            var field = await _context.FootballFields.FindAsync(id);
            if (field != null)
            {
                field.Status = status;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Field '{field.Name}' operational status set to: {field.Status}.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

