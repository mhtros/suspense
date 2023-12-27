using System.ComponentModel.DataAnnotations;
using Suspense.Server.Models;

namespace Suspense.Server.Contracts;

/// <summary>
/// Represents a request to validate a player's move in a game.
/// </summary>
public class ValidateMoveRequest
{
    /// <summary>
    /// Unique identifier of the player making the move.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public required string PlayerId { get; set; }

    /// <summary>
    /// Move made by the player.
    /// </summary>
    [Required]
    public required Card Move { get; set; }
}