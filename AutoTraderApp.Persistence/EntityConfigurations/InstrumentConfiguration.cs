using AutoTraderApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoTraderApp.Persistence.EntityConfigurations;

public class InstrumentConfiguration : IEntityTypeConfiguration<Instrument>
{
    public void Configure(EntityTypeBuilder<Instrument> builder)
    {
        builder.ToTable("Instruments");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Symbol)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.Exchange)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.MinTradeAmount)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(i => i.MaxTradeAmount)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(i => i.PriceDecimalPlaces)
            .IsRequired();

        builder.HasMany(i => i.Prices)
            .WithOne(p => p.Instrument)
            .HasForeignKey(p => p.InstrumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}