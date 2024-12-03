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
        builder.Property(o => o.Symbol).IsRequired().HasMaxLength(50);
        builder.HasOne(o => o.BrokerAccount)
            .WithMany(ba => ba.Orders)
            .HasForeignKey(o => o.BrokerAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
