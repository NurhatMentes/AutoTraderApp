using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoTraderApp.Core.DependencyResolvers;
using AutoTraderApp.Core.Extensions;
using AutoTraderApp.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using AutoTraderApp.Persistence.Seeds;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Autofac Provider
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new CoreModule());
});

// .NET Core Service Collection Module
builder.Services.AddDependencyResolvers(new ICoreModule[] { new CustomCoreModule() });

// SQL Server DbContext Baðlantýsý
builder.Services.AddDbContext<AutoTraderAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

await ApplyMigrationsAndSeed(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Migration ve Seed Ýþlemleri
static async Task ApplyMigrationsAndSeed(IApplicationBuilder app)
{
    using var scope = app.ApplicationServices.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AutoTraderAppDbContext>();

    try
    {
        await context.Database.MigrateAsync();
        await DataSeeder.SeedAsync(context);
        Console.WriteLine("Migration ve seed iþlemleri baþarýlý.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Hata: {ex.Message}");
    }
}