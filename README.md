# XÂY DỰNG HỆ THỐNG QTKD
> **Học phần:** Phát triển ứng dụng hệ thống thông tin hiện đại  
> **Công nghệ:** C# · ASP.NET Core Web API (.NET 10/8) · Entity Framework Core · PostgreSQL

---

## 1. MỤC TIÊU TUẦN 05
- Khởi tạo solution và project ASP.NET Core Web API theo kiến trúc phân tầng rõ ràng (API Controllers, Repositories, Data/Entities).
- Thiết lập kết nối PostgreSQL và ánh xạ các bảng bằng EF Core Code-First (`Customers`, `Contracts`, `PriceLists`, `PriceItems`).
- Tạo migration khởi tạo (`InitialCreate`) và nạp sẵn dữ liệu mẫu (Seed Data) vào PostgreSQL.
- Xây dựng 2 API đọc dữ liệu bắt buộc:
  1. `GET /api/customers`: Lấy danh sách khách hàng từ PostgreSQL.
  2. `GET /api/contracts/{id}`: Xem chi tiết hợp đồng theo ID từ PostgreSQL.
- Thử nghiệm trên Swagger UI và đối chiếu kết quả trả về với dữ liệu thực tế trong database.

---

## 2. MỤC TIÊU TUẦN 06
- Lập trình các API tạo, sửa và xử lý hợp đồng, kiểm tra chặt chẽ các quy tắc nghiệp vụ trước khi lưu (không chỉ làm CRUD đơn thuần).
- **Quản lý Khách hàng (Customer):** Tạo mới, xem danh sách/chi tiết, cập nhật; kiểm tra tính hợp lệ và chống trùng lặp mã khách hàng.
- **Quản lý Hợp đồng (Contract) & Hoàn thiện US03 (Gửi hợp đồng trình duyệt):**
  - Xây dựng endpoint `POST /api/contracts/{id}/submit` xử lý gửi trình duyệt.
  - Thực thi quy tắc **CTR-01:** Chỉ cho phép chỉnh sửa hợp đồng khi đang ở trạng thái `Draft` hoặc `RevisionRequested`; chặn sửa khi ở `PendingApproval`/`Approved`.
  - Thực thi quy tắc **CTR-02:** Chỉ cho gửi duyệt khi Khách hàng tồn tại & `Active`, ngày hiệu lực hợp lệ (`StartDate <= EndDate`), và có đầy đủ tệp tài liệu đính kèm bản thảo bắt buộc. Thiếu một điều kiện thì từ chối và dữ liệu giữ nguyên.
  - Thực thi quy tắc **CTR-03:** Không được chuyển `Draft` thẳng sang `Approved`. Submit thành công chỉ chuyển sang trạng thái `PendingApproval`.
  - **Chống gửi lặp (Idempotency):** Từ chối gửi duyệt nhiều lần đối với hợp đồng đang ở `PendingApproval`.
- **Quản lý Bảng giá (Pricing):** Tạo và xem PriceList; thêm và xem PriceItem; kiểm tra đơn giá dịch vụ hợp lệ (`UnitPrice > 0`) và liên kết PriceItem–PriceList chính xác.
- **Tuân thủ chuẩn kiến trúc phân tầng:** 
  - `Controller` (0 DbContext, 0 Repository, 0 try/catch) $\rightarrow$ `Service` (xử lý nghiệp vụ & ném Exception) $\rightarrow$ `Repository` (đóng gói data access) qua Dependency Injection.
  - Dùng DTOs độc lập cho Request/Response thay vì để API phụ thuộc vào Entity CSDL.
  - Xử lý ngoại lệ tập trung qua `GlobalExceptionMiddleware` (400 cho lỗi nghiệp vụ, 404 cho NotFound).

---

## 3. YÊU CẦU MÔI TRƯỜNG TRƯỚC KHI CHẠY (PREREQUISITES)
Trước khi bắt đầu, hãy đảm bảo máy tính đã cài đặt:
1. **.NET SDK**: Phiên bản .NET 10.0 (hoặc .NET 8.0/9.0 tương thích).  
   *Kiểm tra:* `dotnet --version`
