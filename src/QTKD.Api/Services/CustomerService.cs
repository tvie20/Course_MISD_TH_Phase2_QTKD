using QTKD.Api.DTOs;
using QTKD.Api.Models;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        // Chỉ inject Repository thông qua DI, không gọi DbContext
        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<CustomerResponse>> GetAllAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.Select(MapToResponse);
        }

        public async Task<CustomerResponse?> GetByIdAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            return customer == null ? null : MapToResponse(customer);
        }

        public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request)
        {
            // Kiểm tra trùng mã khách hàng
            var existingCustomer = await _customerRepository.GetByCodeAsync(request.Code);
            if (existingCustomer != null)
            {
                throw new InvalidOperationException($"Mã khách hàng '{request.Code}' đã tồn tại trong hệ thống.");
            }

            // Tạo entity với Status mặc định ban đầu là Active (1)
            var customer = new Customer
            {
                Code = request.Code.Trim().ToUpper(),
                Name = request.Name.Trim(),
                TaxCode = request.TaxCode?.Trim(),
                Address = request.Address?.Trim(),
                Status = CustomerStatus.Active
            };

            var created = await _customerRepository.AddAsync(customer);
            return MapToResponse(created);
        }

        public async Task<bool> UpdateAsync(int id, UpdateCustomerRequest request)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy khách hàng với Id = {id}.");
            }

            customer.Name = request.Name.Trim();
            customer.Status = request.Status;
            customer.TaxCode = request.TaxCode?.Trim();
            customer.Address = request.Address?.Trim();

            await _customerRepository.UpdateAsync(customer);
            return true;
        }

        // Hàm ánh xạ nội bộ Entity -> DTO Response
        private static CustomerResponse MapToResponse(Customer customer)
        {
            return new CustomerResponse
            {
                Id = customer.Id,
                Code = customer.Code,
                Name = customer.Name,
                Status = customer.Status,
                TaxCode = customer.TaxCode,
                Address = customer.Address
            };
        }
    }
}