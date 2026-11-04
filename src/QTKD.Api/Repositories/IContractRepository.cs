using QTKD.Api.Models;

namespace QTKD.Api.Repositories
{
    public interface IContractRepository
    {
        Task<Contract?> GetByIdAsync(int id);
        Task<IEnumerable<Contract>> GetAllAsync();
        Task<Contract?> GetByIdWithCustomerAsync(int id);
        Task<Contract> AddAsync(Contract contract);
        Task UpdateAsync(Contract contract);
        Task<bool> ExistsByContractNoAsync(string contractNo);
    }
}
