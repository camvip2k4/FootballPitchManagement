using System.ComponentModel.DataAnnotations;
using FootballPitchManagement.Models.Enums;

namespace FootballPitchManagement.Models.Entities
{
    public class RefundRequest
    {
        [Key]
        public int RefundId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RefundCode { get; set; } = string.Empty;

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int? PaymentId { get; set; }
        public Payment? Payment { get; set; }

        public decimal Amount { get; set; }

        [Required]
        [MaxLength(255)]
        public string Reason { get; set; } = string.Empty;

        public RefundType RefundType { get; set; }

        public RefundStatus Status { get; set; } = RefundStatus.PENDING;

        [MaxLength(255)]
        public string? EvidenceProof { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        public int? ProcessedByAccountId { get; set; }
        public Account? ProcessedByAccount { get; set; }
    }
}

