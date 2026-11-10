using Microsoft.AspNetCore.Mvc;

namespace FootballPitchManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PitchManagementController : Controller
    {
        public IActionResult Index() => RedirectToAction("Index", "FieldManagement");
        public IActionResult Create() => RedirectToAction("Create", "FieldManagement");
        public IActionResult Edit(int id) => RedirectToAction("Edit", "FieldManagement", new { id });
    }
}
