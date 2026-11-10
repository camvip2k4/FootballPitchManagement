using FootballPitchManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Services
{
    public class BusinessPolicyService : IBusinessPolicyService
    {
        private readonly ApplicationDbContext _context;

        public BusinessPolicyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetDepositPercentAsync()
        {
            var val = await GetPolicyValueAsync("DEPOSIT_PERCENT", "30");
            return int.TryParse(val, out var res) ? res : 30;
        }

        public async Task<int> GetHoldMinutesAsync()
        {
            var val = await GetPolicyValueAsync("HOLD_MINUTES", "15");
            return int.TryParse(val, out var res) ? res : 15;
        }

        public async Task<int> GetCancelCutoffHoursAsync()
        {
            var val = await GetPolicyValueAsync("CANCEL_CUTOFF_HOURS", "24");
            return int.TryParse(val, out var res) ? res : 24;
        }

        public async Task<int> GetRefundPercentAsync()
        {
            var val = await GetPolicyValueAsync("REFUND_PERCENT", "100");
            return int.TryParse(val, out var res) ? res : 100;
        }

        public async Task<int> GetCheckinEarlyMinutesAsync()
        {
            var val = await GetPolicyValueAsync("CHECKIN_EARLY_MIN", "30");
            return int.TryParse(val, out var res) ? res : 30;
        }

        public async Task<int> GetCheckinLateMinutesAsync()
        {
            var val = await GetPolicyValueAsync("CHECKIN_LATE_MIN", "30");
            return int.TryParse(val, out var res) ? res : 30;
        }

        public async Task<int> GetBookingHorizonDaysAsync()
        {
            var val = await GetPolicyValueAsync("BOOKING_HORIZON_DAYS", "30");
            return int.TryParse(val, out var res) ? res : 30;
        }

        public async Task<int> GetMaxPendingPerCustomerAsync()
        {
            var val = await GetPolicyValueAsync("MAX_PENDING_PER_CUSTOMER", "3");
            return int.TryParse(val, out var res) ? res : 3;
        }

        public async Task<string> GetPolicyValueAsync(string paramKey, string defaultValue)
        {
            var policy = await _context.BusinessPolicies.FirstOrDefaultAsync(p => p.ParamKey == paramKey);
            return policy != null ? policy.ParamValue : defaultValue;
        }

        public async Task UpdatePolicyValueAsync(string paramKey, string newValue)
        {
            var policy = await _context.BusinessPolicies.FirstOrDefaultAsync(p => p.ParamKey == paramKey);
            if (policy != null)
            {
                policy.ParamValue = newValue;
                policy.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}

