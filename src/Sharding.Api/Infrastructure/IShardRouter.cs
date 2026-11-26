namespace Sharding.Api.Infrastructure;

public interface IShardRouter
{
    string GetShardId(Guid userId);
}