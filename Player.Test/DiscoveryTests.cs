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
    public class DiscoveryTests
    {
        private Player _player;
        private ConfigurationLoader _configLoader;
        [TestInitialize]
        public void Setup()
        {
            ILogger<ConfigurationLoader> configLogger = new Logger<ConfigurationLoader>(new NullLoggerFactory());
            ILogger<Player> playerLogger = new Logger<Player>(new NullLoggerFactory());
            EmptyStrategyMock strategy = new EmptyStrategyMock();
            _configLoader = new ConfigurationLoader(configLogger);
            _player = new Player(playerLogger, strategy, _configLoader);
            _player._communicator = new SynchronousCommunicator();
        }

        [TestMethod]
        public void TestDiscoverCorner()
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
            DiscoverResponsePayload testDiscPayload = new DiscoverResponsePayload()
            {
                distanceNW = 1, distanceN = 1, distanceNE = 2,
                distanceW = 1, distanceFromCurrent = null, distanceE = 3,
                distanceSW = 1, distanceS = 1, distanceSE = 1
            };
            _player._state = PlayerState.AwaitingGameStart;
            // When
            _player.ProcessStartGameResponse(testPayload);
            _player._lastAction = Action.Discover;
            _player._state = PlayerState.AwaitingGMAcceptanceForMove;
            _player.ProcessDiscoveryResponse(testDiscPayload);
            // Then
            Assert.AreEqual(_player.Board[0, 0].DistToPiece, null);
            Assert.AreEqual(_player.Board[1, 0].DistToPiece, 1);
            Assert.AreEqual(_player.Board[1, 1].DistToPiece, 2);
            Assert.AreEqual(_player.Board[0, 1].DistToPiece, 3);
        }

        [TestMethod]
        public void TestDiscoverSide()
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
                position = new Vector2D() { y = 4, x = 9 },
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
            DiscoverResponsePayload testDiscPayload = new DiscoverResponsePayload()
            {
                distanceNW = 1,
                distanceN = 2,
                distanceNE = 2,
                distanceW = null,
                distanceFromCurrent = 4,
                distanceE = 3,
                distanceSW = 5,
                distanceS = 6,
                distanceSE = 1
            };
            _player._state = PlayerState.AwaitingGameStart;
            // When
            _player.ProcessStartGameResponse(testPayload);
            _player._lastAction = Action.Discover;
            _player._state = PlayerState.AwaitingGMAcceptanceForMove;
            _player.ProcessDiscoveryResponse(testDiscPayload);
            // Then
            Assert.AreEqual(_player.Board[4, 9].DistToPiece, 4);
            Assert.AreEqual(_player.Board[5, 9].DistToPiece, 2);
            Assert.AreEqual(_player.Board[5, 8].DistToPiece, 1);
            Assert.AreEqual(_player.Board[4, 8].DistToPiece, null);
            Assert.AreEqual(_player.Board[3, 8].DistToPiece, 5);
            Assert.AreEqual(_player.Board[3, 9].DistToPiece, 6);
        }

        [TestMethod]
        public void TestDiscoverMiddle()
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
                position = new Vector2D() { y = 4, x = 4 },
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
            DiscoverResponsePayload testDiscPayload = new DiscoverResponsePayload()
            {
                distanceNW = 1,
                distanceN = 2,
                distanceNE = 3,
                distanceW = 4,
                distanceFromCurrent = 5,
                distanceE = 6,
                distanceSW = 7,
                distanceS = 8,
                distanceSE = 9
            };
            _player._state = PlayerState.AwaitingGameStart;
            // When
            _player.ProcessStartGameResponse(testPayload);
            _player._lastAction = Action.Discover;
            _player._state = PlayerState.AwaitingGMAcceptanceForMove;
            _player.ProcessDiscoveryResponse(testDiscPayload);
            // Then
            Assert.AreEqual(_player.Board[5, 3].DistToPiece, 1);
            Assert.AreEqual(_player.Board[5, 4].DistToPiece, 2);
            Assert.AreEqual(_player.Board[5, 5].DistToPiece, 3);
            Assert.AreEqual(_player.Board[4, 3].DistToPiece, 4);
            Assert.AreEqual(_player.Board[4, 4].DistToPiece, 5);
            Assert.AreEqual(_player.Board[4, 5].DistToPiece, 6);
            Assert.AreEqual(_player.Board[3, 3].DistToPiece, 7);
            Assert.AreEqual(_player.Board[3, 4].DistToPiece, 8);
            Assert.AreEqual(_player.Board[3, 5].DistToPiece, 9);
        }

    }
}
