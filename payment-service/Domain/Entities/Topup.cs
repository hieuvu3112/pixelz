using System;

namespace PaymentService.Domain.Entities
{
    public class Topup
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal AvailableAmount { get; set; }
        public decimal UsedAmount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionReference { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public string Notes { get; set; }
    }
}
