using Microsoft.Extensions.DependencyInjection;

namespace AutoTraderApp.Core.DependencyResolvers;

public interface ICoreModule
{
    void Load(IServiceCollection services);
}