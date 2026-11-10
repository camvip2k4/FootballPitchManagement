using FootballPitchManagement.Data;
using FootballPitchManagement.Models.Entities;
using FootballPitchManagement.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Services
{
    public class RefundService : IRefundService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public RefundService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<List<RefundRequest>> GetAllRefundRequestsAsync()
        {
            return await _context.RefundRequests
                .Include(r => r.Booking)
                    .ThenInclude(b => b!.Field)
                .Include(r => r.Payment)
                .Include(r => r.ProcessedByAccount)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<RefundRequest>> GetPendingRefundRequestsAsync()
        {
            return await _context.RefundRequests
                .Include(r => r.Booking)
                    .ThenInclude(b => b!.Field)
                .Include(r => r.Payment)
                .Where(r => r.Status == RefundStatus.PENDING)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> ProcessRefundAsync(
            int refundId,
            string evidenceProof,
            bool isSuccess,
            int staffAccountId)
        {
            var refund = await _context.RefundRequests
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.RefundId == refundId);

            if (refund == null)
            {
                return (false, "Không tìm thấy yêu cầu hoàn tiền.");
            }

            if (refund.Status != RefundStatus.PENDING)
            {
                return (false, "Yêu cầu hoàn tiền này đã được xử lý trước đó.");
            }

            refund.Status = isSuccess ? RefundStatus.SUCCESS : RefundStatus.FAILED;
            refund.EvidenceProof = evidenceProof;
            refund.ProcessedAt = DateTime.UtcNow;
            refund.ProcessedByAccountId = staffAccountId;

            await _context.SaveChangesAsync();

            if (refund.Booking?.AccountId.HasValue == true)
            {
                var notifMsg = isSuccess
                    ? $"Yêu cầu hoàn tiền {refund.RefundCode} số tiền {refund.Amount:N0} đ đã được chuyển khoản thành công. Mã giao dịch đối soát: {evidenceProof}."
                    : $"Yêu cầu hoàn tiền {refund.RefundCode} gặp sự cố khi chuyển khoản. Ghi chú: {evidenceProof}. Vui lòng liên hệ ban quản lý.";

                await _notificationService.SendNotificationAsync(
                    refund.Booking.AccountId.Value,
                    "KẾT_QUẢ_HOÀN_TIỀN",
                    notifMsg);
            }

            return (true, isSuccess ? "Đã xác nhận hoàn tiền thành công." : "Đã ghi nhận hoàn tiền thất bại.");
        }
    }
}
