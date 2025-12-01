using System.Threading;
using System.Threading.Tasks;

namespace LegacyOrderService.Infrastructure.Caching
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
        Task SetAsync<T>(string key, T value, CancellationToken ct = default);
    }
}
