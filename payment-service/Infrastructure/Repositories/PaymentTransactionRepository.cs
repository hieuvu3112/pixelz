using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Repositories;

namespace PaymentService.Infrastructure.Repositories
{
    public class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        private readonly PaymentDbContext _context;

        public PaymentTransactionRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentTransaction> GetByIdAsync(string id)
        {
            return await _context.PaymentTransactions.FindAsync(id);
        }

        public async Task<PaymentTransaction> GetByOrderIdAsync(string orderId)
        {
            return await _context.PaymentTransactions
                .FirstOrDefaultAsync(pt => pt.OrderId == orderId);
        }

        public async Task<List<PaymentTransaction>> GetByCustomerIdAsync(string customerId)
        {
            return await _context.PaymentTransactions
                .Where(pt => pt.CustomerId == customerId)
                .OrderByDescending(pt => pt.CreatedAt)
                .ToListAsync();
        }

        public async Task<PaymentTransaction> CreateAsync(PaymentTransaction transaction)
        {
            if (string.IsNullOrEmpty(transaction.Id))
            {
                transaction.Id = Guid.NewGuid().ToString();
            }
            
            transaction.CreatedAt = DateTime.UtcNow;
            transaction.UpdatedAt = DateTime.UtcNow;

            _context.PaymentTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<PaymentTransaction> UpdateAsync(PaymentTransaction transaction)
        {
            transaction.UpdatedAt = DateTime.UtcNow;
            _context.PaymentTransactions.Update(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task DeleteAsync(string id)
        {
            var transaction = await GetByIdAsync(id);
            if (transaction != null)
            {
                _context.PaymentTransactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<PaymentTransaction>> GetTransactionsByStatusAsync(PaymentStatus status)
        {
            return await _context.PaymentTransactions
                .Where(pt => pt.Status == status)
                .OrderByDescending(pt => pt.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PaymentTransaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.PaymentTransactions
                .Where(pt => pt.CreatedAt >= startDate && pt.CreatedAt <= endDate)
                .OrderByDescending(pt => pt.CreatedAt)
                .ToListAsync();
        }
    }
}
