using Microsoft.AspNetCore.Mvc;

namespace FootballPitchManagement.Controllers
{
    public class PitchController : Controller
    {
        public IActionResult List(DateTime? date)
        {
            return RedirectToAction("Index", "Field", new { date });
        }

        public IActionResult Details(int id, DateTime? date)
        {
            return RedirectToAction("Details", "Field", new { id, date });
        }

        public IActionResult Index(DateTime? date)
        {
            return RedirectToAction("Index", "Field", new { date });
        }
    }
}
