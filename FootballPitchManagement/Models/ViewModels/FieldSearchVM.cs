using FootballPitchManagement.Models.Enums;

namespace FootballPitchManagement.Models.ViewModels
{
    public class FieldSearchVM
    {
        public DateTime BookingDate { get; set; } = DateTime.Today;
        public FieldType? FieldType { get; set; }
        public string? Keyword { get; set; }

        public List<FieldCardVM> Fields { get; set; } = new List<FieldCardVM>();
    }

    public class FieldCardVM
    {
        public int FieldId { get; set; }
        public string Name { get; set; } = string.Empty;
        public FieldType FieldType { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? Description { get; set; }
        public FieldStatus Status { get; set; }
        public bool IsActive { get; set; }

        public List<SlotAvailabilityVM> Slots { get; set; } = new List<SlotAvailabilityVM>();
    }

    public class SlotAvailabilityVM
    {
        public int SlotId { get; set; }
        public string SlotName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public string StatusDisplay { get; set; } = "Còn trống";
        public string BadgeClass { get; set; } = "bg-success";
        public int? CurrentBookingId { get; set; }
        public string? BookingCode { get; set; }
    }
}

