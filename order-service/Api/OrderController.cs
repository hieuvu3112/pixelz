using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain;
using OrderService.Infrastructure;
using Shared.Events;
using GraphQL;
using GraphQL.Types;
using OrderService.GraphQL;

namespace OrderService.Api;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrderDbContext _context;
    private readonly ITopicProducer<OrderCheckedOutEvent> _producer;
    private readonly IDocumentExecuter _documentExecuter;
    private readonly ISchema _schema;

    public OrderController(OrderDbContext context, ITopicProducer<OrderCheckedOutEvent> producer, 
        IDocumentExecuter documentExecuter, ISchema schema)
    {
        _context = context;
        _producer = producer;
        _documentExecuter = documentExecuter;
        _schema = schema;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string name,
        [FromQuery] string status,
        [FromQuery] int? customerId,
        [FromQuery] decimal? minAmount,
        [FromQuery] decimal? maxAmount,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = @"
            query GetOrders($name: String, $status: String, $customerId: Int, $minAmount: Decimal, $maxAmount: Decimal, $fromDate: DateTime, $toDate: DateTime, $page: Int, $pageSize: Int) {
                orders(name: $name, status: $status, customerId: $customerId, minAmount: $minAmount, maxAmount: $maxAmount, fromDate: $fromDate, toDate: $toDate, page: $page, pageSize: $pageSize) {
                    id
                    name
                    customerId
                    amount
                    status
                    createdAt
                }
            }";

        var result = await _documentExecuter.ExecuteAsync(new ExecutionOptions
        {
            Schema = _schema,
            Query = query,
            Variables = new Inputs(new Dictionary<string, object?>
            {
                ["name"] = name,
                ["status"] = status,
                ["customerId"] = customerId,
                ["minAmount"] = minAmount,
                ["maxAmount"] = maxAmount,
                ["fromDate"] = fromDate,
                ["toDate"] = toDate,
                ["page"] = page,
                ["pageSize"] = pageSize
            }),
            UserContext = new Dictionary<string, object?>
            {
                ["dbContext"] = _context
            }
        });

        if (result.Errors?.Any() == true)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Data);
    }

    [HttpPost("{id}/checkout")]
    public async Task<ActionResult<Order>> CheckoutOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        order.Status = "checked_out";
        await _context.SaveChangesAsync();

        // Publish event to Kafka
        await _producer.Produce(new OrderCheckedOutEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Amount = order.Amount,
            CheckoutTime = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid(),
            OrderName = "TEST_ORDER",
        });
        
        return Ok(new { message = "Order checked out successfully", order });
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOrders), new { id = order.Id }, order);
    }
}
