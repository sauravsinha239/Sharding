using Sharding.Api.Configuration;
using Sharding.Api.Infrastructure;
using Sharding.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<ShardOptions>(
    builder.Configuration.GetSection(ShardOptions.SectionName));

builder.Services.AddSingleton<IShardRouter, HashShardRouter>();

builder.Services.AddSingleton<ShardConnectionFactory>();

builder.Services.AddScoped<UserRepository>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Sharding.Api"
}));

app.Run();