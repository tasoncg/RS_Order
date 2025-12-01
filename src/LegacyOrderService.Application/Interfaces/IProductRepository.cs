using System.Threading;
using System.Threading.Tasks;

namespace LegacyOrderService.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<double> GetPriceAsync(string productName, CancellationToken ct = default);
    }
}
