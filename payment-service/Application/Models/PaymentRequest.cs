using System;

namespace PaymentService.Application.Models;

public class PaymentRequest
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public Guid CorrelationId { get; set; }
}