# BÁO CÁO MINH CHỨNG KIỂM THỬ TUẦN 07 — HỆ THỐNG QTKD
---

## 1. KẾT QUẢ THỰC THI `dotnet test` VÀ MINH CHỨNG TRX

### 1.1. Lệnh đã thực thi
```powershell
dotnet test tests/QTKD.Tests/QTKD.Tests.csproj --verbosity normal --logger "trx;LogFileName=TestResults_Tuan07.trx" --results-directory docs/test-results
```

### 1.2. Kết quả tổng hợp (trích xuất từ file TRX thực tế)
- **File minh chứng TRX:** [`docs/test-results/TestResults_Tuan07.trx`](test-results/TestResults_Tuan07.trx)
- **Thời gian chạy:** `1.9488 giây`
- **Tổng số test:** `26`
- **Passed:** `26` (**100%**)
- **Failed:** `0`
- **Skipped:** `0`

```text
Test Run Successful.
Total tests: 26
     Passed: 26
 Total time: 1.9488 Seconds
Build succeeded.
    0 Error(s)
```

---

## 2. BẢNG TỔNG HỢP KẾT QUẢ TEST CASE (TC01–TC08)

Tất cả 8 TC kiểm thử bắt buộc **TC01–TC08 đều đã được tự động hóa 100%** bằng cả Unit Test (`Moq`) và Integration Test (`WebApplicationFactory` + `EF Core InMemory`), **không có TC nào phải chạy thủ công thay thế**.

| Mã TC | Mô tả kịch bản kiểm thử | Hình thức | Tên Test Case Tự Động tương ứng | Kết quả |
| :---: | :--- | :---: | :--- | :---: |
| **TC01** | Tạo Customer hợp lệ, tạo được và đọc lại từ DB | Tự động (Unit + Integration) | • `CustomerServiceTests.CreateAsync_ValidRequest_ReturnsCustomerResponse`<br>• `CustomerServiceTests.GetByIdAsync_ExistingCustomer_ReturnsCustomerResponse`<br>• `ContractApiIntegrationTests.CreateCustomer_ValidRequest_Returns201AndCanBeReadBack` | **PASS** |
| **TC02** | Tạo Customer thiếu thông tin bắt buộc $\rightarrow$ bị từ chối có thông báo lỗi | Tự động (Unit + Integration) | • `CustomerServiceTests.CreateCustomerRequest_MissingCode_FailsValidation`<br>• `CustomerServiceTests.CreateCustomerRequest_MissingName_FailsValidation`<br>• `ContractApiIntegrationTests.CreateCustomer_MissingRequiredFields_Returns400` | **PASS** |
| **TC03** | Sửa Contract ở trạng thái `Draft` $\rightarrow$ thành công khi dữ liệu hợp lệ (**CTR-01**) | Tự động (Unit + Integration) | • `ContractServiceTests.UpdateAsync_ContractIsDraft_ReturnsTrue`<br>• `ContractServiceTests.UpdateAsync_ContractIsRevisionRequested_ReturnsTrue`<br>• `ContractApiIntegrationTests.UpdateContract_WhenDraft_Returns200` | **PASS** |
| **TC04** | Sửa Contract ở `PendingApproval` $\rightarrow$ bị từ chối, dữ liệu giữ nguyên (**CTR-01**) | Tự động (Unit + Integration) | • `ContractServiceTests.UpdateAsync_ContractIsPendingApproval_ThrowsInvalidOperationException`<br>• `ContractServiceTests.UpdateAsync_ContractIsApproved_ThrowsInvalidOperationException`<br>• `ContractApiIntegrationTests.UpdateContract_WhenPendingApproval_Returns400AndDataUnchanged` | **PASS** |
| **TC05** | Submit Contract thiếu điều kiện **CTR-02** $\rightarrow$ bị từ chối, vẫn giữ `Draft` | Tự động (Unit + Integration) | • `ContractServiceTests.SubmitAsync_MissingAttachmentPath_ThrowsInvalidOperationException`<br>• `ContractServiceTests.SubmitAsync_CustomerInactive_ThrowsInvalidOperationException`<br>• `ContractServiceTests.SubmitAsync_StartDateAfterEndDate_ThrowsInvalidOperationException`<br>• `ContractApiIntegrationTests.SubmitContract_MissingAttachment_Returns400AndRemainsAsDraft` | **PASS** |
| **TC06** | Submit Contract hợp lệ $\rightarrow$ chuyển sang `PendingApproval` | Tự động (Unit + Integration) | • `ContractServiceTests.SubmitAsync_ValidContract_ChangesToPendingApproval`<br>• `ContractApiIntegrationTests.SubmitContract_ValidContract_Returns200AndStatusIsPendingApproval` | **PASS** |
| **TC07** | Thử chuyển `Draft` thẳng sang `Approved` $\rightarrow$ bị từ chối (**CTR-03**) | Tự động (Unit + Integration) | • `ContractServiceTests.SubmitAsync_ValidContract_NeverSetApproved`<br>• `ContractApiIntegrationTests.SubmitContract_ValidContract_NeverSetsApproved` | **PASS** |
| **TC08** | Submit lặp một Contract $\rightarrow$ bị từ chối, không tạo kết quả xử lý trùng | Tự động (Unit + Integration) | • `ContractServiceTests.SubmitAsync_AlreadyPendingApproval_ThrowsInvalidOperationException`<br>• `ContractApiIntegrationTests.SubmitContract_SubmitTwice_Returns400OnSecondSubmit` | **PASS** |

