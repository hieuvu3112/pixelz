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

namespace OrderService.Api;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrderDbContext _context;
    private readonly ITopicProducer<OrderCheckedOutEvent> _producer;

    public OrderController(OrderDbContext context, ITopicProducer<OrderCheckedOutEvent> producer)
    {
        _context = context;
        _producer = producer;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders([FromQuery] string name)
    {
        var query = _context.Orders.AsQueryable();
        
        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(o => o.Name.Contains(name));
        }
        
        return await query.ToListAsync();
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
