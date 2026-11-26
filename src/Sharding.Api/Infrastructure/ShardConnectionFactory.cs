using Npgsql;
using Sharding.Api.Configuration;
using Microsoft.Extensions.Options;

namespace Sharding.Api.Infrastructure;

public sealed class ShardConnectionFactory : IDisposable
{
    private readonly Dictionary<string, NpgsqlDataSource> _dataSources;

    public ShardConnectionFactory(IOptions<ShardOptions> options)
    {
        _dataSources = new(StringComparer.OrdinalIgnoreCase);

        foreach (var shard in options.Value.Shards)
        {
            if (string.IsNullOrWhiteSpace(shard.Id) ||
                string.IsNullOrWhiteSpace(shard.ConnectionString))
            {
                throw new InvalidOperationException(
                    "Shard ID and connection string are required.");
            }

            var dataSource = NpgsqlDataSource.Create(
                shard.ConnectionString);

            if (!_dataSources.TryAdd(shard.Id, dataSource))
            {
                dataSource.Dispose();

                throw new InvalidOperationException(
                    $"Duplicate shard ID: {shard.Id}");
            }
        }
    }

    public NpgsqlDataSource GetDataSource(string shardId)
    {
        if (!_dataSources.TryGetValue(shardId, out var dataSource))
        {
            throw new InvalidOperationException(
                $"Shard '{shardId}' is not configured.");
        }

        return dataSource;
    }

    public void Dispose()
    {
        foreach (var dataSource in _dataSources.Values)
        {
            dataSource.Dispose();
        }

        _dataSources.Clear();
    }
}