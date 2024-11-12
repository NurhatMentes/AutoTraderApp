using Autofac;
using AutoTraderApp.Core.Security.JWT;
using Microsoft.AspNetCore.Http;
using AutoTraderApp.Core.CrossCuttingConcerns.Caching;

namespace AutoTraderApp.Core.DependencyResolvers
{
    public class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MemoryCacheManager>().As<ICacheManager>().SingleInstance();
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
            builder.RegisterType<JwtHelper>().As<ITokenHelper>().SingleInstance();
        }
    }
}
