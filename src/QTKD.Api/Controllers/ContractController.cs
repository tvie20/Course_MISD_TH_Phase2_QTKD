using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers
{
    [ApiController]
    [Route("api/contracts")]
    public class ContractController : ControllerBase
    {
        // Chỉ inject IContractService
        private readonly IContractService _contractService;

        public ContractController(IContractService contractService)
        {
            _contractService = contractService;
        }

        // 1. GET: /api/contracts - Lấy danh sách hợp đồng
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var contracts = await _contractService.GetAllAsync();
            return Ok(contracts);
        }

        // 2. GET: /api/contracts/{id} - Xem chi tiết hợp đồng
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var contract = await _contractService.GetByIdAsync(id);
            if (contract == null)
            {
                return NotFound(new { error = $"Không tìm thấy hợp đồng với Id = {id}." }); // 404
            }
            return Ok(contract);
        }

        // 3. POST: /api/contracts - Lập hợp đồng mới (khởi tạo ở trạng thái Draft)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateContractRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdContract = await _contractService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdContract.Id }, createdContract); // 201 Created
        }

        // 4. PUT: /api/contracts/{id} - Chỉnh sửa hợp đồng (CTR-01)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateContractRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _contractService.UpdateAsync(id, request);
            return Ok(new { message = "Cập nhật hợp đồng thành công." });
        }

        // 5. POST: /api/contracts/{id}/submit - Gửi hợp đồng trình duyệt
        [HttpPost("{id}/submit")]
        public async Task<IActionResult> Submit(int id)
        {
            var result = await _contractService.SubmitAsync(id);

            return Ok(new
            {
                message = "Hợp đồng đã được gửi trình duyệt thành công.",
                data = result
            });
        }
    }
}