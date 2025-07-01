using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Repositories;
using PaymentService.Infrastructure;
using Shared.Commands;
using Shared.Events;

namespace PaymentService.Application.Handlers
{
    public class InitiatePaymentCommandHandler : IConsumer<InitiatePaymentCommand>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ITopupRepository _topupRepository;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly PaymentDbContext _dbContext;
        private readonly ILogger<InitiatePaymentCommandHandler> _logger;

        public InitiatePaymentCommandHandler(
            IPublishEndpoint publishEndpoint,
            ITopupRepository topupRepository,
            IPaymentTransactionRepository paymentTransactionRepository,
            PaymentDbContext dbContext,
            ILogger<InitiatePaymentCommandHandler> logger)
        {
            _publishEndpoint = publishEndpoint;
            _topupRepository = topupRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<InitiatePaymentCommand> context)
        {
            var transactionId = Guid.NewGuid().ToString();

            try
            {
                _logger.LogInformation("Processing payment for Order {OrderId}, Amount: {Amount}, Customer: {CustomerId}", 
                    context.Message.OrderId, context.Message.Amount, context.Message.CustomerId);

                var strategy = _dbContext.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _dbContext.Database.BeginTransactionAsync();
                    try
                    {
                        var usedTopups = await ProcessPaymentInTransaction(context, transactionId);
                        
                        // Commit the database transaction
                        await transaction.CommitAsync();
                        
                        // Publish success event after successful commit
                        await PublishPaymentSucceededEvent(context, transactionId, usedTopups);

                        _logger.LogInformation("Payment completed successfully for Order {OrderId}, Transaction {TransactionId}", 
                            context.Message.OrderId, transactionId);
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment processing failed for Order {OrderId}", context.Message.OrderId);
                await PublishPaymentFailedEvent(context, ex.Message, "SYS_001");
            }
        }

        private async Task<List<TopupUsage>> ProcessPaymentInTransaction(ConsumeContext<InitiatePaymentCommand> context, string transactionId)
        {
            var usedTopups = new List<TopupUsage>();

            var availableTopups = await _topupRepository.GetAvailableTopupsByCustomerIdAsync(context.Message.CustomerId);
            
            if (!availableTopups.Any())
            {
                throw new InvalidOperationException("No topup funds available");
            }

            var totalAvailableAmount = availableTopups.Sum(t => t.AvailableAmount);
            if (totalAvailableAmount < context.Message.Amount)
            {
                throw new InvalidOperationException(
                    $"Insufficient funds. Available: {totalAvailableAmount}, Required: {context.Message.Amount}");
            }

            var remainingAmount = context.Message.Amount;
            foreach (var topup in availableTopups)
            {
                if (remainingAmount <= 0) break;

                var amountToDeduct = Math.Min(remainingAmount, topup.AvailableAmount);
                
                // Deduct from topup
                topup.AvailableAmount -= amountToDeduct;
                topup.UsedAmount += amountToDeduct;
                topup.UpdatedAt = DateTime.UtcNow;

                // Track which topups were used
                usedTopups.Add(new TopupUsage
                {
                    TopupId = topup.Id,
                    AmountUsed = amountToDeduct,
                    RemainingBalance = topup.AvailableAmount
                });

                // Update topup in database
                await _topupRepository.UpdateAsync(topup);

                remainingAmount -= amountToDeduct;

                _logger.LogInformation("Deducted {Amount} from Topup {TopupId}, Remaining topup balance: {Balance}", 
                    amountToDeduct, topup.Id, topup.AvailableAmount);
            }

            // Create payment transaction record
            var paymentTransaction = new PaymentTransaction
            {
                Id = transactionId,
                OrderId = context.Message.OrderId,
                CustomerId = context.Message.CustomerId,
                Amount = context.Message.Amount,
                Currency = context.Message.Currency ?? "USD",
                Status = PaymentStatus.Completed,
                PaymentMethod = "Topup",
                ProcessedAt = DateTime.UtcNow,
                TopupUsages = usedTopups
            };

            await _paymentTransactionRepository.CreateAsync(paymentTransaction);
            return usedTopups;
        }

        private async Task PublishPaymentSucceededEvent(ConsumeContext<InitiatePaymentCommand> context, string transactionId, List<TopupUsage> usedTopups)
        {
            await _publishEndpoint.Publish(new PaymentSucceededEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                Amount = context.Message.Amount,
                TransactionId = transactionId,
                ProcessedAt = DateTime.UtcNow,
                PaymentMethod = "Topup",
                Currency = context.Message.Currency ?? "USD",
                TopupsUsed = usedTopups.Select(u => new TopupUsageInfo
                {
                    TopupId = u.TopupId,
                    AmountUsed = u.AmountUsed
                }).ToList()
            });
        }

        private async Task PublishPaymentFailedEvent(ConsumeContext<InitiatePaymentCommand> context, string reason, string errorCode)
        {
            // Determine the appropriate error code based on the reason
            var finalErrorCode = errorCode;
            if (reason.Contains("No topup funds available"))
                finalErrorCode = "PAY_NO_TOPUP";
            else if (reason.Contains("Insufficient funds"))
                finalErrorCode = "PAY_INSUFFICIENT_FUNDS";

            await _publishEndpoint.Publish(new PaymentFailedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                FailureReason = reason,
                ErrorCode = finalErrorCode,
                FailedAt = DateTime.UtcNow,
                RetryCount = 0
            });

            _logger.LogWarning("Payment failed for Order {OrderId}: {Reason}", context.Message.OrderId, reason);
        }
    }
}
