using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoTraderApp.Domain.ExternalModels.Alpaca.Models;

public class CustomOrderStatusConverter : JsonConverter<OrderStatus>
{
    public override OrderStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var status = reader.GetString()?.ToLower();
        return status switch
        {
            "new" => OrderStatus.New,
            "partially_filled" => OrderStatus.PartiallyFilled,
            "filled" => OrderStatus.Filled,
            "done_for_day" => OrderStatus.DoneForDay,
            "canceled" => OrderStatus.Canceled,
            "expired" => OrderStatus.Expired,
            "replaced" => OrderStatus.Replaced,
            "pending_cancel" => OrderStatus.PendingCancel,
            "pending_replace" => OrderStatus.PendingReplace,
            "rejected" => OrderStatus.Rejected,
            "pending_new" => OrderStatus.PendingNew,
            _ => OrderStatus.Unknown
        };
    }

    public override void Write(Utf8JsonWriter writer, OrderStatus value, JsonSerializerOptions options)
    {
        var status = value switch
        {
            OrderStatus.New => "new",
            OrderStatus.PartiallyFilled => "partially_filled",
            OrderStatus.Filled => "filled",
            OrderStatus.DoneForDay => "done_for_day",
            OrderStatus.Canceled => "canceled",
            OrderStatus.Expired => "expired",
            OrderStatus.Replaced => "replaced",
            OrderStatus.PendingCancel => "pending_cancel",
            OrderStatus.PendingReplace => "pending_replace",
            OrderStatus.Rejected => "rejected",
            OrderStatus.PendingNew => "pending_new",
            _ => "unknown"
        };
        writer.WriteStringValue(status);
    }
}