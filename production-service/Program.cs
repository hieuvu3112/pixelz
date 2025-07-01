using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductionService.Infrastructure.Data;

namespace ProductionService;

public static class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
            
        // Apply migrations at startup
        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProductionDbContext>();
            db.Database.Migrate();
        }
            
        host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}