using AutoTraderApp.Domain.Enums;

namespace AutoTraderApp.Application.Features.Strategies.DTOs
{
    public class TradingRuleDto
    {
        public Guid Id { get; set; }
        public string Indicator { get; set; }
        public string Condition { get; set; }
        public decimal Value { get; set; }
        public TradingAction Action { get; set; }
    }
}
