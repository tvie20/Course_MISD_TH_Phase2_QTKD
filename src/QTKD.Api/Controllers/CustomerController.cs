using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Repositories;

namespace QTKD.Api.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerRepository.GetAllAsync();
            var response = customers.Select(c => new CustomerResponse
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Status = c.Status,
                TaxCode = c.TaxCode,
                Address = c.Address
            });

            return Ok(response);
        }
    }
}

