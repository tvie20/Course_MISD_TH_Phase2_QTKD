using QTKD.Api.Models;

namespace QTKD.Api.Repositories
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer?> GetByCodeAsync(string code); // Tìm khách hàng theo Code, dùng để kiểm tra trùng mã khi tạo mới
        Task<Customer> AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task<bool> ExistsAsync(int id);
    }
}
