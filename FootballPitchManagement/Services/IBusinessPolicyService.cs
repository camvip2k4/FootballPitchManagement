namespace FootballPitchManagement.Services
{
    public interface IBusinessPolicyService
    {
        Task<int> GetDepositPercentAsync();
        Task<int> GetHoldMinutesAsync();
        Task<int> GetCancelCutoffHoursAsync();
        Task<int> GetRefundPercentAsync();
        Task<int> GetCheckinEarlyMinutesAsync();
        Task<int> GetCheckinLateMinutesAsync();
        Task<int> GetBookingHorizonDaysAsync();
        Task<int> GetMaxPendingPerCustomerAsync();
        Task<string> GetPolicyValueAsync(string paramKey, string defaultValue);
        Task UpdatePolicyValueAsync(string paramKey, string newValue);
    }
}

