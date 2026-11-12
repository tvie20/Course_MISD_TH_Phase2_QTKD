# BÁO CÁO ĐẶC TẢ API & ĐỐI CHIẾU NGHIỆP VỤ QTKD
---

## 1. DANH MỤC TỔNG HỢP ENDPOINTS

### 1.1. Nhóm Customer (`/api/customers`)
| Method | Endpoint | Mô tả | Status Code |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/customers` | Lấy danh sách toàn bộ khách hàng | `200 OK` |
| `GET` | `/api/customers/{id}` | Xem chi tiết khách hàng theo ID | `200 OK`, `404 Not Found` |
| `POST` | `/api/customers` | Tạo mới khách hàng (mặc định `Active = 1`) | `201 Created`, `400 Bad Request` |
| `PUT` | `/api/customers/{id}` | Cập nhật thông tin/trạng thái khách hàng | `200 OK`, `400 Bad Request`, `404 Not Found` |

### 1.2. Nhóm Contract (`/api/contracts`)
| Method | Endpoint | Mô tả | Status Code |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/contracts` | Lấy danh sách hợp đồng | `200 OK` |
| `GET` | `/api/contracts/{id}` | Xem chi tiết hợp đồng theo ID | `200 OK`, `404 Not Found` |
| `POST` | `/api/contracts` | Tạo mới hợp đồng (luôn khởi tạo `Draft`) | `201 Created`, `400 Bad Request` |
| `PUT` | `/api/contracts/{id}` | Cập nhật hợp đồng (áp dụng **CTR-01**) | `200 OK`, `400 Bad Request`, `404 Not Found` |
| `POST` | `/api/contracts/{id}/submit` | **US03:** Gửi hợp đồng trình duyệt (**CTR-02, CTR-03**) | `200 OK`, `400 Bad Request`, `404 Not Found` |

