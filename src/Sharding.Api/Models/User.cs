namespace Sharding.Api.Models;

public sealed record User(
    Guid UserId,
    string Name,
    string Email,
    DateTime CreatedAt
);

public sealed record CreateUserRequest(
    string Name,
    string Email
);