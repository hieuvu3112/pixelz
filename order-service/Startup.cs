using System.Collections.Generic;
using System.IO;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderService.Infrastructure;
using Shared.Events;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using OrderService.GraphQL;
using OrderService.GraphQL.Types;
using OrderService.GraphQL.Queries;

namespace OrderService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            // ...add EF Core, Kafka, etc...
            services.AddDbContext<OrderDbContext>(options =>
                options.UseMySql(
                    Configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(Configuration.GetConnectionString("DefaultConnection"))
                )
            );
            services.AddMassTransit(x =>
            {
                x.UsingInMemory();

                x.AddRider(rider =>
                {
                    rider.AddProducer<OrderCheckedOutEvent>("pixelz.order.checkout");
                    rider.UsingKafka((context, k) => { k.Host(Configuration.GetValue<string>("Kafka:BootstrapServers")); });
                });
            });

            // Add GraphQL services
            services.AddScoped<OrderType>();
            services.AddScoped<OrderConnectionType>();
            services.AddScoped<OrderQuery>();
            services.AddScoped<ISchema, OrderSchema>();
            services.AddScoped<IDocumentExecuter, DocumentExecuter>();

            // Add Swagger services
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) 
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable Swagger middleware
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();
            
            app.UseEndpoints(endpoints => 
            {
                endpoints.MapControllers();
                endpoints.MapPost("/graphql", async context =>
                {
                    var documentExecuter = context.RequestServices.GetRequiredService<IDocumentExecuter>();
                    var schema = context.RequestServices.GetRequiredService<ISchema>();
                    var dbContext = context.RequestServices.GetRequiredService<OrderDbContext>();
                    
                    var json = await new StreamReader(context.Request.Body).ReadToEndAsync();
                    var query = System.Text.Json.JsonSerializer.Deserialize<GraphQLQuery>(json);
                    
                    var result = await documentExecuter.ExecuteAsync(new ExecutionOptions
                    {
                        Schema = schema,
                        Query = query.Query,
                        Variables = query.Variables != null ? new Inputs(query.Variables) : null,
                        UserContext = new Dictionary<string, object>
                        {
                            ["dbContext"] = dbContext
                        }
                    });
                    
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
                });
            });
        }
    }
    
    public class GraphQLQuery
    {
        public string Query { get; set; }
        public Dictionary<string, object> Variables { get; set; }
    }
}
