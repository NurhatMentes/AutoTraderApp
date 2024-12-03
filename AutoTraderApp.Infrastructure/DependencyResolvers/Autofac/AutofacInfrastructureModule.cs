using Autofac;
using AutoTraderApp.Core.CrossCuttingConcerns.Caching;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;
using AutoTraderApp.Infrastructure.Interfaces;
using AutoTraderApp.Infrastructure.Services.Alpaca;
using AutoTraderApp.Infrastructure.Services.MarketData.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoTraderApp.Infrastructure.DependencyResolvers.Autofac
{
    public class AutofacInfrastructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AlphaVantageService>()
                .As<IMarketDataService>()
                .InstancePerLifetimeScope();


            //AlphaVantage
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


            //Alpaca
            builder.Register(ctx =>
            {
                var configuration = ctx.Resolve<IConfiguration>();
                var settings = new AlpacaSettings();
                configuration.GetSection("AlpacaSettings").Bind(settings);

                var client = new HttpClient
                {
                    BaseAddress = new Uri(settings.BaseUrl)
                };
                client.DefaultRequestHeaders.Add("APCA-API-KEY-ID", settings.ApiKey);
                client.DefaultRequestHeaders.Add("APCA-API-SECRET-KEY", settings.ApiSecret);
                return client;
            }).Named<HttpClient>("alpaca").SingleInstance();

            builder.Register(ctx =>
            {
                var configuration = ctx.Resolve<IConfiguration>();
                var settings = new AlpacaSettings();
                configuration.GetSection("AlpacaSettings").Bind(settings);
                return Options.Create(settings);
            }).As<IOptions<AlpacaSettings>>().SingleInstance();

            builder.Register(ctx =>
            {
                var httpClient = ctx.ResolveNamed<HttpClient>("alpaca");
                var settings = ctx.Resolve<IOptions<AlpacaSettings>>();
                return new AlpacaService(httpClient, settings);
            }).As<IAlpacaService>().InstancePerLifetimeScope();
        }
    }
}
