using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Player.Models;
using Player.Utility;
using Player.Test.Mocks;
using CommunicationUtils.Payloads;
using CommunicationUtils.Structures;
using CommunicationUtils;

namespace Player.Test
{
    [TestClass]
    public class MoveTests
    {
        private ConfigurationLoader _configLoader;
        private Player _player;
        [TestInitialize]
        public void Setup()
        {
            ILogger<ConfigurationLoader> logger = new Logger<ConfigurationLoader>(new NullLoggerFactory());
            ILogger<Player> playerLogger = new Logger<Player>(new NullLoggerFactory());
            EmptyStrategyMock strategy = new EmptyStrategyMock();
            _configLoader = new ConfigurationLoader(logger);
            _player = new Player(playerLogger, strategy, _configLoader);
            _player._communicator = new SynchronousCommunicator();
        }

        [TestMethod]
        public void TestCouldntMove()
        {
            // Given
            StartGamePayload testPayload = new StartGamePayload()
            {
                agentID = 1,
                boardSize = new Vector2D() { y = 10, x = 10 },
                alliesIDs = new int[] { 2, 3, 4 },
                enemiesIDs = new int[] { 5, 6, 7 },
                goalAreaSize = 3,
                leaderID = 2,
                position = new Vector2D() { y = 0, x = 0 },
                penalties = new Penalties()
                {
                    checkForSham = 1,
                    destroyPiece = 1,
                    discovery = 1,
                    move = 1,
                    response = 1,
                    putPiece = 1
                }
            };
            MovementResponsePayload testDiscPayload = new MovementResponsePayload()
            {
                closestPiece = null,
                currentPosition = new Vector2D() { y = 0, x = 0 },
                madeMove = false
            };
            _player._state = PlayerState.AwaitingGameStart;
            // When
            _player.ProcessStartGameResponse(testPayload);
            _player._lastAction = Action.Move;
            _player._state = PlayerState.AwaitingGMAcceptanceForMove;
            _player._couldMove = true;
            _player.ProcessMoveResponse(testDiscPayload);
            // Then
            Assert.AreEqual(_player.Position.x, 0);
            Assert.AreEqual(_player.Position.y, 0);
            Assert.AreEqual(_player._couldMove, false);
            Assert.AreEqual(_player.Board[_player.Position.y, _player.Position.x].PlayerInfo, true);
        }

        [TestMethod]
        public void TestCouldMove()
        {
            // Given
            StartGamePayload testPayload = new StartGamePayload()
            {
                agentID = 1,
                boardSize = new Vector2D() { y = 10, x = 10 },
                alliesIDs = new int[] { 2, 3, 4 },
                enemiesIDs = new int[] { 5, 6, 7 },
                goalAreaSize = 3,
                leaderID = 2,
                position = new Vector2D() { y = 0, x = 0 },
                penalties = new Penalties()
                {
                    checkForSham = 1,
                    destroyPiece = 1,
                    discovery = 1,
                    move = 1,
                    response = 1,
                    putPiece = 1
                }
            };
            MovementResponsePayload testDiscPayload = new MovementResponsePayload()
            {
                closestPiece = null,
                currentPosition = new Vector2D() { y = 1, x = 0 },
                madeMove = true
            };
            _player._state = PlayerState.AwaitingGameStart;
            // When
            _player.ProcessStartGameResponse(testPayload);
            _player._lastAction = Action.Move;
            _player._state = PlayerState.AwaitingGMAcceptanceForMove;
            _player._couldMove = false;
            _player.ProcessMoveResponse(testDiscPayload);
            // Then
            Assert.AreEqual(_player.Position.x, 0);
            Assert.AreEqual(_player.Position.y, 1);
            Assert.AreEqual(_player._couldMove, true);
            Assert.AreEqual(_player.Board[_player.Position.y, _player.Position.x].PlayerInfo, true);
            Assert.AreEqual(_player.Board[_player.Position.y - 1, _player.Position.x].PlayerInfo, false);
        }

        [TestMethod]
        public void TestMoveError()
        {
            // Given
            StartGamePayload testPayload = new StartGamePayload()
            {
                agentID = 1,
                boardSize = new Vector2D() { y = 10, x = 10 },
                alliesIDs = new int[] { 2, 3, 4 },
                enemiesIDs = new int[] { 5, 6, 7 },
                goalAreaSize = 3,
                leaderID = 2,
                position = new Vector2D() { y = 0, x = 0 },
                penalties = new Penalties()
                {
                    checkForSham = 1,
                    destroyPiece = 1,
                    discovery = 1,
                    move = 1,
                    response = 1,
                    putPiece = 1
                }
            };
            MovementErrorPayload testDiscPayload = new MovementErrorPayload()
            {
                position = new Vector2D() { y = 0, x = 0 }
            };
            _player._state = PlayerState.AwaitingGameStart;
            // When
            _player.ProcessStartGameResponse(testPayload);
            _player._lastAction = Action.Move;
            _player._state = PlayerState.AwaitingGMAcceptanceForMove;
            _player._couldMove = true;
            _player.ProcessMoveErrorResponse(testDiscPayload);
            // Then
            Assert.AreEqual(_player.Position.x, 0);
            Assert.AreEqual(_player.Position.y, 0);
            Assert.AreEqual(_player._couldMove, false);
        }
    }
}
