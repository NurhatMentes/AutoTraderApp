using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using AutoTraderApp.Persistence.Context;

namespace AutoTraderApp.Persistence
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AutoTraderAppDbContext>
    {
        public AutoTraderAppDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var builder = new DbContextOptionsBuilder<AutoTraderAppDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlServer(connectionString);

            return new AutoTraderAppDbContext(builder.Options);
        }
    }
}
