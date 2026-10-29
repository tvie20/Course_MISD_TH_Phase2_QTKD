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
            return await _context.PriceLists
                .Include(p => p.PriceItems)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
