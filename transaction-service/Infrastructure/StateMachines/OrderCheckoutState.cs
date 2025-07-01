using System;
using MassTransit;

namespace TransactionService.Infrastructure.StateMachines
{
    public class OrderCheckoutState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        
        public int OrderId { get; set; }
        public string OrderName { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        
        public string CurrentState { get; set; }
        
        public string ErrorMessage { get; set; }
        
        // For optimistic concurrency
        public byte[] RowVersion { get; set; }
        
        // For scheduling timeouts
        public Guid? CheckoutTimeoutTokenId { get; set; }
    }
}
