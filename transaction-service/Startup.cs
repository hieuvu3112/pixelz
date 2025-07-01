using System;
using System.Diagnostics;
using Confluent.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TransactionService.Infrastructure.Data;
using MassTransit;
using MassTransit.Metadata;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared.Events;
using TransactionService.Infrastructure.Consumers;
using TransactionService.Infrastructure.Handlers;
using TransactionService.Infrastructure.StateMachines;

namespace TransactionService;

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
        
        services.AddDbContext<TransactionDbContext>(options =>
            options.UseMySql(
                Configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("DefaultConnection"))
            )
        );
        
        services.AddOpenTelemetry().WithTracing(x =>
        {
            x.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService("api")
                    .AddTelemetrySdk()
                    .AddEnvironmentVariableDetector())
                .AddSource("MassTransit")
                .AddAspNetCoreInstrumentation()
                .AddJaegerExporter(o =>
                {
                    o.AgentHost = HostMetadataCache.IsRunningInContainer ? "jaeger" : "localhost";
                    o.AgentPort = 6831;
                    o.MaxPayloadSizeInBytes = 4096;
                    o.ExportProcessorType = ExportProcessorType.Batch;
                    o.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                    {
                        MaxQueueSize = 2048,
                        ScheduledDelayMilliseconds = 5000,
                        ExporterTimeoutMilliseconds = 30000,
                        MaxExportBatchSize = 512,
                    };
                });
        });
        
        // MassTransit + Outbox + Kafka
        services.AddMassTransit(x =>
        {
            // Register the state machine FIRST
            x.AddSagaStateMachine<OrderWorkflow, OrderWorkflowState, OrderStateDefinition>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                    r.AddDbContext<DbContext, TransactionDbContext>((provider, builder) =>
                    {
                        var connectionString = Configuration.GetConnectionString("DefaultConnection");
                        builder.UseMySql(connectionString!,
                            ServerVersion.AutoDetect(connectionString));
                    });
                });
            
            // Register all consumers
            x.AddConsumer<InitiatePaymentCommandHandler>();
            x.AddConsumer<InitiateProductionCommandHandler>();
            x.AddConsumer<OrderCompletedNotificationHandler>();
            x.AddConsumer<OrderFailedNotificationHandler>();

            x.SetKebabCaseEndpointNameFormatter();
            
            // x.AddConfigureEndpointsCallback((context, name, cfg) =>
            // {
            //     cfg.UseEntityFrameworkOutbox<TransactionDbContext>(context);
            // });

            // Outbox configuration
            x.AddEntityFrameworkOutbox<TransactionDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(1);
                o.UseMySql();
                o.UseBusOutbox();
            });
            
            // Configure In-Memory bus for internal communication (state machine <-> consumers)
            x.UsingInMemory((context, cfg) =>
            {
                // Configure all endpoints using automatic endpoint configuration
                // This will create endpoints for all consumers and the state machine
                cfg.ConfigureEndpoints(context);
            });

            // Configure Kafka rider ONLY for external events
            x.AddRider(rider =>
            {
                rider.AddConsumer<OrderCheckedOutEventConsumer>();

                rider.UsingKafka((context, k) =>
                {
                    var bootstrapServers = Configuration.GetValue<string>("Kafka:BootstrapServers");
                    k.Host(bootstrapServers);
                    
                    k.TopicEndpoint<OrderCheckedOutEvent>("pixelz.order.checkout", "transaction-service-group", e =>
                    {
                        e.AutoOffsetReset = AutoOffsetReset.Earliest;
                        e.ConfigureConsumer<OrderCheckedOutEventConsumer>(context);
                    });
                    
                });
            });
        });
        
        // Add hosted service to process outbox messages
        services.AddHostedService<MassTransitHostedService>();
        
        // Add Swagger for API documentation
        services.AddSwaggerGen();
    }
        
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Ensure database is created and migrations are applied
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
            context.Database.Migrate();
        }
        
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Enable Swagger middleware
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "TransactionService v1");
            c.RoutePrefix = string.Empty;
        });

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
