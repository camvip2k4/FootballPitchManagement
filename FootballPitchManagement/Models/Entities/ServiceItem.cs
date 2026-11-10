namespace FootballPitchManagement.Models.Entities
{
    public class ServiceItem
    {
        public int ServiceItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Unit { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
    }
}

