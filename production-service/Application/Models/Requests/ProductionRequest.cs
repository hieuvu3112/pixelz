using System;

namespace ProductionService.Application.Models.Requests;

public class ProductionRequest
{
    public int OrderId { get; set; }
    public string OrderName { get; set; }
    public Guid CorrelationId { get; set; }
}