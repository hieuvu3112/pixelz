using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Commands;

namespace EmailService.Application.Handlers
{
    public class OrderNotificationHandler : 
        IConsumer<OrderCompletedNotification>,
        IConsumer<OrderFailedNotification>
    {
        private readonly ILogger<OrderNotificationHandler> _logger;

        public OrderNotificationHandler(ILogger<OrderNotificationHandler> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCompletedNotification> context)
        {
            // Mock sending success email
            _logger.LogInformation("Sending order completion email for Order {OrderId}", 
                context.Message.OrderId);
            
            await Task.Delay(100); // Simulate email sending
        }

        public async Task Consume(ConsumeContext<OrderFailedNotification> context)
        {
            // Mock sending failure email
            _logger.LogInformation("Sending order failure email for Order {OrderId}: {Reason}", 
                context.Message.OrderId,
                context.Message.FailureReason);
            
            await Task.Delay(100); // Simulate email sending
        }
    }
}
