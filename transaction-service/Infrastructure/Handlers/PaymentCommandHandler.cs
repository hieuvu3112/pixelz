using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Commands;
using Shared.Events;

namespace TransactionService.Infrastructure.Handlers
{
    public class InitiatePaymentCommandHandler : IConsumer<InitiatePaymentCommand>
    {
        private readonly ILogger<InitiatePaymentCommandHandler> _logger;
        private readonly ITopicProducer<InitiatePaymentCommand> _producer;

        public InitiatePaymentCommandHandler(ILogger<InitiatePaymentCommandHandler> logger,
            ITopicProducer<InitiatePaymentCommand> producer)
        {
            _logger = logger;
            _producer = producer;
        }

        public async Task Consume(ConsumeContext<InitiatePaymentCommand> context)
        {
            _logger.LogInformation("Processing payment for Order {OrderId}, Amount: {Amount}", 
                context.Message.OrderId, context.Message.Amount);
            await _producer.Produce(context.Message, context.CancellationToken);
        }
    }
}
