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
    public class CombinedStockConfiguration : IEntityTypeConfiguration<CombinedStock>
    {
        public void Configure(EntityTypeBuilder<CombinedStock> builder)
        {
            builder.Property(x => x.Symbol).IsRequired().HasMaxLength(10);
            builder.Property(x => x.Category).IsRequired();
            builder.Property(x => x.Price).HasColumnType("decimal(18,2)");
            builder.Property(x => x.ChangePercentage).HasColumnType("decimal(5,2)");
            builder.Property(x => x.Volume);
            builder.Property(x => x.UpdatedAt);
        }
    }

}
