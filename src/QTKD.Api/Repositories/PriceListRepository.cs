using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Models;

namespace QTKD.Api.Repositories
{
    public class PriceListRepository : IPriceListRepository
    {
        private readonly QtkdDbContext _context;

        public PriceListRepository(QtkdDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PriceList>> GetAllAsync()
        {
            return await _context.PriceLists.ToListAsync();
        }

        public async Task<PriceList?> GetByIdWithItemsAsync(int id)
        {
            // Dùng .Include(p => p.PriceItems) để nạp các dòng dịch vụ chi tiết của bảng giá
            return await _context.PriceLists
                .Include(p => p.PriceItems)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PriceList> AddAsync(PriceList priceList)
        {
            _context.PriceLists.Add(priceList);
            await _context.SaveChangesAsync();
            return priceList;
        }

        public async Task<PriceItem> AddItemAsync(PriceItem item)
        {
            _context.PriceItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.PriceLists.AnyAsync(p => p.Id == id);
        }
    }
}