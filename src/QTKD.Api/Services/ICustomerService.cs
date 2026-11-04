using QTKD.Api.DTOs;

namespace QTKD.Api.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerResponse>> GetAllAsync();
        Task<CustomerResponse?> GetByIdAsync(int id);
        Task<CustomerResponse> CreateAsync(CreateCustomerRequest request);
        Task<bool> UpdateAsync(int id, UpdateCustomerRequest request);
    }
}