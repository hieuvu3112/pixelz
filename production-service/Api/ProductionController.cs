using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductionService.Application.Models.Requests;
using ProductionService.Application.Services.Interfaces;

namespace ProductionService.Api
{
    [ApiController]
    [Route("[controller]")]
    public class ProductionController : ControllerBase
    {
        private readonly IProductionService _productionService;
        private readonly ILogger<ProductionController> _logger;

        public ProductionController(IProductionService productionService, ILogger<ProductionController> logger)
        {
            _productionService = productionService;
            _logger = logger;
        }

        // POST /production/orders
        [HttpPost("orders")]
        public async Task<IActionResult> UpdateOrderState([FromBody] ProductionRequest request)
        {
            _logger.LogInformation($"Sending order to production: {request.OrderId}");
            try
            {
                var result = await _productionService.ProcessOrderAsync(request);
                
                return Ok(new { 
                    Success = result.Success,
                    Message = result.Success ? 
                        "Order successfully sent to production" : 
                        "Failed to send order to production"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing production for order {request.OrderId}");
                return StatusCode(500, "An error occurred during production processing");
            }
        }
    }
}
