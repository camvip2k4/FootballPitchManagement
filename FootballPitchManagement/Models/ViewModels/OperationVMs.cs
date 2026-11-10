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

        [Display(Name = "Số tiền thanh toán (VNĐ)")]
        public decimal PaymentAmount { get; set; }

        [Display(Name = "Phương thức thanh toán")]
        public PaymentMethod Method { get; set; } = PaymentMethod.ONLINE_GATEWAY;

        // Công tắc giả lập kiểm thử giao dịch
        public PaymentStatus SimulatedResult { get; set; } = PaymentStatus.SUCCESS;
        public bool SimulateLatePayment { get; set; } = false;
    }

    public class WalkInBookingVM
    {
        [Required(ErrorMessage = "Vui lòng chọn sân bóng.")]
        [Display(Name = "Sân thi đấu")]
        public int FieldId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khung giờ.")]
        [Display(Name = "Khung giờ (Ca đá)")]
        public int SlotId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày thi đấu.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày thi đấu")]
        public DateTime BookingDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng hoặc tên đội.")]
        [Display(Name = "Tên khách hàng / Đội bóng")]
        public string GuestName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại liên hệ.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại liên hệ")]
        public string GuestPhone { get; set; } = string.Empty;

        [Display(Name = "Số tiền thu trực tiếp tại quầy (VNĐ)")]
        public decimal? DirectPaymentAmount { get; set; }

        [Display(Name = "Phương thức thu tiền")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CASH;

        [Display(Name = "Ghi chú phiếu thu / Mã chứng từ")]
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

        [Display(Name = "Thu nốt số tiền còn thiếu tại quầy (VNĐ)")]
        public decimal DirectPaymentCollected { get; set; }

        [Display(Name = "Sân có bị hư hại cơ sở vật chất không?")]
        public bool IsDamaged { get; set; } = false;

        [Display(Name = "Chi tiết biên bản hư hại (Mặt cỏ / Lưới / Đèn)")]
        public string? DamageNote { get; set; }

        [Display(Name = "Đánh dấu nợ chưa quyết toán nếu khách chưa thanh toán đủ")]
        public bool MarkAsUnsettledDebt { get; set; } = false;
    }

    public class RecordPaymentVM
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public decimal BalanceDue { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tiền thu.")]
        [Range(1000, 100000000, ErrorMessage = "Số tiền thu phải từ 1.000 VNĐ trở lên.")]
        [Display(Name = "Số tiền thực thu (VNĐ)")]
        public decimal Amount { get; set; }

        [Display(Name = "Hình thức thu tiền")]
        public PaymentMethod Method { get; set; } = PaymentMethod.CASH;

        [Display(Name = "Ghi chú / Số phiếu thu")]
        public string? ReceiptRef { get; set; }
    }

    public class CancelBookingVM
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public decimal PaidAmount { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do hủy đặt sân.")]
        [Display(Name = "Lý do hủy đặt sân")]
        public string Reason { get; set; } = string.Empty;
    }

    public class ProcessRefundVM
    {
        public int RefundId { get; set; }
        public string RefundCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public RefundType RefundType { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã giao dịch ngân hàng hoặc số phiếu chi hoàn tiền.")]
        [Display(Name = "Chứng từ hoàn tiền / Mã giao dịch ngân hàng")]
        public string EvidenceProof { get; set; } = string.Empty;

        [Display(Name = "Kết quả chuyển trả hoàn tiền")]
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
