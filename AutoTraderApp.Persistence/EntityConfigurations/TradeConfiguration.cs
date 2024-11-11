using AutoTraderApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoTraderApp.Persistence.EntityConfigurations;

public class TradeConfiguration : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> builder)
    {
        builder.ToTable("Trades");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.ExecutedPrice)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(t => t.ExecutedQuantity)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(t => t.Commission)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(t => t.ExternalTradeId)
            .HasMaxLength(100);

        builder.HasOne(t => t.Order)
            .WithMany(o => o.Trades)
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Position)
            .WithMany()
            .HasForeignKey(t => t.PositionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}