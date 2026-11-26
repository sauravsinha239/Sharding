namespace Sharding.Api.Configuration;

public sealed class ShardOptions
{
    public const string SectionName = "Sharding";

    public List<ShardDefinition> Shards { get; set; } = [];
}

public sealed class ShardDefinition
{
    public string Id { get; set; } = string.Empty;

    public string ConnectionString { get; set; } = string.Empty;
}