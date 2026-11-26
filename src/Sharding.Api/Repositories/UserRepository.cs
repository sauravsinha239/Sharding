using Npgsql;
using Sharding.Api.Infrastructure;
using Sharding.Api.Models;

namespace Sharding.Api.Repositories;

public sealed class UserRepository(
    IShardRouter router,
    ShardConnectionFactory connectionFactory)
{
    public async Task<User> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = Guid.NewGuid();

        var shardId = router.GetShardId(userId);

        var dataSource = connectionFactory.GetDataSource(shardId);

        await using var connection =
            await dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            INSERT INTO users (user_id, name, email)
            VALUES (@userId, @name, @email)
            RETURNING user_id, name, email, created_at;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("userId", userId);
        command.Parameters.AddWithValue("name", request.Name);
        command.Parameters.AddWithValue("email", request.Email);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        await reader.ReadAsync(cancellationToken);

        return new User(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetDateTime(3)
        );
    }

    public async Task<User?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var shardId = router.GetShardId(userId);

        var dataSource = connectionFactory.GetDataSource(shardId);

        await using var connection =
            await dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT user_id, name, email, created_at
            FROM users
            WHERE user_id = @userId;
            """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("userId", userId);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new User(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetDateTime(3)
        );
    }
}