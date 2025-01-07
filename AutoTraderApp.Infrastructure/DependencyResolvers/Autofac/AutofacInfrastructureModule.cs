using Autofac;
using AutoTraderApp.Core.CrossCuttingConcerns.Caching;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Services;
using AutoTraderApp.Core.Utilities.Settings;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using AutoTraderApp.Infrastructure.Services.Alpaca;
using AutoTraderApp.Infrastructure.Services.Automation.Playwrights;
using AutoTraderApp.Infrastructure.Services.MarketData;
using AutoTraderApp.Infrastructure.Services.Polygon;
using AutoTraderApp.Infrastructure.Services.Telegram;
using AutoTraderApp.Infrastructure.Services.TradingView;
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
                .As<IAlphaVantageService>()
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
                .As<IAlphaVantageService>()
                .InstancePerLifetimeScope();


            // Alpaca
            builder.Register(ctx =>
            {
                var httpClientFactory = ctx.Resolve<IHttpClientFactory>();
                var brokerAccountRepository = ctx.Resolve<IBaseRepository<BrokerAccount>>();
                var brokerLog = ctx.Resolve<IBaseRepository<BrokerLog>>();
                return new AlpacaService(httpClientFactory, brokerAccountRepository, brokerLog);
            }).As<IAlpacaService>().InstancePerLifetimeScope();


            //TredingView
            builder.Register(c => new HttpClient { BaseAddress = new Uri("https://www.tradingview.com/") })
                .As<HttpClient>()
                .SingleInstance();

            builder.Register(context =>
            {
                var config = context.Resolve<IOptions<TradingViewSettings>>();
                return new TradingViewService(context.Resolve<HttpClient>(), config);
            }).As<ITradingViewService>().InstancePerLifetimeScope();


            //builder.RegisterType<TradingViewService>().As<ITradingViewService>().InstancePerLifetimeScope();

            // ITradingViewAutomationService 
            builder.RegisterType<TradingViewAutomationService>()
                   .As<ITradingViewAutomationService>()
                   .InstancePerLifetimeScope();

            // TradingView Log Service
            builder.RegisterType<TradingViewLogService>()
                   .AsSelf()
                   .InstancePerLifetimeScope();

            // TradingViewSignal Log Service
            builder.RegisterType<TradingViewSignalLogService>()
                   .AsSelf()
                   .InstancePerLifetimeScope();

            // Telegram
            builder.RegisterType<TelegramBotService>()
                .As<ITelegramBotService>()
                .InstancePerLifetimeScope();

            // Polygon
            builder.RegisterType<PolygonService>()
                   .As<IPolygonService>()
                   .InstancePerLifetimeScope();

            // Selenium
            builder.RegisterType<TradingViewSelenium>()
      .As<ITradingViewSeleniumService>()
      .InstancePerLifetimeScope();
        }
    }
}
