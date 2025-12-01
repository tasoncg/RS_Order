using LegacyOrderService.Application.Commands.CreateOrder;
using LegacyOrderService.Application.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LegacyOrderService.Api.Workers
{
    public class OrderWorker : BackgroundService
    {
        private readonly ILogger<OrderWorker> _logger;
        private readonly OrderProcessingChannel _channel;
        private readonly CreateOrderHandler _handler;

        public OrderWorker(
            ILogger<OrderWorker> logger,
            OrderProcessingChannel channel,
            CreateOrderHandler handler)
        {
            _logger = logger;
            _channel = channel;
            _handler = handler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderWorker started.");

            await foreach (CreateOrderCommand cmd in _channel.ReadAllAsync(stoppingToken))
            {
                try
                {
                    var order = await _handler.Handle(cmd, stoppingToken);
                    _logger.LogInformation(
                        "Processed order {OrderId} for {Customer} ({Product} x{Qty}, Total={Total})",
                        order.Id, order.CustomerName, order.ProductName, order.Quantity, order.Price);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order for customer {Customer}", cmd.CustomerName);
                }
            }

            _logger.LogInformation("OrderWorker stopping.");
        }
    }
}
