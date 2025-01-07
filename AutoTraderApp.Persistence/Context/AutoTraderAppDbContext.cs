using AutoTraderApp.Domain.Common;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.Telegram;
using AutoTraderApp.Persistence.EntityConfigurations;
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
        public DbSet<Order> Orders { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<ClosedPosition> ClosedPositions { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<BrokerAccount> BrokerAccounts { get; set; }
        public DbSet<Signal> Signals { get; set; }
        public DbSet<Strategy> Strategies { get; set; }
        public DbSet<UserTradingAccount> UserTradingAccounts { get; set; }
        public DbSet<TradingViewLog> tradingViewLogs { get; set; }
        public DbSet<TradingViewSignalLog> tradingViewSignalLogs { get; set; }
        public DbSet<CombinedStock> combinedStocks { get; set; }
        public DbSet<TelegramUser> TelegramUsers { get; set; }
        public DbSet<TelegramBotConfig> TelegramBotConfigs { get; set; }
        public DbSet<BrokerLog> BrokerLogs { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.LimitPrice).HasPrecision(18, 4);
                entity.Property(e => e.StopPrice).HasPrecision(18, 4);
                entity.Property(e => e.FilledQuantity).HasPrecision(18, 4);
                entity.Property(e => e.FilledPrice).HasPrecision(18, 4);
                entity.Property(e => e.TakeProfitLimitPrice).HasPrecision(18, 4);
                entity.Property(e => e.StopLossLimitPrice).HasPrecision(18, 4);
                entity.Property(e => e.StopLossStopPrice).HasPrecision(18, 4);
            });
            modelBuilder.Entity<ClosedPosition>(entity =>
            {
                entity.Property(e => e.Symbol).IsRequired();
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.RealizedPnL).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.ClosedAt).IsRequired();
            });
            modelBuilder.Entity<Strategy>(entity =>
            {
                entity.ToTable("Strategies");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.StrategyName).IsRequired().HasMaxLength(100);
                entity.Property(s => s.Symbol).IsRequired().HasMaxLength(10);
                entity.Property(s => s.EntryPrice).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(s => s.StopLoss).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(s => s.TakeProfit).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(s => s.TimeFrame).IsRequired().HasMaxLength(10);
                entity.Property(s => s.CreatedAt).IsRequired();
                entity.Property(s => s.WebhookUrl).IsRequired(false);

            });
            modelBuilder.ApplyConfiguration(new UserTradingAccountConfiguration());
            modelBuilder.ApplyConfiguration(new TradingViewLogConfiguration());
            modelBuilder.ApplyConfiguration(new TradingViewSignalLogConfiguration());
            modelBuilder.ApplyConfiguration(new CombinedStockConfiguration());
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
