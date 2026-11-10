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

        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<FootballField> FootballFields { get; set; } = null!;
        public DbSet<TimeSlot> TimeSlots { get; set; } = null!;
        public DbSet<ScheduleBlock> ScheduleBlocks { get; set; } = null!;
        public DbSet<PricingRule> PricingRules { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<RefundRequest> RefundRequests { get; set; } = null!;
        public DbSet<BusinessPolicy> BusinessPolicies { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<ServiceItem> ServiceItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Account unique username and email
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Username)
                .IsUnique();

            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Email)
                .IsUnique();

            // FootballField - TimeSlot (1 - many)
            modelBuilder.Entity<TimeSlot>()
                .HasOne(ts => ts.Field)
                .WithMany(ff => ff.TimeSlots)
                .HasForeignKey(ts => ts.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            // FootballField - ScheduleBlock (1 - many)
            modelBuilder.Entity<ScheduleBlock>()
                .HasOne(sb => sb.Field)
                .WithMany(ff => ff.ScheduleBlocks)
                .HasForeignKey(sb => sb.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            // FootballField - PricingRule (1 - many)
            modelBuilder.Entity<PricingRule>()
                .HasOne(pr => pr.Field)
                .WithMany(ff => ff.PricingRules)
                .HasForeignKey(pr => pr.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PricingRule>()
                .HasOne(pr => pr.Slot)
                .WithMany(ts => ts.PricingRules)
                .HasForeignKey(pr => pr.SlotId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking associations
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Field)
                .WithMany(ff => ff.Bookings)
                .HasForeignKey(b => b.FieldId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.TimeSlot)
                .WithMany(ts => ts.Bookings)
                .HasForeignKey(b => b.SlotId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Account)
                .WithMany(a => a.Bookings)
                .HasForeignKey(b => b.AccountId)
                .OnDelete(DeleteBehavior.SetNull);

            // Payment - Booking (many - 1)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // RefundRequest associations
            modelBuilder.Entity<RefundRequest>()
                .HasOne(r => r.Booking)
                .WithMany(b => b.RefundRequests)
                .HasForeignKey(r => r.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RefundRequest>()
                .HasOne(r => r.Payment)
                .WithMany(p => p.RefundRequests)
                .HasForeignKey(r => r.PaymentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RefundRequest>()
                .HasOne(r => r.ProcessedByAccount)
                .WithMany()
                .HasForeignKey(r => r.ProcessedByAccountId)
                .OnDelete(DeleteBehavior.SetNull);

            // Notification - Account
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Account)
                .WithMany(a => a.Notifications)
                .HasForeignKey(n => n.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // BusinessPolicy unique ParamKey
            modelBuilder.Entity<BusinessPolicy>()
                .HasIndex(bp => bp.ParamKey)
                .IsUnique();

            // Configure decimal precision
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }
        }
    }
}
