using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;
using AutoTraderApp.Persistence.Context;
using System.Security.Claims;

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


        // Strategy Seed
        if (!context.Strategies.Any())
        {
            var strategies = new List<Strategy>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    StrategyName = "Momentum Strategy",
                    Symbol = "AAPL",
                    EntryPrice = 150.00m,
                    StopLoss = 145.00m,
                    TakeProfit = 160.00m,
                    TimeFrame = "1D",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedByUserId =  Guid.Empty   
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    StrategyName = "Reversal Strategy",
                    Symbol = "GOOGL",
                    EntryPrice = 2800.00m,
                    StopLoss = 2750.00m,
                    TakeProfit = 2900.00m,
                    TimeFrame = "1H",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedByUserId =  Guid.Empty
                }
            };


            await context.Strategies.AddRangeAsync(strategies);
            Console.WriteLine("Veritabanı seed işlemleri tamamlandı.");
        }
    }
}
