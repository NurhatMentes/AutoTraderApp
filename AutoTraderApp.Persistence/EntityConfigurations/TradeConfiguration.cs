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

        builder.Property(t => t.Symbol)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(t => t.BrokerAccount)
            .WithMany(ba => ba.Trades) 
            .HasForeignKey(t => t.BrokerAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
