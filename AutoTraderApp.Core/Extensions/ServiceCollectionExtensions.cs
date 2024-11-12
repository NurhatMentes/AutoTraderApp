using AutoTraderApp.Core.DependencyResolvers;
using AutoTraderApp.Core.Utilities.IoC.AutoTraderApp.Core.Utilities.IoC;
using Microsoft.Extensions.DependencyInjection;

namespace AutoTraderApp.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDependencyResolvers(this IServiceCollection services, ICoreModule[] modules)
    {
        foreach (var module in modules)
        {
            module.Load(services);
        }

        return ServiceTool.Create(services);
    }
}