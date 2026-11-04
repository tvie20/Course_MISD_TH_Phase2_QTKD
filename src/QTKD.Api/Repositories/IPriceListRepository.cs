using QTKD.Api.Models;

namespace QTKD.Api.Repositories
{
    public interface IPriceListRepository
    {
        Task<IEnumerable<PriceList>> GetAllAsync();
        Task<PriceList?> GetByIdWithItemsAsync(int id);
        Task<PriceList> AddAsync(PriceList priceList);
        Task<PriceItem> AddItemAsync(PriceItem item);
        Task<bool> ExistsAsync(int id);
    }
}
