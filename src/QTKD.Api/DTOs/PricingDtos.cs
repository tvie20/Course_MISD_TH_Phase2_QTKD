using System.ComponentModel.DataAnnotations;

namespace QTKD.Api.DTOs
{
    public class CreatePriceListRequest
    {
        [Required(ErrorMessage = "Không được để trống tên bảng giá.")]
        public string Name { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }

    public class CreatePriceItemRequest
    {
        [Required(ErrorMessage = "Không được để trống mã dịch vụ.")]
        public string ServiceCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Không được để trống tên dịch vụ.")]
        public string ServiceName { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0.")]
        public decimal UnitPrice { get; set; }
    }

    public class PriceListResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public bool IsActive { get; set; }
        public List<PriceItemResponse> Items { get; set; } = new();
    }

    public class PriceItemResponse
    {
        public int Id { get; set; }
        public int PriceListId { get; set; }
        public string ServiceCode { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
    }
}