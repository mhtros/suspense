# Suspense Card Game - An ASP.NET Application

Welcome to suspense Card Game built with .NET 8 and SignalR! This project provides a delightful digital
rendition of the classic suspense game that I used to play as a child with my siblings, offering a fun and interactive
multiplayer experience.

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![SignalR](https://img.shields.io/badge/SignalR-Latest-brightgreen)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://opensource.org/licenses/GPL-3.0)

## Overview

This project is built on .NET 8 and ASP.NET, leveraging SignalR to facilitate real-time communication between players.
It aims to replicate the mechanics of the classical suspense card game, allowing up to four players to join each
session. Players have the option to create a game and others can join using a unique game ID, fostering an environment
where individuals can connect, play cards, and interact smoothly within a virtual space.

## Rules

Each player receives 7 cards. Players must play a card that matches either the same rank or the same suit as the face-up
card on the table. If they don't have a matching card, they must draw from the deck. If they still don't have a suitable
card, they must pass the turn.

Players have the right to draw a card from the deck, even if they possess a playable card. They can also pass, even with
a playable card on their hand. Regardless, before pass, they must draw a card from the deck.

The first card placed face on onto the table is treated as if a player had dropped it, and all special cards with unique
effects are applied to the first player. So if the card is a 7, then the first player must take 2 cards or roll a 7 and
the next player takes 4 cards and so on. If the card is a 9 then the first player loses his turn. The 8 is treated as a
regular card, as is the ace, which is considered to have been played by the previous player without choosing a different
suit.

When a player throws their last card, then the game ends. They receive zero points, while others accumulate points based
on their remaining cards' values.

If the cards on the deck is about to run out and there only one available undealt card face down, then the cards in the
pile (played cards) are shuffled and placed under the face down card and the play continues as normal until a player
runs out of cards.

### Special Cards

#### Ace

The player can play an ace regardless of the rank or suit facing up. If a player plays an ace, they can declare a suit
of their choice, and the next player must play a card of the declared suit. A player can finish with an ace. If a player
plays an ace without declaring a suit, the ace keeps its original suit. Consecutive aces cannot be played in a row.

#### Seven

If a player plays a 7, the following player draws two cards from the deck, unless they also have a 7. If they do, they
play it, and the subsequent player draws 4 cards, continuing until a player without a 7 is found. If the player
receiving the cards doesn't have a suitable card to play, they draw a card from the deck. When a player draws cards
after the previous 7, finding another 7 allows them to play it, treated as the initial 7 (the next player draws 2
cards). If a player finishes with a 7, the following player draws the corresponding cards, and the player drawing the
cards does not play again.

#### Eight

If a player plays an 8, they must take another turn. If they don't have a suitable card, they must draw from the deck.
If they still don't find a suitable card, they must pass. A player can consecutively play as many 8s as they have.

#### Nine

Playing a 9 skips the next player's turn. In a 2-player game, the same player plays a second turn,\
as if they played an 8.

### Winning Condition

The game ends when the predetermined turn limit is reached. The winner is the player with the fewest points.

### Points

Cards from 2 to 10 have point values equal to their number. Face cards (Jacks, Queens, Kings - J, Q, K) are worth 10
points each. Aces (A) worth 11 points.

### Features

- **Live Action Experience:** Engage in real-time updates and actions for an immersive suspense game session.


- **Multiplayer Compatibility:** Challenge multiple opponents in a thrilling multiplayer setup.


- **User-Friendly Interface:** Enjoy an intuitive and responsive UI for seamless interaction within the suspense game
  environment.


- **Complete Game Mechanics:** Implement the full range of suspense game actions and strategies, including skips, card
  draws, and wild cards.


- **Integrated Chat:** Communicate effortlessly with fellow players using an in-game chat feature, enhancing interaction
  within the suspense game setting.

### Getting Started

To Run the game see [Building and Running Your Application](README.Docker.md) for more information.

### Technologies Used

- **.NET 8 - ASP.NET:** Backend framework for server-side logic.
- **SignalR:** Real-time web functionality to enable communication.
- **React:** Frontend development for a responsive and interactive UI.

## Want to Contribute?

We welcome contributions! Fork the repository, make your changes, and submit a pull request.