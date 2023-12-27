using Microsoft.Extensions.Caching.Distributed;
using Suspense.Server.Entities;
using Suspense.Server.Extensions;

namespace Suspense.Server.Repository;

/// <inheritdoc />
public class GameRepository : IGameRepository
{
    private readonly TimeSpan _expirationTime = TimeSpan.FromHours(4);
    private readonly TimeSpan _slidingExpirationTime = TimeSpan.FromMinutes(15);

    private readonly IDistributedCache _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameRepository"/> class.
    /// </summary>
    public GameRepository(IDistributedCache cache)
    {
        _cache = cache;
    }

    /// <inheritdoc />
    public async Task<GameState> CreateGameAsync(int turns)
    {
        var gameState = new GameState(Guid.NewGuid()) { TurnLimit = turns };
        await _cache.SetModelAsync(gameState.Id, gameState, _expirationTime, _slidingExpirationTime);
        return gameState;
    }

    /// <inheritdoc />
    public async Task<GameState> GetGameAsync(string id)
    {
        return await _cache.GetModelAsync<GameState>(id);
    }

    /// <inheritdoc />
    public async Task DeleteGameAsync(string id)
    {
        await _cache.RemoveAsync(id);
    }

    /// <inheritdoc />
    public async Task<GameState> UpdateGameAsync(GameState updatedState)
    {
        var savedGame = await _cache.GetModelAsync<GameState>(updatedState.Id);

        savedGame.PlayedCards = updatedState.PlayedCards;
        savedGame.Deck = updatedState.Deck;
        savedGame.PlayersData = updatedState.PlayersData;
        savedGame.CurrentTurn = updatedState.CurrentTurn;
        savedGame.CardsLeft = updatedState.CardsLeft;
        savedGame.SuitModificator = updatedState.SuitModificator;

        await _cache.SetModelAsync(savedGame.Id, savedGame, _expirationTime, _slidingExpirationTime);

        // retrieve it again from the cache to ensure that the update was successful
        savedGame = await _cache.GetModelAsync<GameState>(savedGame.Id);
        return savedGame;
    }
}