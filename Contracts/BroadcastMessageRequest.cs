using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Suspense.Server.Contracts;

/// <summary>
/// Represents a request to broadcast a message.
/// </summary>
public class BroadcastMessageRequest
{
    /// <summary>
    /// The content of the message being broadcast.
    /// </summary>
    [Required]
    [NotNull]
    public string? Message { get; set; }
}