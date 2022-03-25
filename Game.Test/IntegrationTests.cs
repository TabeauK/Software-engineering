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
using CommunicationServer;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Integration.Test
{
    [TestClass]
    public class IntegrationTests
    {
        private Player.Player _player1;
        private Player.Player _player2;
        private GameMaster.GameMaster _gm;
        private ConfigurationLoader _configLoader;
        private GameMaster.ConfigurationLoader _gmConfigLoader;
        [TestInitialize]
        public void Setup()
        {
            ILogger<ConfigurationLoader> configLogger = new Logger<ConfigurationLoader>(new NullLoggerFactory());
            ILogger<GameMaster.ConfigurationLoader> configLoggerGM = new Logger<GameMaster.ConfigurationLoader>(new NullLoggerFactory());
            ILogger<Player.Player> playerLogger = new Logger<Player.Player>(new NullLoggerFactory());
            ILogger<GameMaster.GameMaster> gmLogger = new Logger<GameMaster.GameMaster>(new NullLoggerFactory());
            EmptyStrategyMock strategy = new EmptyStrategyMock();
            _configLoader = new ConfigurationLoader(configLogger);
            _gmConfigLoader = new GameMaster.ConfigurationLoader(configLoggerGM);
            _player1 = new Player.Player(playerLogger, strategy, _configLoader);
            _player2 = new Player.Player(playerLogger, strategy, _configLoader);
            _gm = new GameMaster.GameMaster(gmLogger, _gmConfigLoader);
        }

        [TestMethod]
        public void TestConnectivityWithMoveN()
        {
            //given
            Server server = new Server();
            server.Launch();
            Task.Run(() => _gm.StartWithSyncCommunicator(server));
            Thread.Sleep(2000);
            _player1.StartSyncCommunicator(new string[] { "", "localhost", "3729", "Red" }, server);
            _player2.StartSyncCommunicator(new string[] { "", "localhost", "3729", "Blue" }, server);

            Task.Run(() => _player1.InitSyncAgent()); // do po³¹czenia z gr¹
            Task.Run(() => _player2.InitSyncAgent());
            Thread.Sleep(5000);
            // GameMaster.InitSyncGM();
            //when
            _player2.Move(Direction.North);
            Thread.Sleep(5000);
            //then
            Assert.AreEqual(_gm.teamsFull, true);
            Assert.AreEqual(_gm.bluePlayers.First().Value.Position.ToVector2D().y, 1);
            Assert.AreEqual(_gm.bluePlayers.First().Value.Position.ToVector2D().x, 0);
        }

        [TestMethod]
        public void TestConnectivityWithMoveS()
        {
            //given
            Server server = new Server();
            server.Launch();
            Task.Run(() => _gm.StartWithSyncCommunicator(server));
            Thread.Sleep(2000);
            _player1.StartSyncCommunicator(new string[] { "", "localhost", "3729", "Red" }, server);
            _player2.StartSyncCommunicator(new string[] { "", "localhost", "3729", "Blue" }, server);

            Task.Run(() => _player1.InitSyncAgent()); // do po³¹czenia z gr¹
            Task.Run(() => _player2.InitSyncAgent());
            Thread.Sleep(5000);
            // GameMaster.InitSyncGM();
            //when
            _player2.Move(Direction.South);
            Thread.Sleep(5000);
            Assert.IsTrue(_gm.teamsFull);
            Assert.AreEqual(_gm.bluePlayers.First().Value.Position.ToVector2D().y, 0);
            Assert.AreEqual(_gm.bluePlayers.First().Value.Position.ToVector2D().x, 0);
        }

        [TestMethod]
        public void TestConnectivityWithMovesEAndE()
        {
            //given
            Server server = new Server();
            server.Launch();
            Task.Run(() => _gm.StartWithSyncCommunicator(server));
            Thread.Sleep(2000);
            _player1.StartSyncCommunicator(new string[] { "", "localhost", "3729", "Red" }, server);
            _player2.StartSyncCommunicator(new string[] { "", "localhost", "3729", "Blue" }, server);

            Task.Run(() => _player1.InitSyncAgent()); // do po³¹czenia z gr¹
            Task.Run(() => _player2.InitSyncAgent());
            Thread.Sleep(5000);
            // GameMaster.InitSyncGM();
            //when
            _player1.Move(Direction.East);
            Thread.Sleep(5000);
            _player2.Move(Direction.East);
            Thread.Sleep(5000);
            //then
            Assert.IsTrue(_gm.teamsFull);
            Assert.AreEqual(_gm.bluePlayers.First().Value.Position.ToVector2D().y, 0);
            Assert.AreEqual(_gm.bluePlayers.First().Value.Position.ToVector2D().x, 1);
            Assert.AreEqual(_gm.redPlayers.First().Value.Position.ToVector2D().y, 2);
            Assert.AreEqual(_gm.redPlayers.First().Value.Position.ToVector2D().x, 1);
        }

        [TestMethod]
        public void TestConnectivityWithMovesEEAndEE()
        {
            //given
            Server server = new Server();
            server.Launch();
            Task.Run(() => _gm.StartWithSyncCommunicator(server));
            Thread.Sleep(2000);
            _player1.StartSyncCommunicator(new string[] { "", "localhost", "3729", "Red" }, server);
            _player2.StartSyncCommunicator(new string[] { "", "localhost", "3729", "Blue" }, server);

            Task.Run(() => _player1.InitSyncAgent()); // do po³¹czenia z gr¹
            Task.Run(() => _player2.InitSyncAgent());
            Thread.Sleep(5000);
            // GameMaster.InitSyncGM();
            //when
            _player1.Move(Direction.East);
            Thread.Sleep(5000);
            _player2.Move(Direction.East);
            Thread.Sleep(5000);
            _player1.Move(Direction.East);
            Thread.Sleep(5000);
            _player2.Move(Direction.East);
            Thread.Sleep(5000);
            //then
            Assert.IsTrue(_gm.teamsFull);
            Assert.AreEqual(_gm.bluePlayers.First().Value.Position.ToVector2D().y, 0);
            Assert.AreEqual(_gm.bluePlayers.First().Value.Position.ToVector2D().x, 2);
            Assert.AreEqual(_gm.redPlayers.First().Value.Position.ToVector2D().y, 2);
            Assert.AreEqual(_gm.redPlayers.First().Value.Position.ToVector2D().x, 2);
        }

        [TestMethod]
        public void TestConnectivityWithPickingUpPiece()
        {
            //given
            Server server = new Server();
            server.Launch();
            Task.Run(() => _gm.StartWithSyncCommunicator(server));
            Thread.Sleep(2000);
            _player1.StartSyncCommunicator(new string[] { "", "localhost", "3729", "Red" }, server);
            _player2.StartSyncCommunicator(new string[] { "", "localhost", "3729", "Blue" }, server);

            Task.Run(() => _player1.InitSyncAgent()); // do po³¹czenia z gr¹
            Task.Run(() => _player2.InitSyncAgent());
            Thread.Sleep(5000);
            // GameMaster.InitSyncGM();
            //when
            _player1.Move(Direction.South);
            Thread.Sleep(5000);
            _player1.PickupPiece();
            Thread.Sleep(5000);
            _player1.Move(Direction.East);
            Thread.Sleep(5000);
            _player1.PickupPiece();
            Thread.Sleep(5000);
            _player1.Move(Direction.East);
            Thread.Sleep(5000);
            _player1.PickupPiece();
            Thread.Sleep(5000);
            //then
            Assert.IsTrue(_gm.teamsFull);
            Assert.AreEqual(_gm.redPlayers.First().Value.Position.ToVector2D().y, 1);
            Assert.AreEqual(_gm.redPlayers.First().Value.Position.ToVector2D().x, 2);
            Assert.IsNotNull(_gm.redPlayers.First().Value.Holding);
        }

        [TestMethod]
        public void TestConnectivityWithPickingUpNotSham()
        {
            //given
            Server server = new Server();
            server.Launch();
            Task.Run(() => _gm.StartWithSyncCommunicator(server));
            Thread.Sleep(2000);
            _player1.StartSyncCommunicator(new string[] { "", "localhost", "3729", "Red" }, server);
            _player2.StartSyncCommunicator(new string[] { "", "localhost", "3729", "Blue" }, server);

            Task.Run(() => _player1.InitSyncAgent()); // do po³¹czenia z gr¹
            Task.Run(() => _player2.InitSyncAgent());
            Thread.Sleep(5000);
            // GameMaster.InitSyncGM();
            //when
            var direction = Direction.East;

            _player1.Move(Direction.South);
            Thread.Sleep(5000);
            _player1.PickupPiece();
            Thread.Sleep(5000);
            while(_player1.PieceInfo != Player.PieceState.NotSham)
            {
                while(_player1.PieceInfo != Player.PieceState.HasUnknown)
                {
                    _player1.Move(direction);
                    Thread.Sleep(5000);
                    _player1.PickupPiece();
                    Thread.Sleep(5000);
                    _player1.Move(direction);
                    Thread.Sleep(5000);
                    _player1.PickupPiece();
                    Thread.Sleep(5000);
                    if(direction == Direction.East)
                        direction = Direction.West;
                    else
                        direction = Direction.East;

                }
                _player1.CheckPiece();
                Thread.Sleep(5000);
            }
            //then
            Assert.IsTrue(_gm.teamsFull);
            Assert.AreEqual(_gm.redPlayers.First().Value.Position.ToVector2D().y, 1);
            Assert.IsFalse(_gm.redPlayers.First().Value.Holding.IsSham);
        }

        [TestMethod]
        public void TestConnectivityWithScoringGoal()
        {
            //given
            Server server = new Server();
            server.Launch();
            Task.Run(() => _gm.StartWithSyncCommunicator(server));
            Thread.Sleep(2000);
            _player1.StartSyncCommunicator(new string[] { "", "localhost", "3729", "Red" }, server);
            _player2.StartSyncCommunicator(new string[] { "", "localhost", "3729", "Blue" }, server);

            Task.Run(() => _player1.InitSyncAgent()); // do po³¹czenia z gr¹
            Task.Run(() => _player2.InitSyncAgent());
            Thread.Sleep(5000);
            // GameMaster.InitSyncGM();
            //when
            var direction = Direction.East;

            _player1.Move(Direction.South);
            Thread.Sleep(5000);
            _player1.PickupPiece();
            Thread.Sleep(5000);
            while(_player1.PieceInfo != Player.PieceState.NotSham)
            {
                while(_player1.PieceInfo != Player.PieceState.HasUnknown)
                {
                    _player1.Move(direction);
                    Thread.Sleep(5000);
                    _player1.PickupPiece();
                    Thread.Sleep(5000);
                    _player1.Move(direction);
                    Thread.Sleep(5000);
                    _player1.PickupPiece();
                    Thread.Sleep(5000);
                    if(direction == Direction.East)
                        direction = Direction.West;
                    else
                        direction = Direction.East;

                }
                _player1.CheckPiece();
                Thread.Sleep(5000);
            }
            _player1.Move(Direction.North);
            Thread.Sleep(5000);
            _player1.PlacePiece();
            //then
            Assert.IsTrue(_gm.teamsFull);
            Assert.AreEqual(_gm.redPlayers.First().Value.Position.ToVector2D().y, 2);
            Assert.IsNull(_gm.redPlayers.First().Value.Holding);
            Assert.AreEqual(_gm.RedTeamPoints, 1);
        }

        [TestMethod]
        public void TestConnectivityWithWinningGame()
        {
            //given
            Server server = new Server();
            server.Launch();
            Task.Run(() => _gm.StartWithSyncCommunicator(server));
            Thread.Sleep(2000);
            _player1.StartSyncCommunicator(new string[] { "", "localhost", "3729", "Red" }, server);
            _player2.StartSyncCommunicator(new string[] { "", "localhost", "3729", "Blue" }, server);

            Task.Run(() => _player1.InitSyncAgent()); // do po³¹czenia z gr¹
            Task.Run(() => _player2.InitSyncAgent());
            Thread.Sleep(5000);
            // GameMaster.InitSyncGM();
            //when
            var direction = Direction.East;

            _player1.Move(Direction.South);
            Thread.Sleep(5000);
            _player1.PickupPiece();
            Thread.Sleep(5000);
            while(_player1.PieceInfo != Player.PieceState.NotSham)
            {
                while(_player1.PieceInfo != Player.PieceState.HasUnknown)
                {
                    _player1.Move(direction);
                    Thread.Sleep(5000);
                    _player1.PickupPiece();
                    Thread.Sleep(5000);
                    _player1.Move(direction);
                    Thread.Sleep(5000);
                    _player1.PickupPiece();
                    Thread.Sleep(5000);
                    if(direction == Direction.East)
                        direction = Direction.West;
                    else
                        direction = Direction.East;

                }
                _player1.CheckPiece();
                Thread.Sleep(5000);
            }
            _player1.Move(Direction.North);
            Thread.Sleep(5000);
            _player1.PlacePiece(); //1. goal
            Thread.Sleep(5000);

            Direction nextScore = direction;

            _player1.Move(Direction.South);
            Thread.Sleep(5000);
            _player1.PickupPiece();
            Thread.Sleep(5000);
            while(_player1.PieceInfo != Player.PieceState.NotSham)
            {
                while(_player1.PieceInfo != Player.PieceState.HasUnknown)
                {
                    _player1.Move(direction);
                    Thread.Sleep(5000);
                    _player1.PickupPiece();
                    Thread.Sleep(5000);
                    _player1.Move(direction);
                    Thread.Sleep(5000);
                    _player1.PickupPiece();
                    Thread.Sleep(5000);
                    if(direction == Direction.East)
                        direction = Direction.West;
                    else
                        direction = Direction.East;

                }
                _player1.CheckPiece();
                Thread.Sleep(5000);
            }
            if(direction == nextScore)
            {
                _player1.Move(nextScore);
                Thread.Sleep(5000);
                _player1.Move(nextScore);
                Thread.Sleep(5000);
                if(direction == Direction.East)
                {
                    direction = Direction.West;
                    nextScore = Direction.West;
                }
            }
            _player1.Move(Direction.North);
            Thread.Sleep(5000);
            _player1.PlacePiece(); //2. goal
            Thread.Sleep(5000);

            _player1.Move(Direction.South);
            Thread.Sleep(5000);
            _player1.PickupPiece();
            Thread.Sleep(5000);
            while(_player1.PieceInfo != Player.PieceState.NotSham)
            {
                while(_player1.PieceInfo != Player.PieceState.HasUnknown)
                {
                    _player1.Move(direction);
                    Thread.Sleep(5000);
                    _player1.PickupPiece();
                    Thread.Sleep(5000);
                    _player1.Move(direction);
                    Thread.Sleep(5000);
                    _player1.PickupPiece();
                    Thread.Sleep(5000);
                    if(direction == Direction.East)
                        direction = Direction.West;
                    else
                        direction = Direction.East;

                }
                _player1.CheckPiece();
                Thread.Sleep(5000);
            }
            _player1.Move(direction);
            Thread.Sleep(5000);
            _player1.Move(Direction.North);
            Thread.Sleep(5000);
            _player1.PlacePiece(); //3. goal
            Thread.Sleep(5000);


            //then

            Assert.IsTrue(_gm.teamsFull);
            Assert.AreEqual(_gm.redPlayers.First().Value.Position.ToVector2D().y, 2);
            Assert.IsNull(_gm.redPlayers.First().Value.Holding);
            Assert.AreEqual(_gm.RedTeamPoints, 3);
            Assert.IsTrue(_gm.gameMasterState == GameMaster.GameMasterState.PostGame);
        }
    }
}