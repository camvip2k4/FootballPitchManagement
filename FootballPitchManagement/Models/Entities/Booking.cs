using System.ComponentModel.DataAnnotations;
using FootballPitchManagement.Models.Enums;

namespace FootballPitchManagement.Models.Entities
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        [MaxLength(50)]
        public string BookingCode { get; set; } = string.Empty;

        public DateTime BookingDate { get; set; }

        public int FieldId { get; set; }
        public FootballField? Field { get; set; }

        public int SlotId { get; set; }
        public TimeSlot? TimeSlot { get; set; }

        public int? AccountId { get; set; }
        public Account? Account { get; set; }

        public decimal AgreedPrice { get; set; }

        public decimal RequiredPrepayment { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal BalanceDue { get; set; }

        public SettlementStatus SettlementStatus { get; set; } = SettlementStatus.UNPAID;

        public BookingStatus Status { get; set; } = BookingStatus.PENDING_PAYMENT;

        public DateTime HoldExpiresAt { get; set; }

        public CancellationType? CancellationType { get; set; }

        [MaxLength(255)]
        public string? CancelReason { get; set; }

        [MaxLength(100)]
        public string? GuestName { get; set; }

        [MaxLength(20)]
        public string? GuestPhone { get; set; }

        public DateTime? CheckedInAt { get; set; }

        public DateTime? CheckedOutAt { get; set; }

        public DateTime? NoShowAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();
    }
}
