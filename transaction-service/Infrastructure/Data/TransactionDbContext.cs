using MassTransit;
using Microsoft.EntityFrameworkCore;
using MassTransit.EntityFrameworkCoreIntegration;
using TransactionService.Infrastructure.StateMachines;

namespace TransactionService.Infrastructure.Data;

public class TransactionDbContext: DbContext
{
    public TransactionDbContext(DbContextOptions<TransactionDbContext> options)
        : base(options)
    {
    }

    public DbSet<OrderWorkflowState> OrderWorkflows { get; set; }
    public DbSet<OutboxState> OutboxStates { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<OrderWorkflowState>(entity =>
        {
            entity.HasKey(x => x.CorrelationId);
            entity.Property(x => x.RowVersion).IsRowVersion();
        });
        
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
    }
}
