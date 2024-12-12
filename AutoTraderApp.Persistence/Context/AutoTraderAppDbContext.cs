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
        public DbSet<Order> Orders { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<ClosedPosition> ClosedPositions { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<BrokerAccount> BrokerAccounts { get; set; }
        public DbSet<Signal> Signals { get; set; }


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
