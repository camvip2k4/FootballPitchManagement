namespace FootballPitchManagement.Models.ViewModels
{
    public class InvoiceReportVM
    {
        public int InvoiceId { get; set; }
        public int BookingId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
    }
}

