using System.Threading.Tasks;
using ProductionService.Application.Models.Requests;
using ProductionService.Application.Models.Responses;
using ProductionService.Application.Services.Interfaces;

namespace ProductionService.Application.Services;

public class ProductionService: IProductionService
{
    public Task<ProductionResponse> ProcessOrderAsync(ProductionRequest request)
    {
        return Task.FromResult(new ProductionResponse() 
        {
            Success = true
        });
    }
}