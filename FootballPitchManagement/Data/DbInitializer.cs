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
                // Đồng bộ và chuẩn hóa 100% dữ liệu sang tiếng Việt cho hệ thống đã có sẵn
                SyncVietnameseData(context);
                return;
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
                    Unit = "tiếng",
                    Description = "Thời hạn tối thiểu trước giờ đá để khách được hủy sân và nhận lại cọc"
                },
                new BusinessPolicy
                {
                    PolicyCode = "POL-04",
                    ParamKey = "REFUND_PERCENT",
                    ParamValue = "100",
                    DataType = "Integer",
                    Unit = "%",
                    Description = "Tỷ lệ hoàn tiền cọc khi khách hủy đúng hạn"
                },
                new BusinessPolicy
                {
                    PolicyCode = "POL-05",
                    ParamKey = "CHECKIN_EARLY_MIN",
                    ParamValue = "30",
                    DataType = "Integer",
                    Unit = "phút",
                    Description = "Thời gian cho phép check-in sớm trước giờ thi đấu"
                },
                new BusinessPolicy
                {
                    PolicyCode = "POL-06",
                    ParamKey = "CHECKIN_LATE_MIN",
                    ParamValue = "20",
                    DataType = "Integer",
                    Unit = "phút",
                    Description = "Thời gian trễ tối đa trước khi hệ thống ghi nhận vắng mặt (No-Show)"
                },
                new BusinessPolicy
                {
                    PolicyCode = "POL-07",
                    ParamKey = "BOOKING_HORIZON_DAYS",
                    ParamValue = "14",
                    DataType = "Integer",
                    Unit = "ngày",
                    Description = "Số ngày tối đa khách được phép đặt sân trước"
                },
                new BusinessPolicy
                {
                    PolicyCode = "POL-08",
                    ParamKey = "MAX_PENDING_PER_CUSTOMER",
                    ParamValue = "2",
                    DataType = "Integer",
                    Unit = "đơn",
                    Description = "Số lượng đơn đặt giữ chỗ chờ thanh toán tối đa của mỗi khách hàng"
                }
            };

            context.BusinessPolicies.AddRange(policies);
            context.SaveChanges();

            // 2. Tài khoản mẫu cho từng vai trò người dùng (Mật khẩu mặc định: 123456)
            var defaultHash = BCrypt.Net.BCrypt.HashPassword("123456");

            var admin = new Account
            {
                Username = "admin",
                Email = "admin@sportarena.vn",
                PasswordHash = defaultHash,
                FullName = "Nguyễn Hoàng Long (Tổng Giám Đốc)",
                Phone = "0901112222",
                Role = RoleType.ADMIN,
                Status = AccountStatus.ACTIVE
            };

            var manager = new Account
            {
                Username = "manager",
                Email = "quanly@sportarena.vn",
                PasswordHash = defaultHash,
                FullName = "Trần Đình Trọng (Quản Lý Cơ Sở)",
                Phone = "0902223333",
                Role = RoleType.MANAGER,
                Status = AccountStatus.ACTIVE
            };

            var staff = new Account
            {
                Username = "staff",
                Email = "letan@sportarena.vn",
                PasswordHash = defaultHash,
                FullName = "Nguyễn Thu Hà (Lễ Tân Trực Sân)",
                Phone = "0903334444",
                Role = RoleType.STAFF,
                Status = AccountStatus.ACTIVE
            };

            var customer = new Account
            {
                Username = "customer",
                Email = "khachhang@gmail.com",
                PasswordHash = defaultHash,
                FullName = "Lê Hoàng Nam (Khách Hàng Thân Thiết)",
                Phone = "0987654321",
                Role = RoleType.CUSTOMER,
                Status = AccountStatus.ACTIVE
            };

            context.Accounts.AddRange(admin, manager, staff, customer);
            context.SaveChanges();

            // 3. Danh mục sân bóng đá cao cấp SPORT ARENA
            var field1 = new FootballField
            {
                Name = "Sân 5A - Mini Stadium",
                FieldType = FieldType.FIELD_5,
                Location = "Khu A - Cổng số 1, Tổ hợp thể thao Sport Arena",
                Description = "Mặt cỏ nhân tạo sợi kim cương tiêu chuẩn FIFA, hệ thống đèn LED chống chói 300 Lux, có lưới chắn bóng an toàn và cabin kỹ thuật.",
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
                Description = "Sân 7 người tiêu chuẩn thi đấu phong trào, mặt sân êm ái đàn hồi cao giảm chấn thương, có cabin ban huấn luyện và khán đài mini có mái che.",
                OpenTime = new TimeSpan(6, 0, 0),
                CloseTime = new TimeSpan(23, 0, 0),
                Status = FieldStatus.AVAILABLE,
                IsActive = true
            };

            var field3 = new FootballField
            {
                Name = "Sân 11C - Grand Arena",
                FieldType = FieldType.FIELD_11,
                Location = "Khu C - Sân vận động trung tâm",
                Description = "Sân bóng 11 người kích thước chuẩn quốc tế, trang bị giàn đèn cao áp công suất lớn, bảng tỉ số điện tử, hệ thống âm thanh vòm và phòng thay đồ VIP.",
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
                ("Ca 6 (20:00 - 21:30) [Giờ vàng]", new TimeSpan(20, 0, 0), new TimeSpan(21, 30, 0)),
                ("Ca 7 (21:30 - 23:00)", new TimeSpan(21, 30, 0), new TimeSpan(23, 0, 0))
            };

            var allFields = new[] { field1, field2, field3 };
            var allSlots = new List<TimeSlot>();

            foreach (var f in allFields)
            {
                foreach (var def in slotDefinitions)
                {
                    allSlots.Add(new TimeSlot
                    {
                        FieldId = f.FieldId,
                        SlotName = def.name,
                        StartTime = def.start,
                        EndTime = def.end,
                        IsActive = true
                    });
                }
            }

            context.TimeSlots.AddRange(allSlots);
            context.SaveChanges();

            // 5. Bảng giá động linh hoạt (Pricing Rules)
            var pricingRules = new List<PricingRule>
            {
                // Sân 5A
                new PricingRule { FieldId = field1.FieldId, DayType = DayType.WEEKDAY, TimeTier = TimeTier.NORMAL, Price = 250000, Priority = 1 },
                new PricingRule { FieldId = field1.FieldId, DayType = DayType.WEEKDAY, TimeTier = TimeTier.PEAK, Price = 380000, Priority = 2 },
                new PricingRule { FieldId = field1.FieldId, DayType = DayType.WEEKEND, TimeTier = TimeTier.NORMAL, Price = 320000, Priority = 3 },
                new PricingRule { FieldId = field1.FieldId, DayType = DayType.WEEKEND, TimeTier = TimeTier.PEAK, Price = 450000, Priority = 4 },

                // Sân 7B
                new PricingRule { FieldId = field2.FieldId, DayType = DayType.WEEKDAY, TimeTier = TimeTier.NORMAL, Price = 450000, Priority = 1 },
                new PricingRule { FieldId = field2.FieldId, DayType = DayType.WEEKDAY, TimeTier = TimeTier.PEAK, Price = 650000, Priority = 2 },
                new PricingRule { FieldId = field2.FieldId, DayType = DayType.WEEKEND, TimeTier = TimeTier.NORMAL, Price = 550000, Priority = 3 },
                new PricingRule { FieldId = field2.FieldId, DayType = DayType.WEEKEND, TimeTier = TimeTier.PEAK, Price = 800000, Priority = 4 },

                // Sân 11C
                new PricingRule { FieldId = field3.FieldId, DayType = DayType.WEEKDAY, TimeTier = TimeTier.NORMAL, Price = 1200000, Priority = 1 },
                new PricingRule { FieldId = field3.FieldId, DayType = DayType.WEEKDAY, TimeTier = TimeTier.PEAK, Price = 1800000, Priority = 2 },
                new PricingRule { FieldId = field3.FieldId, DayType = DayType.WEEKEND, TimeTier = TimeTier.NORMAL, Price = 1500000, Priority = 3 },
                new PricingRule { FieldId = field3.FieldId, DayType = DayType.WEEKEND, TimeTier = TimeTier.PEAK, Price = 2200000, Priority = 4 }
            };

            context.PricingRules.AddRange(pricingRules);

            // 6. Dịch vụ tiện ích tại quầy (F&B, thuê trang thiết bị)
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

            // 7. Tạo dữ liệu mẫu thực tế
            SeedRealisticDemoData(context, field1, field2, field3, allSlots, customer, admin);
        }

        private static void SyncVietnameseData(ApplicationDbContext context)
        {
            // Cập nhật tên và mô tả tiếng Việt cho tất cả các sân hiện có trong DB
            var fields = context.FootballFields.ToList();
            foreach (var f in fields)
            {
                if (f.FieldType == FieldType.FIELD_5 || f.Name.Contains("5"))
                {
                    f.Name = "Sân 5A - Mini Stadium";
                    f.Location = "Khu A - Cổng số 1, Tổ hợp thể thao Sport Arena";
                    f.Description = "Mặt cỏ nhân tạo sợi kim cương tiêu chuẩn FIFA, hệ thống đèn LED chống chói 300 Lux, có lưới chắn bóng an toàn và cabin kỹ thuật.";
                }
                else if (f.FieldType == FieldType.FIELD_7 || f.Name.Contains("7"))
                {
                    f.Name = "Sân 7B - Champions Turf";
                    f.Location = "Khu B - Khu trung tâm thể thao";
                    f.Description = "Sân 7 người tiêu chuẩn thi đấu phong trào, mặt sân êm ái đàn hồi cao giảm chấn thương, có cabin ban huấn luyện và khán đài mini có mái che.";
                }
                else if (f.FieldType == FieldType.FIELD_11 || f.Name.Contains("11"))
                {
                    f.Name = "Sân 11C - Grand Arena";
                    f.Location = "Khu C - Sân vận động trung tâm";
                    f.Description = "Sân bóng 11 người kích thước chuẩn quốc tế, trang bị giàn đèn cao áp công suất lớn, bảng tỉ số điện tử, hệ thống âm thanh vòm và phòng thay đồ VIP.";
                }
            }

            // Cập nhật tên ca đá sang tiếng Việt
            var slots = context.TimeSlots.ToList();
            foreach (var s in slots)
            {
                if (s.SlotName.StartsWith("Slot 1") || s.StartTime == new TimeSpan(6, 0, 0)) s.SlotName = "Ca 1 (06:00 - 07:30)";
                else if (s.SlotName.StartsWith("Slot 2") || s.StartTime == new TimeSpan(7, 30, 0)) s.SlotName = "Ca 2 (07:30 - 09:00)";
                else if (s.SlotName.StartsWith("Slot 3") || s.StartTime == new TimeSpan(15, 30, 0)) s.SlotName = "Ca 3 (15:30 - 17:00)";
                else if (s.SlotName.StartsWith("Slot 4") || s.StartTime == new TimeSpan(17, 0, 0)) s.SlotName = "Ca 4 (17:00 - 18:30) [Giờ vàng]";
                else if (s.SlotName.StartsWith("Slot 5") || s.StartTime == new TimeSpan(18, 30, 0)) s.SlotName = "Ca 5 (18:30 - 20:00) [Giờ vàng]";
                else if (s.SlotName.StartsWith("Slot 6") || s.StartTime == new TimeSpan(20, 0, 0)) s.SlotName = "Ca 6 (20:00 - 21:30) [Giờ vàng]";
                else if (s.SlotName.StartsWith("Slot 7") || s.StartTime == new TimeSpan(21, 30, 0)) s.SlotName = "Ca 7 (21:30 - 23:00)";
            }

            // Cập nhật chính sách POL-01..07 sang tiếng Việt
            var policies = context.BusinessPolicies.ToList();
            foreach (var p in policies)
            {
                if (p.ParamKey == "DEPOSIT_PERCENT") { p.Description = "Tỷ lệ tiền cọc tối thiểu để xác nhận giữ sân"; p.Unit = "%"; }
                else if (p.ParamKey == "HOLD_MINUTES") { p.Description = "Thời gian tạm giữ chỗ chờ thanh toán đặt cọc"; p.Unit = "phút"; }
                else if (p.ParamKey == "CANCEL_CUTOFF_HOURS") { p.Description = "Thời hạn tối thiểu trước giờ đá để khách được hủy sân và nhận lại cọc"; p.Unit = "tiếng"; }
                else if (p.ParamKey == "REFUND_PERCENT") { p.Description = "Tỷ lệ hoàn tiền cọc khi khách hủy đúng hạn"; p.Unit = "%"; }
                else if (p.ParamKey == "CHECKIN_EARLY_MIN") { p.Description = "Thời gian cho phép check-in sớm trước giờ thi đấu"; p.Unit = "phút"; }
                else if (p.ParamKey == "CHECKIN_LATE_MIN") { p.Description = "Thời gian trễ tối đa trước khi hệ thống ghi nhận vắng mặt (No-Show)"; p.Unit = "phút"; }
                else if (p.ParamKey == "BOOKING_HORIZON_DAYS") { p.Description = "Số ngày tối đa khách được phép đặt sân trước"; p.Unit = "ngày"; }
                else if (p.ParamKey == "MAX_PENDING_PER_CUSTOMER") { p.Description = "Số lượng đơn đặt giữ chỗ chờ thanh toán tối đa của mỗi khách hàng"; p.Unit = "đơn"; }
            }

            // Cập nhật tên hiển thị tài khoản mẫu
            var accounts = context.Accounts.ToList();
            foreach (var a in accounts)
            {
                if (a.Username == "admin") a.FullName = "Nguyễn Hoàng Long (Tổng Giám Đốc)";
                else if (a.Username == "manager") a.FullName = "Trần Đình Trọng (Quản Lý Cơ Sở)";
                else if (a.Username == "staff") a.FullName = "Nguyễn Thu Hà (Lễ Tân Trực Sân)";
                else if (a.Username == "customer") a.FullName = "Lê Hoàng Nam (Khách Hàng Thân Thiết)";
            }

            context.SaveChanges();
        }

        private static void SeedRealisticDemoData(
            ApplicationDbContext context,
            FootballField field1,
            FootballField field2,
            FootballField field3,
            List<TimeSlot> allSlots,
            Account customer,
            Account admin)
        {
            var today = DateTime.Today;
            var now = DateTime.UtcNow;

            var slot4_Field1 = allSlots.First(s => s.FieldId == field1.FieldId && s.StartTime == new TimeSpan(17, 0, 0));
            var slot5_Field1 = allSlots.First(s => s.FieldId == field1.FieldId && s.StartTime == new TimeSpan(18, 30, 0));
            var slot4_Field2 = allSlots.First(s => s.FieldId == field2.FieldId && s.StartTime == new TimeSpan(17, 0, 0));

            // Đơn 1: Đã xác nhận & đặt cọc 380,000đ (Ca 4 hôm nay)
            var b1 = new Booking
            {
                BookingCode = "BK-" + today.ToString("yyyyMMdd") + "-0001",
                FieldId = field1.FieldId,
                SlotId = slot4_Field1.SlotId,
                BookingDate = today,
                AccountId = customer.AccountId,
                AgreedPrice = 380000,
                RequiredPrepayment = 114000,
                PaidAmount = 114000,
                BalanceDue = 266000,
                Status = BookingStatus.CONFIRMED,
                SettlementStatus = SettlementStatus.PARTIALLY_PAID,
                HoldExpiresAt = now.AddMinutes(15),
                CreatedAt = now.AddHours(-3)
            };

            // Đơn 2: Đang thi đấu trên sân 7B (Checked-in)
            var b2 = new Booking
            {
                BookingCode = "BK-" + today.ToString("yyyyMMdd") + "-0002",
                FieldId = field2.FieldId,
                SlotId = slot4_Field2.SlotId,
                BookingDate = today,
                GuestName = "Đội bóng FC Anh Em",
                GuestPhone = "0912345678",
                AgreedPrice = 650000,
                RequiredPrepayment = 195000,
                PaidAmount = 650000,
                BalanceDue = 0,
                Status = BookingStatus.CHECKED_IN,
                SettlementStatus = SettlementStatus.PAID,
                HoldExpiresAt = now.AddMinutes(15),
                CreatedAt = now.AddHours(-2)
            };

            // Đơn 3: Chờ thanh toán đặt cọc (Giữ chỗ 15 phút)
            var b3 = new Booking
            {
                BookingCode = "BK-" + today.ToString("yyyyMMdd") + "-0003",
                FieldId = field1.FieldId,
                SlotId = slot5_Field1.SlotId,
                BookingDate = today,
                AccountId = customer.AccountId,
                AgreedPrice = 380000,
                RequiredPrepayment = 114000,
                PaidAmount = 0,
                BalanceDue = 380000,
                Status = BookingStatus.PENDING_PAYMENT,
                SettlementStatus = SettlementStatus.UNPAID,
                HoldExpiresAt = now.AddMinutes(12),
                CreatedAt = now.AddMinutes(-3)
            };

            context.Bookings.AddRange(b1, b2, b3);
            context.SaveChanges();

            // Giao dịch thanh toán cọc cho đơn 1
            var pay1 = new Payment
            {
                BookingId = b1.BookingId,
                TransactionRef = "TXN-MBB-17982341",
                Amount = 114000,
                Method = PaymentMethod.ONLINE_GATEWAY,
                Channel = PaymentChannel.ONLINE,
                Status = PaymentStatus.SUCCESS,
                ReceiptRef = "Chuyển khoản VietQR thành công",
                PaidAt = now.AddHours(-3)
            };

            // Giao dịch thanh toán đủ tại quầy cho đơn 2
            var pay2 = new Payment
            {
                BookingId = b2.BookingId,
                TransactionRef = "TXN-CASH-8812736",
                Amount = 650000,
                Method = PaymentMethod.CASH,
                Channel = PaymentChannel.DIRECT,
                Status = PaymentStatus.SUCCESS,
                ReceiptRef = "Thu tiền mặt tại quầy lễ tân khi nhận sân",
                PaidAt = now.AddHours(-2)
            };

            context.Payments.AddRange(pay1, pay2);

            // Đơn hủy cần hoàn tiền (Demo hàng đợi hoàn tiền UC38)
            var bCancel = new Booking
            {
                BookingCode = "BK-" + today.AddDays(1).ToString("yyyyMMdd") + "-0099",
                FieldId = field3.FieldId,
                SlotId = allSlots.First(s => s.FieldId == field3.FieldId && s.StartTime == new TimeSpan(17, 0, 0)).SlotId,
                BookingDate = today.AddDays(1),
                AccountId = customer.AccountId,
                AgreedPrice = 1800000,
                RequiredPrepayment = 540000,
                PaidAmount = 540000,
                BalanceDue = 1260000,
                Status = BookingStatus.CANCELLED,
                SettlementStatus = SettlementStatus.PARTIALLY_PAID,
                HoldExpiresAt = now,
                CreatedAt = now.AddDays(-1)
            };

            context.Bookings.Add(bCancel);
            context.SaveChanges();

            var refundRequest = new RefundRequest
            {
                BookingId = bCancel.BookingId,
                RefundCode = "REF-" + today.ToString("yyyyMMdd") + "-001",
                Amount = 540000,
                Reason = "Khách hủy lịch trước 24 tiếng theo đúng chính sách hoàn tiền 100%",
                RefundType = RefundType.CUSTOMER_CANCEL,
                Status = RefundStatus.PENDING,
                CreatedAt = now.AddHours(-1)
            };

            context.RefundRequests.Add(refundRequest);

            // Thông báo gửi tới khách hàng
            var notif = new Notification
            {
                AccountId = customer.AccountId,
                Type = "Xác nhận đặt sân",
                Message = $"Đơn đặt sân {b1.BookingCode} tại {field1.Name} ngày {today:dd/MM/yyyy} đã được xác nhận thành công.",
                IsRead = false,
                CreatedAt = now.AddHours(-3)
            };

            context.Notifications.Add(notif);
            context.SaveChanges();
        }
    }
}
