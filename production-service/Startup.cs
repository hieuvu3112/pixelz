using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using ProductionService.Infrastructure.Data;
using ProductionService.Application.Repositories;
using ProductionService.Application.Repositories.Interfaces;
using ProductionService.Application.Services.Interfaces;
using ProductionService.Infrastructure;

namespace ProductionService
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            
            // Configure EF Core with MySQL
            services.AddDbContext<ProductionDbContext>(options =>
                options.UseMySql(
                    _configuration.GetConnectionString("ProductionDatabase"),
                    ServerVersion.AutoDetect(_configuration.GetConnectionString("ProductionDatabase"))
                )
            );
            
            // Register repositories and services
            services.AddScoped<IProductionRepository, ProductionRepository>();
            services.AddScoped<IProductionService, Application.Services.ProductionService>();
            
            // Configure Kafka
            services.Configure<KafkaSettings>(_configuration.GetSection("Kafka"));

            // Add Swagger services
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            // Enable Swagger middleware
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Production Service API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
