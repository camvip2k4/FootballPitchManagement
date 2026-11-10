namespace FootballPitchManagement.Models.Entities
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int PitchId { get; set; }
        public Pitch? Pitch { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}

