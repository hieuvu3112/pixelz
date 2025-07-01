using System;

namespace OrderService.Domain;

public class Order
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "created";
    public int CustomerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
}
