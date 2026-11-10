using FootballPitchManagement.Models.Entities;
using FootballPitchManagement.Models.Enums;

namespace FootballPitchManagement.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            try
            {
                // Verify if Accounts table exists. If not, recreate cleanly with new schema.
                if (!context.Accounts.Any())
                {
                    // Fresh database or schema update
                }
            }
            catch
            {
                // Previous schema did not have Accounts table, recreate with complete new schema
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            context.Database.EnsureCreated();

            if (context.Accounts.Any())
            {
                return; // Already seeded
            }

            // 1. Seed Business Policies (POL-01 to POL-07, BR-32)
            var policies = new List<BusinessPolicy>
            {
                new BusinessPolicy
                {
                    PolicyCode = "POL-01",
                    ParamKey = "DEPOSIT_PERCENT",
                    ParamValue = "30",
                    DataType = "Integer",
                    Unit = "%",
                    Description = "Required prepayment percentage of a booking"
                },
                new BusinessPolicy
                {
                    PolicyCode = "POL-02",
                    ParamKey = "HOLD_MINUTES",
                    ParamValue = "15",
                    DataType = "Integer",
                    Unit = "minutes",
                    Description = "Duration to hold slot for unconfirmed/pending payment"
                },
                new BusinessPolicy
                {
                    PolicyCode = "POL-03",
                    ParamKey = "CANCEL_CUTOFF_HOURS",
                    ParamValue = "24",
                    DataType = "Integer",
                    Unit = "hours",
                    Description = "Cutoff hours before slot start time for customer cancellation"
                },
                new BusinessPolicy
                {
                    PolicyCode = "POL-03",
                    ParamKey = "REFUND_PERCENT",
                    ParamValue = "100",
                    DataType = "Integer",
                    Unit = "%",
                    Description = "Refund percentage when cancelled before cutoff"
                },
                new BusinessPolicy
                {
                    PolicyCode = "POL-04",
                    ParamKey = "CHECKIN_EARLY_MIN",
                    ParamValue = "30",
                    DataType = "Integer",
                    Unit = "minutes",
                    Description = "Allowed early arrival window before slot start"
                },
                new BusinessPolicy
                {
                    PolicyCode = "POL-04",
                    ParamKey = "CHECKIN_LATE_MIN",
                    ParamValue = "30",
                    DataType = "Integer",
                    Unit = "minutes",
                    Description = "Threshold minutes past start time before marking no-show"
                },
                new BusinessPolicy
                {
                    PolicyCode = "POL-05",
                    ParamKey = "BOOKING_HORIZON_DAYS",
                    ParamValue = "30",
                    DataType = "Integer",
                    Unit = "days",
                    Description = "Furthest days in advance customer can book"
                },
                new BusinessPolicy
                {
                    PolicyCode = "BR-32",
                    ParamKey = "MAX_PENDING_PER_CUSTOMER",
                    ParamValue = "3",
                    DataType = "Integer",
                    Unit = "bookings",
                    Description = "Maximum simultaneous pending-payment bookings allowed per customer"
                }
            };
            context.BusinessPolicies.AddRange(policies);
            context.SaveChanges();

            // 2. Seed Accounts (Password "123456")
            string defaultHash = BCrypt.Net.BCrypt.HashPassword("123456");

            var admin = new Account
            {
                Username = "admin",
                Email = "admin@footballpitch.com",
                PasswordHash = defaultHash,
                FullName = "System Administrator",
                Phone = "0901112222",
                Role = RoleType.ADMIN,
                Status = AccountStatus.ACTIVE
            };

            var manager = new Account
            {
                Username = "manager",
                Email = "manager@footballpitch.com",
                PasswordHash = defaultHash,
                FullName = "Facility Manager",
                Phone = "0902223333",
                Role = RoleType.MANAGER,
                Status = AccountStatus.ACTIVE
            };

            var staff = new Account
            {
                Username = "staff",
                Email = "staff@footballpitch.com",
                PasswordHash = defaultHash,
                FullName = "Front Desk Staff",
                Phone = "0903334444",
                Role = RoleType.STAFF,
                Status = AccountStatus.ACTIVE
            };

            var customer = new Account
            {
                Username = "customer",
                Email = "customer@gmail.com",
                PasswordHash = defaultHash,
                FullName = "Nguyen Van A",
                Phone = "0987654321",
                Role = RoleType.CUSTOMER,
                Status = AccountStatus.ACTIVE
            };

            context.Accounts.AddRange(admin, manager, staff, customer);
            context.SaveChanges();

            // 3. Seed Football Fields
            var field1 = new FootballField
            {
                Name = "Pitch 5A - Mini Star",
                FieldType = FieldType.FIELD_5,
                Location = "Zone A - South Gate",
                Description = "High quality synthetic turf with LED night lights for 5-a-side matches.",
                OpenTime = new TimeSpan(6, 0, 0),
                CloseTime = new TimeSpan(23, 0, 0),
                Status = FieldStatus.AVAILABLE,
                IsActive = true
            };

            var field2 = new FootballField
            {
                Name = "Pitch 7B - Champions Turf",
                FieldType = FieldType.FIELD_7,
                Location = "Zone B - Central Area",
                Description = "Standard 7-a-side pitch with high-grade monofilament grass and covered benches.",
                OpenTime = new TimeSpan(6, 0, 0),
                CloseTime = new TimeSpan(23, 0, 0),
                Status = FieldStatus.AVAILABLE,
                IsActive = true
            };

            var field3 = new FootballField
            {
                Name = "Pitch 11C - Grand Arena",
                FieldType = FieldType.FIELD_11,
                Location = "Zone C - Stadium Wing",
                Description = "Full-size 11-a-side tournament pitch with electronic scoreboard and sound system.",
                OpenTime = new TimeSpan(6, 0, 0),
                CloseTime = new TimeSpan(23, 0, 0),
                Status = FieldStatus.AVAILABLE,
                IsActive = true
            };

            context.FootballFields.AddRange(field1, field2, field3);
            context.SaveChanges();

            // 4. Seed Time Slots for Each Field
            var slotDefinitions = new (string name, TimeSpan start, TimeSpan end)[]
            {
                ("Slot 1 (06:00 - 07:30)", new TimeSpan(6, 0, 0), new TimeSpan(7, 30, 0)),
                ("Slot 2 (07:30 - 09:00)", new TimeSpan(7, 30, 0), new TimeSpan(9, 0, 0)),
                ("Slot 3 (15:30 - 17:00)", new TimeSpan(15, 30, 0), new TimeSpan(17, 0, 0)),
                ("Slot 4 (17:00 - 18:30)", new TimeSpan(17, 0, 0), new TimeSpan(18, 30, 0)),
                ("Slot 5 (18:30 - 20:00)", new TimeSpan(18, 30, 0), new TimeSpan(20, 0, 0)),
                ("Slot 6 (20:00 - 21:30)", new TimeSpan(20, 0, 0), new TimeSpan(21, 30, 0))
            };

            var fields = new[] { field1, field2, field3 };
            foreach (var f in fields)
            {
                foreach (var def in slotDefinitions)
                {
                    context.TimeSlots.Add(new TimeSlot
                    {
                        FieldId = f.FieldId,
                        SlotName = def.name,
                        StartTime = def.start,
                        EndTime = def.end,
                        IsActive = true
                    });
                }
            }
            context.SaveChanges();

            // 5. Seed Pricing Rules (Weekday / Weekend, Normal / Peak)
            var allFields = context.FootballFields.ToList();
            foreach (var f in allFields)
            {
                decimal baseRate = f.FieldType switch
                {
                    FieldType.FIELD_5 => 200000m,
                    FieldType.FIELD_7 => 350000m,
                    _ => 700000m
                };

                // Weekday Normal
                context.PricingRules.Add(new PricingRule
                {
                    FieldId = f.FieldId,
                    Price = baseRate,
                    DayType = DayType.WEEKDAY,
                    TimeTier = TimeTier.NORMAL,
                    Priority = 1,
                    EffectiveFrom = DateTime.Today.AddDays(-30),
                    EffectiveTo = DateTime.Today.AddYears(1)
                });

                // Weekday Peak (17:00 onwards)
                context.PricingRules.Add(new PricingRule
                {
                    FieldId = f.FieldId,
                    Price = baseRate * 1.35m,
                    DayType = DayType.WEEKDAY,
                    TimeTier = TimeTier.PEAK,
                    Priority = 2,
                    EffectiveFrom = DateTime.Today.AddDays(-30),
                    EffectiveTo = DateTime.Today.AddYears(1)
                });

                // Weekend Normal
                context.PricingRules.Add(new PricingRule
                {
                    FieldId = f.FieldId,
                    Price = baseRate * 1.25m,
                    DayType = DayType.WEEKEND,
                    TimeTier = TimeTier.NORMAL,
                    Priority = 1,
                    EffectiveFrom = DateTime.Today.AddDays(-30),
                    EffectiveTo = DateTime.Today.AddYears(1)
                });

                // Weekend Peak
                context.PricingRules.Add(new PricingRule
                {
                    FieldId = f.FieldId,
                    Price = baseRate * 1.6m,
                    DayType = DayType.WEEKEND,
                    TimeTier = TimeTier.PEAK,
                    Priority = 2,
                    EffectiveFrom = DateTime.Today.AddDays(-30),
                    EffectiveTo = DateTime.Today.AddYears(1)
                });
            }
            context.SaveChanges();

            // 6. Seed Service Items
            var serviceItems = new ServiceItem[]
            {
                new ServiceItem { Name = "Aquafina Mineral Water (500ml)", Price = 10000, Unit = "Bottle", IsAvailable = true },
                new ServiceItem { Name = "Revive Isotonic Drink", Price = 15000, Unit = "Bottle", IsAvailable = true },
                new ServiceItem { Name = "FIFA Standard Ball Rental", Price = 30000, Unit = "Ball/Match", IsAvailable = true },
                new ServiceItem { Name = "Training Bib Set (7 pcs)", Price = 50000, Unit = "Set", IsAvailable = true },
                new ServiceItem { Name = "Professional Referee", Price = 200000, Unit = "Match", IsAvailable = true }
            };
            context.ServiceItems.AddRange(serviceItems);
            context.SaveChanges();

            // 7. Seed Sample Bookings for Today & Tomorrow
            var f2Slots = context.TimeSlots.Where(s => s.FieldId == field2.FieldId).ToList();
            if (f2Slots.Count >= 4)
            {
                // A confirmed booking for today
                var booking1 = new Booking
                {
                    BookingCode = "BK-" + DateTime.Today.ToString("yyyyMMdd") + "-001",
                    BookingDate = DateTime.Today,
                    FieldId = field2.FieldId,
                    SlotId = f2Slots[3].SlotId, // 17:00 - 18:30
                    AccountId = customer.AccountId,
                    AgreedPrice = 472500,
                    RequiredPrepayment = 141750,
                    PaidAmount = 141750,
                    BalanceDue = 330750,
                    SettlementStatus = SettlementStatus.PARTIALLY_PAID,
                    Status = BookingStatus.CONFIRMED,
                    HoldExpiresAt = DateTime.UtcNow.AddHours(2),
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                };

                context.Bookings.Add(booking1);
                context.SaveChanges();

                var payment1 = new Payment
                {
                    BookingId = booking1.BookingId,
                    TransactionRef = "TXN-DEMO-001",
                    Amount = 141750,
                    Method = PaymentMethod.ONLINE_GATEWAY,
                    Channel = PaymentChannel.ONLINE,
                    Status = PaymentStatus.SUCCESS,
                    PaidAt = DateTime.UtcNow.AddHours(-1)
                };
                context.Payments.Add(payment1);

                // A pending payment booking
                var booking2 = new Booking
                {
                    BookingCode = "BK-" + DateTime.Today.ToString("yyyyMMdd") + "-002",
                    BookingDate = DateTime.Today,
                    FieldId = field2.FieldId,
                    SlotId = f2Slots[4].SlotId, // 18:30 - 20:00
                    AccountId = customer.AccountId,
                    AgreedPrice = 472500,
                    RequiredPrepayment = 141750,
                    PaidAmount = 0,
                    BalanceDue = 472500,
                    SettlementStatus = SettlementStatus.UNPAID,
                    Status = BookingStatus.PENDING_PAYMENT,
                    HoldExpiresAt = DateTime.UtcNow.AddMinutes(12),
                    CreatedAt = DateTime.UtcNow.AddMinutes(-3)
                };
                context.Bookings.Add(booking2);

                context.Notifications.Add(new Notification
                {
                    AccountId = customer.AccountId,
                    Type = "BOOKING_CONFIRMED",
                    Message = $"Your booking {booking1.BookingCode} for {field2.Name} on {DateTime.Today:dd/MM/yyyy} is confirmed!",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                });

                context.SaveChanges();
            }
        }
    }
}
