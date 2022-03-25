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
    public class StartGameInitTests
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
        public void TestStartGameNotAwaiting()
        {
            // Given
            StartGamePayload testPayload = new StartGamePayload()
            {

            };
            _player._state = PlayerState.Finished;
            // When
            _player.ProcessStartGameResponse(testPayload);
            // Then
            Assert.AreEqual(_player._state, PlayerState.ReadyToFinish);
        }
        //[TestMethod]
        //public void TestStartGameFaultyPenalties()
        //{
        //    // Given
        //    StartGamePayload testPayload = new StartGamePayload()
        //    {
        //        agentID = 1,
        //        boardSize = new Vector2D() { y = 4, x = 4 },
        //        alliesIDs = new int[] { 2, 3, 4 },
        //        enemiesIDs = new int[] { 5, 6, 7 },
        //        goalAreaSize = 1,
        //        leaderID = 2,
        //        position = new Vector2D() { y = 2, x = 2 },
        //        penalties = new Penalties()
        //        {
        //            checkForScham = 1000,
        //            destroyPiece = 500,
        //            discovery = 350,
        //            move = 213,
        //            informationExchange = 432,
        //            putPiece = -73 //TODO tutaj był napis "liczba"
        //        }
        //    };
        //    _player._state = PlayerState.AwaitingGameStart;
        //    // When
        //    _player.ProcessStartGameResponse(testPayload);
        //    // Then
        //    Assert.AreEqual(_player._state, PlayerState.Finished);
        //}

        [TestMethod]
        public void TestStartGameAllGood()
        {
            // Given
            StartGamePayload testPayload = new StartGamePayload()
            {
                agentID = 1,
                boardSize = new Vector2D() { y = 4, x = 4 },
                alliesIDs = new int[] { 2, 3, 4 },
                enemiesIDs = new int[] { 5, 6, 7 },
                goalAreaSize = 1,
                leaderID = 2,
                position = new Vector2D() { y = 2, x = 2 },
                penalties = new Penalties()
                {
                    checkForSham = 1000,
                    destroyPiece = 500,
                    discovery = 350,
                    move = 213,
                    response = 432,
                    putPiece = 2000
                }
            };
            _player._state = PlayerState.AwaitingGameStart;
            // When
            _player.ProcessStartGameResponse(testPayload);
            // Then
            Assert.AreEqual(_player._state, PlayerState.Ready);
        }
    }
}
