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
    public class PickupPieceTests
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
        public void TestPickupCouldPickup()
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
            PickupPieceResponsePayload testPickupPayload = new PickupPieceResponsePayload()
            {
                
            };
            _player._state = PlayerState.AwaitingGameStart;
            // When
            _player.ProcessStartGameResponse(testPayload);
            _player._lastAction = Action.PickupPiece;
            _player._state = PlayerState.AwaitingGMAcceptanceForMove;
            _player.ProcessPickupResponse(testPickupPayload);
            // Then
            Assert.AreEqual(_player.PieceInfo, PieceState.HasUnknown);
        }

        [TestMethod]
        public void TestPickupCouldntPickup()
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
            PickupPieceErrorPayload testPickupErrorPayload = new PickupPieceErrorPayload()
            {
                errorSubtype = PickupPieceErrorType.AlreadyCarriesPiece
            };
            _player._state = PlayerState.AwaitingGameStart;
            // When
            _player.ProcessStartGameResponse(testPayload);
            _player._lastAction = Action.PickupPiece;
            _player._state = PlayerState.AwaitingGMAcceptanceForMove;
            _player._couldPickup = true;
            _player._pickupError = PickupPieceErrorType.NoPieceToPickUp;
            _player.ProcessPickupErrorResponse(testPickupErrorPayload);
            // Then
            Assert.AreEqual(_player._couldPickup, false);
            Assert.AreEqual(_player._pickupError, PickupPieceErrorType.AlreadyCarriesPiece);
        }
    }
}
