using FootballPitchManagement.Data;
using FootballPitchManagement.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Controllers
{
    public class PitchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PitchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Pitch/List
        public async Task<IActionResult> List()
        {
            var pitches = await _context.Pitches
                .Include(p => p.PitchType)
                .Where(p => p.IsActive)
                .Select(p => new PitchDetailVM
                {
                    PitchId = p.PitchId,
                    PitchName = p.Name,
                    PitchTypeName = p.PitchType != null ? p.PitchType.Name : string.Empty,
                    HourlyRate = p.HourlyRate,
                    IsActive = p.IsActive,
                    Description = p.Description
                })
                .ToListAsync();

            return View(pitches);
        }

        // GET: Pitch/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var pitch = await _context.Pitches
                .Include(p => p.PitchType)
                .FirstOrDefaultAsync(p => p.PitchId == id);

            if (pitch == null)
            {
                return NotFound();
            }

            var vm = new PitchDetailVM
            {
                PitchId = pitch.PitchId,
                PitchName = pitch.Name,
                PitchTypeName = pitch.PitchType?.Name ?? string.Empty,
                HourlyRate = pitch.HourlyRate,
                IsActive = pitch.IsActive,
                Description = pitch.Description
            };

            return View(vm);
        }
    }
}

