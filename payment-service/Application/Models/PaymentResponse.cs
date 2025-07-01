namespace PaymentService.Application.Models;

public class PaymentResponse
{
    public bool Success { get; set; }
    public string TransactionId { get; set; }
}