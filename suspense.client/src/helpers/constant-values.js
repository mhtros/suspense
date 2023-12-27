import Clubs10 from "../../src/cards/clubs_10.png";
import Clubs02 from "../../src/cards/clubs_2.png";
import Clubs03 from "../../src/cards/clubs_3.png";
import Clubs04 from "../../src/cards/clubs_4.png";
import Clubs05 from "../../src/cards/clubs_5.png";
import Clubs06 from "../../src/cards/clubs_6.png";
import Clubs07 from "../../src/cards/clubs_7.png";
import Clubs08 from "../../src/cards/clubs_8.png";
import Clubs09 from "../../src/cards/clubs_9.png";
import ClubsAce from "../../src/cards/clubs_ace.png";
import ClubsJack from "../../src/cards/clubs_jack.png";
import ClubsKing from "../../src/cards/clubs_king.png";
import ClubsQueen from "../../src/cards/clubs_queen.png";
import Diamonds10 from "../../src/cards/diamonds_10.png";
import Diamonds02 from "../../src/cards/diamonds_2.png";
import Diamonds03 from "../../src/cards/diamonds_3.png";
import Diamonds04 from "../../src/cards/diamonds_4.png";
import Diamonds05 from "../../src/cards/diamonds_5.png";
import Diamonds06 from "../../src/cards/diamonds_6.png";
import Diamonds07 from "../../src/cards/diamonds_7.png";
import Diamonds08 from "../../src/cards/diamonds_8.png";
import Diamonds09 from "../../src/cards/diamonds_9.png";
import DiamondsAce from "../../src/cards/diamonds_ace.png";
import DiamondsJack from "../../src/cards/diamonds_jack.png";
import DiamondsKing from "../../src/cards/diamonds_king.png";
import DiamondsQueen from "../../src/cards/diamonds_queen.png";
import Hearts10 from "../../src/cards/hearts_10.png";
import Hearts02 from "../../src/cards/hearts_2.png";
import Hearts03 from "../../src/cards/hearts_3.png";
import Hearts04 from "../../src/cards/hearts_4.png";
import Hearts05 from "../../src/cards/hearts_5.png";
import Hearts06 from "../../src/cards/hearts_6.png";
import Hearts07 from "../../src/cards/hearts_7.png";
import Hearts08 from "../../src/cards/hearts_8.png";
import Hearts09 from "../../src/cards/hearts_9.png";
import HeartsAce from "../../src/cards/hearts_ace.png";
import HeartsJack from "../../src/cards/hearts_jack.png";
import HeartsKing from "../../src/cards/hearts_king.png";
import HeartsQueen from "../../src/cards/hearts_queen.png";
import Back from "../../src/cards/red.png";
import Spades10 from "../../src/cards/spades_10.png";
import Spades02 from "../../src/cards/spades_2.png";
import Spades03 from "../../src/cards/spades_3.png";
import Spades04 from "../../src/cards/spades_4.png";
import Spades05 from "../../src/cards/spades_5.png";
import Spades06 from "../../src/cards/spades_6.png";
import Spades07 from "../../src/cards/spades_7.png";
import Spades08 from "../../src/cards/spades_8.png";
import Spades09 from "../../src/cards/spades_9.png";
import SpadesAce from "../../src/cards/spades_ace.png";
import SpadesJack from "../../src/cards/spades_jack.png";
import SpadesKing from "../../src/cards/spades_king.png";
import SpadesQueen from "../../src/cards/spades_queen.png";

const DEBUGGING = false;
const countdownSec = 60;

const apiVersion = "/api/v1";
const serverBaseUri = process.env.REACT_APP_SERVER_BASE_URI || "http://localhost:5114";

const defaultTurns = 1;
const playerDataHashIndex = 1;

const defaultPlayer = {id: "", name: "", connectionId: ""};

const cards = {
    1: Back,
    22: Clubs02,
    23: Clubs03,
    24: Clubs04,
    25: Clubs05,
    26: Clubs06,
    27: Clubs07,
    28: Clubs08,
    29: Clubs09,
    210: Clubs10,
    21: ClubsAce,
    211: ClubsJack,
    213: ClubsKing,
    212: ClubsQueen,
    12: Diamonds02,
    13: Diamonds03,
    14: Diamonds04,
    15: Diamonds05,
    16: Diamonds06,
    17: Diamonds07,
    18: Diamonds08,
    19: Diamonds09,
    110: Diamonds10,
    11: DiamondsAce,
    111: DiamondsJack,
    113: DiamondsKing,
    112: DiamondsQueen,
    32: Hearts02,
    33: Hearts03,
    34: Hearts04,
    35: Hearts05,
    36: Hearts06,
    37: Hearts07,
    38: Hearts08,
    39: Hearts09,
    310: Hearts10,
    31: HeartsAce,
    311: HeartsJack,
    313: HeartsKing,
    312: HeartsQueen,
    42: Spades02,
    43: Spades03,
    44: Spades04,
    45: Spades05,
    46: Spades06,
    47: Spades07,
    48: Spades08,
    49: Spades09,
    410: Spades10,
    41: SpadesAce,
    411: SpadesJack,
    413: SpadesKing,
    412: SpadesQueen,
};

const rank = {
    Pass: -2,
    Invalid: -1,
    Ace: 1,
    Two: 2,
    Three: 3,
    Four: 4,
    Five: 5,
    Six: 6,
    Seven: 7,
    Eight: 8,
    Nine: 9,
    Ten: 10,
    Jack: 11,
    Queen: 12,
    King: 13,
};

const suit = {
    Pass: -2,
    Invalid: -1,
    Diamonds: 1,
    Clubs: 2,
    Hearts: 3,
    Spades: 4,
};

const passCard = {suit: suit.Pass, rank: rank.Pass};

const defaultGame = {
    id: "",
    turnLimit: 0,
    currentTurn: 0,
    cardsLeft: 0,
    players: [],
    deck: [],
    playedCards: [],
    unusedDealtCards: [],
};

export {
    DEBUGGING,
    apiVersion,
    cards,
    countdownSec,
    defaultGame,
    defaultPlayer,
    defaultTurns,
    passCard,
    playerDataHashIndex,
    rank,
    serverBaseUri,
    suit,
};