*Ghi chú: Ngoài 8 TC chính (20 test), bộ test còn có các TC bổ trợ như chống trùng mã khách hàng (`CreateAsync_DuplicateCode`), kiểm tra DTO hợp lệ (`BothCodeAndName_PassesValidation`), kiểm tra trả 404 khi không tìm thấy (`GetByIdAsync_NotFound`, `SubmitAsync_ContractNotFound`). Do đó tổng số TC tự động là 26/26.*

---

## 3. GHI NHẬN CHI TIẾT CÁC LỖI ĐÃ TÌM THẤY VÀ KHẮC PHỤC

Trong quá trình thiết lập và chạy kiểm thử tự động tuần 07, em đã phát hiện và xử lý **4 lỗi kỹ thuật**:

### 3.1. Lỗi 1: Thiếu khai báo `public partial class Program { }` trong `src/QTKD.Api/Program.cs`

| Hạng mục | Nội dung chi tiết |
| :--- | :--- |
| **Mô tả lỗi** | Project Integration Test (`QTKD.Tests`) không thể khởi tạo Test Server thông qua `WebApplicationFactory<Program>`. |
| **Cách tái hiện** | 1. Tạo `CustomWebApplicationFactory : WebApplicationFactory<Program>` trong `tests/QTKD.Tests/IntegrationTests/`.<br>2. Biên dịch project test bằng lệnh `dotnet build` hoặc `dotnet test`.<br>3. Trình biên dịch báo lỗi biên dịch: `error CS0122: 'Program' is inaccessible due to its protection level` hoặc `CS0246: The type or namespace name 'Program' could not be found`. |
| **Nguyên nhân** | - Dự án sử dụng cú pháp **Top-level statements** của ASP.NET Core (.NET 8/10) trong `src/QTKD.Api/Program.cs` (không viết tường minh `namespace` và `public class Program`).<br>- Khi biên dịch, C# Compiler tự động sinh ra một lớp `Program` ngầm nhưng với mức độ truy cập mặc định là **`internal`**.<br>- Vì `QTKD.Tests` là một Assembly tách biệt hoàn toàn với `QTKD.Api`, lớp `internal Program` không thể nhìn thấy từ bên ngoài, dẫn đến `WebApplicationFactory<Program>` không thể truy cập làm Entry Point khởi chạy host test. |
| **Khắc phục** | Bổ sung khai báo tường minh ở dòng cuối cùng của file `src/QTKD.Api/Program.cs`:<br>```csharp<br>app.MapControllers();<br>app.Run();<br><br>public partial class Program { }<br>```<br>Từ khóa `partial` kết hợp `public` mở rộng phạm vi truy cập của lớp `Program` tự sinh từ `internal` thành `public` mà không phá vỡ luồng top-level statements hiện có. |

---

### 3.2. Lỗi 2: Circular Dependency trong `src/QTKD.Api/QTKD.Api.csproj`

