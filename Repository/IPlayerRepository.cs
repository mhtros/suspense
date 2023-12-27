using Suspense.Server.Entities;
using Suspense.Server.Exceptions;

namespace Suspense.Server.Repository;

/// <summary>
/// Represents a repository for managing players in the card game.
/// </summary>
public interface IPlayerRepository
{
    /// <summary>
    /// Creates a new player with the specified name asynchronously. The player will have a 4 hour expiration time
    /// and 15 minute sliding expiration time.
    /// </summary>
    /// <param name="name">The name of the player to create.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the newly created player.</returns>
    public Task<Player> CreatePlayerAsync(string name);

    /// <summary>
    /// Retrieves a player with the specified ID asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the player to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the player with the specified ID.
    /// Throws an <see cref="EntityException"/> if the player is not found.
    /// </returns>
    public Task<Player> GetPlayerAsync(string id);

    /// <summary>
    /// Deletes a player with the specified ID asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the player to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task DeletePlayerAsync(string id);

    /// <summary>
    /// Updates the player's information asynchronously.
    /// </summary>
    /// <param name="updatedPlayer">The <see cref="Player"/> object representing the updated player information.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the updated <see cref="Player"/>.
    /// Throws an <see cref="EntityException"/> if the player does not exist.
    /// </returns>
    public Task<Player> UpdatePlayerAsync(Player updatedPlayer);
}