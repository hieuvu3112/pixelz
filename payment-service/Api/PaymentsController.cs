using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Models;
using PaymentService.Application.Services.Interfaces;

namespace PaymentService.Api
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(ILogger<PaymentsController> logger)
        {
            _logger = logger;
        }

        // POST /payments
        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request, [FromServices] IPaymentService paymentService)
        {
            _logger.LogInformation($"Processing payment for order: {request.OrderId}");
            try
            {
                var result = await paymentService.ProcessPaymentAsync(request);
                
                return Ok(new { 
                    Success = result.Success,
                    TransactionId = result.TransactionId,
                    Message = result.Success ? "Payment processed successfully" : "Payment failed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing payment for order {request.OrderId}");
                return StatusCode(500, "An error occurred during payment processing");
            }
        }
    }
}
