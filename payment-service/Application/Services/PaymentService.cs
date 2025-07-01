using System;
using System.Threading.Tasks;
using PaymentService.Application.Models;
using PaymentService.Application.Services.Interfaces;
using PaymentService.Domain.Repositories;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Services;

public class PaymentService: IPaymentService
{
    private readonly ITopupRepository _topupRepository;

    public PaymentService(ITopupRepository topupRepository)
    {
        _topupRepository = topupRepository;
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest paymentRequest)
    {
        // Simulate payment processing
        var transactionId = $"TXN{DateTime.UtcNow.Ticks}";
        
        // Create topup record
        var topup = new Topup
        {
            Id = Guid.NewGuid().ToString(),
            AvailableAmount = paymentRequest.Amount,
        };

        await _topupRepository.CreateAsync(topup);

        return new PaymentResponse()
        {
            Success = true,
            TransactionId = transactionId,
        };
    }
}