- **Cách tái hiện:** Chạy `dotnet build` tại thư mục `src/QTKD.Api` $\rightarrow$ phát sinh lỗi `MSB4006: There is a circular dependency in the target dependency graph involving target "_GenerateRestoreProjectPathWalk"`.
- **Nguyên nhân:** File `QTKD.Api.csproj` bị ghi nhầm cấu hình `<ProjectReference Include="..\..\src\QTKD.Api\QTKD.Api.csproj" />` tự tham chiếu chính mình cùng các gói của test project.
- **Khắc phục:** Khôi phục lại đúng file cấu hình Web API cho `QTKD.Api.csproj`, dọn thư mục cache `bin/` và `obj/`.

---

### 3.3. Lỗi 3: Xung đột phiên bản EF Core khi cài `Microsoft.EntityFrameworkCore.InMemory`

- **Cách tái hiện:** Sau khi thêm `Microsoft.EntityFrameworkCore.InMemory` bản `10.0.0`, chạy `dotnet test` báo lỗi biên dịch `CS1705: Assembly 'QTKD.Api' uses 'Microsoft.EntityFrameworkCore, Version=10.0.14.0' which has a higher version than referenced assembly 'Microsoft.EntityFrameworkCore' with identity 'Microsoft.EntityFrameworkCore, Version=10.0.4.0'`.
- **Nguyên nhân:** Project chính `QTKD.Api` dùng `Microsoft.EntityFrameworkCore.Design` bản `10.0.14`, trong khi project test kéo bản `10.0.0` (tương ứng EF Core `10.0.4`).
- **Cách khắc phục:** Nâng cấp `Microsoft.AspNetCore.Mvc.Testing` và `Microsoft.EntityFrameworkCore.InMemory` trong `QTKD.Tests.csproj` lên đồng bộ phiên bản **`10.0.14`**.

---

### 3.4. Lỗi 4: Xung đột hai Database Provider (`Npgsql` và `InMemory`) trong .NET 10 Integration Test

- **Cách tái hiện:** Chạy `dotnet test` lần đầu (sau khi build thành công): 18 Unit Test Pass nhưng toàn bộ 8 Integration Test Fail với ngoại lệ: `System.InvalidOperationException: Services for database providers 'Npgsql.EntityFrameworkCore.PostgreSQL', 'Microsoft.EntityFrameworkCore.InMemory' have been registered in the service provider.`
- **Nguyên nhân:** Trong EF Core 9/.NET 10, gọi `AddDbContext<QtkdDbContext>` trong `Program.cs` còn đăng ký thêm dịch vụ `IDbContextOptionsConfiguration<QtkdDbContext>`. Nếu trong `CustomWebApplicationFactory` chỉ xóa `DbContextOptions<QtkdDbContext>`, cấu hình `UseNpgsql` vẫn tồn tại song song với `UseInMemoryDatabase`. Ngoài ra, việc đặt `Guid.NewGuid()` bên trong lambda của `AddDbContext` có thể tạo DB tên mới cho mỗi scope HTTP request.
- **Khắc phục:** Xóa toàn bộ service descriptors liên quan đến `QtkdDbContext` (bao gồm các generic type chứa `QtkdDbContext` như `IDbContextOptionsConfiguration<QtkdDbContext>`) trước khi gọi `AddDbContext` với tên InMemory DB khởi tạo cố định 1 lần cho mỗi instance `CustomWebApplicationFactory`.

---

## 4. TÌNH TRẠNG LỖI CÒN TỒN TẠI
- **Số lỗi còn tồn tại:** **0 lỗi**.
- Toàn bộ kịch bản luồng dữ liệu Tuần 06 (Customer $\rightarrow$ Contract Draft $\rightarrow$ Submit thiếu tệp [Thất bại] $\rightarrow$ Cập nhật bổ sung [Thành công] $\rightarrow$ Submit lại [Thành công `PendingApproval`] $\rightarrow$ Submit lặp [Thất bại] $\rightarrow$ Sửa khi `PendingApproval` [Thất bại]) đã được kiểm chứng hoạt động ổn định cả qua bộ test tự động (`26/26 Passed`) lẫn thử nghiệm trực tiếp trên Swagger UI và đối chiếu PostgreSQL.

