# HƯỚNG DẪN THỰC HÀNH TUẦN 05 — HỆ THỐNG QTKD
> **Học phần:** Phát triển ứng dụng Hệ thống Thông tin Hiện đại  
> **Phase 2:** Xây dựng hệ thống QTKD
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

## 2. YÊU CẦU MÔI TRƯỜNG TRƯỚC KHI CHẠY (PREREQUISITES)
Trước khi bắt đầu, hãy đảm bảo máy tính của bạn đã cài đặt:
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

## 3. CÁC BƯỚC CÀI ĐẶT VÀ KHỞI ĐỘNG DỰ ÁN

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

## 4. HƯỚNG DẪN THỰC THI 2 ENDPOINTS TRÊN GIAO DIỆN SWAGGER UI

Trên giao diện Swagger UI, bạn sẽ thấy 2 nhóm API: **Customer** và **Contract**. Hãy thực hiện các bước sau để kiểm thử:

---

### 4.1. Endpoint 1: Lấy danh sách khách hàng (`GET /api/customers`)

- **Bước 1: Mở rộng endpoint**  
  Tìm nhóm **Customer**, nhấp chuột vào thanh màu xanh lá mang tên **`GET /api/customers`** để mở rộng khung thao tác.

- **Bước 2: Bật chế độ thử nghiệm**  
  Nhấp vào nút **`Try it out`** nằm ở góc trên bên phải của khung.

- **Bước 3: Thực thi**  
  Nhấp vào nút màu xanh dương **`Execute`** (endpoint này đọc toàn bộ danh sách nên không cần điền tham số).

- **Bước 4: Kiểm tra kết quả phản hồi**  
  Cuộn xuống phần **Responses** ngay bên dưới, quan sát mục **Server response**:
  - **Code:** `200` (Thành công).
  - **Response body:** Hiển thị danh sách khách hàng dạng JSON được truy vấn trực tiếp từ bảng `Customers` trong PostgreSQL:
    ```json
    [
      {
        "id": 1,
        "code": "KH00001",
        "name": "Công ty TNHH Minh Phát",
        "status": 1,
        "statusName": "Active",
        "taxCode": "0912345678",
        "address": "123 Lê Lợi, Quận 1, TP.HCM"
      },
      {
        "id": 2,
        "code": "KH00002",
        "name": "Công ty Cổ phần An Khang",
        "status": 0,
        "statusName": "Inactive",
        "taxCode": "0738297382",
        "address": "456 Nguyễn Trãi, Quận 5, TP.HCM"
      }
    ]
    ```

---

### 4.2. Endpoint 2: Xem chi tiết hợp đồng theo ID (`GET /api/contracts/{id}`)

#### Trường hợp A: Hợp đồng hợp lệ có trong database (ID = 1)
- **Bước 1: Mở rộng endpoint**  
  Tìm nhóm **Contract**, nhấp chuột vào thanh màu xanh lá **`GET /api/contracts/{id}`**.

- **Bước 2: Bật chế độ thử nghiệm**  
  Nhấp vào nút **`Try it out`** ở góc trên bên phải.

- **Bước 3: Nhập ID cần tra cứu**  
  Tại ô nhập liệu của tham số **`id * integer ($int32)`**, nhập giá trị: **`1`**.

- **Bước 4: Thực thi**  
  Nhấp nút **`Execute`**.

- **Bước 5: Kiểm tra kết quả phản hồi**  
  - **Code:** `200`
  - **Response body:** Trả về chi tiết hợp đồng mẫu `HD-2026-018` đang ở trạng thái `Draft` (status = 0) liên kết với Công ty Minh Phát:
    ```json
    {
      "id": 1,
      "contractNo": "HD-2026-018",
      "customerId": 1,
      "customerName": "Công ty TNHH Minh Phát",
      "startDate": "2026-11-01T00:00:00Z",
      "endDate": "2027-10-31T00:00:00Z",
      "status": 0,
      "statusName": "Draft",
      "totalAmount": 45000000.00,
      "attachmentPath": "/attachments/hd-2026-018-draft.pdf",
      "description": "Hợp đồng cung cấp dịch vụ CNTT cho Công ty Minh Phát"
    }
    ```

#### Trường hợp B: Hợp đồng không tồn tại (ID = 999)
- **Bước 1:** Vẫn tại ô nhập **`id`**, xóa số 1 và nhập số **`999`**.
- **Bước 2:** Nhấp nút **`Execute`**.
- **Bước 3: Kiểm tra kết quả phản hồi**  
  - **Code:** `404` (Not Found).
  - **Response body:**
    ```json
    {
      "error": "Không tìm thấy hợp đồng 999."
    }
    ```

---

## 5. ĐỐI CHIẾU DỮ LIỆU VỚI POSTGRESQL

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

## 6. XỬ LÝ SỰ CỐ THƯỜNG GẶP (TROUBLESHOOTING)

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
