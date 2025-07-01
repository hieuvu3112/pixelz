using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Events;

namespace TransactionService.Infrastructure.Consumers;

public class OrderCheckedOutEventConsumer : IConsumer<OrderCheckedOutEvent>
{
    private readonly ILogger<OrderCheckedOutEventConsumer> _logger;
    private readonly IBus _bus;

    public OrderCheckedOutEventConsumer(
        ILogger<OrderCheckedOutEventConsumer> logger,
        IBus bus)
    {
        _logger = logger;
        _bus = bus;
    }

    public async Task Consume(ConsumeContext<OrderCheckedOutEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation("Received OrderCheckedOutEvent: OrderId={OrderId}, Amount={Amount}, CorrelationId={CorrelationId}",
            message.OrderId, message.Amount, message.CorrelationId);

        // Publish event to internal bus so state machine can consume it
        await _bus.Publish<OrderCheckedOutEvent>(message, context.CancellationToken);
        _logger.LogInformation("Order workflow triggered for OrderId={OrderId}", message.OrderId);
    }
}
