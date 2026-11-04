using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers
{
    [ApiController]
    [Route("api/pricelists")]
    public class PricingController : ControllerBase
    {
        private readonly IPricingService _pricingService;

        public PricingController(IPricingService pricingService)
        {
            _pricingService = pricingService;
        }

        // 1. GET: /api/pricelists - Lấy danh sách bảng giá
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var priceLists = await _pricingService.GetAllPriceListsAsync();
            return Ok(priceLists);
        }

        // 2. GET: /api/pricelists/{id} - Xem chi tiết bảng giá kèm danh sách PriceItems
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var priceList = await _pricingService.GetPriceListByIdAsync(id);
            if (priceList == null)
            {
                return NotFound(new { error = $"Không tìm thấy bảng giá với Id = {id}." });
            }
            return Ok(priceList);
        }

        // 3. POST: /api/pricelists - Tạo mới bảng giá
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePriceListRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var created = await _pricingService.CreatePriceListAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // 4. POST: /api/pricelists/{id}/items - Thêm dịch vụ vào bảng giá
        [HttpPost("{id}/items")]
        public async Task<IActionResult> AddItem(int id, [FromBody] CreatePriceItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Service kiểm tra bảng giá tồn tại và UnitPrice > 0
            var item = await _pricingService.AddPriceItemAsync(id, request);
            return Created($"/api/pricelists/{id}", item);
        }
    }
}