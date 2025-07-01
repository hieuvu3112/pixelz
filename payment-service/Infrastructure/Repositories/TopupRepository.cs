using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Repositories;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Infrastructure.Repositories
{
    public class TopupRepository : ITopupRepository
    {
        private readonly PaymentDbContext _context;

        public TopupRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<Topup> GetByIdAsync(string id)
        {
            return await _context.Topups.FindAsync(id);
        }

        public async Task<List<Topup>> GetAvailableTopupsByCustomerIdAsync(string customerId)
        {
            return await _context.Topups
                .Where(t => t.CustomerId == customerId 
                           && t.IsActive 
                           && t.AvailableAmount > 0
                           && (t.ExpiresAt == null || t.ExpiresAt > DateTime.UtcNow))
                .OrderBy(t => t.CreatedAt) // FIFO - First In, First Out
                .ToListAsync();
        }

        public async Task<Topup> CreateAsync(Topup topup)
        {
            topup.Id = Guid.NewGuid().ToString();
            topup.CreatedAt = DateTime.UtcNow;
            topup.UpdatedAt = DateTime.UtcNow;
            topup.IsActive = true;

            _context.Topups.Add(topup);
            await _context.SaveChangesAsync();
            return topup;
        }

        public async Task<Topup> UpdateAsync(Topup topup)
        {
            topup.UpdatedAt = DateTime.UtcNow;
            _context.Topups.Update(topup);
            await _context.SaveChangesAsync();
            return topup;
        }

        public async Task DeleteAsync(string id)
        {
            var topup = await GetByIdAsync(id);
            if (topup != null)
            {
                _context.Topups.Remove(topup);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Topup>> GetTopupsByCustomerIdAsync(string customerId)
        {
            return await _context.Topups
                .Where(t => t.CustomerId == customerId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAvailableAmountByCustomerIdAsync(string customerId)
        {
            return await _context.Topups
                .Where(t => t.CustomerId == customerId 
                           && t.IsActive 
                           && t.AvailableAmount > 0
                           && (t.ExpiresAt == null || t.ExpiresAt > DateTime.UtcNow))
                .SumAsync(t => t.AvailableAmount);
        }
    }
}
