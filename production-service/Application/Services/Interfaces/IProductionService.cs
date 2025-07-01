using System.Threading.Tasks;
using ProductionService.Application.Models.Requests;
using ProductionService.Application.Models.Responses;

namespace ProductionService.Application.Services.Interfaces;

public interface IProductionService
{
    Task<ProductionResponse> ProcessOrderAsync(ProductionRequest request);
}