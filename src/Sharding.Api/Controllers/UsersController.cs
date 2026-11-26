using Microsoft.AspNetCore.Mvc;
using Sharding.Api.Infrastructure;
using Sharding.Api.Models;
using Sharding.Api.Repositories;

namespace Sharding.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController(
    UserRepository repository,
    IShardRouter router) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Name and email are required.");
        }

        var user = await repository.CreateAsync(
            request, cancellationToken);

        var shardId = router.GetShardId(user.UserId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = user.UserId },
            new
            {
                user,
                shardId
            });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(
            id, cancellationToken);

        if (user is null)
            return NotFound();

        return Ok(new
        {
            user,
            shardId = router.GetShardId(id)
        });
    }
}