using System.ComponentModel.DataAnnotations;
using QTKD.Api.Models;

namespace QTKD.Api.DTOs
{
    public class CreateContractRequest
    {
        [Required(ErrorMessage = "Không được để trống số hợp đồng.")]
        public string ContractNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Không được để trống mã khách hàng.")]
        public int CustomerId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? AttachmentPath { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateContractRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? AttachmentPath { get; set; }
        public string? Description { get; set; }
    }
    public class ContractResponse
    {
        public int Id { get; set; }
        public string ContractNo { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ContractStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public decimal TotalAmount { get; set; }
        public string? AttachmentPath { get; set; }
        public string? Description { get; set; }
    }
}
