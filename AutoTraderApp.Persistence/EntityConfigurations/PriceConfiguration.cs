using AutoTraderApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoTraderApp.Persistence.EntityConfigurations;

public class PriceConfiguration : IEntityTypeConfiguration<Price>
{
    public void Configure(EntityTypeBuilder<Price> builder)
    {
        builder.ToTable("Prices");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Open)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(p => p.High)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(p => p.Low)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(p => p.Close)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(p => p.Volume)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(p => p.Timestamp)
            .IsRequired();

        builder.HasOne(p => p.Instrument)
            .WithMany(i => i.Prices)
            .HasForeignKey(p => p.InstrumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.InstrumentId, p.Timestamp });
    }
}