### 1.3. Nhóm Pricing (`/api/pricelists`)
| Method | Endpoint | Mô tả | Status Code |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/pricelists` | Lấy danh sách bảng giá | `200 OK` |
| `GET` | `/api/pricelists/{id}` | Xem chi tiết bảng giá kèm danh sách `Items` | `200 OK`, `404 Not Found` |
| `POST` | `/api/pricelists` | Tạo mới bảng giá | `201 Created`, `400 Bad Request` |
| `POST` | `/api/pricelists/{id}/items` | Thêm dịch vụ vào bảng giá (kiểm tra `UnitPrice > 0`) | `201 Created`, `400 Bad Request`, `404 Not Found` |

---

## 2. REQUEST / RESPONSE MẪU

### 2.1. Tạo Khách hàng (`POST /api/customers`)
**Request:**
```json
{
  "code": "KH00003",
  "name": "Công ty TNHH Phát Đạt",
  "taxCode": "0312345678",
  "address": "789 Cách Mạng Tháng 8, Quận 3, TP.HCM"
}
```
**Response (`201 Created`):**
```json
{
  "id": 3,
  "code": "KH00003",
  "name": "Công ty TNHH Phát Đạt",
  "status": 1,
  "statusName": "Active",
  "taxCode": "0312345678",
  "address": "789 Cách Mạng Tháng 8, Quận 3, TP.HCM"
}
```

### 2.2. Tạo Hợp đồng (`POST /api/contracts`)
**Request:**
```json
{
  "contractNo": "HD-2026-020",
  "customerId": 1,
  "startDate": "2026-11-01T00:00:00Z",
  "endDate": "2027-10-31T00:00:00Z",
  "totalAmount": 50000000,
  "attachmentPath": null,
  "description": "Hợp đồng cung cấp giải pháp phần mềm"
}
```
**Response (`201 Created`):**
```json
{
  "id": 2,
  "contractNo": "HD-2026-020",
  "customerId": 1,
  "customerName": "Công ty TNHH Minh Phát",
  "startDate": "2026-11-01T00:00:00Z",
  "endDate": "2027-10-31T00:00:00Z",
  "status": 0,
  "statusName": "Draft",
  "totalAmount": 50000000,
  "attachmentPath": null,
  "description": "Hợp đồng cung cấp giải pháp phần mềm"
}
```

### 2.3. Gửi Hợp đồng trình duyệt — US03 (`POST /api/contracts/{id}/submit`)
**Response thành công (`200 OK`):**
```json
{
  "message": "Hợp đồng đã được gửi trình duyệt thành công.",
  "data": {
    "id": 2,
    "contractNo": "HD-2026-020",
    "customerId": 1,
    "customerName": "Công ty TNHH Minh Phát",
    "startDate": "2026-11-01T00:00:00Z",
    "endDate": "2027-10-31T00:00:00Z",
    "status": 1,
    "statusName": "PendingApproval",
    "totalAmount": 50000000,
    "attachmentPath": "/attachments/hd-2026-020-draft.pdf",
    "description": "Hợp đồng cung cấp giải pháp phần mềm"
  }
}
```

**Response thất bại khi vi phạm CTR-02 (`400 Bad Request`):**
```json
{
  "error": "Hợp đồng thiếu tệp tài liệu đính kèm bản thảo bắt buộc.",
  "statusCode": 400
}
```

**Response khi gửi lặp lần 2 (`400 Bad Request`):**
```json
{
  "error": "Hợp đồng này đã được gửi trình duyệt trước đó, đang chờ xử lý.",
  "statusCode": 400
}
```

### 2.4. Thêm Dịch vụ vào Bảng giá (`POST /api/pricelists/{id}/items`)
**Request hợp lệ:**
```json
{
  "serviceCode": "SRV00003",
  "serviceName": "Bảo trì hệ thống hàng năm",
  "unitPrice": 12000000
}
```
**Response lỗi khi `unitPrice <= 0` (`400 Bad Request`):**
```json
{
  "errors": {
    "UnitPrice": ["Đơn giá phải lớn hơn 0."]
  }
}
```

---

## 3. BẢNG ĐỐI CHIẾU US03 & QUY TẮC NGHIỆP VỤ (CTR-01 – CTR-03)

| Mã quy tắc | Mô tả yêu cầu | Vị trí xử lý trong mã nguồn | Điều kiện & hành vi xử lý |
| :--- | :--- | :--- | :--- |
| **US03** | Gửi hợp đồng trình duyệt | `ContractController.Submit(int id)` $\rightarrow$ `ContractService.SubmitAsync(int id)` | Tiếp nhận `POST /api/contracts/{id}/submit`, kiểm tra toàn bộ điều kiện trước khi lưu và trả DTO kết quả. |
| **CTR-01** | Chỉ cho sửa hợp đồng ở trạng thái `Draft` hoặc `RevisionRequested` | `ContractService.UpdateAsync(...)`<br>*(File `ContractService.cs`, dòng 77–81)* | Kiểm tra `contract.Status != Draft && contract.Status != RevisionRequested`. Nếu vi phạm, ném `InvalidOperationException` $\rightarrow$ Middleware trả HTTP `400` và dữ liệu không đổi. |
| **CTR-02** | Chỉ gửi khi Customer hợp lệ, ngày hiệu lực đúng, tài liệu bắt buộc đầy đủ | `ContractService.SubmitAsync(...)`<br>*(File `ContractService.cs`, dòng 113–128)* | 1. `contract.Customer == null \|\| Customer.Status != Active`<br>2. `contract.StartDate > contract.EndDate`<br>3. `string.IsNullOrWhiteSpace(contract.AttachmentPath)`<br>Thiếu bất kỳ điều kiện nào $\rightarrow$ ném `InvalidOperationException` (`400`), trạng thái giữ nguyên `Draft`. |
| **CTR-03** | Không được chuyển `Draft` thẳng sang `Approved` | `ContractService.SubmitAsync(...)`<br>*(File `ContractService.cs`, dòng 131)* | Gán duy nhất `contract.Status = ContractStatus.PendingApproval`. Không tồn tại nhánh chuyển sang `Approved`. |
| **Idempotency** | Chống gửi lặp cùng một hợp đồng nhiều lần | `ContractService.SubmitAsync(...)`<br>*(File `ContractService.cs`, dòng 101–104)* | Kiểm tra `if (contract.Status == PendingApproval)` $\rightarrow$ ném `InvalidOperationException` (`400`) *"Hợp đồng này đã được gửi trình duyệt trước đó, đang chờ xử lý."* |

> **Ghi chú giả định & giới hạn phiên bản hiện tại:**  
> - Do Phase 1 chưa định danh chi tiết loại file đính kèm bắt buộc và quy trình phê duyệt (`Approval`), phiên bản hiện tại mô phỏng tài liệu bắt buộc qua trường `AttachmentPath` (chuỗi đường dẫn không rỗng) và dừng quy trình ở trạng thái `PendingApproval`.

