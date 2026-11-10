using FootballPitchManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballPitchManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Pitch> Pitches { get; set; } = null!;
        public DbSet<PitchType> PitchTypes { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<ServiceItem> ServiceItems { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity relationships and constraints if needed
            modelBuilder.Entity<Pitch>()
                .HasOne(p => p.PitchType)
                .WithMany(pt => pt.Pitches)
                .HasForeignKey(p => p.PitchTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Pitch)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.PitchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Booking)
                .WithMany(b => b.Invoices)
                .HasForeignKey(i => i.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

