using System;
using System.Collections.Generic;

namespace Shared.Events
{
    public class OrderCheckedOutEvent
    {
        public Guid CorrelationId { get; set; }
        public int OrderId { get; set; }
        public string OrderName { get; set; }
        public decimal Amount { get; set; }
        public int CustomerId { get; set; }
        public DateTime CheckoutTime { get; set; }
    }

    public class PaymentSucceededEvent
    {
        public Guid CorrelationId { get; set; }
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string PaymentMethod { get; set; }
        public string Currency { get; set; }
        public IEnumerable<TopupUsageInfo> TopupsUsed { get; set; }
    }

    public class PaymentFailedEvent
    {
        public Guid CorrelationId { get; set; }
        public string OrderId { get; set; }
        public string FailureReason { get; set; }
        public string ErrorCode { get; set; }
        public DateTime FailedAt { get; set; }
        public int RetryCount { get; set; }
    }

    public class ProductionCompletedEvent
    {
        public Guid CorrelationId { get; set; }
        public string OrderId { get; set; }
        public string OrderName { get; set; }
        public DateTime CompletedAt { get; set; }
        public string ProductionId { get; set; }
        public string Status { get; set; }
        public Dictionary<string, string> ProductionMetadata { get; set; }
    }

    public class ProductionFailedEvent
    {
        public Guid CorrelationId { get; set; }
        public int OrderId { get; set; }
        public string OrderName { get; set; }
        public string FailureReason { get; set; }
        public string ErrorCode { get; set; }
        public DateTime FailedAt { get; set; }
        public Dictionary<string, string> ErrorDetails { get; set; }
        public bool CanRetry { get; set; }
    }

    public class TopupUsageInfo
    {
        public string TopupId { get; set; }
        public decimal AmountUsed { get; set; }
    }

    public static class OrderStatus
    {
        public const string Created = "Created";
        public const string CheckedOut = "CheckedOut";
        public const string PaymentPending = "PaymentPending";
        public const string PaymentFailed = "PaymentFailed";
        public const string PaymentSucceeded = "PaymentSucceeded";
        public const string InProduction = "InProduction";
        public const string ProductionFailed = "ProductionFailed";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }
}
