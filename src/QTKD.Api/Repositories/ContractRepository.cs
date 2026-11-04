using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Models;

namespace QTKD.Api.Repositories
{
    public class ContractRepository : IContractRepository
    {
        private readonly QtkdDbContext _context;

        public ContractRepository(QtkdDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Contract>> GetAllAsync()
        {
            // Dùng .Include(c => c.Customer) để lấy sẵn tên khách hàng hiển thị lên danh sách
            return await _context.Contracts
                .Include(c => c.Customer)
                .OrderByDescending(c => c.Id)
                .ToListAsync();
        }

        public async Task<Contract?> GetByIdAsync(int id)
        {
            return await _context.Contracts.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Contract?> GetByIdWithCustomerAsync(int id)
        {
            // Kỹ thuật Eager Loading: Tải hợp đồng đồng thời JOIN sang bảng Customers
            // Giúp Service kiểm tra: contract.Customer.Status == CustomerStatus.Active (CTR-02)
            return await _context.Contracts
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Contract> AddAsync(Contract contract)
        {
            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task UpdateAsync(Contract contract)
        {
            _context.Contracts.Update(contract);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByContractNoAsync(string contractNo)
        {
            return await _context.Contracts.AnyAsync(c => c.ContractNo == contractNo);
        }
    }
}