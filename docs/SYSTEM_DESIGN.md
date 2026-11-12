# BÁO CÁO THIẾT KẾ KIẾN TRÚC HỆ THỐNG QTKD
> **Công nghệ:** ASP.NET Core Web API (.NET 10) · Entity Framework Core · PostgreSQL · xUnit & Moq

---

## 1. TỔNG QUAN KIẾN TRÚC PHÂN TẦNG (N-TIER / LAYERED ARCHITECTURE)

Hệ thống Quản trị kinh doanh (QTKD) được thiết kế theo mô hình kiến trúc phân tầng rõ ràng nhằm **tách biệt trách nhiệm (Separation of Concerns)**, đảm bảo dễ bảo trì, dễ mở rộng và dễ kiểm thử tự động.

```text
Client (Swagger UI / Postman / xUnit HttpClient)
   │
   ▼
GlobalExceptionMiddleware  ──► Bắt lỗi tập trung (400 BadRequest, 404 NotFound, 500)
   │
   ▼
Controllers (CustomerController, ContractController, PricingController)
   │  (Chỉ nhận Request DTO, kiểm tra ModelState, điều phối Service, trả HTTP Status)
   ▼
Services (CustomerService, ContractService, PricingService)
   │  (Thực thi Business Rules: CTR-01, CTR-02, CTR-03, Check trùng mã, UnitPrice > 0)
   ▼
Repositories (CustomerRepository, ContractRepository, PriceListRepository)
   │  (Đóng gói truy vấn EF Core, sử dụng Interface để hỗ trợ Mock trong Unit Test)
   ▼
QtkdDbContext (EF Core Code-First + Npgsql Provider)
   │
   ▼
PostgreSQL (QTKDDb) / InMemory DB (khi chạy Integration Test)
```

> Xem thêm sơ đồ trực quan tại [`architecture.png`](architecture.png) và bảng trách nhiệm chi tiết tại [`ARCHITECTURE.md`](ARCHITECTURE.md).

---

## 2. THIẾT KẾ CƠ SỞ DỮ LIỆU & DOMAIN MODELS

### 2.1. Sơ đồ quan hệ thực thể (ERD)

```text
+------------------+          1      N          +--------------------+
|    Customers     |----------------------------|     Contracts      |
+------------------+   OnDelete: Restrict       +--------------------+
| PK Id            |                            | PK Id              |
|    Code (50)     |                            |    ContractNo (100)|
|    Name (255)    |                            | FK CustomerId      |
|    Status (str)  |                            |    StartDate       |
|    TaxCode       |                            |    EndDate         |
|    Address       |                            |    Status (str)    |
+------------------+                            |    TotalAmount     |
                                                |    AttachmentPath  |
                                                |    Description     |
                                                |    CreatedAt       |
                                                +--------------------+

+------------------+          1      N          +--------------------+
|    PriceLists    |----------------------------|     PriceItems     |
+------------------+   OnDelete: Cascade        +--------------------+
| PK Id            |                            | PK Id              |
|    Name (255)    |                            | FK PriceListId     |
|    EffectiveDate |                            |    ServiceCode (50)|
|    IsActive      |                            |    ServiceName(255)|
+------------------+                            |    UnitPrice(18,2) |
                                                +--------------------+
```

### 2.2. Trạng thái nghiệp vụ
- **`CustomerStatus`**: `Inactive = 0`, `Active = 1`. (Lưu chuỗi trong PostgreSQL qua `.HasConversion<string>()`).
- **`ContractStatus`**: `Draft = 0`, `PendingApproval = 1`, `Approved = 2`, `Rejected = 3`, `RevisionRequested = 4`.

### 2.3. Quy ước toàn vẹn dữ liệu
- **Customer $\rightarrow$ Contract (`OnDelete: Restrict`):** Không cho phép xóa khách hàng nếu đang có hợp đồng tham chiếu.
- **PriceList $\rightarrow$ PriceItem (`OnDelete: Cascade`):** Xóa bảng giá sẽ tự động dọn dẹp các dòng đơn giá con thuộc bảng giá đó.
- **Tỷ lệ tiền tệ (`decimal(18, 2)`):** Sử dụng `HasPrecision(18, 2)` cho `TotalAmount` và `UnitPrice` để tránh sai số dấu phẩy động.

---

## 3. THIẾT KẾ CÁC QUY TẮC THIẾT KẾ BẮT BUỘC

| Nguyên tắc thiết kế | Cách hiện thực trong mã nguồn | Lợi ích |
| :--- | :--- | :--- |
| **Controller độc lập với DB** | Controller chỉ inject `ICustomerService`, `IContractService`, `IPricingService`. Không chứa `DbContext` hay `Repository`. | Tuân thủ Single Responsibility Principle, dễ nâng cấp hoặc thay thế tầng lưu trữ. |
| **Độc lập Entity & DTO** | Toàn bộ dữ liệu vào/ra qua `DTOs/` (`CreateCustomerRequest`, `ContractResponse`,...). Không trả thẳng Entity ra ngoài. | Tránh rò rỉ dữ liệu nhạy cảm, tránh lỗi vòng lặp JSON (circular reference) và mass-assignment. |
| **Xử lý ngoại lệ tập trung** | `GlobalExceptionMiddleware` bắt `InvalidOperationException` $\rightarrow$ HTTP 400, `KeyNotFoundException` $\rightarrow$ HTTP 404. Controller không có `try/catch`. | Mã nguồn Controller gọn gàng, format JSON báo lỗi thống nhất toàn hệ thống. |
| **Validation 2 lớp** | **Lớp 1:** Data Annotations (`[Required]`, `[Range]`) trên DTO.<br>**Lớp 2:** Kiểm tra logic sâu (trùng mã, CTR-01..03) trong Service. | Chặn dữ liệu sai định dạng từ cửa ngõ, bảo toàn tính nhất quán nghiệp vụ trước khi ghi DB. |
| **Repository Pattern & DI** | Định nghĩa cặp Interface/Class cho từng Repository (`ICustomerRepository` / `CustomerRepository`, v.v.) và đăng ký `AddScoped` trong `Program.cs`. | Cho phép dùng **Moq** giả lập Repository trong **Unit Test** (Tuần 07) mà không cần kết nối PostgreSQL. |

---

## 4. THIẾT KẾ KIẾN TRÚC KIỂM THỬ

Hệ thống kiểm thử được xây dựng trong project riêng `tests/QTKD.Tests` tham chiếu tới `src/QTKD.Api`:
1. **Unit Tests (`UnitTests/`):**
   - Dùng **xUnit** + **Moq** giả lập `IContractRepository` và `ICustomerRepository`.
   - Cô lập hoàn toàn Service Layer để kiểm chứng chính xác logic **CTR-01**, **CTR-02**, **CTR-03**, kiểm tra chống gửi lặp (Idempotency) và validation trên DTO (`Validator.TryValidateObject`).
2. **Integration Tests (`IntegrationTests/`):**
   - Dùng `CustomWebApplicationFactory : WebApplicationFactory<Program>` khởi chạy toàn bộ HTTP pipeline thực tế (Middleware $\rightarrow$ Controller $\rightarrow$ Service $\rightarrow$ Repository).
   - Thay thế provider Npgsql/PostgreSQL bằng **EF Core InMemory Database** độc lập (`QTKDTestDb_<Guid>`), cô lập hoàn toàn với dữ liệu thật trong `QTKDDb`.
   - Khai báo `public partial class Program { }` ở cuối `src/QTKD.Api/Program.cs` để cho phép Test Assembly truy cập Entry Point.

