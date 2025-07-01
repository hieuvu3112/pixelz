using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Repositories;
using PaymentService.Application.Services.Interfaces;
using PaymentService.Domain.Repositories;
using PaymentService.Infrastructure;

namespace PaymentService
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
            services.AddDbContext<PaymentDbContext>(options =>
                options.UseMySql(
                    _configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(_configuration.GetConnectionString("DefaultConnection"))
                )
            );
            
            // Register repositories and services
            services.AddScoped<IPaymentService, Application.Services.PaymentService>();

            services.AddScoped<ITopupRepository, TopupRepository>();
            services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Service API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
