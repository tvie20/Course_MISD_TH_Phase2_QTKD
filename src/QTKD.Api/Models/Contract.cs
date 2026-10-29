namespace QTKD.Api.Models
{
    public class Contract
    {
        public int Id { get; set; }
        public string ContractNo { get; set; } = string.Empty;
        
        // Khóa ngoại liên kết tới Customer
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ContractStatus Status { get; set; } = ContractStatus.Draft;
        public decimal TotalAmount { get; set; }
        public string? AttachmentPath { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
