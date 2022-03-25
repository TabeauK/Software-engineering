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
    public class InfoExchangeTests
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
        public void TestSimpleResponsePayloadSquareBoard()
        {
            // Given
            StartGamePayload testPayload = new StartGamePayload()
            {
                agentID = 1,
                boardSize = new Vector2D() { y = 5, x = 5 },
                alliesIDs = new int[] { 2, 3, 4 },
                enemiesIDs = new int[] { 5, 6, 7 },
                goalAreaSize = 2,
                leaderID = 2,
                position = new Vector2D() { y = 0, x = 0 },
                penalties = new Penalties()
                {
                    checkForSham = 1,
                    destroyPiece = 1,
                    discovery = 1,
                    move = 1,
                    ask = 1,
                    putPiece = 1,
                    response = 1
                }
            };
            InformationExchangeResponsePayload p = new InformationExchangeResponsePayload()
            {
                respondToID = 1,
                distances = new int?[]
                {
                    0,0,0,0,0,
                    1,1,1,1,1,
                    2,2,2,2,2,
                    3,3,3,3,3,
                    4,4,4,4,4,
                },
                blueTeamGoalAreaInformations = new GoalAreaTileInformation[]
                {
                    GoalAreaTileInformation.Unknown, GoalAreaTileInformation.Unknown, GoalAreaTileInformation.Unknown, GoalAreaTileInformation.Unknown, GoalAreaTileInformation.Unknown,
                    GoalAreaTileInformation.Unknown, GoalAreaTileInformation.Goal, GoalAreaTileInformation.NotGoal, GoalAreaTileInformation.Unknown, GoalAreaTileInformation.Unknown
                },
                redTeamGoalAreaInformations = new GoalAreaTileInformation[]
                {
                    GoalAreaTileInformation.NotGoal, GoalAreaTileInformation.Unknown, GoalAreaTileInformation.Unknown, GoalAreaTileInformation.Goal, GoalAreaTileInformation.Unknown,
                    GoalAreaTileInformation.Unknown, GoalAreaTileInformation.Goal, GoalAreaTileInformation.NotGoal, GoalAreaTileInformation.Unknown, GoalAreaTileInformation.Unknown
                }
            };
            _player._state = PlayerState.AwaitingGameStart;
            // When
            _player.ProcessStartGameResponse(testPayload);
            _player.ProcessInformationResponse(p);
            // Then
            Assert.AreEqual(_player.Board[0, 0].GoalInfo, GoalAreaTileInformation.Unknown);
            Assert.AreEqual(_player.Board[0, 1].GoalInfo, GoalAreaTileInformation.Goal);
            Assert.AreEqual(_player.Board[0, 2].GoalInfo, GoalAreaTileInformation.NotGoal);
            Assert.AreEqual(_player.Board[4, 0].GoalInfo, GoalAreaTileInformation.NotGoal);
            Assert.AreEqual(_player.Board[4, 1].GoalInfo, GoalAreaTileInformation.Unknown);
            Assert.AreEqual(_player.Board[3, 1].GoalInfo, GoalAreaTileInformation.Goal);
        }
        [TestMethod]
        public void TestSimpleResponsePayloadNonSquareBoard()
        {
            // Given
            StartGamePayload testPayload = new StartGamePayload()
            {
                agentID = 1,
                boardSize = new Vector2D() { y = 5, x = 4 },
                alliesIDs = new int[] { 2, 3, 4 },
                enemiesIDs = new int[] { 5, 6, 7 },
                goalAreaSize = 2,
                leaderID = 2,
                position = new Vector2D() { y = 0, x = 0 },
                penalties = new Penalties()
                {
                    checkForSham = 1,
                    destroyPiece = 1,
                    discovery = 1,
                    move = 1,
                    ask = 1,
                    putPiece = 1,
                    response = 1
                }
            };
            InformationExchangeResponsePayload p = new InformationExchangeResponsePayload()
            {
                respondToID = 1,
                distances = new int?[]
                {
                    0,0,0,0,
                    1,1,1,1,
                    2,2,2,2,
                    3,3,3,3,
                    4,4,4,4,
                },
                blueTeamGoalAreaInformations = new GoalAreaTileInformation[]
                {
                    GoalAreaTileInformation.Unknown, GoalAreaTileInformation.Unknown, GoalAreaTileInformation.Unknown, GoalAreaTileInformation.Unknown,
                    GoalAreaTileInformation.Unknown, GoalAreaTileInformation.Goal, GoalAreaTileInformation.NotGoal, GoalAreaTileInformation.Unknown
                },
                redTeamGoalAreaInformations = new GoalAreaTileInformation[]
                {
                    GoalAreaTileInformation.NotGoal, GoalAreaTileInformation.Unknown, GoalAreaTileInformation.Unknown, GoalAreaTileInformation.Goal,
                    GoalAreaTileInformation.Unknown, GoalAreaTileInformation.Goal, GoalAreaTileInformation.NotGoal, GoalAreaTileInformation.Goal
                }
            };
            _player._state = PlayerState.AwaitingGameStart;
            // When
            _player.ProcessStartGameResponse(testPayload);
            _player.ProcessInformationResponse(p);
            // Then
            Assert.AreEqual(_player.Board[0, 0].GoalInfo, GoalAreaTileInformation.Unknown);
            Assert.AreEqual(_player.Board[0, 1].GoalInfo, GoalAreaTileInformation.Goal);
            Assert.AreEqual(_player.Board[0, 2].GoalInfo, GoalAreaTileInformation.NotGoal);
            Assert.AreEqual(_player.Board[4, 0].GoalInfo, GoalAreaTileInformation.NotGoal);
            Assert.AreEqual(_player.Board[4, 1].GoalInfo, GoalAreaTileInformation.Unknown);
            Assert.AreEqual(_player.Board[3, 1].GoalInfo, GoalAreaTileInformation.Goal);
            Assert.AreEqual(_player.Board[4, 3].GoalInfo, GoalAreaTileInformation.Goal);
        }
    }
}