2. **PostgreSQL**: Phiên bản 14 trở lên đang chạy service trên máy (Port mặc định: `5432`).  
   *Kiểm tra:* Mở pgAdmin hoặc lệnh `psql -U postgres`.
3. **Công cụ dòng lệnh `dotnet-ef`**:  
   *Cài đặt nếu máy chưa có:*
   ```bash
   dotnet tool install --global dotnet-ef
   ```

---

## 4. CÁC BƯỚC CÀI ĐẶT VÀ KHỞI ĐỘNG DỰ ÁN

### Bước 1: Mở Terminal tại thư mục project
Mở Terminal/PowerShell và điều hướng tới thư mục chứa project Web API:
```bash
cd "src/QTKD.Api"
```

---

### Bước 2: Cấu hình kết nối Database PostgreSQL
1. Mở file `appsettings.json`.
2. Chỉnh sửa mật khẩu tài khoản PostgreSQL của bạn tại mục `ConnectionStrings:DefaultConnection`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=QTKDDb;Username=postgres;Password=MAT_KHAU_POSTGRES_CUA_BAN"
  }
}
```

> **Lưu ý:** 
> - Thay `MAT_KHAU_POSTGRES_CUA_BAN` bằng mật khẩu người dùng `postgres` trên máy bạn.
> - Database `QTKDDb` chưa cần tạo thủ công, EF Core sẽ tự động tạo ở bước tiếp theo.

---

### Bước 3: Biên dịch dự án (Build Project)
Chạy lệnh kiểm tra và biên dịch mã nguồn:
```bash
dotnet build
```

**Kết quả mong đợi:**
- Lệnh sẽ tự động restore các NuGet packages cần thiết (`Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Design`, v.v.).
- Terminal hiển thị thông báo biên dịch thành công:
  ```text
  Build succeeded.
      0 Warning(s)
      0 Error(s)
  ```

---

### Bước 4: Chạy Migration để tạo Database và nạp dữ liệu mẫu
Thực thi lệnh sau:
```bash
dotnet ef database update
```

**Kết quả mong đợi:**
- Terminal thông báo `Build succeeded` $\rightarrow$ `Applying migration '20261028092934_InitialCreate'` $\rightarrow$ **`Done.`**
- Database `QTKDDb` được tự động tạo với 4 bảng và nạp sẵn:
  - 02 Khách hàng (`KH00001` - Công ty TNHH Minh Phát, `KH00002` - Công ty Cổ phần An Khang).
  - 01 Hợp đồng (`HD-2026-018` trạng thái `Draft` ứng với Khách hàng 1).
  - 01 Bảng giá (`Bảng giá dịch vụ 2026`) kèm 02 Dịch vụ chi tiết (`SRV00001`, `SRV00002`).

---

### Bước 5: Khởi động ứng dụng Web API
Chạy lệnh:
```bash
dotnet run
```

Sau khi khởi động, console sẽ hiển thị đường dẫn của ứng dụng:
```text
Now listening on: http://localhost:5174
Now listening on: https://localhost:7015
```

Mở trình duyệt truy cập vào giao diện **Swagger UI**:  
👉 **`http://localhost:5174/swagger`** *(hoặc `https://localhost:7015/swagger`)*

---

## 5. HƯỚNG DẪN THỰC THI ENDPOINTS TRÊN GIAO DIỆN SWAGGER UI

Trên giao diện Swagger UI có 3 nhóm API hoàn chỉnh: **Customer**, **Contract** và **Pricing**.

---

### 5.1. Nhóm Customer: Đọc danh sách (`GET /api/customers`)
- Mở rộng endpoint **`GET /api/customers`** $\rightarrow$ Nhấp **`Try it out`** $\rightarrow$ Nhấp **`Execute`**.
- Quan sát **Code: 200** và danh sách khách hàng JSON đọc trực tiếp từ PostgreSQL.

---

