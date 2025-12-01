using LegacyOrderService.Application.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LegacyOrderService.Infrastructure.Persistence.Sqlite
{
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly Dictionary<string, double> _prices = new()
        {
            ["Widget"] = 12.99,
            ["Gadget"] = 15.50,
            ["Doohickey"] = 8.75
        };

        public Task<double> GetPriceAsync(string productName, CancellationToken ct = default)
        {
            if (_prices.TryGetValue(productName, out var price))
            {
                return Task.FromResult(price);
            }

            throw new KeyNotFoundException($"Product '{productName}' not found.");
        }
    }
}
