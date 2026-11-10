using FootballPitchManagement.Models.Entities;

namespace FootballPitchManagement.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any pitch types
            if (context.PitchTypes.Any())
            {
                return; // DB has been seeded
            }

            var pitchTypes = new PitchType[]
            {
                new PitchType { Name = "5-a-side", Description = "Indoor/Outdoor 5-a-side pitch" },
                new PitchType { Name = "7-a-side", Description = "Outdoor 7-a-side pitch" },
                new PitchType { Name = "11-a-side", Description = "Standard full-size 11-a-side pitch" }
            };

            context.PitchTypes.AddRange(pitchTypes);
            context.SaveChanges();

            var pitches = new Pitch[]
            {
                new Pitch { Name = "Pitch A1 (5v5)", PitchTypeId = pitchTypes[0].PitchTypeId, HourlyRate = 200000, IsActive = true, Description = "Synthetic grass pitch with LED lighting" },
                new Pitch { Name = "Pitch B1 (7v7)", PitchTypeId = pitchTypes[1].PitchTypeId, HourlyRate = 350000, IsActive = true, Description = "Standard artificial turf pitch" },
                new Pitch { Name = "Pitch C1 (11v11)", PitchTypeId = pitchTypes[2].PitchTypeId, HourlyRate = 700000, IsActive = true, Description = "High-grade artificial turf football stadium" }
            };

            context.Pitches.AddRange(pitches);
            context.SaveChanges();

            var serviceItems = new ServiceItem[]
            {
                new ServiceItem { Name = "Mineral Water", Price = 10000, Unit = "Bottle", IsAvailable = true },
                new ServiceItem { Name = "Match Ball Rental", Price = 30000, Unit = "Ball", IsAvailable = true },
                new ServiceItem { Name = "Training Bib Set", Price = 50000, Unit = "Set", IsAvailable = true },
                new ServiceItem { Name = "Referee Service", Price = 150000, Unit = "Match", IsAvailable = true }
            };

            context.ServiceItems.AddRange(serviceItems);
            context.SaveChanges();
        }
    }
}

