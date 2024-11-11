using AutoTraderApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoTraderApp.Persistence.EntityConfigurations;

public class TradingSessionConfiguration : IEntityTypeConfiguration<TradingSession>
{
    public void Configure(EntityTypeBuilder<TradingSession> builder)
    {
        builder.ToTable("TradingSessions");

        builder.HasKey(ts => ts.Id);

        builder.Property(ts => ts.StartBalance)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(ts => ts.CurrentBalance)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(ts => ts.ProfitLoss)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.HasOne(ts => ts.Strategy)
            .WithMany()
            .HasForeignKey(ts => ts.StrategyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}