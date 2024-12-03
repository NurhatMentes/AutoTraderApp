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


        await context.SaveChangesAsync();
        Console.WriteLine("Veritabanı seed işlemleri tamamlandı.");
    }
}
