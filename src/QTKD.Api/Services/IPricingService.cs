using QTKD.Api.DTOs;

namespace QTKD.Api.Services
{
    public interface IPricingService
    {
        Task<IEnumerable<PriceListResponse>> GetAllPriceListsAsync();
        Task<PriceListResponse?> GetPriceListByIdAsync(int id);
        Task<PriceListResponse> CreatePriceListAsync(CreatePriceListRequest request);
        Task<PriceItemResponse> AddPriceItemAsync(int priceListId, CreatePriceItemRequest request);
    }
}