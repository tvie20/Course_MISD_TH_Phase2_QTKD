namespace QTKD.Api.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public CustomerStatus Status { get; set; } = CustomerStatus.Active;
        public string? TaxCode { get; set; }
        public string? Address { get; set; }

        // 1 Customer có nhiều Contracts
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
