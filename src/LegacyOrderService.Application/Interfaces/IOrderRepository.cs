using LegacyOrderService.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LegacyOrderService.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task SaveAsync(Order order, CancellationToken ct = default);
        Task<IEnumerable<Order>> LoadAllAsync(CancellationToken ct = default);
    }
}
