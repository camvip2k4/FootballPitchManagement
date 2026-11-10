using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PitchManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PitchManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/PitchManagement
        public async Task<IActionResult> Index()
        {
            var pitches = await _context.Pitches
                .Include(p => p.PitchType)
                .OrderBy(p => p.PitchId)
                .ToListAsync();

            return View(pitches);
        }

        // GET: Admin/PitchManagement/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.PitchTypes = new SelectList(await _context.PitchTypes.ToListAsync(), "PitchTypeId", "Name");
            return View();
        }

        // POST: Admin/PitchManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pitch pitch)
        {
            if (ModelState.IsValid)
            {
                _context.Pitches.Add(pitch);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.PitchTypes = new SelectList(await _context.PitchTypes.ToListAsync(), "PitchTypeId", "Name", pitch.PitchTypeId);
            return View(pitch);
        }

        // GET: Admin/PitchManagement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var pitch = await _context.Pitches.FindAsync(id);
            if (pitch == null)
            {
                return NotFound();
            }

            ViewBag.PitchTypes = new SelectList(await _context.PitchTypes.ToListAsync(), "PitchTypeId", "Name", pitch.PitchTypeId);
            return View(pitch);
        }

        // POST: Admin/PitchManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pitch pitch)
        {
            if (id != pitch.PitchId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(pitch);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.PitchTypes = new SelectList(await _context.PitchTypes.ToListAsync(), "PitchTypeId", "Name", pitch.PitchTypeId);
            return View(pitch);
        }

        // POST: Admin/PitchManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pitch = await _context.Pitches.FindAsync(id);
            if (pitch != null)
            {
                _context.Pitches.Remove(pitch);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

