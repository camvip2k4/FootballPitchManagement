using FootballPitchManagement.Models.Entities;

namespace FootballPitchManagement.Services
{
    public interface IRefundService
    {
        Task<List<RefundRequest>> GetAllRefundRequestsAsync();
        Task<List<RefundRequest>> GetPendingRefundRequestsAsync();
        Task<(bool Success, string Message)> ProcessRefundAsync(int refundId, string evidenceProof, bool isSuccess, int staffAccountId);
    }
}

