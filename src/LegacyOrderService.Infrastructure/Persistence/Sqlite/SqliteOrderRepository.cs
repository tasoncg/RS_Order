using LegacyOrderService.Application.Interfaces;
using LegacyOrderService.Domain.Entities;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LegacyOrderService.Infrastructure.Persistence.Sqlite
{
    public class SqliteOrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        public SqliteOrderRepository()
        {
            var dbPath = Path.Combine(AppContext.BaseDirectory, "orders.db");
            _connectionString = $"Data Source={dbPath}";
        }

        public async Task SaveAsync(Order order, CancellationToken ct = default)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using (var create = conn.CreateCommand())
            {
                create.CommandText =
                    @"CREATE TABLE IF NOT EXISTS Orders (
                        Id TEXT PRIMARY KEY,
                        CustomerName TEXT,
                        ProductName TEXT,
                        Quantity INTEGER,
                        Price REAL,
                        CreatedAt TEXT
                    );";
                await create.ExecuteNonQueryAsync(ct);
            }

            await using var cmd = conn.CreateCommand();
            cmd.CommandText =
                @"INSERT OR REPLACE INTO Orders 
                    (Id, CustomerName, ProductName, Quantity, Price, CreatedAt) 
                  VALUES (@id, @cn, @pn, @q, @p, @ca);";
            cmd.Parameters.AddWithValue("@id", order.Id.ToString());
            cmd.Parameters.AddWithValue("@cn", order.CustomerName ?? string.Empty);
            cmd.Parameters.AddWithValue("@pn", order.ProductName ?? string.Empty);
            cmd.Parameters.AddWithValue("@q", order.Quantity);
            cmd.Parameters.AddWithValue("@p", order.Price);
            cmd.Parameters.AddWithValue("@ca", order.CreatedAt.ToString("o"));

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<IEnumerable<Order>> LoadAllAsync(CancellationToken ct = default)
        {
            var result = new List<Order>();

            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, CustomerName, ProductName, Quantity, Price, CreatedAt FROM Orders;";
            await using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                var o = new Order
                {
                    Id = Guid.Parse(reader.GetString(0)),
                    CustomerName = reader.GetString(1),
                    ProductName = reader.GetString(2),
                    Quantity = reader.GetInt32(3),
                    Price = reader.GetDouble(4),
                    CreatedAt = DateTimeOffset.Parse(reader.GetString(5))
                };
                result.Add(o);
            }

            return result;
        }
    }
}
