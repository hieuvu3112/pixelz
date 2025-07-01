using System.Threading.Tasks;
using PaymentService.Application.Models;

namespace PaymentService.Application.Services.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest paymentRequest);
} 