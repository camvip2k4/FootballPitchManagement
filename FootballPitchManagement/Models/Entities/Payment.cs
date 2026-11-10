using System.ComponentModel.DataAnnotations;
using FootballPitchManagement.Models.Enums;

namespace FootballPitchManagement.Models.Entities
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        [Required]
        [MaxLength(100)]
        public string TransactionRef { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public PaymentMethod Method { get; set; } = PaymentMethod.ONLINE_GATEWAY;

        public PaymentChannel Channel { get; set; } = PaymentChannel.ONLINE;

        public PaymentStatus Status { get; set; } = PaymentStatus.PENDING;

        [MaxLength(100)]
        public string? ReceiptRef { get; set; }

        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();
    }
}

