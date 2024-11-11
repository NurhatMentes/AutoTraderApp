using AutoTraderApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoTraderApp.Persistence.EntityConfigurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("Positions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Quantity)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(p => p.EntryPrice)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(p => p.CurrentPrice)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(p => p.UnrealizedPnL)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(p => p.RealizedPnL)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.BrokerAccount)
            .WithMany(ba => ba.Positions)
            .HasForeignKey(p => p.BrokerAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Strategy)
            .WithMany()
            .HasForeignKey(p => p.StrategyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}