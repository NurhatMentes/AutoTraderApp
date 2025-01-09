using AutoTraderApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoTraderApp.Persistence.EntityConfigurations;

public partial class BrokerAccountConfiguration : IEntityTypeConfiguration<BrokerAccount>
{
    public void Configure(EntityTypeBuilder<BrokerAccount> builder)
    {
        builder.ToTable("BrokerAccounts");

        builder.HasKey(ba => ba.Id);

        builder.Property(ba => ba.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ba => ba.ApiKey)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(ba => ba.ApiSecret)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(ba => ba.ApiPassphrase)
            .HasMaxLength(500);

        builder.Property(ba => ba.Balance)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.HasOne(ba => ba.User)
            .WithMany()
            .HasForeignKey(ba => ba.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}