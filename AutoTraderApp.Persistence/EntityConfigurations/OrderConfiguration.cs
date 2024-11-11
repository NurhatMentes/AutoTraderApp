using AutoTraderApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoTraderApp.Persistence.EntityConfigurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Quantity)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(o => o.Price)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(o => o.StopLoss)
            .HasPrecision(18, 8);

        builder.Property(o => o.TakeProfit)
            .HasPrecision(18, 8);

        builder.Property(o => o.ExternalOrderId)
            .HasMaxLength(100);

        builder.Property(o => o.RejectionReason)
            .HasMaxLength(500);

        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Signal)
            .WithMany()
            .HasForeignKey(o => o.SignalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.BrokerAccount)
            .WithMany(ba => ba.Orders)
            .HasForeignKey(o => o.BrokerAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Instrument)
            .WithMany()
            .HasForeignKey(o => o.InstrumentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}