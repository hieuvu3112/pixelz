using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using System.Text.Json;

namespace PaymentService.Infrastructure
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
            : base(options)
        {
        }

        public DbSet<Topup> Topups { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Topup entity
            modelBuilder.Entity<Topup>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomerId).IsRequired().HasMaxLength(255);
                entity.Property(e => e.OriginalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.AvailableAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.UsedAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Currency).HasMaxLength(3).HasDefaultValue("USD");
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.TransactionReference).HasMaxLength(255);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => new { e.CustomerId, e.IsActive, e.AvailableAmount });
            });

            // Configure PaymentTransaction entity
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderId).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CustomerId).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Currency).HasMaxLength(3).HasDefaultValue("USD");
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.TransactionReference).HasMaxLength(255);
                entity.Property(e => e.FailureReason).HasMaxLength(1000);
                entity.Property(e => e.ErrorCode).HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                // Configure TopupUsages as JSON
                entity.Property(e => e.TopupUsages)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                        v => JsonSerializer.Deserialize<List<TopupUsage>>(v, JsonSerializerOptions.Default));

                entity.HasIndex(e => e.OrderId).IsUnique();
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}
