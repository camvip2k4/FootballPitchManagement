using System.ComponentModel.DataAnnotations;
using FootballPitchManagement.Models.Entities;
using FootballPitchManagement.Models.Enums;

namespace FootballPitchManagement.Models.ViewModels
{
    public class MakePaymentVM
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string SlotName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public decimal AgreedPrice { get; set; }
        public decimal RequiredPrepayment { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceDue { get; set; }
        public DateTime HoldExpiresAt { get; set; }
        public BookingStatus BookingStatus { get; set; }

        public decimal PaymentAmount { get; set; }
        public PaymentMethod Method { get; set; } = PaymentMethod.ONLINE_GATEWAY;

        // Simulation switches for testing UC14
        public PaymentStatus SimulatedResult { get; set; } = PaymentStatus.SUCCESS;
        public bool SimulateLatePayment { get; set; } = false;
    }

    public class WalkInBookingVM
    {
        [Required]
        public int FieldId { get; set; }

        [Required]
        public int SlotId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Guest Name")]
        public string GuestName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Guest Phone")]
        public string GuestPhone { get; set; } = string.Empty;

        [Display(Name = "Direct Payment Amount (VND)")]
        public decimal? DirectPaymentAmount { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CASH;

        public string? ReceiptRef { get; set; }
    }

    public class CheckOutVM
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string CustomerInfo { get; set; } = string.Empty;
        public decimal AgreedPrice { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceDue { get; set; }

        [Display(Name = "Collect Remaining Balance (VND)")]
        public decimal DirectPaymentCollected { get; set; }

        [Display(Name = "Is Pitch Damaged?")]
        public bool IsDamaged { get; set; } = false;

        [Display(Name = "Damage Details")]
        public string? DamageNote { get; set; }

        [Display(Name = "Mark as Unsettled Debt if not fully paid")]
        public bool MarkAsUnsettledDebt { get; set; } = false;
    }

    public class RecordPaymentVM
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public decimal BalanceDue { get; set; }

        [Required]
        [Range(1000, 100000000)]
        public decimal Amount { get; set; }

        public PaymentMethod Method { get; set; } = PaymentMethod.CASH;

        public string? ReceiptRef { get; set; }
    }

    public class CancelBookingVM
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public decimal PaidAmount { get; set; }

        [Required]
        [Display(Name = "Reason for Cancellation")]
        public string Reason { get; set; } = string.Empty;
    }

    public class ProcessRefundVM
    {
        public int RefundId { get; set; }
        public string RefundCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public RefundType RefundType { get; set; }

        [Required]
        [Display(Name = "Evidence Proof / Transaction Ref")]
        public string EvidenceProof { get; set; } = string.Empty;

        public bool IsSuccess { get; set; } = true;
    }

    public class DashboardVM
    {
        public int TotalFields { get; set; }
        public int ActiveFields { get; set; }
        public int TodayBookings { get; set; }
        public int PendingRefunds { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }

        public List<Booking> RecentBookings { get; set; } = new List<Booking>();
        public List<RefundRequest> PendingRefundRequests { get; set; } = new List<RefundRequest>();
    }
}

