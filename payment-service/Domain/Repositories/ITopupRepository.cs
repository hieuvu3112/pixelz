using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentService.Domain.Entities;

namespace PaymentService.Domain.Repositories
{
    public interface ITopupRepository
    {
        Task<Topup> GetByIdAsync(string id);
        Task<List<Topup>> GetAvailableTopupsByCustomerIdAsync(string customerId);
        Task<Topup> CreateAsync(Topup topup);
        Task<Topup> UpdateAsync(Topup topup);
        Task DeleteAsync(string id);
        Task<List<Topup>> GetTopupsByCustomerIdAsync(string customerId);
        Task<decimal> GetTotalAvailableAmountByCustomerIdAsync(string customerId);
    }
}
