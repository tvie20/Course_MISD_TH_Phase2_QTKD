using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        // Chỉ inject Service, không inject Repository hay DbContext
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // 1. GET: /api/customers - Lấy danh sách toàn bộ khách hàng
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerService.GetAllAsync();
            return Ok(customers); // 200 OK
        }

        // 2. GET: /api/customers/{id} - Xem chi tiết khách hàng theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound(new { error = $"Không tìm thấy khách hàng với Id = {id}." }); // 404
            }
            return Ok(customer); // 200 OK
        }

        // 3. POST: /api/customers - Tạo mới khách hàng
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
        {
            // Kiểm tra tính hợp lệ cơ bản của DTO (Format validation: [Required], [StringLength])
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400
            }

            var createdCustomer = await _customerService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdCustomer.Id }, createdCustomer); // 201 Created
        }

        // 4. PUT: /api/customers/{id} - Cập nhật thông tin khách hàng
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _customerService.UpdateAsync(id, request);
            return Ok(new { message = "Cập nhật thông tin khách hàng thành công." }); // 200 OK
        }
    }
}