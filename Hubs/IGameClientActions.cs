using Suspense.Server.Entities;
using Suspense.Server.Models;

namespace Suspense.Server.Hubs;

/// <summary>
/// Defines the contract for client-side operations in a game-related SignalR hub.
/// </summary>
public interface IGameClientActions
{
    /// <summary>
    /// Notifies clients when a player joins a game.
    /// </summary>
    /// <param name="gameId">The identifier of the game the player joined.</param>
    Task PlayerJoined(string gameId);

    /// <summary>
    /// Notifies clients when the game state is updated.
    /// </summary>
    /// <param name="game">The updated game state.</param>
    Task GameUpdated(GameState game);

    /// <summary>
    /// Notifies clients when a broadcast message is received.
    /// </summary>
    /// <param name="senderUsername">The username of the message sender.</param>
    /// <param name="message">The received broadcast message.</param>
    Task BroadcastMessageReceived(string senderUsername, string message);

    /// <summary>
    /// Notifies clients when one of the data fields has been updated.
    /// </summary>
    /// <param name="playerData">The updated player data.</param>
    Task PlayerDataUpdated(PlayerData playerData);

    /// <summary>
    /// Notifies a specific client when a private message is received.
    /// </summary>
    /// <param name="senderUsername">The username of the message sender.</param>
    /// <param name="receiverPlayerId">The identifier of the message receiver.</param>
    /// <param name="message">The received personal message.</param>
    Task PrivateMessageReceived(string senderUsername, string receiverPlayerId, string message);

    /// <summary>
    /// Waits for a specific player to take their turn in a card game.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// The Task will complete with a <see cref="Card"/> object representing the result of the turn.
    /// </returns>
    Task<Card> PlayTurn(CancellationToken cancellationToken);

    /// <summary>
    /// Changes the current suit to one specified by the player. 
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// The Task will complete with a <see cref="Card.SuitType"/> object representing the changed suit.
    /// </returns>    
    Task<Card.SuitType> ChangeSuit(CancellationToken cancellationToken);
}