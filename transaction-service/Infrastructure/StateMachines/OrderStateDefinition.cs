using MassTransit;
using TransactionService.Infrastructure.Data;

namespace TransactionService.Infrastructure.StateMachines;

public class OrderStateDefinition: SagaDefinition<OrderWorkflowState>
{
    protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator,
        ISagaConfigurator<OrderWorkflowState> consumerConfigurator, IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.Intervals(100, 500, 1000, 1000, 1000, 1000, 1000));

        endpointConfigurator.UseEntityFrameworkOutbox<TransactionDbContext>(context);
    }
}