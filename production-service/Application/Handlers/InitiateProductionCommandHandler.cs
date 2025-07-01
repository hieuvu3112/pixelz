using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit;
using Shared.Commands;
using Shared.Events;

namespace ProductionService.Application.Handlers
{
    public class InitiateProductionCommandHandler : IConsumer<InitiateProductionCommand>
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public InitiateProductionCommandHandler(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<InitiateProductionCommand> context)
        {
            try
            {
                // Mock production processing
                var isSuccess = new Random().Next(0, 10) > 1; // 90% success rate

                if (isSuccess)
                {
                    await _publishEndpoint.Publish(new ProductionCompletedEvent
                    {
                        CorrelationId = context.Message.CorrelationId,
                        OrderId = context.Message.OrderId,
                        OrderName = context.Message.OrderName,
                        CompletedAt = DateTime.UtcNow,
                        ProductionId = Guid.NewGuid().ToString(),
                        Status = "Completed",
                        ProductionMetadata = new Dictionary<string, string>
                        {
                            { "ProcessingTime", "120" },
                            { "Quality", "High" }
                        }
                    });
                }
                else
                {
                    await _publishEndpoint.Publish(new ProductionFailedEvent
                    {
                        CorrelationId = context.Message.CorrelationId,
                        OrderId = context.Message.OrderId,
                        OrderName = context.Message.OrderName,
                        FailureReason = "Production system error",
                        ErrorCode = "PROD_001",
                        FailedAt = DateTime.UtcNow,
                        ErrorDetails = new Dictionary<string, string>
                        {
                            { "ErrorType", "SystemError" },
                            { "Component", "ImageProcessor" }
                        },
                        CanRetry = true
                    });
                }
            }
            catch (Exception ex)
            {
                await _publishEndpoint.Publish(new ProductionFailedEvent
                {
                    CorrelationId = context.Message.CorrelationId,
                    OrderId = context.Message.OrderId,
                    OrderName = context.Message.OrderName,
                    FailureReason = ex.Message,
                    ErrorCode = "SYS_001",
                    FailedAt = DateTime.UtcNow,
                    ErrorDetails = new Dictionary<string, string>
                    {
                        { "ErrorType", "UnhandledException" },
                        { "StackTrace", ex.StackTrace }
                    },
                    CanRetry = false
                });
            }
        }
    }
}
