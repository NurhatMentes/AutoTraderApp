namespace AutoTraderApp.Domain.ExternalModels.Alpaca.Models
{
    public class AlpacaSettings
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string BaseUrl { get; set; }
        public bool IsPaper { get; set; }
    }
}
