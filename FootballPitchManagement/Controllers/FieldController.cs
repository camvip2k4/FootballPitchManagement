using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Enums;
using FootballPitchManagement.Models.ViewModels;
using FootballPitchManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Controllers
{
    public class FieldController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAvailabilityChecker _availabilityChecker;
        private readonly IPricingService _pricingService;

        public FieldController(
            ApplicationDbContext context,
            IAvailabilityChecker availabilityChecker,
            IPricingService pricingService)
        {
            _context = context;
            _availabilityChecker = availabilityChecker;
            _pricingService = pricingService;
        }

        // GET: /Field or /Field/Index (UC06: Search Football Fields)
        public async Task<IActionResult> Index(DateTime? date, FieldType? fieldType, string? keyword)
        {
            var searchDate = (date ?? DateTime.Today).Date;
            var searchNextDate = searchDate.AddDays(1);

            var query = _context.FootballFields
                .Include(f => f.TimeSlots.Where(s => s.IsActive))
                .Where(f => f.IsActive);

            if (fieldType.HasValue)
            {
                query = query.Where(f => f.FieldType == fieldType.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(f => f.Name.ToLower().Contains(kw) || f.Location.ToLower().Contains(kw));
            }

            var fields = await query.ToListAsync();
            var fieldCards = new List<FieldCardVM>();

            var now = DateTime.UtcNow;

            foreach (var f in fields)
            {
                var card = new FieldCardVM
                {
                    FieldId = f.FieldId,
                    Name = f.Name,
                    FieldType = f.FieldType,
                    Location = f.Location,
                    Description = f.Description,
                    Status = f.Status,
                    IsActive = f.IsActive,
                    Slots = new List<SlotAvailabilityVM>()
                };

                // Get bookings for this field on the target date
                var dayBookings = await _context.Bookings
                    .Where(b => b.FieldId == f.FieldId && b.BookingDate >= searchDate && b.BookingDate < searchNextDate)
                    .ToListAsync();

                foreach (var s in f.TimeSlots.OrderBy(s => s.StartTime))
                {
                    var price = await _pricingService.CalculatePriceAsync(f.FieldId, s.SlotId, searchDate);
                    var isAvailable = await _availabilityChecker.IsSlotAvailableAsync(f.FieldId, s.SlotId, searchDate);

                    var booking = dayBookings.FirstOrDefault(b => b.SlotId == s.SlotId &&
                        (b.Status == BookingStatus.CONFIRMED ||
                         b.Status == BookingStatus.CHECKED_IN ||
                         (b.Status == BookingStatus.PENDING_PAYMENT && b.HoldExpiresAt > now)));

                    string statusDisplay = "Còn trống";
                    string badgeClass = "bg-success";

                    if (f.Status == FieldStatus.MAINTENANCE)
                    {
                        statusDisplay = "Bảo trì";
                        badgeClass = "bg-danger";
                        isAvailable = false;
                    }
                    else if (booking != null)
                    {
                        if (booking.Status == BookingStatus.PENDING_PAYMENT)
                        {
                            statusDisplay = "Đang giữ chỗ (15p)";
                            badgeClass = "bg-warning text-dark";
                        }
                        else if (booking.Status == BookingStatus.CHECKED_IN)
                        {
                            statusDisplay = "Đang thi đấu";
                            badgeClass = "bg-info text-dark";
                        }
                        else
                        {
                            statusDisplay = "Đã có người đặt";
                            badgeClass = "bg-secondary";
                        }
                    }

                    card.Slots.Add(new SlotAvailabilityVM
                    {
                        SlotId = s.SlotId,
                        SlotName = s.SlotName,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        Price = price,
                        IsAvailable = isAvailable,
                        StatusDisplay = statusDisplay,
                        BadgeClass = badgeClass,
                        CurrentBookingId = booking?.BookingId,
                        BookingCode = booking?.BookingCode
                    });
                }

                fieldCards.Add(card);
            }

            var model = new FieldSearchVM
            {
                BookingDate = searchDate,
                FieldType = fieldType,
                Keyword = keyword,
                Fields = fieldCards
            };

            return View(model);
        }

        // GET: /Field/Details/5 (UC07/UC08: View Details & Slots)
        public async Task<IActionResult> Details(int id, DateTime? date)
        {
            var searchDate = (date ?? DateTime.Today).Date;
            var searchNextDate = searchDate.AddDays(1);
            var field = await _context.FootballFields
                .Include(f => f.TimeSlots.Where(s => s.IsActive))
                .FirstOrDefaultAsync(f => f.FieldId == id);

            if (field == null)
            {
                return NotFound();
            }

            var card = new FieldCardVM
            {
                FieldId = field.FieldId,
                Name = field.Name,
                FieldType = field.FieldType,
                Location = field.Location,
                Description = field.Description,
                Status = field.Status,
                IsActive = field.IsActive,
                Slots = new List<SlotAvailabilityVM>()
            };

            var now = DateTime.UtcNow;
            var dayBookings = await _context.Bookings
                .Where(b => b.FieldId == field.FieldId && b.BookingDate >= searchDate && b.BookingDate < searchNextDate)
                .ToListAsync();

            foreach (var s in field.TimeSlots.OrderBy(s => s.StartTime))
            {
                var price = await _pricingService.CalculatePriceAsync(field.FieldId, s.SlotId, searchDate);
                var isAvailable = await _availabilityChecker.IsSlotAvailableAsync(field.FieldId, s.SlotId, searchDate);

                var booking = dayBookings.FirstOrDefault(b => b.SlotId == s.SlotId &&
                    (b.Status == BookingStatus.CONFIRMED ||
                     b.Status == BookingStatus.CHECKED_IN ||
                     (b.Status == BookingStatus.PENDING_PAYMENT && b.HoldExpiresAt > now)));

                string statusDisplay = "Còn trống";
                string badgeClass = "bg-success";

                if (field.Status == FieldStatus.MAINTENANCE)
                {
                    statusDisplay = "Bảo trì";
                    badgeClass = "bg-danger";
                    isAvailable = false;
                }
                else if (booking != null)
                {
                    if (booking.Status == BookingStatus.PENDING_PAYMENT)
                    {
                        statusDisplay = "Đang giữ chỗ (15p)";
                        badgeClass = "bg-warning text-dark";
                    }
                    else if (booking.Status == BookingStatus.CHECKED_IN)
                    {
                        statusDisplay = "Đang thi đấu";
                        badgeClass = "bg-info text-dark";
                    }
                    else
                    {
                        statusDisplay = "Đã có người đặt";
                        badgeClass = "bg-secondary";
                    }
                }

                card.Slots.Add(new SlotAvailabilityVM
                {
                    SlotId = s.SlotId,
                    SlotName = s.SlotName,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Price = price,
                    IsAvailable = isAvailable,
                    StatusDisplay = statusDisplay,
                    BadgeClass = badgeClass,
                    CurrentBookingId = booking?.BookingId,
                    BookingCode = booking?.BookingCode
                });
            }

            ViewBag.SearchDate = searchDate;
            return View(card);
        }
    }
}