### 5.2. Nhóm Contract: Xem chi tiết theo ID (`GET /api/contracts/{id}`)
- Mở rộng endpoint **`GET /api/contracts/{id}`** $\rightarrow$ Nhấp **`Try it out`**.
- Nhập `id = 1` $\rightarrow$ Nhấp **`Execute`** $\rightarrow$ Nhận **Code: 200** với hợp đồng `HD-2026-018` ở trạng thái `Draft`.
- Thử nhập `id = 999` $\rightarrow$ Nhấp **`Execute`** $\rightarrow$ Nhận **Code: 404 Not Found** ("Không tìm thấy hợp đồng với Id = 999.").

---

### 5.3. Nhóm Contract: Gửi trình duyệt US03 (`POST /api/contracts/{id}/submit`)
- Mở rộng endpoint **`POST /api/contracts/{id}/submit`** $\rightarrow$ Nhấp **`Try it out`**.
- Nhập `id = 1` $\rightarrow$ Nhấp **`Execute`**.
- Hệ thống thẩm định quy tắc **CTR-02** và chuyển trạng thái sang **PendingApproval** (thỏa mãn quy tắc **CTR-03**).
- Thử submit lần 2 $\rightarrow$ Nhận **Code: 400 Bad Request** ("Hợp đồng này đã được gửi trình duyệt trước đó...").

---

### 5.4. Nhóm Pricing: Quản lý bảng giá & dịch vụ
- `GET /api/pricelists/{id}`: Xem chi tiết bảng giá kèm danh sách mặt hàng (`PriceItems`).
- `POST /api/pricelists/{id}/items`: Thêm mặt hàng dịch vụ (kiểm tra chặn lỗi nếu đơn giá $\le 0$).

---

## 6. ĐỐI CHIẾU DỮ LIỆU VỚI POSTGRESQL

Mở **pgAdmin** hoặc công cụ quản lý database, kết nối vào database `QTKDDb` và mở **Query Tool** để chạy các câu lệnh SQL đối chiếu với kết quả trả về trên Swagger:

```sql
-- 1. Xem dữ liệu bảng Customers
SELECT * FROM "Customers";

-- 2. Xem dữ liệu bảng Contracts (liên kết với Customer)
SELECT * FROM "Contracts";

-- 3. Xem bảng giá và chi tiết bảng giá
SELECT * FROM "PriceLists";
SELECT * FROM "PriceItems";
```

*Lưu ý:* Bắt buộc phải để tên bảng trong cặp dấu ngoặc kép `""` để PostgreSQL phân biệt chữ in hoa/thường theo đúng schema của Entity Framework Core.

---

## 7. XỬ LÝ SỰ CỐ THƯỜNG GẶP (TROUBLESHOOTING)

1. **Lỗi: `Failed to connect to 127.0.0.1:5432`**:
   - Kiểm tra xem PostgreSQL Service đã bật chưa (vào `services.msc` trên Windows $\rightarrow$ kiểm tra service `postgresql-x64-xx` đang chạy).
   - Kiểm tra lại username, port và password trong `appsettings.json`.

2. **Lỗi: `relation "Customers" already exists`**:
   - Xảy ra khi database cũ đã có bảng nhưng lịch sử migration chưa đồng bộ.
   - Khắc phục nhanh:
     ```bash
     dotnet ef database drop
     dotnet ef database update
     ```

3. **Lỗi: `PendingModelChangesWarning`**:
   - Trong EF Core 9/.NET 10, dự án đã được cấu hình tự động bỏ qua cảnh báo này và đồng bộ mô hình trong `QtkdDbContext.cs`, đảm bảo lệnh `dotnet ef database update` luôn chạy thành công.

---

## 8. CẤU TRÚC THƯ MỤC DỰ ÁN (PROJECT STRUCTURE)

