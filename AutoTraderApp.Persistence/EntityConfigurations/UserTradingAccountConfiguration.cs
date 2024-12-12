using AutoTraderApp.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace AutoTraderApp.Persistence.EntityConfigurations
{
    public class UserTradingAccountConfiguration : IEntityTypeConfiguration<UserTradingAccount>
    {
        public void Configure(EntityTypeBuilder<UserTradingAccount> builder)
        {
            builder.ToTable("UserTradingAccounts");

            builder.HasKey(x => x.Id);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.Email)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(x => x.EncryptedPassword)
                   .IsRequired();

            builder.Property(x => x.TwoFactorExpiry)
                   .IsRequired(false); 
        }
    }
}
