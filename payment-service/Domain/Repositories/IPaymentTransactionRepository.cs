using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentService.Domain.Entities;

namespace PaymentService.Domain.Repositories
{
    public interface IPaymentTransactionRepository
    {
        Task<PaymentTransaction> GetByIdAsync(string id);
        Task<PaymentTransaction> GetByOrderIdAsync(string orderId);
        Task<List<PaymentTransaction>> GetByCustomerIdAsync(string customerId);
        Task<PaymentTransaction> CreateAsync(PaymentTransaction transaction);
        Task<PaymentTransaction> UpdateAsync(PaymentTransaction transaction);
        Task DeleteAsync(string id);
        Task<List<PaymentTransaction>> GetTransactionsByStatusAsync(PaymentStatus status);
        Task<List<PaymentTransaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
