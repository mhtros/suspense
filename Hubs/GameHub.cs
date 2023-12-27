using Microsoft.AspNetCore.SignalR;
using Suspense.Server.Repository;
using Suspense.Server.Services;

namespace Suspense.Server.Hubs;

/// <inheritdoc cref="IGameServerActions"/>
public class GameHub : Hub<IGameClientActions>, IGameServerActions
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameManagerFactory _gameManagerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameHub"/> class.
    /// </summary>
    /// <param name="gameRepository">The game repository used for managing game data.</param>
    /// <param name="gameManagerFactory">The factory for creating game managers.</param>
    public GameHub(IGameRepository gameRepository, IGameManagerFactory gameManagerFactory)
    {
        _gameRepository = gameRepository;
        _gameManagerFactory = gameManagerFactory;
    }

    /// <inheritdoc />
    public async Task JoinGame(string gameId, string playerId, bool isLeader)
    {
        var game = await _gameRepository.GetGameAsync(gameId);
        var manager = _gameManagerFactory.CreateInstance(game);
        await manager.JoinGameAsync(playerId, isLeader, Context.ConnectionId);
    }

    /// <inheritdoc />
    public async Task StartGame(string initiatorId, string gameId)
    {
        var game = await _gameRepository.GetGameAsync(gameId);
        var gameManager = _gameManagerFactory.CreateInstance(game);
        await gameManager.StartGameAsync(initiatorId);
    }
}