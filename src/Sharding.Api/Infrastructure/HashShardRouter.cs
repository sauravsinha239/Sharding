using System.Security.Cryptography;
using Sharding.Api.Configuration;
using Microsoft.Extensions.Options;

namespace Sharding.Api.Infrastructure;

public sealed class HashShardRouter : IShardRouter
{
    private readonly string[] _shardIds;

    public HashShardRouter(IOptions<ShardOptions> options)
    {
        _shardIds = options.Value.Shards
            .Select(x => x.Id)
            .ToArray();

        if (_shardIds.Length == 0)
            throw new InvalidOperationException("No database shards configured.");

        if (_shardIds.Distinct(StringComparer.OrdinalIgnoreCase).Count()
            != _shardIds.Length)
        {
            throw new InvalidOperationException("Duplicate shard IDs.");
        }
    }

    public string GetShardId(Guid userId)
    {
        byte[] hash = SHA256.HashData(userId.ToByteArray());

        uint value = BitConverter.ToUInt32(hash, 0);

        int index = (int)(value % (uint)_shardIds.Length);

        return _shardIds[index];
    }
}