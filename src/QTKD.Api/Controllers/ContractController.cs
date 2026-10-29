using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Repositories;

namespace QTKD.Api.Controllers
{
    [ApiController]
    [Route("api/contracts")]
    public class ContractController : ControllerBase
    {
        private readonly IContractRepository _contractRepository;

        public ContractController(IContractRepository contractRepository)
        {
            _contractRepository = contractRepository;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
            {
                return NotFound(new { error = $"Không tìm thấy hợp đồng {id}." });
            }

            var response = new ContractResponse
            {
                Id = contract.Id,
                ContractNo = contract.ContractNo,
                CustomerId = contract.CustomerId,
                CustomerName = contract.Customer?.Name ?? string.Empty,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = contract.Status,
                TotalAmount = contract.TotalAmount,
                AttachmentPath = contract.AttachmentPath,
                Description = contract.Description
            };

            return Ok(response);
        }
    }
}
