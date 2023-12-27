using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Suspense.Server.Contracts;
using Suspense.Server.Entities;
using Suspense.Server.Hubs;
using Suspense.Server.Models;
using Suspense.Server.Repository;
using Suspense.Server.Services;

namespace Suspense.Server.Controllers.v1;

/// <summary>
/// Controller for managing sessions in the game.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class GameController : ControllerBase
{
    private readonly ILogger<GameController> _logger;
    private readonly IGameRepository _gameRepository;
    private readonly IGameManagerFactory _gameManagerFactory;
    private readonly IHubContext<GameHub, IGameClientActions> _hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameController"/> class.
    /// </summary>
    public GameController(IGameRepository gameRepository, IGameManagerFactory gameManagerFactory,
        ILogger<GameController> logger, IHubContext<GameHub, IGameClientActions> hubContext)
    {
        _gameRepository = gameRepository;
        _gameManagerFactory = gameManagerFactory;
        _logger = logger;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Creates a new game.
    /// </summary>
    /// <param name="request">The <see cref="CreateGameRequest"/> containing information for creating the game.</param>
    /// <returns>The newly created game.</returns>
    [HttpPost("new")]
    public async Task<ActionResult<GameState>> CreateGameAsync([FromBody] CreateGameRequest request)
    {
        try
        {
            var game = await _gameRepository.CreateGameAsync(request.Turns);
            return Ok(game);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on {MethodName} with message: {Message}", nameof(CreateGameAsync), ex.Message);

            return StatusCode(StatusCodes.Status500InternalServerError,
                "Something has occured while trying to create a new game.");
        }
    }

    /// <summary>
    /// Validates a player's move in a game.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game.</param>
    /// <param name="request">The <see cref="ValidateMoveRequest"/> containing information about the player's move.</param>
    /// <returns>
    /// A bool that indicates whether the player's move is valid. Returns true if the move is valid, false otherwise.
    /// </returns>
    [HttpPost("{gameId}/validate-move")]
    public async Task<ActionResult<bool>> ValidateMoveAsync(string gameId, [FromBody] ValidateMoveRequest request)
    {
        try
        {
            var game = await _gameRepository.GetGameAsync(gameId);
            var playerExists = game.PlayersData.Values.Any(pd => pd.Player.Id == request.PlayerId);

            if (playerExists == false)
                return NotFound($"The player does not exist in the game: {gameId}.");

            var playerData = game.PlayersData[request.PlayerId];

            if (playerData.IsHisTurn == false)
                return BadRequest($"It is not player {playerData.Player.Name} turn.");

            if (playerData.HasDraw == false && request.Move is { Rank: Card.RankType.Pass, Suit: Card.SuitType.Pass })
                return BadRequest("You can't pass if you haven't drawn a card first.");

            var gameManager = _gameManagerFactory.CreateInstance(game);
            var isValid = gameManager.ValidateMove(playerData.Player.Id, request.Move);

            if (isValid)
                return Ok(isValid);

            return BadRequest("Invalid Move!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on {MethodName} with message: {Message}",
                nameof(ValidateMoveAsync), ex.Message);

            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Something has occured while trying to validate the move.");
        }
    }

    /// <summary>
    /// Draws a card for a player in a game.
    /// </summary>
    /// <param name="gameId">The unique identifier of the game.</param>
    /// <param name="playerId">The unique identifier of the player.</param>
    [HttpGet("{gameId}/{playerId}/draw-card")]
    public async Task<ActionResult> DrawCardAsync(string gameId, string playerId)
    {
        try
        {
            var game = await _gameRepository.GetGameAsync(gameId);
            var playerExists = game.PlayersData.Values.Any(pd => pd.Player.Id == playerId);

            if (playerExists == false)
                return NotFound($"Player with ID: {playerId} not exists.");

            var playerData = game.PlayersData[playerId];

            if (playerData.IsHisTurn == false)
                return BadRequest($"It is not player {playerData.Player.Name} turn.");

            if (playerData.HasDraw)
                return BadRequest("You already have draw a card!");

            var gameManager = _gameManagerFactory.CreateInstance(game);
            await gameManager.DrawCardAsync(playerId);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on {MethodName} with message: {Message}",
                nameof(DrawCardAsync), ex.Message);

            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Something has occured while trying to draw a card.");
        }
    }

    /// <summary>
    /// Broadcast a message to all the players connected to the game session.
    /// </summary>
    [HttpPost("{gameId}/{playerId}/broadcast-message")]
    public async Task<ActionResult> BroadcastMessageAsync(string gameId, string playerId,
        [FromBody] BroadcastMessageRequest request)
    {
        try
        {
            var game = await _gameRepository.GetGameAsync(gameId);
            var playerExists = game.PlayersData.Values.Any(pd => pd.Player.Id == playerId);

            if (playerExists == false)
                return NotFound($"Player with ID: {playerId} not exists.");

            var playerData = game.PlayersData[playerId];

            await _hubContext.Clients.Group(gameId).BroadcastMessageReceived(playerData.Player.Name, request.Message);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on {MethodName} with message: {Message}",
                nameof(BroadcastMessageAsync), ex.Message);

            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Something has occured while trying to broadcast a message.");
        }
    }
}