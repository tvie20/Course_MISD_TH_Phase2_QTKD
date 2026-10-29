using QTKD.Api.Models;

namespace QTKD.Api.Repositories
{
    public interface IPriceListRepository
    {
        Task<IEnumerable<PriceList>> GetAllAsync();
        Task<PriceList?> GetByIdWithItemsAsync(int id);
    }
}
