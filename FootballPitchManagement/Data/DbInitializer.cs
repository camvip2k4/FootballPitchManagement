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
                if (!context.Accounts.Any())
                {
                    // Fresh or needs recreation
                }
            }
            catch
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            context.Database.EnsureCreated();

            if (context.Accounts.Any())
            {
                return; // Already initialized
            }

            // 1. Chính sách kinh doanh & vận hành hệ thống (POL-01 -> POL-07)
            var policies = new List<BusinessPolicy>
            {
                new BusinessPolicy
                {
                    PolicyCode = "POL-01",
                    ParamKey = "DEPOSIT_PERCENT",
                    ParamValue = "30",
                    DataType = "Integer",
                    Unit = "%",
                    Description = "Tỷ lệ tiền cọc tối thiểu để xác nhận giữ sân"
                },
                new BusinessPolicy
                {
                    PolicyCode = "POL-02",
                    ParamKey = "HOLD_MINUTES",
                    ParamValue = "15",
                    DataType = "Integer",
                    Unit = "phút",
                    Description = "Thời gian tạm giữ chỗ chờ thanh toán đặt cọc"
                },
                new BusinessPolicy
                {
                    PolicyCode = "POL-03",
                    ParamKey = "CANCEL_CUTOFF_HOURS",
                    ParamValue = "24",
                    DataType = "Integer",
                    Unit = "giờ",
                    Description = "Thời hạn tối thiểu trước giờ đá để khách được hủy sân và nhận lại cọc"
                },
                new BusinessPolicy
                {
                    PolicyCode = "POL-03",
                    ParamKey = "REFUND_PERCENT",
                    ParamValue = "100",
                    DataType = "Integer",
                    Unit = "%",
                    Description = "Tỷ lệ hoàn tiền cọc khi khách hủy đúng hạn"
                },
                new BusinessPolicy
                {
                    PolicyCode = "POL-04",
                    ParamKey = "CHECKIN_EARLY_MIN",
                    ParamValue = "30",
                    DataType = "Integer",
                    Unit = "phút",
                    Description = "Thời gian cho phép check-in sớm trước giờ thi đấu"
                },
                new BusinessPolicy
                {
                    PolicyCode = "POL-04",
                    ParamKey = "CHECKIN_LATE_MIN",
                    ParamValue = "30",
                    DataType = "Integer",
                    Unit = "phút",
                    Description = "Thời gian trễ tối đa trước khi hệ thống ghi nhận vắng mặt (No-Show)"
                },
                new BusinessPolicy
                {
                    PolicyCode = "POL-05",
                    ParamKey = "BOOKING_HORIZON_DAYS",
                    ParamValue = "30",
                    DataType = "Integer",
                    Unit = "ngày",
                    Description = "Số ngày tối đa khách được phép đặt sân trước"
                },
                new BusinessPolicy
                {
                    PolicyCode = "BR-32",
                    ParamKey = "MAX_PENDING_PER_CUSTOMER",
                    ParamValue = "3",
                    DataType = "Integer",
                    Unit = "đơn",
                    Description = "Số lượng đơn đặt giữ chỗ chờ thanh toán tối đa của mỗi khách hàng"
                }
            };
            context.BusinessPolicies.AddRange(policies);
            context.SaveChanges();

            // 2. Tài khoản mẫu cho hệ thống (Mật khẩu mặc định: 123456)
            string defaultHash = BCrypt.Net.BCrypt.HashPassword("123456");

            var admin = new Account
            {
                Username = "admin",
                Email = "admin@sportarena.vn",
                PasswordHash = defaultHash,
                FullName = "Quản Trị Viên Hệ Thống",
                Phone = "0901112222",
                Role = RoleType.ADMIN,
                Status = AccountStatus.ACTIVE
            };

            var manager = new Account
            {
                Username = "manager",
                Email = "quanly@sportarena.vn",
                PasswordHash = defaultHash,
                FullName = "Nguyễn Văn Quản Lý",
                Phone = "0902223333",
                Role = RoleType.MANAGER,
                Status = AccountStatus.ACTIVE
            };

            var staff = new Account
            {
                Username = "staff",
                Email = "letan@sportarena.vn",
                PasswordHash = defaultHash,
                FullName = "Trần Thị Lễ Tân",
                Phone = "0903334444",
                Role = RoleType.STAFF,
                Status = AccountStatus.ACTIVE
            };

            var customer = new Account
            {
                Username = "customer",
                Email = "khachhang@gmail.com",
                PasswordHash = defaultHash,
                FullName = "Lê Hoàng Nam",
                Phone = "0987654321",
                Role = RoleType.CUSTOMER,
                Status = AccountStatus.ACTIVE
            };

            context.Accounts.AddRange(admin, manager, staff, customer);
            context.SaveChanges();

            // 3. Danh mục sân bóng đá cao cấp Sport Arena
            var field1 = new FootballField
            {
                Name = "Sân 5A - Mini Stadium",
                FieldType = FieldType.FIELD_5,
                Location = "Khu A - Cổng số 1, Sân bóng Sport Arena",
                Description = "Mặt cỏ nhân tạo sợi kim cương nhập khẩu, hệ thống đèn chiếu sáng LED chống chói 300 Lux, có lưới chắn bóng tiêu chuẩn.",
                OpenTime = new TimeSpan(6, 0, 0),
                CloseTime = new TimeSpan(23, 0, 0),
                Status = FieldStatus.AVAILABLE,
                IsActive = true
            };

            var field2 = new FootballField
            {
                Name = "Sân 7B - Champions Turf",
                FieldType = FieldType.FIELD_7,
                Location = "Khu B - Khu trung tâm thể thao",
                Description = "Sân 7 người tiêu chuẩn thi đấu phong trào, mặt sân êm ái chống chấn thương, có cabin kỹ thuật và khán đài mini có mái che.",
                OpenTime = new TimeSpan(6, 0, 0),
                CloseTime = new TimeSpan(23, 0, 0),
                Status = FieldStatus.AVAILABLE,
                IsActive = true
            };

            var field3 = new FootballField
            {
                Name = "Sân 11C - Grand Arena",
                FieldType = FieldType.FIELD_11,
                Location = "Khu C - Sân vận động lớn",
                Description = "Sân bóng 11 người kích thước chuẩn FIFA, trang bị dàn đèn công suất lớn, bảng tỉ số điện tử, dàn âm thanh và phòng thay đồ cao cấp.",
                OpenTime = new TimeSpan(6, 0, 0),
                CloseTime = new TimeSpan(23, 0, 0),
                Status = FieldStatus.AVAILABLE,
                IsActive = true
            };

            context.FootballFields.AddRange(field1, field2, field3);
            context.SaveChanges();

            // 4. Các khung giờ thi đấu trong ngày cho từng sân
            var slotDefinitions = new (string name, TimeSpan start, TimeSpan end)[]
            {
                ("Ca 1 (06:00 - 07:30)", new TimeSpan(6, 0, 0), new TimeSpan(7, 30, 0)),
                ("Ca 2 (07:30 - 09:00)", new TimeSpan(7, 30, 0), new TimeSpan(9, 0, 0)),
                ("Ca 3 (15:30 - 17:00)", new TimeSpan(15, 30, 0), new TimeSpan(17, 0, 0)),
                ("Ca 4 (17:00 - 18:30) [Giờ vàng]", new TimeSpan(17, 0, 0), new TimeSpan(18, 30, 0)),
                ("Ca 5 (18:30 - 20:00) [Giờ vàng]", new TimeSpan(18, 30, 0), new TimeSpan(20, 0, 0)),
                ("Ca 6 (20:00 - 21:30) [Giờ vàng]", new TimeSpan(20, 0, 0), new TimeSpan(21, 30, 0))
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

            // 5. Cấu hình bảng giá linh hoạt (Ngày thường, Cuối tuần, Khung giờ thường, Khung giờ vàng)
            var allFields = context.FootballFields.ToList();
            foreach (var f in allFields)
            {
                decimal baseRate = f.FieldType switch
                {
                    FieldType.FIELD_5 => 220000m,
                    FieldType.FIELD_7 => 380000m,
                    _ => 750000m
                };

                // Ngày thường - Giờ thường
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

                // Ngày thường - Giờ vàng (Từ 17:00)
                context.PricingRules.Add(new PricingRule
                {
                    FieldId = f.FieldId,
                    Price = Math.Round(baseRate * 1.35m / 10000) * 10000,
                    DayType = DayType.WEEKDAY,
                    TimeTier = TimeTier.PEAK,
                    Priority = 2,
                    EffectiveFrom = DateTime.Today.AddDays(-30),
                    EffectiveTo = DateTime.Today.AddYears(1)
                });

                // Cuối tuần - Giờ thường
                context.PricingRules.Add(new PricingRule
                {
                    FieldId = f.FieldId,
                    Price = Math.Round(baseRate * 1.25m / 10000) * 10000,
                    DayType = DayType.WEEKEND,
                    TimeTier = TimeTier.NORMAL,
                    Priority = 1,
                    EffectiveFrom = DateTime.Today.AddDays(-30),
                    EffectiveTo = DateTime.Today.AddYears(1)
                });

                // Cuối tuần - Giờ vàng
                context.PricingRules.Add(new PricingRule
                {
                    FieldId = f.FieldId,
                    Price = Math.Round(baseRate * 1.6m / 10000) * 10000,
                    DayType = DayType.WEEKEND,
                    TimeTier = TimeTier.PEAK,
                    Priority = 2,
                    EffectiveFrom = DateTime.Today.AddDays(-30),
                    EffectiveTo = DateTime.Today.AddYears(1)
                });
            }
            context.SaveChanges();

            // 6. Dịch vụ tiện ích tại sân
            var serviceItems = new ServiceItem[]
            {
                new ServiceItem { Name = "Nước khoáng Aquafina (500ml)", Price = 10000, Unit = "Chai", IsAvailable = true },
                new ServiceItem { Name = "Nước điện giải Revive bù khoáng", Price = 15000, Unit = "Chai", IsAvailable = true },
                new ServiceItem { Name = "Thuê bóng thi đấu đạt chuẩn FIFA", Price = 30000, Unit = "Quả/Trận", IsAvailable = true },
                new ServiceItem { Name = "Bộ áo bib phân đội tập luyện (7 áo)", Price = 50000, Unit = "Bộ/Trận", IsAvailable = true },
                new ServiceItem { Name = "Thuê trọng tài bắt chính chuyên nghiệp", Price = 200000, Unit = "Trận", IsAvailable = true }
            };
            context.ServiceItems.AddRange(serviceItems);
            context.SaveChanges();

            // 7. Tạo sẵn một số đơn đặt sân thực tế cho hôm nay
            var f2Slots = context.TimeSlots.Where(s => s.FieldId == field2.FieldId).ToList();
            if (f2Slots.Count >= 5)
            {
                // Đơn đã thanh toán cọc và xác nhận cho ca 17:00
                var booking1 = new Booking
                {
                    BookingCode = "BK-" + DateTime.Today.ToString("yyyyMMdd") + "-001",
                    BookingDate = DateTime.Today,
                    FieldId = field2.FieldId,
                    SlotId = f2Slots[3].SlotId,
                    AccountId = customer.AccountId,
                    AgreedPrice = 510000,
                    RequiredPrepayment = 153000,
                    PaidAmount = 153000,
                    BalanceDue = 357000,
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
                    TransactionRef = "VNPAY-20261110-098812",
                    Amount = 153000,
                    Method = PaymentMethod.ONLINE_GATEWAY,
                    Channel = PaymentChannel.ONLINE,
                    Status = PaymentStatus.SUCCESS,
                    PaidAt = DateTime.UtcNow.AddHours(-1)
                };
                context.Payments.Add(payment1);

                // Đơn đang tạm giữ chỗ chờ thanh toán cho ca 18:30
                var booking2 = new Booking
                {
                    BookingCode = "BK-" + DateTime.Today.ToString("yyyyMMdd") + "-002",
                    BookingDate = DateTime.Today,
                    FieldId = field2.FieldId,
                    SlotId = f2Slots[4].SlotId,
                    AccountId = customer.AccountId,
                    AgreedPrice = 510000,
                    RequiredPrepayment = 153000,
                    PaidAmount = 0,
                    BalanceDue = 510000,
                    SettlementStatus = SettlementStatus.UNPAID,
                    Status = BookingStatus.PENDING_PAYMENT,
                    HoldExpiresAt = DateTime.UtcNow.AddMinutes(12),
                    CreatedAt = DateTime.UtcNow.AddMinutes(-3)
                };
                context.Bookings.Add(booking2);

                context.Notifications.Add(new Notification
                {
                    AccountId = customer.AccountId,
                    Type = "XÁC NHẬN ĐẶT SÂN",
                    Message = $"Đơn đặt sân {booking1.BookingCode} tại {field2.Name} ngày {DateTime.Today:dd/MM/yyyy} đã được xác nhận thành công!",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                });

                context.SaveChanges();
            }
        }
    }
}
