using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LegacyOrderService.Infrastructure.Caching
{
    public class InMemoryCacheService : ICacheService
    {
        private readonly ConcurrentDictionary<string, string> _store = new();

        public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            if (_store.TryGetValue(key, out var json))
            {
                var value = JsonSerializer.Deserialize<T>(json);
                return Task.FromResult<T?>(value);
            }

            return Task.FromResult<T?>(default);
        }

        public Task SetAsync<T>(string key, T value, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(value);
            _store[key] = json;
            return Task.CompletedTask;
        }
    }
}
