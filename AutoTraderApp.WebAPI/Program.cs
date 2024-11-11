using AutoTraderApp.Persistence.Context;
using AutoTraderApp.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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