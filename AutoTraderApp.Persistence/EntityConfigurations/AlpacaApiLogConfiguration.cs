using AutoTraderApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoTraderApp.Persistence.EntityConfigurations;

public partial class BrokerAccountConfiguration
{
    public class AlpacaApiLogConfiguration : IEntityTypeConfiguration<AlpacaApiLog>
    {
        public void Configure(EntityTypeBuilder<AlpacaApiLog> builder)
        {
            builder.ToTable("AlpacaApiLogs");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequestUrl).IsRequired().HasMaxLength(500);
            builder.Property(x => x.HttpMethod).IsRequired().HasMaxLength(10);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.LogType).HasDefaultValue("Info").HasMaxLength(50);
        }
    }

}