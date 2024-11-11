using AutoTraderApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoTraderApp.Persistence.EntityConfigurations;

public class TradingRuleConfiguration : IEntityTypeConfiguration<TradingRule>
{
    public void Configure(EntityTypeBuilder<TradingRule> builder)
    {
        builder.ToTable("TradingRules");

        builder.HasKey(tr => tr.Id);

        builder.Property(tr => tr.Indicator)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(tr => tr.Condition)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(tr => tr.Value)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.HasOne(tr => tr.Strategy)
            .WithMany(s => s.TradingRules)
            .HasForeignKey(tr => tr.StrategyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}