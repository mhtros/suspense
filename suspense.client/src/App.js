import * as signalR from "@microsoft/signalr";
import React, { useEffect, useRef, useState } from "react";
import {
  apiVersion,
  cards,
  countdownSec,
  DEBUGGING,
  defaultGame,
  defaultPlayer,
  defaultTurns,
  passCard,
  playerDataHashIndex,
  rank,
  serverBaseUri,
  suit,
} from "./helpers/constant-values";
import { customFetch, guid, handleError } from "./helpers/services";

const btnAction = {
  createPlayer: 1,
  createGame: 2,
  joinGame: 3,
  broadcastMessage: 4,
  createBotPlayer: 5,
};

function App() {
  // === USE STATES ===
  const [connection, setConnection] = useState();

  const [player, setPlayer] = useState(defaultPlayer);
  const [playerLoading, setPlayerLoading] = useState(false);
  const [playerBotLoading, setPlayerBotLoading] = useState(false);

  const [game, setGame] = useState(defaultGame);
  const [gameLoading, setGameLoading] = useState(false);

  const [toggleChangeSuit, setToggleChangeSuit] = useState(false);
  const [toggleJoinCreate, setToggleJoinCreate] = useState(false);

  const [gameTurns, setGameTurns] = useState(defaultTurns);
  const [gameToJoin, setGameToJoin] = useState("");

  const [messages, setMessages] = useState([]);
  const [GameMessages, setGameMessages] = useState([]);
  const [messageInputText, setMessageInputText] = useState("");
  const [isMessageButtonDisabled, setIsMessageButtonDisabled] = useState(true);

  const [countdown, setCountdown] = useState(countdownSec);
  const [isCountdownActive, setIsCountdownActive] = useState(false);
  const [timeoutObj, setTimeoutObj] = useState({});

  const [playTurnResolver, setPlayTurnResolver] = useState(null);
  const [changeSuitResolver, setChangeSuitResolver] = useState(null);

  const playerRef = useRef(player); // Create a mutable ref for player

  // === USE EFFECTS ===

  // Establish connection with the signalR server
  useEffect(() => {
    const connect = new signalR.HubConnectionBuilder()
      .withUrl(`${serverBaseUri}/hubs/game`)
      .withAutomaticReconnect()
      .build();

    setConnection(connect);
    // Uncomment only for debugging purposes
    // connect.serverTimeoutInMilliseconds = 3600000;
    // connect.keepAliveIntervalInMilliseconds = 3600000;
  }, []);

  // Update the player ref when player state changes
  useEffect(() => {
    playerRef.current = player;
  }, [player]);

  // SignalR server events
  useEffect(() => {
    if (connection) {
      connection
        .start()
        .then(() => {
          // Enable messages
          setIsMessageButtonDisabled(false);

          connection.on("BroadcastMessageReceived", (player, message) => {
            if (player.toLowerCase() === "game")
              setGameMessages((prev) => [...prev, `${player}: ${message}`]);
            else setMessages((prev) => [...prev, `${player}: ${message}`]);
          });

          connection.on(
            "PrivateMessageReceived",
            (player, receiver, message) => {
              if (player.toLowerCase() === "game")
                setGameMessages((prev) => [...prev, `${player}: ${message}`]);
              else setMessages((prev) => [...prev, `${player}: ${message}`]);
            }
          );

          connection.on("PlayerJoined", (name) => {
            setMessages((prev) => [...prev, `-- ${name} has join the game --`]);
          });

          connection.on("GameUpdated", (updatedGame) => {
            setGame(updatedGame);
          });

          connection.on("PlayerDataUpdated", (playerData) => {
            setGame((prevGame) => {
              const newGame = { ...prevGame };
              newGame.playersData[playerRef.current.id] = playerData;
              return newGame;
            });
          });

          connection.on("PlayTurn", async () => {
            return promiseResolver(setPlayTurnResolver, passCard);
          });

          connection.on("ChangeSuit", async () => {
            return promiseResolver(setChangeSuitResolver, suit.Pass);
          });
        })
        .catch((error) => alert(error));
    }
    return () => connection?.stop();
  }, [connection]);

  // Countdown timer event
  useEffect(() => {
    let interval = null;
    if (isCountdownActive) {
      interval = setInterval(() => {
        setCountdown((prev) => prev - 1);
      }, 1000);
    } else if (!isCountdownActive && countdown === 0) {
      clearInterval(interval);
    }
    return () => clearInterval(interval);
  }, [isCountdownActive, countdown]);

  // === FUNCTIONS ===

  const promiseResolver = (resolver, defaultValue) => {
    setIsCountdownActive(true); // Start countdown

    // This promise will eventually be send to server
    return new Promise((resolve) => {
      resolver(() => (value) => resolve(value)); // Keep resolve method instance so it can be called from the outside
      let id = setTimeout(() => {
        resolve(defaultValue); // The man forgot to play and times is up! (resolve it with the default value)
        setIsCountdownActive(false);
        setCountdown(countdownSec);
      }, (countdownSec + 2) * 1000); // For some reason I must add 2 seconds on the timeout in order to match the real countdown
      setTimeoutObj(id); // Keep the timeout id in order to clear it from the outside
    });
  };

  const playTurn = async (cardToPlay) => {
    const valid = await validateMove(cardToPlay);
    if (!valid) return;

    clearTimeout(timeoutObj);
    setTimeoutObj({});
    await playTurnResolver(cardToPlay);
    setIsCountdownActive(false);
    setCountdown(countdownSec);

    if (cardToPlay.rank === rank.Ace) setToggleChangeSuit(true);
  };

  const changeSuit = async (suit) => {
    clearTimeout(timeoutObj);
    setTimeoutObj({});
    await changeSuitResolver(suit);
    setToggleChangeSuit(false);
    setIsCountdownActive(false);
    setCountdown(countdownSec);
  };

  const validateMove = async (move) => {
    const requestInfo = {
      url: `${serverBaseUri}${apiVersion}/Game/${game.id}/validate-move`,
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: { playerId: player.id, move: move },
    };

    return await customFetch(requestInfo);
  };

  const createPlayer = async () => {
    const requestInfo = {
      url: `${serverBaseUri}${apiVersion}/Player/new?Name=${player.name}`,
      method: "POST",
      headers: { "Content-Type": "application/json" },
    };

    setPlayerLoading(true);
    const response = await customFetch(requestInfo);
    setPlayerLoading(false);

    if (!response) return;
    setPlayer(response);
  };

  const createBotPlayer = async () => {
    if (!connection) return;

    const requestInfo = {
      url: `${serverBaseUri}${apiVersion}/Player/new?Name=bot&isBot=true`,
      method: "POST",
      headers: { "Content-Type": "application/json" },
    };

    try {
      setPlayerBotLoading(true);
      const response = await customFetch(requestInfo);
      const isLeader = false;
      await connection.invoke("JoinGame", game.id, response.id, isLeader);
    } catch (errorObj) {
      handleError(errorObj);
    } finally {
      setPlayerBotLoading(false);
    }
  };

  const createGame = async () => {
    const requestInfo = {
      url: `${serverBaseUri}${apiVersion}/Game/new`,
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: { playerId: player.id, turns: gameTurns },
    };

    setGameLoading(true);
    const response = await customFetch(requestInfo);
    setGame(response);
    const isLeader = true;
    if (connection)
      connection.invoke("JoinGame", response.id, player.id, isLeader);
    setGameLoading(false);
  };

  const joinGame = async () => {
    setGameLoading(true);
    if (!connection) return;

    try {
      const isLeader = false;
      await connection.invoke("JoinGame", gameToJoin, player.id, isLeader);
      setGame((prev) => ({ ...prev, id: gameToJoin }));
    } catch (errorObj) {
      handleError(errorObj);
    } finally {
      setGameLoading(false);
    }
  };

  const startGame = async () => {
    try {
      if (connection) await connection.invoke("StartGame", player.id, game.id);
    } catch (errorObj) {
      handleError(errorObj);
    }
  };

  const drawCard = async () => {
    const requestInfo = {
      url: `${serverBaseUri}${apiVersion}/Game/${game.id}/${player.id}/draw-card`,
      headers: { "Content-Type": "application/json" },
    };

    await customFetch(requestInfo);
  };

  const broadcastMessage = async () => {
    const requestInfo = {
      url: `${serverBaseUri}${apiVersion}/Game/${game.id}/${player.id}/broadcast-message`,
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: { message: messageInputText },
    };

    await customFetch(requestInfo);
    setMessageInputText("");
  };

  const handleInputKeyDown = async (event, action) => {
    if (event.key !== "Enter") return;
    event.preventDefault();
    if (action === btnAction.createPlayer) await createPlayer();
    if (action === btnAction.createGame) await createGame();
    if (action === btnAction.joinGame) await joinGame();
    if (action === btnAction.broadcastMessage) await broadcastMessage();
    if (action === btnAction.createBotPlayer) await createBotPlayer();
  };

  // === JSX ===

  return (
    <div style={{ display: "flex", flexFlow: "column", height: "100vh" }}>
      <span id="forkongithub">
        <a
          target="_blank"
          href="https://github.com/mhtros/suspense"
          rel="noreferrer"
        >
          Fork me on GitHub
        </a>
      </span>
      <header
        style={{
          backgroundColor: "#333",
          color: "white",
          textAlign: "center",
          fontSize: 30,
          padding: "5px",
          marginBottom: "1rem",
        }}
      >
        Lets Play Suspense!
      </header>
      <div style={{ padding: "4px 4px 0 4px", flexGrow: 1 }}>
        {/* DEBUGGING */}
        {DEBUGGING && (
          <div>
            <div style={{ display: "flex", gap: "2rem" }}>
              <div>
                <h3>Player</h3>
                <pre>{JSON.stringify(player, null, 2)}</pre>
              </div>
              <div>
                <h3>Game</h3>
                <pre>{JSON.stringify(game, null, 2)}</pre>
              </div>
            </div>
            <hr />
          </div>
        )}

        {/* NAME */}
        <label htmlFor="name">Playername: </label>

        {/* If name is not set that means the players are not created on the server yet so show the input */}
        {playerLoading === false && player.id === "" && (
          <input
            id="name"
            style={{ marginRight: "4px" }}
            value={player.name}
            onKeyDown={(e) => handleInputKeyDown(e, btnAction.createPlayer)}
            onChange={(i) =>
              setPlayer((prevState) => ({
                ...prevState,
                name: i.target.value,
              }))
            }
          />
        )}

        {/* else the players have been created on the server so show the name and the set button */}
        {playerLoading === false && player.id !== "" && (
          <span style={{ marginRight: "4px" }}>{player.name}</span>
        )}

        {playerLoading === true && <span>Loading...</span>}

        {playerLoading === false && player.id === "" && (
          <button onClick={createPlayer}>Set</button>
        )}

        {/* If server has created the player but the game is not yet created then show available actions join/create game */}
        {player.id !== "" && game.id === "" && (
          <>
            <div style={{ marginTop: "1rem" }}>
              <label htmlFor="joinGame">Join/Create: </label>
              <input
                id="joinGame"
                type="checkbox"
                value={toggleJoinCreate}
                onChange={() => setToggleJoinCreate((prev) => !prev)}
              />
            </div>

            {/* CREATE GAME */}
            {!toggleJoinCreate && (
              <>
                {/* Loading */}
                {gameLoading === true && <span>Loading...</span>}
                {/* Display */}
                {gameLoading === false && (
                  <>
                    <label htmlFor="noOfTurns">Numbers of Turns: </label>
                    <select
                      id="noOfTurns"
                      onChange={(i) => setGameTurns(i.target.value)}
                      defaultValue={defaultTurns}
                    >
                      {[1, 2, 3, 5, 10, 15, 20].map((no, index) => {
                        return <option key={index}>{no}</option>;
                      })}
                    </select>
                    <button style={{ marginLeft: "1rem" }} onClick={createGame}>
                      Create Game
                    </button>
                  </>
                )}
              </>
            )}

            {/* JOIN GAME */}
            {toggleJoinCreate && (
              <>
                {/* Loading */}
                {gameLoading === true && <span>Loading...</span>}
                {/* Display */}
                {gameLoading === false && (
                  <>
                    <input
                      value={gameToJoin}
                      style={{
                        marginRight: "4px",
                        minWidth: "270px",
                        height: "1.3rem",
                      }}
                      onKeyDown={(e) =>
                        handleInputKeyDown(e, btnAction.joinGame)
                      }
                      onChange={(input) => {
                        setGameToJoin(input.target.value);
                      }}
                    />
                    <button style={{ height: "1.6rem" }} onClick={joinGame}>
                      Join Game
                    </button>
                  </>
                )}
              </>
            )}
          </>
        )}

        {!!game.id && (
          <div style={{ marginBottom: "1rem" }}>
            Game: {game.id}{" "}
            <button onClick={() => navigator.clipboard.writeText(game.id)}>
              Copy
            </button>
          </div>
        )}

        {/* START GAME */}
        {!!player?.id &&
          !!game?.playersData &&
          !!game?.id &&
          game?.playersData[`${player?.id}`]?.isLeader &&
          !playerBotLoading && (
            <div style={{ margin: "1rem 0" }}>
              {game.currentTurn === 0 && (
                <>
                  <button onClick={createBotPlayer}>Create BOT</button>
                  <button
                    style={{ marginLeft: "1rem" }}
                    disabled={Object.keys(game.playersData).length <= 1}
                    onClick={startGame}
                  >
                    Start Game
                  </button>
                </>
              )}
              {Object.keys(game.playersData).length <= 1 && (
                <span style={{ color: "#aaa", marginLeft: "4px" }}>
                  {" "}
                  awaiting players to join...
                </span>
              )}
            </div>
          )}

        {/* Loading for START GAME or CREATE BOT */}
        {!!player?.id &&
          !!game?.playersData &&
          !!game?.id &&
          game?.playersData[`${player?.id}`]?.isLeader &&
          !!playerBotLoading && (
            <div style={{ margin: "1rem 0" }}>Loading...</div>
          )}

        {/* GAME STATISTICS AND ACTIONS */}
        {!!game.playedCards && game.playedCards.length > 0 && (
          <div>
            <div style={{ marginBottom: "1rem" }}>
              {/* Turns and Time */}
              <div>
                Turn: {game.currentTurn} of {game.turnLimit}
                {countdown > 0 && isCountdownActive && (
                  <>
                    ( Time:{" "}
                    <span
                      style={{
                        color: `${countdown <= 10 ? "red" : "green"}`,
                        fontWeight: "bold",
                      }}
                    >
                      {countdown}
                    </span>
                    )
                  </>
                )}
              </div>
              {/* Current player */}
              <div>
                CurrentPlayer:{" "}
                <span style={{ color: "blue" }}>
                  {
                    game?.playersData[
                      Object.keys(game?.playersData).find(
                        (key) => game?.playersData[key]?.isHisTurn === true
                      )
                    ]?.player?.name
                  }
                </span>
              </div>
            </div>

            <div style={{ display: "flex", gap: "1.5rem", marginLeft: "10px" }}>
              <div style={{ width: "max-content", marginBottom: "1rem" }}>
                <div style={{ textAlign: "center" }}>At Table</div>
                <img
                  alt="Current card on table"
                  className="card-image"
                  style={{ width: "80px", height: "110px", margin: 0 }}
                  src={
                    cards[
                      `${game.playedCards.at(-1).suit}${
                        game.playedCards.at(-1).rank
                      }`
                    ]
                  }
                ></img>
              </div>
              <button
                style={{
                  width: "100px",
                  height: "50px",
                  marginTop: "50px",
                  fontSize: "1rem",
                }}
                onClick={drawCard}
              >
                Draw ({game.cardsLeft})
              </button>
              <button
                style={{
                  width: "100px",
                  height: "50px",
                  marginTop: "50px",
                  fontSize: "1rem",
                }}
                onClick={() => playTurn(passCard)}
              >
                Pass
              </button>
            </div>
          </div>
        )}

        {/* CHANGE SUIT */}
        {!!toggleChangeSuit && (
          <div style={{ marginTop: "0.5rem", marginBottom: "0.2rem" }}>
            <h3>Change Suit</h3>
            <img
              alt="Ace diamond"
              onClick={() => changeSuit(suit.Diamonds)}
              style={{ width: "70px", height: "100px", marginRight: "10px" }}
              className="card-image"
              src={cards[11]}
            ></img>
            <img
              alt="Ace clubs"
              onClick={() => changeSuit(suit.Clubs)}
              style={{ width: "70px", height: "100px", marginRight: "10px" }}
              className="card-image"
              src={cards[21]}
            ></img>
            <img
              alt="Ace hearts"
              onClick={() => changeSuit(suit.Hearts)}
              style={{ width: "70px", height: "100px", marginRight: "10px" }}
              className="card-image"
              src={cards[31]}
            ></img>
            <img
              alt="Ace spades"
              onClick={() => changeSuit(suit.Spades)}
              style={{ width: "70px", height: "100px", marginRight: "10px" }}
              className="card-image"
              src={cards[41]}
            ></img>
          </div>
        )}

        {/* PLAYERS HANDS */}
        {!!game?.playersData &&
          !!Object.keys(game.playersData) &&
          Object.entries(game.playersData).map((playerData) => {
            return (
              <div
                key={playerData[playerDataHashIndex].player.id}
                style={{ maxWidth: "800px" }}
              >
                {/* player name */}
                <div>
                  {playerData[playerDataHashIndex].player.name} ( Score:{" "}
                  {playerData[playerDataHashIndex].score})
                </div>

                {/* CURRENT player hand */}
                {playerData[playerDataHashIndex].player.id === player?.id && (
                  <ul style={{ listStyleType: "none", padding: "0" }}>
                    {playerData[playerDataHashIndex]?.hand.map((card) => (
                      <li
                        style={{ display: "inline-block", marginLeft: "10px" }}
                        key={`${playerData[playerDataHashIndex].player.id}_${card.rank}_${card.suit}`}
                      >
                        <img
                          alt="Players cards"
                          onClick={() => playTurn(card)}
                          className="card-image"
                          src={cards[`${card.suit}${card.rank}`]}
                        ></img>
                      </li>
                    ))}
                  </ul>
                )}

                {/* OTHER players hands */}
                {playerData[playerDataHashIndex].player.id !== player?.id && (
                  <ul style={{ listStyleType: "none", padding: "0" }}>
                    {Array.from({ length: playerData[1].cardsLeft }, (_, i) => (
                      <li
                        style={{ display: "inline-block", marginLeft: "10px" }}
                        key={`${i}_oppoment_card`}
                      >
                        <img
                          alt="Opponents cards"
                          className="card-image"
                          style={{ width: "80px", height: "110px" }}
                          src={cards[1]}
                        ></img>
                      </li>
                    ))}
                  </ul>
                )}
              </div>
            );
          })}

        {!!game.id && (
          <>
            {/* GAME CHAT MESSAGES */}
            <div style={{ maxWidth: "800px", marginBottom: "1rem" }}>
              <div
                style={{
                  height: "200px",
                  background: "#fff1ee",
                  overflow: "auto",
                  margin: "10px 0",
                }}
              >
                <ul
                  style={{
                    listStyleType: "none",
                    margin: 0,
                    padding: "20px 0 0 10px",
                  }}
                >
                  {GameMessages.map((m) => (
                    <li key={guid()}>{m}</li>
                  ))}
                </ul>
              </div>
            </div>

            {/* PLAYERES CHAT MESSAGES */}
            <div style={{ maxWidth: "800px", marginBottom: "1rem" }}>
              <div
                style={{
                  height: "200px",
                  background: "#eeeeef",
                  overflow: "auto",
                  margin: "10px 0",
                }}
              >
                <ul
                  style={{
                    listStyleType: "none",
                    margin: 0,
                    padding: "20px 0 0 10px",
                  }}
                >
                  {messages.map((m) => (
                    <li key={guid()}>{m}</li>
                  ))}
                </ul>
              </div>
              <div style={{ display: "flex", gap: "4px" }}>
                <input
                  value={messageInputText}
                  style={{ width: "100%", height: "1.5rem" }}
                  placeholder="Enter message..."
                  onKeyDown={(e) =>
                    handleInputKeyDown(e, btnAction.broadcastMessage)
                  }
                  onChange={(input) => {
                    setMessageInputText(input.target.value);
                  }}
                />
                <button
                  style={{ height: "1.8rem", width: "4rem" }}
                  disabled={isMessageButtonDisabled}
                  onClick={broadcastMessage}
                  type="primary"
                >
                  Send
                </button>
              </div>
            </div>
          </>
        )}
      </div>
      <footer style={{ height: 30, borderTop: "2px solid #eee" }}>
        <div style={{ fontSize: 12, margin: "6px 1rem" }}>
          Created by{" "}
          <a
            target="_blank"
            href="https://gr.linkedin.com/in/panagiotis-mitropanos-66b479219"
            rel="noreferrer"
          >
            Panagiotis Mitropanos
          </a>
          {" - "}
          {new Date().getFullYear()}
        </div>
      </footer>
    </div>
  );
}

export default App;
