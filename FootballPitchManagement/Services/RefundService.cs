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
                return (false, "Refund request not found.");
            }

            if (refund.Status != RefundStatus.PENDING)
            {
                return (false, "This refund request has already been processed.");
            }

            refund.Status = isSuccess ? RefundStatus.SUCCESS : RefundStatus.FAILED;
            refund.EvidenceProof = evidenceProof;
            refund.ProcessedAt = DateTime.UtcNow;
            refund.ProcessedByAccountId = staffAccountId;

            await _context.SaveChangesAsync();

            if (refund.Booking?.AccountId.HasValue == true)
            {
                var notifMsg = isSuccess
                    ? $"Your refund request {refund.RefundCode} of {refund.Amount:N0} VND has been processed successfully. Proof: {evidenceProof}."
                    : $"Your refund request {refund.RefundCode} failed during transfer. Reason/Proof: {evidenceProof}. Please contact reception.";

                await _notificationService.SendNotificationAsync(
                    refund.Booking.AccountId.Value,
                    "REFUND_PROCESSED",
                    notifMsg);
            }

            return (true, isSuccess ? "Refund processed successfully." : "Refund recorded as failed.");
        }
    }
}

