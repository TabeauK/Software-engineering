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
    public class TestPieceTests
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
        public void TestPieceNoPieceTest()
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

            TestPieceResponsePayload testTestPayload = new TestPieceResponsePayload()
            {
                sham = false
            };

            _player._state = PlayerState.AwaitingGameStart;

            // When

            _player.ProcessStartGameResponse(testPayload);
            _player._lastAction = Action.TestPiece;
            _player._state = PlayerState.AwaitingGMAcceptanceForMove;
            _player.PieceInfo = PieceState.NoPiece;
            _player.ProcessTestPieceResponse(testTestPayload);

            // Then

            Assert.AreEqual(_player.PieceInfo, PieceState.NoPiece);

        }

        [TestMethod]
        public void TestPieceShamPiece()
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

            TestPieceResponsePayload testTestPayload = new TestPieceResponsePayload()
            {
                sham = true
            };

            _player._state = PlayerState.AwaitingGameStart;

            // When

            _player.ProcessStartGameResponse(testPayload);
            _player._lastAction = Action.TestPiece;
            _player._state = PlayerState.AwaitingGMAcceptanceForMove;
            _player.PieceInfo = PieceState.HasUnknown;
            _player.ProcessTestPieceResponse(testTestPayload);

            // Then

            Assert.AreEqual(_player.PieceInfo, PieceState.NoPiece);

        }

        [TestMethod]
        public void TestPieceNotShamPiece()
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

            TestPieceResponsePayload testTestPayload = new TestPieceResponsePayload()
            {
                sham = false
            };

            _player._state = PlayerState.AwaitingGameStart;

            // When

            _player.ProcessStartGameResponse(testPayload);
            _player._lastAction = Action.TestPiece;
            _player._state = PlayerState.AwaitingGMAcceptanceForMove;
            _player.PieceInfo = PieceState.HasUnknown;
            _player.ProcessTestPieceResponse(testTestPayload);

            // Then

            Assert.AreEqual(_player.PieceInfo, PieceState.NotSham);

        }
    }
}
