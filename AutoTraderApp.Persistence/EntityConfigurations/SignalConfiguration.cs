using AutoTraderApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoTraderApp.Persistence.EntityConfigurations;

public class SignalConfiguration : IEntityTypeConfiguration<Signal>
{
    public void Configure(EntityTypeBuilder<Signal> builder)
    {
        builder.ToTable("Signals");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Price)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(s => s.Confidence)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.HasOne(s => s.Strategy)
            .WithMany()
            .HasForeignKey(s => s.StrategyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Instrument)
            .WithMany()
            .HasForeignKey(s => s.InstrumentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}