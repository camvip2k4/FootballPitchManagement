namespace FootballPitchManagement.Services
{
    public interface IAvailabilityChecker
    {
        Task<bool> IsSlotAvailableAsync(int fieldId, int slotId, DateTime bookingDate);
    }
}

