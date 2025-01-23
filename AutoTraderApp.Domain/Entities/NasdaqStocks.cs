using AutoTraderApp.Domain.Common;

namespace AutoTraderApp.Domain.Entities
{
    public class NasdaqStock:BaseEntity
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Exchange { get; set; }
        public string AssetType { get; set; }
        public string IpoDate { get; set; }
        public string DelistingDate { get; set; }
        public string Status { get; set; }
    }
}
