using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Suspense.Server.Contracts;
using Suspense.Server.Entities;
using Suspense.Server.Hubs;
using Suspense.Server.Models;
using Suspense.Server.Repository;

namespace Suspense.Server.Services;

/// <inheritdoc />
public class GameManager : IGameManager
{
    private const int BufferDuration = 10;
    private const int CountdownDuration = 60;
    private const int GamePlayerCapacity = 4;

    private GameState _game;
    private readonly IGameRepository _gameRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IHubContext<GameHub, IGameClientActions> _hubContext;
    private readonly IBotMoveCalculator _moveCalculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameManager"/> class.
    /// </summary>
    public GameManager(GameState game, IGameRepository gameRepository, IPlayerRepository playerRepository,
        IHubContext<GameHub, IGameClientActions> hubContext, IBotMoveCalculator moveCalculator)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
        _gameRepository = gameRepository;
        _playerRepository = playerRepository;
        _hubContext = hubContext;
        _moveCalculator = moveCalculator;
    }

    /// <inheritdoc />
    public async Task DrawCardAsync(string playerId)
    {
        var card = DealCards(1);
        var data = _game.PlayersData[playerId];
        data.Hand.AddRange(card);
        data.HasDraw = true;
        await GameAnnouncementAsync($"Player {data.Player.Name} has draw a card.");
        await UpdateAndNotifySessionAsync();
    }

    /// <inheritdoc />
    public bool ValidateMove(string playerId, Card move)
    {
        var lastPlayedCard = _game.PlayedCards.Last();

        // Two aces in a row are not allowed!
        if (lastPlayedCard.Rank == Card.RankType.Ace && move.Rank == Card.RankType.Ace)
            return false;

        var playerData = _game.PlayersData[playerId];

        var isValidPass = playerData.HasDraw && move is { Rank: Card.RankType.Pass, Suit: Card.SuitType.Pass };
        var isIncompatible = move.Rank != lastPlayedCard.Rank && move.Rank != Card.RankType.Ace;

        if (lastPlayedCard.Rank == Card.RankType.Ace)
            isIncompatible = isIncompatible && move.Suit != _game.SuitModificator;
        else
            isIncompatible = isIncompatible && move.Suit != lastPlayedCard.Suit;

        return isValidPass || !isIncompatible;
    }

    /// <inheritdoc />
    public async Task JoinGameAsync(string playerId, bool isLeader, string connectionId)
    {
        var player = await _playerRepository.GetPlayerAsync(playerId);

        if (_game.PlayersData.Count >= GamePlayerCapacity)
            throw new HubException(
                $"Game {_game.Id} has reach its Max Capacity of {GamePlayerCapacity} Players. Try to create a new game or join another!"
            );

        if (isLeader && _game.PlayersData.Values.Any(pd => pd.IsLeader))
            throw new HubException($"There is already a leader for the game: {_game.Id}.");

        // The player already exists on the game no need to proceed further
        if (_game.PlayersData.Values.Any(pd => pd.Player.Id == player.Id)) return;

        AddPlayer(player, isLeader);

        // If the player has successfully joined the game and is not a bot
        // then update him to save the session ConnectionId and add it to SignalR
        if (player.IsBot == false)
        {
            player.ConnectionId = connectionId; // Store current Hub session
            await _playerRepository.UpdatePlayerAsync(player);
            await _hubContext.Groups.AddToGroupAsync(player.ConnectionId, _game.Id);
        }

        await SaveGameStateAsync();

        await _hubContext.Clients.Group(_game.Id).PlayerJoined(player.Name);
        await _hubContext.Clients.Group(_game.Id).GameUpdated(_game);
    }

    /// <inheritdoc />
    public async Task StartGameAsync(string initiatorId)
    {
        _game.CurrentTurn++;

        ShufflePlayers();
        var shuffledPlayersIds = _game.PlayersData.Values.Select(pd => pd.Player.Id).ToArray();

        while (_game.CurrentTurn <= _game.TurnLimit)
        {
            await GameAnnouncementAsync($"Turn no. {_game.CurrentTurn} Starts.");

            InitializeDeckAndDealHands();
            DealFirstCard();
            await UpdateAndNotifySessionAsync();

            var emptyHandPlayer = false;

            while (!emptyHandPlayer)
            {
                for (var i = 0; i < shuffledPlayersIds.Length; i++)
                {
                    StartTurnLabel:

                    // If there is a player who has no other cards in their hand,
                    // then end the current turn
                    if (emptyHandPlayer) break;

                    // If the player is the last one, set the first player as the next player,
                    // otherwise make the i+1 next
                    var nextIndex = i + 1 > shuffledPlayersIds.Length - 1 ? 0 : i + 1;
                    var nextId = shuffledPlayersIds[nextIndex];
                    var currentId = shuffledPlayersIds[i];

                    _game.PlayersData[currentId].IsHisTurn = true;

                    await UpdateAndNotifySessionAsync();
                    await GameAnnouncementAsync($"Start Turn - Player {_game.PlayersData[currentId].Player.Name}.");

                    // Check if the player lost their turn due to a previous 9 card
                    if (_game.PlayersData[currentId].LostHisTurn)
                    {
                        await GameAnnouncementAsync($"Lost Turn - Player {_game.PlayersData[currentId].Player.Name}.");
                        _game.PlayersData[currentId].LostHisTurn = false;
                        _game.PlayersData[currentId].IsHisTurn = false;
                        await UpdateAndNotifySessionAsync();
                        continue; // End player turn
                    }

                    Card? counterMove = null;

                    // Check if the player has to draw penalty cards due to a previous 7 card
                    if (_game.PlayersData[currentId].PenaltyCardsCount > 0)
                    {
                        var hasSevenToCounteract =
                            _game.PlayersData[currentId].Hand.Any(x => x.Rank == Card.RankType.Seven);

                        // Draw the penalty cards and continue your turn
                        if (hasSevenToCounteract == false)
                        {
                            await DrawPenaltyCardsAsync(currentId);
                        }
                        else
                        {
                            try
                            {
                                if (_game.PlayersData[currentId].Player.IsBot)
                                {
                                    var is1Vs1 = _game.PlayersData.Count == 2;

                                    var botResponse = await _moveCalculator.CalculateMoveAsync(
                                        _game.PlayersData[currentId],
                                        _game.PlayedCards, _game.PlayersData[nextId].CardsLeft, is1Vs1,
                                        _game.SuitModificator, DrawCardAsync);

                                    counterMove = botResponse.Move;
                                }
                                else
                                {
                                    // Lucky! you have the opportunity to counterattack
                                    var cts = new CancellationTokenSource();
                                    cts.CancelAfter(TimeSpan.FromSeconds(CountdownDuration + BufferDuration));

                                    await GamePrivateAnnouncementAsync(
                                        _game.PlayersData[currentId].Player.ConnectionId!,
                                        $"You have the option to play a seven card to transfer the penalty ({_game.PlayersData[currentId].PenaltyCardsCount} cards) to the next player.");

                                    counterMove = await GetPlayerMove(
                                        _game.PlayersData[currentId].Player.ConnectionId!, cts.Token);
                                }

                                var isMoveValid = ValidateMove(_game.PlayersData[currentId].Player.Id, counterMove);

                                if (isMoveValid == false)
                                {
                                    counterMove.Invalidate();

                                    emptyHandPlayer =
                                        await EndPlayerTurnAsync(_game.PlayersData[currentId], counterMove);
                                    continue; // End player turn
                                }

                                if (isMoveValid && counterMove.Rank == Card.RankType.Seven)
                                {
                                    // Pass the penalty to the next player and increase it by 2
                                    _game.PlayersData[nextId].PenaltyCardsCount =
                                        _game.PlayersData[currentId].PenaltyCardsCount += 2;

                                    _game.PlayersData[currentId].PenaltyCardsCount = 0;

                                    emptyHandPlayer =
                                        await EndPlayerTurnAsync(_game.PlayersData[currentId], counterMove);
                                    continue; // End player turn
                                }
                            }
                            catch (Exception e) when (e.Message == "Invocation canceled by the server.")
                            {
                                // Missed the opportunity to counteract, take penalty cards, and continue the turn
                                await DrawPenaltyCardsAsync(currentId);
                            }
                        }
                    }

                    // THIS IS WHERE THE PLAYER'S NORMAL TURN BEGINS

                    try
                    {
                        var answeredMove = counterMove;
                        BotMoveCalculatorResponse? botResponse = null;

                        // if the player already has played his move then prevent him to throw another card
                        if (answeredMove == null)
                        {
                            if (_game.PlayersData[currentId].Player.IsBot)
                            {
                                var is1Vs1 = _game.PlayersData.Count == 2;

                                botResponse = await _moveCalculator.CalculateMoveAsync(_game.PlayersData[currentId],
                                    _game.PlayedCards, _game.PlayersData[nextId].CardsLeft, is1Vs1,
                                    _game.SuitModificator, DrawCardAsync);

                                answeredMove = botResponse.Move;
                            }
                            else
                            {
                                // Initialize only if the player has not yet play his move
                                var answerCts = new CancellationTokenSource();
                                answerCts.CancelAfter(TimeSpan.FromSeconds(CountdownDuration + BufferDuration));
                                answeredMove = await GetPlayerMove(_game.PlayersData[currentId].Player.ConnectionId!,
                                    answerCts.Token);
                            }
                        }

                        var isMoveValid = ValidateMove(_game.PlayersData[currentId].Player.Id, answeredMove);

                        if (isMoveValid == false)
                        {
                            answeredMove.Invalidate();

                            emptyHandPlayer = await EndPlayerTurnAsync(_game.PlayersData[currentId], answeredMove);
                            continue; // End player turn
                        }

                        if (answeredMove.Rank == Card.RankType.Nine)
                        {
                            _game.PlayersData[nextId].LostHisTurn = true;
                        }

                        if (answeredMove.Rank == Card.RankType.Seven)
                        {
                            _game.PlayersData[nextId].PenaltyCardsCount += 2;
                        }

                        if (answeredMove.Rank == Card.RankType.Eight)
                        {
                            await GameAnnouncementAsync(
                                $"Player {_game.PlayersData[currentId].Player.Name} play's another turn.");

                            _game.PlayedCards.AddRange(new List<Card> { answeredMove });

                            var hRmv = _game.PlayersData[currentId].Hand.Single(r =>
                                r.Rank == answeredMove.Rank && r.Suit == answeredMove.Suit);

                            _game.PlayersData[currentId].Hand.Remove(hRmv);

                            // Reset player values back to defaults
                            _game.PlayersData[currentId].HasDraw = false;
                            _game.PlayersData[currentId].IsHisTurn = false;
                            _game.PlayersData[currentId].LostHisTurn = false;
                            _game.PlayersData[currentId].PenaltyCardsCount = 0;

                            await UpdateAndNotifySessionAsync();

                            emptyHandPlayer = _game.PlayersData[currentId].Hand.Count == 0;

                            // Re-run the user's turn
                            goto StartTurnLabel;
                        }

                        if (answeredMove.Rank == Card.RankType.Ace)
                        {
                            try
                            {
                                Card.SuitType suit;

                                if (_game.PlayersData[currentId].Player.IsBot)
                                {
                                    suit = botResponse!.SuitModificator ?? answeredMove.Suit;
                                }
                                else
                                {
                                    var cts = new CancellationTokenSource();
                                    suit = await _hubContext.Clients
                                        .Client(_game.PlayersData[currentId].Player.ConnectionId!)
                                        .ChangeSuit(cts.Token);
                                }

                                var validSuits = new List<int>
                                {
                                    (int)Card.SuitType.Diamonds,
                                    (int)Card.SuitType.Hearts,
                                    (int)Card.SuitType.Spades,
                                    (int)Card.SuitType.Clubs
                                };

                                // Keep the initial suit or Change it with the answer suit
                                _game.SuitModificator = !validSuits.Contains((int)suit) ? answeredMove.Suit : suit;

                                await GameAnnouncementAsync($"The Suit has been change to {_game.SuitModificator}");

                                emptyHandPlayer = await EndPlayerTurnAsync(_game.PlayersData[currentId], answeredMove);
                                continue; // End player turn
                            }
                            catch (Exception e) when (e.Message == "Invocation canceled by the server.")
                            {
                                var invalidAnswer = new Card(Card.SuitType.Invalid, Card.RankType.Invalid);
                                emptyHandPlayer = await EndPlayerTurnAsync(_game.PlayersData[currentId], invalidAnswer);
                                continue; // End player turn
                            }
                        }

                        // Regular card
                        emptyHandPlayer = await EndPlayerTurnAsync(_game.PlayersData[currentId], answeredMove);
                    }
                    catch (Exception e) when (e.Message == "Invocation canceled by the server.")
                    {
                        var invalidAnswer = new Card(Card.SuitType.Invalid, Card.RankType.Invalid);
                        emptyHandPlayer = await EndPlayerTurnAsync(_game.PlayersData[currentId], invalidAnswer);
                    }
                }
            }

            CalculatePlayersScore();
            await GameAnnouncementAsync($"Turn {_game.CurrentTurn} Ends.");
            _game.CurrentTurn++;
        }

        await UpdateAndNotifySessionAsync();
        var winner = _game.PlayersData.Values.Aggregate((x, y) => x.Score < y.Score ? x : y);
        await GameAnnouncementAsync($"Game Over. The winner is :{winner.Player.Name}");
    }

    // FROM THIS POINT AND DOWN THEY ARE STARTING THE PRIVATE HELPING METHODS

    private async Task<Card> GetPlayerMove(string connectionId, CancellationToken ct)
    {
        await UpdateAndNotifySessionAsync();
        var move = await _hubContext.Clients.Client(connectionId).PlayTurn(ct);
        await RefreshGameState();

        return move;
    }

    private void CalculatePlayersScore()
    {
        var countsAs10 = new[] { Card.RankType.Jack, Card.RankType.Queen, Card.RankType.King };

        foreach (var pd in _game.PlayersData.Values)
        foreach (var card in pd.Hand)
        {
            pd.Score += card.Rank switch
            {
                var rank when countsAs10.Contains(rank) => 10,
                Card.RankType.Ace => 11,
                Card.RankType.Invalid or Card.RankType.Pass => 0, // How on earth you ended up here?
                _ => (int)card.Rank
            };
        }
    }

    private async Task<bool> EndPlayerTurnAsync(PlayerData playerData, Card answeredMove)
    {
        if (answeredMove is { Rank: Card.RankType.Invalid, Suit: Card.SuitType.Invalid })
        {
            // Take a card for someone who's likely cheating or just idly waiting for time to run out without doing anything
            await GameAnnouncementAsync($"Player{playerData.Player.Name} TIMEOUT.");
            var card = DealCards(1);
            playerData.Hand.AddRange(card);
        }
        // Valid pass
        else if (playerData.HasDraw && answeredMove is { Rank: Card.RankType.Pass, Suit: Card.SuitType.Pass })
        {
            await GameAnnouncementAsync($"Player{playerData.Player.Name} pass the turn.");
        }
        else
        {
            // Place the card on the played stack and remove it form the players hand
            _game.PlayedCards.AddRange(new List<Card> { answeredMove });
            var hRmv = playerData.Hand.Single(r => r.Rank == answeredMove.Rank && r.Suit == answeredMove.Suit);
            playerData.Hand.Remove(hRmv);
        }

        await GameAnnouncementAsync($"End Turn - Player {playerData.Player.Name}.");

        // Reset player values back to defaults
        playerData.HasDraw = false;
        playerData.IsHisTurn = false;
        playerData.LostHisTurn = false;
        playerData.PenaltyCardsCount = 0;

        // Reset the card modifier
        var lastPlayedCard = _game.PlayedCards.Last();
        if (lastPlayedCard.Rank != Card.RankType.Ace)
            _game.SuitModificator = null;

        await UpdateAndNotifySessionAsync();

        return playerData.Hand.Count == 0;
    }

    private async Task DrawPenaltyCardsAsync(string playerId)
    {
        var data = _game.PlayersData[playerId];
        await GameAnnouncementAsync($"Player {data.Player.Name} draws({data.PenaltyCardsCount}) to cards.");

        var penaltyCards = DealCards(data.PenaltyCardsCount);
        data.Hand.AddRange(penaltyCards);
        data.PenaltyCardsCount = 0; // Reset penalty counter

        await UpdateAndNotifySessionAsync();
    }

    private async Task NotifyHandsChangeAsync()
    {
        // Notify each player separately to keep their hand secret from the other players  
        var tasks = _game.PlayersData.Values
            .Where(pd => pd.Player.IsBot == false)
            .Select(playerData => _hubContext.Clients.Client(playerData.Player.ConnectionId!)
                .PlayerDataUpdated(playerData)
            );

        await Task.WhenAll(tasks);
    }

    private async Task BroadcastGameStateAsync(GameState commonState)
    {
        await _hubContext.Clients.Group(_game.Id).GameUpdated(commonState);
    }

    private GameState GetCommonState()
    {
        // TODO: Create a method that does the same without using the time-process consuming JsonSerializer every time

        // Hard copy the game state.
        var json = JsonSerializer.Serialize(_game);
        var commonState = JsonSerializer.Deserialize<GameState>(json)!;

        // The players are not allowed to see the deck stack.
        // Note that even that deck is cleared the CardsLeft property isn't
        commonState.Deck.Clear();

        // The players must not see what each players hand is.
        // Note that even that hand is cleared the CardsLeft property isn't
        foreach (var playerData in commonState.PlayersData.Values)
            playerData.Hand.Clear();

        return commonState;
    }

    private void DealFirstCard()
    {
        var firstCard = DealCards(1);
        var firstPlayer = _game.PlayersData.First().Value;

        if (firstCard.First().Rank == Card.RankType.Nine)
            firstPlayer.LostHisTurn = true;

        if (firstCard.First().Rank == Card.RankType.Seven)
            firstPlayer.PenaltyCardsCount += 2;

        // Uncomment only for debugging purposes
        // firstPlayer.Hand.Add(new Card(Card.SuitType.Clubs, Card.RankType.Ace));
        // firstPlayer.Hand.Add(new Card(Card.SuitType.Spades, Card.RankType.Ace));
        //
        // var lastPlayer = _game.PlayersData.Last().Value;
        //
        // lastPlayer.Hand.Add(new Card(Card.SuitType.Hearts, Card.RankType.Ace));
        // lastPlayer.Hand.Add(new Card(Card.SuitType.Diamonds, Card.RankType.Ace));

        _game.PlayedCards.AddRange(firstCard);
    }

    private void DealCardsToAllPlayers(int numberOfCards)
    {
        foreach (var playerData in _game.PlayersData.Values)
        {
            var hand = DealCards(numberOfCards);
            playerData.Hand.AddRange(hand);
        }
    }

    private void ShuffleDeck()
    {
        var random = new Random();
        var n = _game.Deck.Count;

        while (n > 1)
        {
            n--;
            var k = random.Next(n + 1);
            (_game.Deck[k], _game.Deck[n]) = (_game.Deck[n], _game.Deck[k]);
        }
    }

    private void UpdateCardsLeftCounters()
    {
        _game.CardsLeft = _game.Deck.Count;

        foreach (var playerData in _game.PlayersData.Values)
            playerData.CardsLeft = playerData.Hand.Count;
    }

    private void InitializeDeck()
    {
        var suits = Enum.GetValues(typeof(Card.SuitType)).Cast<Card.SuitType>()
            .Where(suit => suit != Card.SuitType.Invalid && suit != Card.SuitType.Pass);

        var ranks = Enum.GetValues(typeof(Card.RankType)).Cast<Card.RankType>()
            .Where(rank => rank != Card.RankType.Invalid && rank != Card.RankType.Pass).ToArray();

        foreach (var suit in suits)
        foreach (var rank in ranks)
            _game.Deck.Add(new Card(suit, rank));

        ShuffleDeck();
    }

    private void InitializeDeckAndDealHands()
    {
        _game.PlayedCards.Clear();
        _game.Deck.Clear();

        foreach (var playersValue in _game.PlayersData.Values)
            playersValue.Hand.Clear();

        InitializeDeck();

        // Uncomment only for debugging purposes
        //_game.Deck = _game.Deck.Where(x => x.Rank != Card.RankType.Ace).ToList();

        DealCardsToAllPlayers(7);
    }

    private void ShufflePlayers()
    {
        var random = new Random();
        var shuffledPlayers = _game.PlayersData.OrderBy(_ => random.Next()).ToArray();
        _game.PlayersData = shuffledPlayers.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    private async Task SaveGameStateAsync()
    {
        await _gameRepository.UpdateGameAsync(_game);
        await RefreshGameState();
    }

    private void AddPlayer(Player player, bool isLeader = false, bool isHisTurn = false)
    {
        _game.PlayersData.Add(player.Id, new PlayerData
        {
            Score = 0,
            Player = player,
            CardsLeft = 0,
            Hand = new List<Card>(),
            IsLeader = isLeader,
            IsHisTurn = isHisTurn
        });
    }

    private async Task RefreshGameState()
    {
        _game = await _gameRepository.GetGameAsync(_game.Id);
    }

    private async Task GameAnnouncementAsync(string message)
    {
        await _hubContext.Clients.Group(_game.Id).BroadcastMessageReceived("Game", message);
    }

    private async Task GamePrivateAnnouncementAsync(string receiverPlayerId, string message)
    {
        await _hubContext.Clients.Client(receiverPlayerId).PrivateMessageReceived("Game", receiverPlayerId, message);
    }

    private async Task UpdateAndNotifySessionAsync()
    {
        UpdateCardsLeftCounters();
        await SaveGameStateAsync();
        var commonState = GetCommonState();
        await BroadcastGameStateAsync(commonState);
        await NotifyHandsChangeAsync();
    }

    private IList<Card> DealCards(int numberOfCards)
    {
        if (_game.Deck.Count < numberOfCards)
        {
            RepopulateDeck();

            if (_game.Deck.Count < numberOfCards)
            {
                // If for some reason the deck still has fewer cards than requested,
                // return only the cards contained in the deck
                numberOfCards = _game.Deck.Count;
            }
        }

        var cards = _game.Deck.GetRange(0, numberOfCards);
        _game.Deck.RemoveRange(0, numberOfCards);

        if (_game.Deck.Count == 0) RepopulateDeck();

        return cards;
    }

    private void RepopulateDeck()
    {
        // Take all except the last played card
        var cardsToBeReused = _game.PlayedCards.Take(_game.PlayedCards.Count - 1).ToArray();

        // Remove the taken cards from the played list
        _game.PlayedCards.RemoveRange(0, _game.PlayedCards.Count - 1);

        // Add them back to deck
        _game.Deck.AddRange(cardsToBeReused);

        ShuffleDeck();
    }
}