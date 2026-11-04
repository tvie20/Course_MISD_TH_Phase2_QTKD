using QTKD.Api.DTOs;

namespace QTKD.Api.Services
{
    public interface IContractService
    {
        Task<IEnumerable<ContractResponse>> GetAllAsync();
        Task<ContractResponse?> GetByIdAsync(int id);
        Task<ContractResponse> CreateAsync(CreateContractRequest request);
        Task<bool> UpdateAsync(int id, UpdateContractRequest request);
        
        // US03: Gửi hợp đồng trình duyệt
        Task<ContractResponse> SubmitAsync(int id);
    }
}