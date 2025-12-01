using System;

namespace LegacyOrderService.Application.Commands.CreateOrder
{
    public sealed class CreateOrderCommand
    {
        public CreateOrderCommand(string customerName, string productName, int quantity)
        {
            CustomerName = customerName;
            ProductName = productName;
            Quantity = quantity;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public string CustomerName { get; }
        public string ProductName { get; }
        public int Quantity { get; }
    }
}
