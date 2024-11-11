using AutoTraderApp.Domain.Common;
using AutoTraderApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AutoTraderApp.Persistence.Context
{
    public class AutoTraderAppDbContext : DbContext
    {
        public AutoTraderAppDbContext(DbContextOptions<AutoTraderAppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserOperationClaim> UserOperationClaims { get; set; }
        public DbSet<OperationClaim> OperationClaims { get; set; }
        public DbSet<Strategy> Strategies { get; set; }
        public DbSet<Instrument> Instruments { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<BrokerAccount> BrokerAccounts { get; set; }
        public DbSet<TradingSession> TradingSessions { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<NotificationSetting> NotificationSettings { get; set; }
        public DbSet<BacktestResult> BacktestResults { get; set; }
        public DbSet<TradingRule> TradingRules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (
                    e.State == EntityState.Added
                    || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                ((BaseEntity)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;

                if (entityEntry.State == EntityState.Added)
                {
                    ((BaseEntity)entityEntry.Entity).CreatedAt = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
