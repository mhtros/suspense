using Suspense.Server.Entities;
using Suspense.Server.Exceptions;

namespace Suspense.Server.Repository;

/// <summary>
/// Represents a repository for managing game data in the card game.
/// </summary>
public interface IGameRepository
{
    /// <summary>
    /// Creates a new game.
    /// </summary>
    /// <param name="turns">The max number of turns in the game.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains the newly created game.</returns>
    Task<GameState> CreateGameAsync(int turns);

    /// <summary>
    /// Gets the game with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the game to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the game with the specified ID.
    /// Throws an <see cref="EntityException"/> if the game is not found.
    /// </returns>
    Task<GameState> GetGameAsync(string id);

    /// <summary>
    /// Deletes a game with the specified ID asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the game to be deleted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task DeleteGameAsync(string id);

    /// <summary>
    /// Updates a game with new information asynchronously.
    /// </summary>
    /// <param name="updatedState">The <see cref="GameState"/> object containing the updated information.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the updated <see cref="GameState"/>.
    /// Throws an <see cref="EntityException"/> if the entity does not exist.
    /// </returns>
    public Task<GameState> UpdateGameAsync(GameState updatedState);
}