using System;
using MassTransit;
using Shared.Commands;
using Shared.Events;

namespace TransactionService.Infrastructure.StateMachines
{
    public class OrderWorkflowState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public int OrderId { get; set; }
        public string OrderName { get; set; }
        public decimal Amount { get; set; }
        public int CustomerId { get; set; }
        public string CurrentState { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string TransactionId { get; set; }
        public string ProductionId { get; set; }
        public int RetryCount { get; set; }
        public byte[] RowVersion { get; set; }
    }

    public class OrderWorkflow : MassTransitStateMachine<OrderWorkflowState>
    {
        public OrderWorkflow()
        {
            // Configure the instance state property
            InstanceState(x => x.CurrentState);

            // Define events correlation
            Event(() => OrderCheckedOut, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => PaymentSucceeded, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => PaymentFailed, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => ProductionCompleted, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => ProductionFailed, x => x.CorrelateById(context => context.Message.CorrelationId));

            // Define initial state
            Initially(
                When(OrderCheckedOut)
                    .Then(context =>
                    {
                        context.Saga.OrderId = context.Message.OrderId;
                        context.Saga.OrderName = context.Message.OrderName;
                        context.Saga.Amount = context.Message.Amount;
                        context.Saga.CustomerId = context.Message.CustomerId;
                        context.Saga.CreatedAt = context.Message.CheckoutTime;
                        context.Saga.RetryCount = 0;
                    })
                    .TransitionTo(AwaitingPayment)
                    .ThenAsync(async context =>
                    {
                        await context.Publish<InitiatePaymentCommand>(new
                        {
                            context.Message.CorrelationId,
                            context.Message.OrderId,
                            context.Message.Amount,
                            context.Message.CustomerId
                        });
                    })
            );

            // Payment state transitions
            During(AwaitingPayment,
                When(PaymentSucceeded)
                    .Then(context =>
                    {
                        context.Saga.TransactionId = context.Message.TransactionId;
                    })
                    .TransitionTo(AwaitingProduction)
                    .ThenAsync(async context =>
                    {
                        await context.Publish<InitiateProductionCommand>(new
                        {
                            context.Message.CorrelationId,
                            context.Saga.OrderId,
                            context.Saga.OrderName
                        });
                    }),
                
                When(PaymentFailed)
                    .Then(context =>
                    {
                        context.Saga.ErrorMessage = context.Message.FailureReason;
                        context.Saga.RetryCount = context.Message.RetryCount;
                    })
                    .TransitionTo(Failed)
                    .ThenAsync(async context =>
                    {
                        await context.Publish<OrderFailedNotification>(new
                        {
                            context.Message.CorrelationId,
                            context.Saga.OrderId,
                            FailureReason = context.Message.FailureReason,
                            FailedAt = context.Message.FailedAt
                        });
                    })
            );

            // Production state transitions
            During(AwaitingProduction,
                When(ProductionCompleted)
                    .Then(context =>
                    {
                        context.Saga.ProductionId = context.Message.ProductionId;
                        context.Saga.CompletedAt = context.Message.CompletedAt;
                    })
                    .TransitionTo(Completed)
                    .ThenAsync(async context =>
                    {
                        await context.Publish<OrderCompletedNotification>(new
                        {
                            context.Message.CorrelationId,
                            context.Saga.OrderId,
                            context.Message.CompletedAt,
                            context.Message.ProductionId
                        });
                    }),
                
                When(ProductionFailed)
                    .Then(context =>
                    {
                        context.Saga.ErrorMessage = context.Message.FailureReason;
                    })
                    .TransitionTo(Failed)
                    .ThenAsync(async context =>
                    {
                        await context.Publish<OrderFailedNotification>(new
                        {
                            context.Message.CorrelationId,
                            context.Saga.OrderId,
                            FailureReason = context.Message.FailureReason,
                            FailedAt = context.Message.FailedAt
                        });
                    })
            );

            // Set final states
            SetCompletedWhenFinalized();
        }

        // State machine states
        public State AwaitingPayment { get; private set; }
        public State AwaitingProduction { get; private set; }
        public State Completed { get; private set; }
        public State Failed { get; private set; }

        // Events
        public Event<OrderCheckedOutEvent> OrderCheckedOut { get; private set; }
        public Event<PaymentSucceededEvent> PaymentSucceeded { get; private set; }
        public Event<PaymentFailedEvent> PaymentFailed { get; private set; }
        public Event<ProductionCompletedEvent> ProductionCompleted { get; private set; }
        public Event<ProductionFailedEvent> ProductionFailed { get; private set; }
    }
}
