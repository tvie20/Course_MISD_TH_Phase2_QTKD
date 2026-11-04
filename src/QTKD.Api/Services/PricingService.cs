using QTKD.Api.DTOs;
using QTKD.Api.Models;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services
{
    public class PricingService : IPricingService
    {
        private readonly IPriceListRepository _priceListRepository;

        public PricingService(IPriceListRepository priceListRepository)
        {
            _priceListRepository = priceListRepository;
        }

        public async Task<IEnumerable<PriceListResponse>> GetAllPriceListsAsync()
        {
            var priceLists = await _priceListRepository.GetAllAsync();
            return priceLists.Select(p => new PriceListResponse
            {
                Id = p.Id,
                Name = p.Name,
                EffectiveDate = p.EffectiveDate,
                IsActive = p.IsActive
            });
        }

        public async Task<PriceListResponse?> GetPriceListByIdAsync(int id)
        {
            var priceList = await _priceListRepository.GetByIdWithItemsAsync(id);
            if (priceList == null) return null;

            return new PriceListResponse
            {
                Id = priceList.Id,
                Name = priceList.Name,
                EffectiveDate = priceList.EffectiveDate,
                IsActive = priceList.IsActive,
                Items = priceList.PriceItems.Select(item => new PriceItemResponse
                {
                    Id = item.Id,
                    PriceListId = item.PriceListId,
                    ServiceCode = item.ServiceCode,
                    ServiceName = item.ServiceName,
                    UnitPrice = item.UnitPrice
                }).ToList()
            };
        }

        public async Task<PriceListResponse> CreatePriceListAsync(CreatePriceListRequest request)
        {
            var priceList = new PriceList
            {
                Name = request.Name.Trim(),
                EffectiveDate = DateTime.SpecifyKind(request.EffectiveDate, DateTimeKind.Utc),
                IsActive = request.IsActive
            };

            var created = await _priceListRepository.AddAsync(priceList);
            return new PriceListResponse
            {
                Id = created.Id,
                Name = created.Name,
                EffectiveDate = created.EffectiveDate,
                IsActive = created.IsActive,
                Items = new List<PriceItemResponse>()
            };
        }

        public async Task<PriceItemResponse> AddPriceItemAsync(int priceListId, CreatePriceItemRequest request)
        {
            // Kiểm tra bảng giá có tồn tại không
            var priceListExists = await _priceListRepository.ExistsAsync(priceListId);
            if (!priceListExists)
            {
                throw new KeyNotFoundException($"Không tìm thấy bảng giá với Id = {priceListId}.");
            }

            // Đơn giá hợp lệ (UnitPrice > 0)
            if (request.UnitPrice <= 0)
            {
                throw new InvalidOperationException("Đơn giá dịch vụ phải lớn hơn 0.");
            }

            var item = new PriceItem
            {
                PriceListId = priceListId,
                ServiceCode = request.ServiceCode.Trim().ToUpper(),
                ServiceName = request.ServiceName.Trim(),
                UnitPrice = request.UnitPrice
            };

            var created = await _priceListRepository.AddItemAsync(item);
            return new PriceItemResponse
            {
                Id = created.Id,
                PriceListId = created.PriceListId,
                ServiceCode = created.ServiceCode,
                ServiceName = created.ServiceName,
                UnitPrice = created.UnitPrice
            };
        }
    }
}