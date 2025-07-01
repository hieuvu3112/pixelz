using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Commands;
using Shared.Events;

namespace TransactionService.Infrastructure.Handlers
{
    public class InitiateProductionCommandHandler : IConsumer<InitiateProductionCommand>
    {
        private readonly ILogger<InitiateProductionCommandHandler> _logger;
        private readonly ITopicProducer<InitiateProductionCommand> _producer;

        public InitiateProductionCommandHandler(ILogger<InitiateProductionCommandHandler> logger,
            ITopicProducer<InitiateProductionCommand> producer)
        {
            _logger = logger;
            _producer = producer;
        }

        public async Task Consume(ConsumeContext<InitiateProductionCommand> context)
        {
            _logger.LogInformation("Starting production for Order {OrderId}: {OrderName}", 
                context.Message.OrderId, context.Message.OrderName);
            await _producer.Produce(context.Message, context.CancellationToken);
        }
    }
}
