using QTKD.Api.Models;

namespace QTKD.Api.DTOs
{
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
