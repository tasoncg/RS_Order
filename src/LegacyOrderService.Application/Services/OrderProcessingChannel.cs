using LegacyOrderService.Application.Commands.CreateOrder;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace LegacyOrderService.Application.Services
{
    public class OrderProcessingChannel
    {
        private readonly Channel<CreateOrderCommand> _channel;

        public OrderProcessingChannel()
        {
            var options = new BoundedChannelOptions(256)
            {
                SingleReader = false,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            };
            _channel = Channel.CreateBounded<CreateOrderCommand>(options);
        }

        public ValueTask WriteAsync(CreateOrderCommand cmd, CancellationToken ct = default) =>
            _channel.Writer.WriteAsync(cmd, ct);

        public IAsyncEnumerable<CreateOrderCommand> ReadAllAsync(CancellationToken ct = default) =>
            _channel.Reader.ReadAllAsync(ct);
    }
}
