using System.Threading.Tasks;
using PaymentService.Application.Models;
using PaymentService.Application.Services.Interfaces;

namespace PaymentService.Application.Services;

public class PaymentService: IPaymentService
{
    public Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest paymentRequest)
    {
        return Task.FromResult(new PaymentResponse()
        {
            Success = true,
            TransactionId = "TXN123456789",
        });
    }
}