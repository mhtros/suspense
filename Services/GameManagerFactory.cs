using Microsoft.AspNetCore.SignalR;
using Suspense.Server.Entities;
using Suspense.Server.Hubs;
using Suspense.Server.Repository;

namespace Suspense.Server.Services;

/// <summary>
/// Represents a factory for creating instances of the game manager.
/// </summary>
public interface IGameManagerFactory
{
    /// <summary>
    /// Creates a new instance of the game manager based on the provided game state.
    /// </summary>
    /// <param name="game">The game state for which to create a game manager.</param>
    /// <returns>An instance of the game manager.</returns>
    public IGameManager CreateInstance(GameState game);
}

/// <inheritdoc />
public class GameManagerFactory : IGameManagerFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameManagerFactory"/> class.
    /// </summary>
    public GameManagerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public IGameManager CreateInstance(GameState game)
    {
        var gameRepository = _serviceProvider.GetRequiredService<IGameRepository>();
        var playerRepository = _serviceProvider.GetRequiredService<IPlayerRepository>();
        var hubContext = _serviceProvider.GetRequiredService<IHubContext<GameHub, IGameClientActions>>();
        return new GameManager(game, gameRepository, playerRepository, hubContext);
    }
}