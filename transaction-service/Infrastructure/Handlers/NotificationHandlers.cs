using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Commands;
using Shared.Events;

namespace TransactionService.Infrastructure.Handlers
{
    public class OrderCompletedNotificationHandler : IConsumer<OrderCompletedNotification>
    {
        private readonly ILogger<OrderCompletedNotificationHandler> _logger;

        public OrderCompletedNotificationHandler(ILogger<OrderCompletedNotificationHandler> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCompletedNotification> context)
        {
            _logger.LogInformation("Order {OrderId} completed successfully. Production ID: {ProductionId}", 
                context.Message.OrderId, context.Message.ProductionId);
            
            await Task.CompletedTask;
        }
    }

    public class OrderFailedNotificationHandler : IConsumer<OrderFailedNotification>
    {
        private readonly ILogger<OrderFailedNotificationHandler> _logger;

        public OrderFailedNotificationHandler(ILogger<OrderFailedNotificationHandler> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderFailedNotification> context)
        {
            _logger.LogError("Order {OrderId} failed. Reason: {FailureReason}", 
                context.Message.OrderId, context.Message.FailureReason);
            
            await Task.CompletedTask;
        }
    }
}
