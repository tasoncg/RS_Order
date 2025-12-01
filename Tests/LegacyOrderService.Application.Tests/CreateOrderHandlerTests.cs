using LegacyOrderService.Application.Commands.CreateOrder;
using LegacyOrderService.Application.Interfaces;
using LegacyOrderService.Domain.Entities;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Xunit;
namespace LegacyOrderService.Application.Tests
{
    public class CreateOrderHandlerTests
    {
        [Fact]
        public async Task Handle_SavesOrder_WhenValid()
        {
            var repo = new Mock<IOrderRepository>();
            var products = new Mock<IProductRepository>();

            products.Setup(p => p.GetPriceAsync("Widget", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(10.0);

            var handler = new CreateOrderHandler(repo.Object, products.Object);
            var cmd = new CreateOrderCommand("Alice", "Widget", 2);

            var result = await handler.Handle(cmd, CancellationToken.None);

            repo.Verify(r => r.SaveAsync(It.Is<Order>(o =>
                    o.CustomerName == "Alice" &&
                    o.ProductName == "Widget" &&
                    o.Quantity == 2 &&
                    Math.Abs(o.Price - 20.0) < 0.001),
                It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.Equal("Alice", result.CustomerName);
            Assert.Equal("Widget", result.ProductName);
            Assert.Equal(2, result.Quantity);
            Assert.Equal(20.0, result.Price, 3);
        }

        [Fact]
        public async Task Handle_Throws_OnInvalidInput()
        {
            var repo = new Mock<IOrderRepository>();
            var products = new Mock<IProductRepository>();
            var handler = new CreateOrderHandler(repo.Object, products.Object);

            var cmd = new CreateOrderCommand("", "", 0);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                handler.Handle(cmd, CancellationToken.None));
        }
    }
}
