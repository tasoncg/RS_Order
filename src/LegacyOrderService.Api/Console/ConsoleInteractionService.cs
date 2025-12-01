using LegacyOrderService.Application.Commands.CreateOrder;
using LegacyOrderService.Application.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LegacyOrderService.Api.Console
{
    public class ConsoleInteractionService : IHostedService
    {
        private readonly ILogger<ConsoleInteractionService> _logger;
        private readonly OrderProcessingChannel _channel;
        private Task? _task;
        private CancellationTokenSource? _cts;

        public ConsoleInteractionService(
            ILogger<ConsoleInteractionService> logger,
            OrderProcessingChannel channel)
        {
            _logger = logger;
            _channel = channel;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _task = Task.Run(() => RunLoopAsync(_cts.Token), CancellationToken.None);
            return Task.CompletedTask;
        }

        private async Task RunLoopAsync(CancellationToken ct)
        {
            _logger.LogInformation("Console UI started. Press ENTER on empty name to exit.");

            while (!ct.IsCancellationRequested)
            {
                System.Console.WriteLine("Welcome to Order Processor!");
                System.Console.Write("Enter customer name (empty to exit): ");
                var name = System.Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name))
                {
                    System.Console.WriteLine("Exiting console UI.");
                    break;
                }

                System.Console.Write("Product name: ");
                var product = System.Console.ReadLine() ?? string.Empty;

                System.Console.Write("Quantity: ");
                var qtyStr = System.Console.ReadLine();
                var qty = int.TryParse(qtyStr, out var q) ? q : 1;

                var cmd = new CreateOrderCommand(name, product, qty);
                try
                {
                    // push into the channel; worker will process
                    await _channel.WriteAsync(cmd, ct);
                    System.Console.WriteLine("Order enqueued for processing.");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Error enqueuing order: {ex.Message}");
                    _logger.LogError(ex, "Error enqueuing order from console");
                }

                System.Console.WriteLine(new string('-', 40));
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts?.Cancel();
            if (_task != null)
            {
                await _task;
            }
        }
    }
}
