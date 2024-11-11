using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;
using AutoTraderApp.Persistence.Context;

namespace AutoTraderApp.Persistence.Seeds;

public static class DataSeeder
{
    public static async Task SeedAsync(AutoTraderAppDbContext context)
    {
        // OperationClaims Seed
        if (!context.OperationClaims.Any())
        {
            var claims = new List<OperationClaim>
            {
                new() { Id = Guid.NewGuid(), Name = "Admin", CreatedByUserId = Guid.Empty },
                new() { Id = Guid.NewGuid(), Name = "User", CreatedByUserId = Guid.Empty },
                new() { Id = Guid.NewGuid(), Name = "Trader", CreatedByUserId = Guid.Empty },
                new() { Id = Guid.NewGuid(), Name = "Premium", CreatedByUserId = Guid.Empty }
            };

            await context.OperationClaims.AddRangeAsync(claims);
            Console.WriteLine("OperationClaims seed başarılı.");
        }

        // Instruments Seed
        if (!context.Instruments.Any())
        {
            var instruments = new List<Instrument>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Symbol = "AAPL",
                    Name = "Apple Inc.",
                    Type = InstrumentType.Stock,
                    Exchange = "NASDAQ",
                    MinTradeAmount = 1,
                    MaxTradeAmount = 10000,
                    PriceDecimalPlaces = 2,
                    Status = InstrumentStatus.Active,
                    CreatedByUserId = Guid.Empty
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Symbol = "BTCUSD",
                    Name = "Bitcoin/USD",
                    Type = InstrumentType.Crypto,
                    Exchange = "Binance",
                    MinTradeAmount = 0.0001m,
                    MaxTradeAmount = 100,
                    PriceDecimalPlaces = 8,
                    Status = InstrumentStatus.Active,
                    CreatedByUserId = Guid.Empty
                }
            };

            await context.Instruments.AddRangeAsync(instruments);
            Console.WriteLine("Instruments seed başarılı.");
        }

        await context.SaveChangesAsync();
        Console.WriteLine("Veritabanı seed işlemleri tamamlandı.");
    }
}
