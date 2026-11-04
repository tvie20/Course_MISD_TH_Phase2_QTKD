using System.ComponentModel.DataAnnotations;
using QTKD.Api.Models;

namespace QTKD.Api.DTOs
{
    public class CreateCustomerRequest
    {
        [Required(ErrorMessage = "Không được để trống mã khách hàng.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Không được để trống tên khách hàng.")]
        public string Name { get; set; } = string.Empty;

        public string? TaxCode { get; set; }
        public string? Address { get; set; }
    }

    public class UpdateCustomerRequest
    {
        [Required(ErrorMessage = "Không được để trống tên khách hàng.")]
        public string Name { get; set; } = string.Empty;
        public CustomerStatus Status { get; set; } = CustomerStatus.Active;
        public string? TaxCode { get; set; }
        public string? Address { get; set; }
    }

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
