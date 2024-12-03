using AutoTraderApp.Persistence.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Persistence.Repositories;
using AutoTraderApp.Persistence.Seeds;

namespace AutoTraderApp.Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static async Task<IServiceCollection> AddPersistenceServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AutoTraderAppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IOperationClaimRepository, OperationClaimRepository>();
            services.AddScoped<IUserOperationClaimRepository, UserOperationClaimRepository>();

            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AutoTraderAppDbContext>();
                await DataSeeder.SeedAsync(dbContext);
            }

            return services;
        }
    }
}
