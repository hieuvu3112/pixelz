using System;
using System.Collections.Generic;

namespace Shared.Commands
{
    public record InitiatePaymentCommand
    {
        public Guid CorrelationId { get; init; }
        public string OrderId { get; init; }
        public decimal Amount { get; init; }
        public string CustomerId { get; init; }
        public string Currency { get; init; } = "USD";
    }

    public record InitiateProductionCommand
    {
        public Guid CorrelationId { get; init; }
        public string OrderId { get; init; }
        public string OrderName { get; init; }
        public Dictionary<string, string> ProductionMetadata { get; init; }
    }

    public record OrderFailedNotification
    {
        public Guid CorrelationId { get; init; }
        public string OrderId { get; init; }
        public string FailureReason { get; init; }
        public DateTime FailedAt { get; init; }
        public string ErrorCode { get; init; }
        public bool CanRetry { get; init; }
    }

    public record OrderCompletedNotification
    {
        public Guid CorrelationId { get; init; }
        public string OrderId { get; init; }
        public DateTime CompletedAt { get; init; }
        public string ProductionId { get; init; }
        public Dictionary<string, string> CompletionMetadata { get; init; }
    }
}
