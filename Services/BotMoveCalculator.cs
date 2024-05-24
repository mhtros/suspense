using Suspense.Server.Contracts;
using Suspense.Server.Models;
using Suspense.Server.Extensions;

namespace Suspense.Server.Services;

/// <inheritdoc />
public class BotMoveCalculator : IBotMoveCalculator
{
    private readonly Random _random = new();

    /// <inheritdoc />
    public async Task<BotMoveCalculatorResponse> CalculateMoveAsync(PlayerData data, IList<Card> playedCards,
        int nextHandCount, bool is1Vs1, Card.SuitType? suitModificator, Func<string, Task> drawCardAsync)
    {
        // Add some artificial delay
        await Task.Delay(2000);

        var currentCard = playedCards.Last();

        var response = new BotMoveCalculatorResponse
        {
            SuitModificator = currentCard.Rank == Card.RankType.Ace && suitModificator.HasValue
                ? suitModificator.Value
                : null
        };

        if (data.Hand.Count == 1)
        {
            var lastCard = data.Hand.First();
            var sameSuit = lastCard.Suit == currentCard.Suit;
            var sameRank = lastCard.Rank == currentCard.Rank;
            var isAce = lastCard.Rank == Card.RankType.Ace;

            if (sameSuit || sameRank || isAce)
                return response.SetMoveAndReturnResponse(lastCard);
        }

        if (currentCard.Rank == Card.RankType.Seven)
        {
            Card? move7 = null;
            while (move7 == null)
            {
                var hand7 = data.Hand.Where(c => c.Rank == Card.RankType.Seven).ToArray();
                var sameSuitHighest = data.Hand.Where(c => c.Suit == currentCard.Suit).MaxBy(c => c.Rank);

                if (hand7.Length != 0)
                {
                    // How many 7 cards have been played so far
                    var deck7Count = playedCards.Count(c => c.Rank == Card.RankType.Seven);

                    // Calculate the probability to counter the seven by play another seven
                    var moveProbability = Math.Floor((decimal)hand7.Length / (4 - deck7Count) * 100);
                    var random = RandomNumberBetweenRange(1, 100);

                    if (random <= moveProbability)
                    {
                        // Choose one randomly
                        move7 = _random.GetItems(hand7, 1).First();
                    }
                }

                // If you already found a move there is no need to continue further
                if (move7 != null)
                {
                    return response.SetMoveAndReturnResponse(move7);
                }

                if (sameSuitHighest != null) move7 = sameSuitHighest;
                else if (data.HasDraw == false) await drawCardAsync(data.Player.Id); // No card to play draw one
                else break; // Stop the loop
            }
        }

        var hand9 = data.Hand.Where(c => c.Rank == Card.RankType.Nine).ToArray();
        if (hand9.Length != 0)
        {
            var isCurrent9 = currentCard.Rank == Card.RankType.Nine;
            var hand9SameSuiteAsCurrent = hand9.Where(c => c.Suit == currentCard.Suit).ToArray();

            if (hand9SameSuiteAsCurrent.Length != 0 || isCurrent9)
            {
                var random = RandomNumberBetweenRange(1, 100);
                var move9 = GetHighestCommonSuit(data.Hand, hand9);

                // If are more than 2 players and the next player has 1 card left then throw it
                // TODO: Maybe check and the player after the next player if have only 1 card left
                if (is1Vs1 == false && nextHandCount < 2)
                {
                    return isCurrent9
                        ? response.SetMoveAndReturnResponse(move9 ?? hand9.First())
                        : response.SetMoveAndReturnResponse(move9 ?? hand9SameSuiteAsCurrent.First());
                }

                if (random <= 50) // 50% chance to throw it
                {
                    return isCurrent9
                        ? response.SetMoveAndReturnResponse(move9 ?? hand9.First())
                        : response.SetMoveAndReturnResponse(move9 ?? hand9SameSuiteAsCurrent.First());
                }
            }
        }

        var hand8 = data.Hand.Where(c => c.Rank == Card.RankType.Eight).ToArray();
        if (hand8.Length != 0)
        {
            var isCurrent8 = currentCard.Rank == Card.RankType.Eight;
            var hand8SameSuiteAsCurrent = hand8.Where(c => c.Suit == currentCard.Suit).ToArray();

            if (hand8SameSuiteAsCurrent.Length != 0 || isCurrent8)
            {
                var random = RandomNumberBetweenRange(1, 100);
                var move8 = GetHighestCommonSuit(data.Hand, hand8);

                if (random <= 50) // 50% chance to throw it
                {
                    return isCurrent8
                        ? response.SetMoveAndReturnResponse(move8 ?? hand8.First())
                        : response.SetMoveAndReturnResponse(move8 ?? hand8SameSuiteAsCurrent.First());
                }
            }
        }

        Card? move = null;
        while (move == null)
        {
            var handSameSuiteAsCurrent = data.Hand.Where(c => c.Suit == currentCard.Suit).ToArray();
            if (handSameSuiteAsCurrent.Length != 0)
            {
                var highestRank = handSameSuiteAsCurrent.MaxBy(c => c.Rank);

                if (highestRank?.Rank == Card.RankType.Ace)
                {
                    var mostCardsSuit = data.Hand.GroupBy(c => c.Suit).MaxBy(g => g.Count())?.Key!;
                    response.SuitModificator = mostCardsSuit;
                }

                move = highestRank;
                break; // Stop the loop
            }

            var handSameRankAsCurrent = data.Hand.Where(c => c.Rank == currentCard.Rank).ToArray();
            if (handSameRankAsCurrent.Length != 0)
            {
                move = GetHighestCommonSuit(data.Hand, handSameRankAsCurrent) ?? handSameRankAsCurrent.First();
                break; // Stop the loop
            }

            var handAces = data.Hand.Where(c => c.Rank == Card.RankType.Ace).ToArray();
            if (handAces.Length != 0)
            {
                var mostCardsSuit = data.Hand.GroupBy(c => c.Suit).MaxBy(g => g.Count())?.Key!;
                response.SuitModificator = mostCardsSuit;
                move = handAces.First();
                break; // Stop the loop
            }

            if (data.HasDraw == false)
            {
                await drawCardAsync(data.Player.Id);
                continue; // Restart the loop
            }

            move = new Card(Card.SuitType.Pass, Card.RankType.Pass);
        }

        return response.SetMoveAndReturnResponse(move!);
    }

    private int RandomNumberBetweenRange(int from, int to) => _random.Next(from, to + 1);

    private static Card? GetHighestCommonSuit(IList<Card> initialHand, IList<Card> otherHand)
    {
        // Remove the other hand form the hand if exists duplicates
        var remainingHandGroupBySuit = otherHand.Intersect(initialHand).GroupBy(c => c.Suit);
        var otherHandGroupBySuit = otherHand.GroupBy(c => c.Suit).ToDictionary();

        // Find the highest common suit between the two hands
        var highestCardCountSuit = remainingHandGroupBySuit
            .Where(d => otherHandGroupBySuit.ContainsKey(d.Key))
            .MaxBy(g => g.Count())
            ?.Key;

        return otherHand.FirstOrDefault(c => c.Suit == highestCardCountSuit);
    }
}