```text
QTKD/
├── .gitignore                          # Bỏ qua các file build (bin, obj), cache IDE và bảo mật chuỗi kết nối
├── README.md                           # Hướng dẫn cài đặt, khởi chạy, kiểm thử và cấu trúc solution
├── docs/                               # Chứa tài liệu thiết kế, sơ đồ kiến trúc, minh chứng kiểm thử
│   ├── ARCHITECTURE.md                 # Tài liệu mô tả trách nhiệm từng thành phần kiến trúc
│   ├── architecture.png                # Sơ đồ kiến trúc phân tầng 9 layer
│   └── VIDEO_DEMO_SCRIPT_TUAN06.md     # Kịch bản quay video demo nghiệm thu Tuần 06
├── tests/                              # Thư mục chứa project kiểm thử xUnit (triển khai ở Tuần 07)
│
└── src/
    └── QTKD.Api/                       # Project ASP.NET Core Web API chính
        ├── Controllers/                # Tầng Controller tiếp nhận HTTP Request & trả HTTP Status Codes
        │   ├── ContractController.cs   # API Hợp đồng (GET, POST, PUT, POST submit US03)
        │   ├── CustomerController.cs   # API Khách hàng (GET, POST, PUT)
        │   └── PricingController.cs    # API Bảng giá & Dịch vụ (GET, POST)
        │
        ├── Services/                   # Tầng Nghiệp vụ (Business Logic Layer)
        │   ├── ICustomerService.cs     & CustomerService.cs (Check mã trùng, kiểm tra hợp lệ)
        │   ├── IContractService.cs     & ContractService.cs (Cài đặt CTR-01, CTR-02, CTR-03, chống gửi lặp)
        │   └── IPricingService.cs      & PricingService.cs  (Check UnitPrice > 0, PriceList tồn tại)
        │
        ├── Repositories/               # Tầng trừu tượng hóa truy cập CSDL (Repository Pattern)
        │   ├── ICustomerRepository.cs  & CustomerRepository.cs
        │   ├── IContractRepository.cs  & ContractRepository.cs (kèm .Include(c => c.Customer))
        │   └── IPriceListRepository.cs & PriceListRepository.cs (kèm .Include(p => p.PriceItems))
        │
        ├── Data/                       # Tầng truy xuất dữ liệu & ORM
        │   └── QTKDDbContext.cs        # DbContext cấu hình quan hệ 1-N và nạp Seed Data vào PostgreSQL
        │
        ├── DTOs/                       # Data Transfer Objects tách biệt thực thể DB với dữ liệu API
        │   ├── ContractDtos.cs         # Request & Response DTOs của Hợp đồng
        │   ├── CustomerDtos.cs         # Request & Response DTOs của Khách hàng
        │   └── PricingDtos.cs          # Request & Response DTOs của Bảng giá & Dịch vụ
        │
        ├── Middleware/                 # Pipeline xử lý trung gian
        │   └── GlobalExceptionMiddleware.cs  # Bắt ngoại lệ tập trung (400 lỗi nghiệp vụ, 404 NotFound)
        │
        ├── Migrations/                 # Lịch sử EF Core Code-First Migrations
        │   ├── 20261028092934_InitialCreate.cs
        │   ├── 20261028092934_InitialCreate.Designer.cs
        │   └── QtkdDbContextModelSnapshot.cs
        │
        ├── Models/                     # Tầng thực thể nghiệp vụ (Domain Entities & Enums)
        │   ├── Customer.cs             # Thực thể Khách hàng
        │   ├── CustomerStatus.cs       # Enum trạng thái Khách hàng (Inactive = 0, Active = 1)
        │   ├── Contract.cs             # Thực thể Hợp đồng
        │   ├── ContractStatus.cs       # Enum trạng thái Hợp đồng (Draft, PendingApproval, RevisionRequested)
        │   ├── PriceList.cs            # Thực thể Bảng giá
        │   └── PriceItem.cs            # Thực thể Chi tiết đơn giá dịch vụ
        │
        ├── Properties/
        │   └── launchSettings.json     # Cấu hình cổng chạy ứng dụng (HTTP 5174, HTTPS 7015)
        │
        ├── appsettings.json            # Chuỗi kết nối PostgreSQL (không lưu mật khẩu thật)
        ├── appsettings.Development.json
        ├── Program.cs                  # Điểm khởi đầu ứng dụng, cấu hình DI, Swagger và Middleware
        ├── QTKD.Api.csproj             # File cấu hình project và các NuGet dependencies
        └── QTKD.Api.http               # File kịch bản gọi API nhanh trong VS Code / Visual Studio
```
