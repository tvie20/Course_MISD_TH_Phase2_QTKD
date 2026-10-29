using QTKD.Api.Models;

namespace QTKD.Api.Repositories
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllAsync();
    }
}
