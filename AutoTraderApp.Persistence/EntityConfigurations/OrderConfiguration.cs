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
            .IsRequired(false);  

        builder.Property(o => o.StopLoss)
            .HasPrecision(18, 8)
            .IsRequired(false);  

        builder.Property(o => o.TakeProfit)
            .HasPrecision(18, 8)
            .IsRequired(false); 

        builder.Property(o => o.ExternalOrderId)
            .HasMaxLength(100)
            .IsRequired(false);  

        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Instrument)
            .WithMany()
            .HasForeignKey(o => o.InstrumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.BrokerAccount)
            .WithMany()
            .HasForeignKey(o => o.BrokerAccountId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(o => o.RejectionReason)
            .HasMaxLength(500)
            .IsRequired(false);
    }
}