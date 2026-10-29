namespace QTKD.Api.Models
{
    public class PriceItem
    {
        public int Id { get; set; }
        
        // Khóa ngoại liên kết tới PriceList
        public int PriceListId { get; set; }
        public PriceList? PriceList { get; set; }

        public string ServiceCode { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
    }
}
