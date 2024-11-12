using Microsoft.Extensions.DependencyInjection;
using AutoTraderApp.Core.CrossCuttingConcerns.Caching;
using AutoTraderApp.Core.Security.JWT;
using Microsoft.AspNetCore.Http;

namespace AutoTraderApp.Core.DependencyResolvers;

public class CustomCoreModule : ICoreModule
{
    public void Load(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheManager, MemoryCacheManager>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<ITokenHelper, JwtHelper>();
    }
}