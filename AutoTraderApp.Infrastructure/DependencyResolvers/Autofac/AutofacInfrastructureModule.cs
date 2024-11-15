using Autofac;
using AutoTraderApp.Application.Interfaces;
using AutoTraderApp.Core.CrossCuttingConcerns.Caching;
using AutoTraderApp.Infrastructure.MarketData.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoTraderApp.Infrastructure.DependencyResolvers.Autofac
{
    public class AutofacInfrastructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AlphaVantageService>()
                .As<IMarketDataService>()
                .InstancePerLifetimeScope();

            builder.Register(c =>
                {
                    var configuration = c.Resolve<IConfiguration>();
                    var apiKey = configuration["AlphaVantage:ApiKey"];

                    var client = new HttpClient
                    {
                        BaseAddress = new Uri("https://www.alphavantage.co/")
                    };

                    return client;
                })
                .Named<HttpClient>("alphaVantage")
                .SingleInstance();

            builder.Register(c =>
                {
                    var logger = c.Resolve<ILogger<AlphaVantageService>>();
                    var cacheManager = c.Resolve<ICacheManager>();
                    var configuration = c.Resolve<IConfiguration>();
                    var httpClient = c.ResolveNamed<HttpClient>("alphaVantage");

                    return new AlphaVantageService(configuration, logger, cacheManager);
                })
                .As<IMarketDataService>()
                .InstancePerLifetimeScope();
        }
    }
}
