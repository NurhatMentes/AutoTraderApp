using Microsoft.Extensions.DependencyInjection;

namespace AutoTraderApp.Core.Utilities.IoC
{
    namespace AutoTraderApp.Core.Utilities.IoC
    {
        public static class ServiceTool
        {
            public static IServiceProvider ServiceProvider { get; private set; }

            public static IServiceCollection Create(IServiceCollection services)
            {
                ServiceProvider = services.BuildServiceProvider();
                return services;
            }
        }
    }
}
