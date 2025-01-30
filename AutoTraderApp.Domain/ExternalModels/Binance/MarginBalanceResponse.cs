namespace AutoTraderApp.Domain.ExternalModels.Binance
{
    public class MarginBalanceResponse
    {
        public bool TradeEnabled { get; set; }
        public bool BorrowEnabled { get; set; }
        public bool TransferEnabled { get; set; }
        public decimal MarginLevel { get; set; }
        public decimal TotalAssetOfBtc { get; set; }
        public decimal TotalLiabilityOfBtc { get; set; }
        public decimal TotalNetAssetOfBtc { get; set; }
        public List<MarginBalanceDetail> UserAssets { get; set; }
    }

    public class MarginBalanceDetail
    {
        public string Asset { get; set; }
        public decimal Free { get; set; }
        public decimal Locked { get; set; }
        public decimal Borrowed { get; set; }
        public decimal Interest { get; set; }
        public decimal NetAsset { get; set; }
    }

}
