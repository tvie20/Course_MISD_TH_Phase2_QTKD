namespace QTKD.Api.Models
{
    public class PriceList
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public bool IsActive { get; set; } = true;

        // 1 PriceList có nhiều PriceItems
        public ICollection<PriceItem> PriceItems { get; set; } = new List<PriceItem>();
    }
}