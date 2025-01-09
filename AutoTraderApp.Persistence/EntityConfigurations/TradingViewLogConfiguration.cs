using AutoTraderApp.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace AutoTraderApp.Persistence.EntityConfigurations
{
    public class TradingViewLogConfiguration : IEntityTypeConfiguration<TradingViewLog>
    {
        public void Configure(EntityTypeBuilder<TradingViewLog> builder)
        {
            builder.ToTable("TradingViewLogs");

            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.BrokerAccount)
                   .WithMany()
                   .HasForeignKey(x => x.BrokerAccountId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Strategy)
                   .WithMany()
                   .HasForeignKey(x => x.StrategyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Step)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(x => x.Status)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.Symbol)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(x => x.Message)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(x => x.CreatedAt)
                   .IsRequired();
        }
    }
}
