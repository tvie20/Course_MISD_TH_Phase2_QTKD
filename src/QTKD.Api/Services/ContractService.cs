using QTKD.Api.DTOs;
using QTKD.Api.Models;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly ICustomerRepository _customerRepository;

        // Dependency Injection: Chỉ nhận Repository, không nhận DbContext
        public ContractService(IContractRepository contractRepository, ICustomerRepository customerRepository)
        {
            _contractRepository = contractRepository;
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<ContractResponse>> GetAllAsync()
        {
            var contracts = await _contractRepository.GetAllAsync();
            return contracts.Select(MapToResponse);
        }

        public async Task<ContractResponse?> GetByIdAsync(int id)
        {
            var contract = await _contractRepository.GetByIdWithCustomerAsync(id);
            return contract == null ? null : MapToResponse(contract);
        }

        // TẠO HỢP ĐỒNG MỚI (Mặc định luôn bắt đầu từ trạng thái Draft)
        public async Task<ContractResponse> CreateAsync(CreateContractRequest request)
        {
            // 1. Kiểm tra khách hàng liên kết có tồn tại hay không
            var customerExists = await _customerRepository.ExistsAsync(request.CustomerId);
            if (!customerExists)
            {
                throw new InvalidOperationException($"Khách hàng với Id = {request.CustomerId} không tồn tại.");
            }

            // 2. Kiểm tra trùng số hợp đồng
            var isDuplicateContractNo = await _contractRepository.ExistsByContractNoAsync(request.ContractNo);
            if (isDuplicateContractNo)
            {
                throw new InvalidOperationException($"Số hợp đồng '{request.ContractNo}' đã tồn tại trong hệ thống.");
            }

            // 3. Khởi tạo hợp đồng: Luôn gán status là Draft
            var contract = new Contract
            {
                ContractNo = request.ContractNo.Trim(),
                CustomerId = request.CustomerId,
                StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc),
                TotalAmount = request.TotalAmount,
                AttachmentPath = string.IsNullOrWhiteSpace(request.AttachmentPath) ? null : request.AttachmentPath.Trim(),
                Description = request.Description?.Trim(),
                Status = ContractStatus.Draft, // Bắt buộc khởi tạo từ Draft
                CreatedAt = DateTime.UtcNow
            };

            var created = await _contractRepository.AddAsync(contract);
            var contractWithCustomer = await _contractRepository.GetByIdWithCustomerAsync(created.Id);
            return MapToResponse(contractWithCustomer ?? created);
        }

        // CẬP NHẬT HỢP ĐỒNG (CTR-01)
        public async Task<bool> UpdateAsync(int id, UpdateContractRequest request)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy hợp đồng {id}.");
            }

            // Chỉ cho sửa hợp đồng ở trạng thái Draft hoặc RevisionRequested
            if (contract.Status != ContractStatus.Draft && contract.Status != ContractStatus.RevisionRequested)
            {
                throw new InvalidOperationException(
                    $"Không thể sửa hợp đồng ở trạng thái '{contract.Status}'. Chỉ cho phép chỉnh sửa khi hợp đồng ở trạng thái Draft hoặc RevisionRequested.");
            }

            contract.StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);
            contract.EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc);
            contract.TotalAmount = request.TotalAmount;
            contract.AttachmentPath = string.IsNullOrWhiteSpace(request.AttachmentPath) ? null : request.AttachmentPath.Trim();
            contract.Description = request.Description?.Trim();

            await _contractRepository.UpdateAsync(contract);
            return true;
        }

        // GỬI HỢP ĐỒNG TRÌNH DUYỆT
        public async Task<ContractResponse> SubmitAsync(int id)
        {
            var contract = await _contractRepository.GetByIdWithCustomerAsync(id);
            if (contract == null)
                throw new KeyNotFoundException($"Không tìm thấy hợp đồng {id}.");

            // 1. Kiểm tra chống submit lặp
            if (contract.Status == ContractStatus.PendingApproval)
            {
                throw new InvalidOperationException("Hợp đồng này đã được gửi trình duyệt trước đó, đang chờ xử lý.");
            }

            // 2. Kiểm tra trạng thái hiện tại (chỉ được gửi từ Draft hoặc RevisionRequested)
            if (contract.Status != ContractStatus.Draft && contract.Status != ContractStatus.RevisionRequested)
            {
                throw new InvalidOperationException($"Không thể gửi hợp đồng đang ở trạng thái '{contract.Status}'.");
            }

            // 3. Kiểm tra customer hợp lệ & active (CTR-02)
            if (contract.Customer == null || contract.Customer.Status != CustomerStatus.Active)
            {
                throw new InvalidOperationException("Khách hàng liên kết không tồn tại hoặc đang ở trạng thái Inactive.");
            }

            // 4. Kiểm tra ngày hiệu lực (StartDate <= EndDate) (CTR-02)
            if (contract.StartDate > contract.EndDate)
            {
                throw new InvalidOperationException("Ngày bắt đầu hiệu lực không được lớn hơn ngày kết thúc hiệu lực.");
            }

            // 5. Kiểm tra tài liệu/tệp bắt buộc (CTR-02)
            if (string.IsNullOrWhiteSpace(contract.AttachmentPath))
            {
                throw new InvalidOperationException("Hợp đồng thiếu tệp tài liệu đính kèm bản thảo bắt buộc.");
            }

            // 6. Submit hợp lệ chỉ chuyển sang PendingApproval, Không chuyển sang Approved (CTR-03)
            contract.Status = ContractStatus.PendingApproval;

            await _contractRepository.UpdateAsync(contract);
            return MapToResponse(contract);
        }

        // Hàm ánh xạ phụ trợ Entity -> DTO Response
        private static ContractResponse MapToResponse(Contract contract)
        {
            return new ContractResponse
            {
                Id = contract.Id,
                ContractNo = contract.ContractNo,
                CustomerId = contract.CustomerId,
                CustomerName = contract.Customer?.Name ?? string.Empty,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = contract.Status,
                TotalAmount = contract.TotalAmount,
                AttachmentPath = contract.AttachmentPath,
                Description = contract.Description
            };
        }
    }
}