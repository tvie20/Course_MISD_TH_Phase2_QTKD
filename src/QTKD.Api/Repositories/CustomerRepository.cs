using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Models;

namespace QTKD.Api.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly QtkdDbContext _context;

        public CustomerRepository(QtkdDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers.ToListAsync();
        }
        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task<Customer?> GetByCodeAsync(string code)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Code == code);
        }
        public async Task<Customer> AddAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            // Lưu thay đổi xuống CSDL, PostgreSQL tự động sinh trường Id tăng dần
            await _context.SaveChangesAsync();
            return customer;
        }
        public async Task UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Customers.AnyAsync(c => c.Id == id);
        }
    }
}
