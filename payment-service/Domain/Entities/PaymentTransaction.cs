using System;
using System.Collections.Generic;

namespace PaymentService.Domain.Entities
{
    public class PaymentTransaction
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public PaymentStatus Status { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionReference { get; set; }
        public DateTime ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string FailureReason { get; set; }
        public string ErrorCode { get; set; }
        public int RetryCount { get; set; }
        public List<TopupUsage> TopupUsages { get; set; } = new List<TopupUsage>();
        public string Notes { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled,
        Refunded
    }

    public class TopupUsage
    {
        public string TopupId { get; set; }
        public decimal AmountUsed { get; set; }
        public decimal RemainingBalance { get; set; }
    }
}
