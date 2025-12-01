using LegacyOrderService.Application.Interfaces;
using LegacyOrderService.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LegacyOrderService.Application.Commands.CreateOrder
{
    public class CreateOrderHandler
    {
        private readonly IOrderRepository _repo;
        private readonly IProductRepository _productRepo;

        public CreateOrderHandler(IOrderRepository repo, IProductRepository productRepo)
        {
            _repo = repo;
            _productRepo = productRepo;
        }

        public async Task<Order> Handle(CreateOrderCommand cmd, CancellationToken ct = default)
        {
            Validate(cmd);

            var pricePerUnit = await _productRepo.GetPriceAsync(cmd.ProductName, ct);
            var order = new Order
            {
                Id = cmd.Id,
                CustomerName = cmd.CustomerName,
                ProductName = cmd.ProductName,
                Quantity = cmd.Quantity,
                Price = pricePerUnit * cmd.Quantity,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _repo.SaveAsync(order, ct);
            return order;
        }

        private static void Validate(CreateOrderCommand cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd.CustomerName))
                throw new ArgumentException("Customer name is required", nameof(cmd.CustomerName));

            if (string.IsNullOrWhiteSpace(cmd.ProductName))
                throw new ArgumentException("Product name is required", nameof(cmd.ProductName));

            if (cmd.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0", nameof(cmd.Quantity));
        }
    }
}
