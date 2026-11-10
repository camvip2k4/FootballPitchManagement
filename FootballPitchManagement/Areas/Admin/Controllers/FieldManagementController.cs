using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Entities;
using FootballPitchManagement.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "MANAGER,Manager,ADMIN,Admin")]
    public class FieldManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FieldManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/FieldManagement
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

                // Tạo các ca đá tiêu chuẩn cho sân mới
                var defaultSlots = new (string name, TimeSpan start, TimeSpan end)[]
                {
                    ("Ca 1 (06:00 - 07:30)", new TimeSpan(6, 0, 0), new TimeSpan(7, 30, 0)),
                    ("Ca 2 (07:30 - 09:00)", new TimeSpan(7, 30, 0), new TimeSpan(9, 0, 0)),
                    ("Ca 3 (15:30 - 17:00)", new TimeSpan(15, 30, 0), new TimeSpan(17, 0, 0)),
                    ("Ca 4 (17:00 - 18:30) [Giờ vàng]", new TimeSpan(17, 0, 0), new TimeSpan(18, 30, 0)),
                    ("Ca 5 (18:30 - 20:00) [Giờ vàng]", new TimeSpan(18, 30, 0), new TimeSpan(20, 0, 0)),
                    ("Ca 6 (20:00 - 21:30) [Giờ vàng]", new TimeSpan(20, 0, 0), new TimeSpan(21, 30, 0)),
                    ("Ca 7 (21:30 - 23:00)", new TimeSpan(21, 30, 0), new TimeSpan(23, 0, 0))
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

                TempData["SuccessMessage"] = $"Đã thêm mới sân bóng '{field.Name}' thành công kèm các ca thi đấu mặc định.";
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
                TempData["SuccessMessage"] = $"Đã cập nhật thông tin sân '{field.Name}' thành công.";
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
                string statusText = field.IsActive ? "Đang mở nhận khách" : "Tạm đóng nhận khách";
                TempData["SuccessMessage"] = $"Trạng thái mở đặt của sân '{field.Name}' đã chuyển sang: {statusText}.";
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
                string opText = status switch
                {
                    FieldStatus.AVAILABLE => "Sẵn sàng hoạt động",
                    FieldStatus.MAINTENANCE => "Đang bảo trì cỏ/đèn",
                    _ => "Đang có trận đấu"
                };
                TempData["SuccessMessage"] = $"Trạng thái vận hành thực tế của sân '{field.Name}' đã cập nhật thành: {opText}.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
