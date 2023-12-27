using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Suspense.Server.Contracts;

/// <summary>
/// Represents a request to create a new game.
/// </summary>
public class CreateGameRequest
{
    /// <summary>
    /// Unique identifier of the player creating the game.
    /// </summary>
    [Required]
    [NotNull]
    public string? PlayerId { get; set; }

    /// <summary>
    /// Number of turns allowed for the game.
    /// </summary>
    [Required]
    [Range(1, 20)]
    public int Turns { get; set; }
}