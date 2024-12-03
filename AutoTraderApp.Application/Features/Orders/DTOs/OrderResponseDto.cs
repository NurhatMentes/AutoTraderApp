using System.Text.Json.Serialization;

namespace AutoTraderApp.Application.Features.Orders.DTOs;

public class OrderResponseDto
{
    public Guid AccountId { get; set; }
    public string OrderId { get; set; }  
    public string Status { get; set; } 
    public decimal FilledQuantity { get; set; }  
    public decimal? FilledPrice { get; set; }  
    public decimal? LimitPrice { get; set; }  
    public string? ErrorMessage { get; set; }
}