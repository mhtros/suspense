using Microsoft.Extensions.Caching.Distributed;
using Suspense.Server.Entities;
using Suspense.Server.Extensions;

namespace Suspense.Server.Repository;

/// <inheritdoc />
public class PlayerRepository : IPlayerRepository
{
    private readonly TimeSpan _expirationTime = TimeSpan.FromHours(4);
    private readonly TimeSpan _slidingExpirationTime = TimeSpan.FromMinutes(15);

    private readonly IDistributedCache _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerRepository"/> class.
    /// </summary>
    public PlayerRepository(IDistributedCache cache)
    {
        _cache = cache;
    }

    /// <inheritdoc />
    public async Task<Player> CreatePlayerAsync(string name, bool isBot = false)
    {
        var playerGuid = Guid.NewGuid();

        if (isBot)
            name = $"Bot_{playerGuid.ToString("N").Substring(0, 5)}";

        var player = new Player(name, playerGuid, isBot);
        await _cache.SetModelAsync(player.Id, player, _expirationTime, _slidingExpirationTime);
        return player;
    }

    /// <inheritdoc />
    public async Task<Player> GetPlayerAsync(string id)
    {
        return await _cache.GetModelAsync<Player>(id);
    }

    /// <inheritdoc />
    public async Task DeletePlayerAsync(string id)
    {
        await _cache.RemoveAsync(id);
    }

    /// <inheritdoc />
    public async Task<Player> UpdatePlayerAsync(Player updatedPlayer)
    {
        var savedPlayer = await _cache.GetModelAsync<Player>(updatedPlayer.Id);

        savedPlayer.Name = updatedPlayer.Name;
        savedPlayer.ConnectionId = updatedPlayer.ConnectionId;

        await _cache.SetModelAsync(savedPlayer.Id, savedPlayer, _expirationTime, _slidingExpirationTime);

        // Retrieve it again from the cache to ensure that the update was successful
        savedPlayer = await _cache.GetModelAsync<Player>(savedPlayer.Id);
        return savedPlayer;
    }
}