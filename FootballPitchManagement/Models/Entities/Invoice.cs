namespace FootballPitchManagement.Models.Entities
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string PaymentStatus { get; set; } = "Unpaid";
        public string? Notes { get; set; }
    }
}

