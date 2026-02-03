using System;
using System.Text.Json;
using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
    const string GetGameEndpointName = "GetGame";

    public static RouteGroupBuilder MapGamesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/games")
                        .WithParameterValidation();

        // GET /games
        group.MapGet("/", async (GameStoreContext dbContext) =>
            await dbContext.Games
            .Include(genre => genre.Genre)
                .Select(game => game.ToGameSummaryDto())
                .AsNoTracking()
                .ToListAsync());

        // GET /games/1
        group.MapGet("/{id}", async (
            int id,
            GameStoreContext dbContext,
            IDistributedCache cache,
            ILogger<Program> logger) =>
        {
            var cachedJson = await cache.GetStringAsync($"game:{id}");
            if (cachedJson is not null)
            {
                return Results.Ok(JsonSerializer.Deserialize<GameDetailsDto>(cachedJson)!);
            }

            logger.LogInformation("Cache miss for game {GameId}", id);

            Game? game = await dbContext.Games.FindAsync(id);

            if (game is null)
            {
                return Results.NotFound();
            }

            var dto = game.ToGameDetailsDto();

            await cache.SetStringAsync(
                key: $"game:{id}",
                value: JsonSerializer.Serialize(dto),
                options: new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

            return Results.Ok(dto);
        })
        .WithName(GetGameEndpointName);

        // POST /games
        group.MapPost("/", async (CreateGameDto newGame, GameStoreContext dbContext) =>
        {
            Game game = newGame.ToEntity();

            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(
                GetGameEndpointName,
                new { id = game.Id },
                game.ToGameDetailsDto());
        });

        // PUT /games
        group.MapPut("/{id}", async (int id, UpdateGameDto updatedGame, GameStoreContext dbContext) =>
        {
            var existingGame = await dbContext.Games.FindAsync(id);

            if (existingGame is null)
            {
                return Results.NotFound();
            }

            dbContext.Entry(existingGame)
                .CurrentValues
                .SetValues(updatedGame.ToEntity(id));

            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        // DELETE /games/1
        group.MapDelete("/{id}", async (int id, GameStoreContext dbContext) =>
        {
            await dbContext.Games
                .Where(game => game.Id == id)
                .ExecuteDeleteAsync();

            return Results.NoContent();
        });

        return group;
    }
}
