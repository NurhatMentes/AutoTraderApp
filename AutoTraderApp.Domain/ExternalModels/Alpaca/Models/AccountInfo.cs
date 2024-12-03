using AutoTraderApp.Domain.Enums;
using System.Text.Json.Serialization;

namespace AutoTraderApp.Domain.ExternalModels.Alpaca.Models;

public class AccountInfo
{
    [JsonPropertyName("id")]
    public string AccountId { get; set; }

    [JsonPropertyName("cash")]
    public decimal Cash { get; set; }

    [JsonPropertyName("buying_power")]
    public decimal? BuyingPower { get; set; }

    [JsonPropertyName("portfolio_value")]
    public decimal? PortfolioValue { get; set; }

    [JsonPropertyName("multiplier")]
    public string Multiplier { get; set; } // Örn: "2" (margin hesabı için)

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("pattern_day_trader")]
    public bool IsPatternDayTrader { get; set; } 

    [JsonPropertyName("trade_suspended_by_user")]
    public bool IsTradeSuspendedByUser { get; set; } 

    [JsonPropertyName("shorting_enabled")]
    public bool IsShortingEnabled { get; set; } 

    [JsonPropertyName("is_paper")]
    public bool IsPaper { get; set; }
}

