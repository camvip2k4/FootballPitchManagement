using FootballPitchManagement.Models.Entities;

namespace FootballPitchManagement.Services
{
    public interface IPricingService
    {
        Task<decimal> CalculatePriceAsync(int fieldId, int slotId, DateTime bookingDate);
    }
}

