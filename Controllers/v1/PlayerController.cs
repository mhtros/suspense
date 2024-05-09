using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Suspense.Server.Entities;
using Suspense.Server.Repository;

namespace Suspense.Server.Controllers.v1;

/// <summary>
/// Controller for managing players in a game.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly ILogger<PlayerController> _logger;
    private readonly IPlayerRepository _playerRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerController"/> class.
    /// </summary>
    public PlayerController(ILogger<PlayerController> logger, IPlayerRepository playerRepository)
    {
        _logger = logger;
        _playerRepository = playerRepository;
    }

    /// <summary>
    /// Creates a new player with the specified name.
    /// </summary>
    /// <param name="name">The name of the new player.</param>
    /// <param name="isBot">Indicating whether the player is a bot or not.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains the newly created player.</returns>
    [HttpPost("new")]
    public async Task<ActionResult<Player>> CreatePlayerAsync([FromQuery] string name, [FromQuery] bool isBot = false)
    {
        try
        {
            if (name.ToLowerInvariant() == "game")
                return BadRequest("Invalid name.");

            var player = await _playerRepository.CreatePlayerAsync(name, isBot);
            return Ok(player);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on {MethodName} with message: {Message}",
                nameof(CreatePlayerAsync), ex.Message);

            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Something has occured while trying to create a player.");
        }
    }
}