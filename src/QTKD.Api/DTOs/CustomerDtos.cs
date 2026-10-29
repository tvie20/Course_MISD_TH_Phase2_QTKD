using QTKD.Api.Models;

namespace QTKD.Api.DTOs
{
    public class CustomerResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public CustomerStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public string? TaxCode { get; set; }
        public string? Address { get; set; }
    }
}
