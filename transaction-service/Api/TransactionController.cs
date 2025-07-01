
using System;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace TransactionService.Api;

[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    
    // GET
    public ActionResult Get()
    {
        return Ok(new {message = "Transaction Service is running"});
    }

    public ActionResult Post([FromServices] IServiceProvider serviceProvider)
    {
        var busControl = serviceProvider.GetService<IBusControl>();
        if (busControl == null)
        {
            return StatusCode(500, "Bus control is not configured.");
        }
        return Ok(new {message = "Transaction Service is running"});
    }
}