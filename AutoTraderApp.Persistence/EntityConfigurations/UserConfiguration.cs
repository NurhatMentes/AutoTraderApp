using AutoTraderApp.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Persistence.EntityConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.FirstName)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(u => u.LastName)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(u => u.UserName)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Address)
                     .HasColumnName("Email")
                     .HasMaxLength(100)
                     .IsRequired();
            });

            builder.Property(u => u.PasswordHash)
                   .IsRequired();

            builder.Property(u => u.PasswordSalt)
                   .IsRequired();

            builder.HasMany(u => u.UserOperationClaims)
                   .WithOne(uoc => uoc.User)
                   .HasForeignKey(uoc => uoc.UserId);
        }
    }
}