# SPORT ARENA - Hệ Thống Quản Lý & Đặt Sân Bóng Đá

Hệ thống quản lý và đặt sân bóng đá thương mại chuẩn MVC, được xây dựng theo tài liệu đặc tả phân tích **Analysis Model v2 (SWD392)**.

---

## 🚀 Hướng Dẫn Chạy Nhanh Dự Án (Quick Start)

Dự án được cấu hình **Zero-Configuration** (tự động khởi tạo cơ sở dữ liệu và dữ liệu mẫu khi chạy, không cần cài đặt SQL Server hay chạy lệnh migration thủ công).

### Cách 1: Chạy bằng Visual Studio 2022
1. Clone dự án về máy:
   ```bash
   git clone https://github.com/camvip2k4/FootballPitchManagement.git
   ```
2. Mở file giải pháp **`FootballPitchManagement.sln`** bằng Visual Studio 2022.
3. Nhấn **F5** (hoặc `Ctrl + F5`), trình duyệt sẽ tự động mở lên tại `https://localhost:7132` (hoặc `http://localhost:5127`).

### Cách 2: Chạy bằng dòng lệnh (Terminal / CLI)
```bash
git clone https://github.com/camvip2k4/FootballPitchManagement.git
cd FootballPitchManagement/FootballPitchManagement
dotnet run --launch-profile https
```

---

## 🔑 Tài Khoản Mẫu Để Kiểm Thử (Demo Accounts)

Hệ thống có sẵn **nút đăng nhập nhanh 1-Click** ngay tại trang `/Account/Login` cho 4 vai trò:

| Vai trò | Tài khoản | Mật khẩu | Quyền hạn & Chức năng chính |
| :--- | :--- | :--- | :--- |
| **Khách Hàng** | `customer` | `123456` | Đặt sân online (UC10), giữ chỗ 15 phút (POL-02/UC14), quét VietQR giả lập (UC12), xem lịch sử đặt sân (UC17), yêu cầu hủy sân hoàn tiền (UC16). |
| **Lễ Tân Trực Sân** | `staff` | `123456` | Bàn điều độ lễ tân (`/Staff`), tiếp nhận khách vãng lai (UC20), Check-in nhận sân (UC22), bán nước/dịch vụ phụ (UC26), Check-out kiểm tra phụ thu tài sản & thu tiền mặt (UC23/UC24). |
| **Quản Lý Cơ Sở** | `manager` | `123456` | Báo cáo doanh thu & KPI (`/Admin/Dashboard` - UC34), cấu hình bảng giá động (UC31), duyệt hàng đợi hoàn tiền (`/Staff/RefundQueue` - UC38), kích hoạt hủy do sự cố/mưa bão hoàn cọc 100% (UC39). |
| **Quản Trị Viên** | `admin` | `123456` | Toàn quyền hệ thống + Quản lý người dùng, phân quyền tài khoản (`/Admin/AccountManagement` - UC06/UC07/UC08). |

---

## 🛠️ Công Nghệ Sử Dụng

* **Nền tảng:** .NET 8.0 (C# 12, ASP.NET Core MVC)
* **ORM:** Entity Framework Core 8.0 (Code-First)
* **Cơ sở dữ liệu:** 
  * Mặc định: **SQLite** (Chạy độc lập, không cần cài đặt phần mềm CSDL)
  * Tùy chọn: **SQL Server / LocalDB** (Có thể chuyển đổi trong `appsettings.json`)
* **Giao diện:** Bootstrap 5, Bootstrap Icons, Responsive UI, Hỗ trợ 100% Tiếng Việt
* **Bảo mật:** Cookie Authentication, Claims-based Authorization, BCrypt Password Hashing, AntiForgeryToken
* **Chạy ngầm (Background Service):** Quét giải phóng các ca đặt quá hạn 15 phút chưa thanh toán (UC40 / System Timer)

---

## 📁 Cấu Trúc Dự Án

```text
FootballPitchManagement/
├── Areas/
│   └── Admin/
│       ├── Controllers/      # DashboardController, FieldManagementController, PricingManagementController, PolicyManagementController, FacilityCancelController, AccountManagementController
│       └── Views/            # Giao diện quản trị, báo cáo KPI, bảng giá động, phân quyền
├── Controllers/              # HomeController, FieldController, BookingController, StaffController, AccountController
├── Data/
│   ├── ApplicationDbContext.cs   # DbContext cấu hình quan hệ bảng
│   └── DbInitializer.cs          # Khởi tạo CSDL, 8 chính sách POL, tài khoản mẫu và các ca thi đấu
├── Models/
│   ├── Entities/             # FootballField, TimeSlot, Booking, Payment, RefundRequest, BusinessPolicy, Account,...
│   ├── Enums/                # BookingStatus, PaymentStatus, RoleType, FieldType, TimeTier,...
│   └── ViewModels/           # ViewModels truyền dữ liệu cho các View
├── Services/                 # IBookingService, IPricingService, IAvailabilityChecker, HoldExpiryBackgroundService,...
└── Views/                    # Shared (_Layout), Home, Field, Booking, Staff, Account
```
