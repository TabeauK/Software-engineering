using CommunicationUtils;
using CommunicationUtils.Structures;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using CommunicationUtils.Payloads;
using System.Threading;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Immutable;
using GameMaster.Fields;
using GameMaster.Pieces;
using CommunicationServer;

[assembly: InternalsVisibleTo("GameMaster.Test")]
[assembly: InternalsVisibleTo("Player.Test")]
[assembly: InternalsVisibleTo("Integration.Test")]
namespace GameMaster
{
    public partial class GameMaster
    {
        internal ILogger _logger;
        internal ImmutableDictionary<int, Player> bluePlayers = ImmutableDictionary<int, Player>.Empty;
        internal ImmutableDictionary<int, Player> redPlayers = ImmutableDictionary<int, Player>.Empty;
        internal int blueLeaderId;
        internal int redLeaderId;
        internal IGameMasterField[,] map;
        internal Vector2D legalKnowledgeReplies; // ? unique?
        internal Configuration _config;
        internal ConfigurationLoader _configLoader;
        internal ICommunicator _communicator;
        internal readonly object threadSync = new object();
        internal MessageHandlerState messageHandlerState = MessageHandlerState.Idle;
        internal bool teamsFull = false;
        internal ManualResetEventSlim _waitAgentsMre = new ManualResetEventSlim(false);
        internal ManualResetEventSlim _CSMre = new ManualResetEventSlim(false);
        internal ManualResetEventSlim _endGameMre = new ManualResetEventSlim(false);
        internal ImmutableList<AbstractPiece> pieces = ImmutableList<AbstractPiece>.Empty;
        internal GameMasterState gameMasterState = GameMasterState.NotConnected;

        public int RedTeamPoints { get; private set; }
        public int BlueTeamPoints { get; private set; }

        // Wywoływany tylko raz, po wygenerowaniu planszy.
        public event EventHandler GameBoardGenerated;

        // Wywoływany tylko raz, po rozpoczęciu gry. Startuje zegar w gui.
        public event EventHandler GameStarted;
        
        // Wywoływany tylko raz, po zakończeniu gry.
        public event EventHandler<GameEndArgs> GameEnded;


        public GameMaster(ILogger<GameMaster> logger, ConfigurationLoader configLoader)
        {
            _logger = logger;
            _configLoader = configLoader;
        }

        //do testowania
        private void printBoard()
        {
            for (int i = _config.Y - 1; i >= 0; i--)
            {
                for (int j = 0; j < _config.X; j++)
                {
                    if (map[i, j] is GoalField g)
                    {
                        if (g.Team == TeamColor.Blue)
                            Console.Write("B");
                        else
                            Console.Write("R");
                    }
                    else
                    {
                        Console.Write("T");
                    }
                }
                Console.WriteLine();
            }
        }

        public void Start()
        {
            _logger.LogInformation("*** GameMaster started ***");
            if(!_configLoader.TryLoadFromFile("config.json", out _config))
            {
                _configLoader.LoadDefaultConfig(out _config);
            }
            _logger.LogInformation("Succesfully loaded configuration");
            if(!connectToCS())
            {
                _logger.LogCritical("Couldn't connect to the Communication Server. Aborting...");
                Stop();
                return;
            }
            generateBoard();
            GameBoardGenerated?.Invoke(this, EventArgs.Empty);

            generatePieces();
            waitForPlayers();
            startGame();
            GameStarted?.Invoke(this, EventArgs.Empty);
            //Czekanie az gra sie skonczy
            while (gameMasterState == GameMasterState.Playing)
            {
                _endGameMre.Reset();
                _endGameMre.Wait();
                if (BlueTeamPoints == _config.NumberOfGoals)
                    endGame(TeamColor.Blue);
                else if (RedTeamPoints == _config.NumberOfGoals)
                    endGame(TeamColor.Red);
            }
            Stop();
        }

        public void StartWithSyncCommunicator(Server server)
        {
            _logger.LogInformation("*** GameMaster started ***");
            if(!_configLoader.TryLoadFromFile("config.json", out _config))
            {
                _configLoader.LoadDefaultTestConfig(out _config);
            }
            _logger.LogInformation("Succesfully loaded configuration");
            if(!StartSyncCommunicator(server))
            {
                _logger.LogCritical("Couldn't connect to the Communication Server. Aborting...");
                Stop();
                return;
            }
            generateBoard();
            GameBoardGenerated?.Invoke(this, EventArgs.Empty);

            generatePieces();
            waitForPlayers();
            Thread.Sleep(1000);
            startGame();
            GameStarted?.Invoke(this, EventArgs.Empty);
            //Czekanie az gra sie skonczy
            while(gameMasterState == GameMasterState.Playing)
            {
                _endGameMre.Reset();
                _endGameMre.Wait();
                if(BlueTeamPoints == _config.NumberOfGoals)
                    endGame(TeamColor.Blue);
                else if(RedTeamPoints == _config.NumberOfGoals)
                    endGame(TeamColor.Red);
            }
            Stop();
        }

        public void Stop()
        {
            _logger.LogInformation("Stopping the Game Master...");
            _communicator.Dispose();
            _waitAgentsMre.Dispose();
            _endGameMre.Dispose();
            _CSMre.Dispose();
            _logger.LogInformation("Thank you for choosing our service. Goodnight");
        }

        internal void endGame(TeamColor winner)
        {
            if (gameMasterState == GameMasterState.PostGame)
                return;
            gameMasterState = GameMasterState.PostGame;
            _logger.LogInformation($"Game ended. {winner.ToString()} team wins!\n " +
                $"Final score: Blue {BlueTeamPoints.ToString()} - Red {RedTeamPoints.ToString()}");
            _logger.LogInformation("Sending EndGame to all players");
            IPayload payload = new EndGamePayload()
            {
                winner = winner
            };
            Message m = new Message()
            {
                payload = payload
            };
            foreach (var player in bluePlayers.Concat(redPlayers))
            {
                m.agentID = player.Key;
                _communicator.SendMessage(m);
            }
            _logger.LogInformation("EndGame sent to all players");
            _endGameMre.Set();
            GameEnded?.Invoke(this, new GameEndArgs(winner));
        }

        public bool StartSyncCommunicator(Server server)
        {
            var com = new SynchronousCommunicator();
            _communicator = com;
            com.StateChanged += communicatorStateChangedHandler;
            com.MessageReceived += messageReceivedHandler;
            _CSMre.Reset();
            server.AddGameMaster(com);
            _CSMre.Wait();
            if (_communicator.GetState() == CommunicatorState.Connected)
            {
                return true;
            }
            return false;
        }


        internal bool connectToCS()
        {
            _logger.LogInformation("Trying to connect to the Communication Server...");
            var com = new Communicator();
            _communicator = com;
            com.StateChanged += communicatorStateChangedHandler;
            com.MessageReceived += messageReceivedHandler;
            com.Connect(_config.CSIP, _config.CSPort);
            //Czekanie na az zmieni sie stan komunikatora
            _CSMre.Reset();
            _CSMre.Wait();
            if(_communicator.GetState() == CommunicatorState.Connected)
            {
                gameMasterState = GameMasterState.WaitingForPlayers;
                return true;
            }
            return false;
        }

        internal void communicatorStateChangedHandler(object sender, StateChangedEventArgs e)
        {
            switch(e.NewState)
            {
                case CommunicatorState.Connecting:
                    _logger.LogInformation("Connecting to the Communication Server...");
                    break;
                case CommunicatorState.Connected:
                    _logger.LogInformation("Succesfully connected to the Communication Server");
                    _CSMre.Set();
                    break;
                case CommunicatorState.Disconnected:
                    _logger.LogInformation("Communication Server disconnected");
                    _CSMre.Set();
                    break;
            }
        }

        internal void waitForPlayers()
        {
            _logger.LogInformation("Waiting for Agents...");
            while(bluePlayers.Count < _config.NumberOfPlayers || redPlayers.Count < _config.NumberOfPlayers)
            {
                _waitAgentsMre.Reset();
                _waitAgentsMre.Wait();
            }
            gameMasterState = GameMasterState.Playing;
            _logger.LogInformation("Both teams full.");
            teamsFull = true;
        }

        internal void startGame()
        {
            _logger.LogInformation("Starting the game...");
            chooseLeaders();
            putPlayersOnBoard();
            sendStartGameInfoToAll();
            _logger.LogInformation("Game started");

        }

        internal void chooseLeaders()
        {
            _logger.LogInformation("Choosing the leaders");
            Random r = new Random();
            int blueLeaderIndex = r.Next(_config.NumberOfPlayers);
            int redLeaderIndex = r.Next(_config.NumberOfPlayers);
            blueLeaderId = bluePlayers.ElementAt(blueLeaderIndex).Key;
            redLeaderId = redPlayers.ElementAt(redLeaderIndex).Key;
            bluePlayers[blueLeaderId].IsLeader = true;
            redPlayers[redLeaderId].IsLeader = true;
            _logger.LogInformation($"Leader ids: Blue #{blueLeaderId}, Red #{redLeaderId}");
        }

        internal void putPlayersOnBoard()
        {
            _logger.LogInformation($"Putting players on the board...");
            //Blue players
            int index = 0;
            for (int i = 0; i < _config.Y; i++)
            {
                for (int j = 0; j < _config.X; j++)
                {
                    Player p = bluePlayers.ElementAt(index++).Value;
                    map[i, j].MoveHere(p);
                    if (index == _config.NumberOfPlayers)
                        break;
                }
                if (index == _config.NumberOfPlayers)
                    break;
            }

            //Red players
            index = 0;
            for (int i = 0; i < _config.Y; i++)
            {
                for (int j = 0; j < _config.X; j++)
                {
                    Player p = redPlayers.ElementAt(index++).Value;
                    map[_config.Y - 1 - i, j].MoveHere(p);
                    if (index == _config.NumberOfPlayers)
                        break;
                }
                if (index == _config.NumberOfPlayers)
                    break;
            }

            _logger.LogInformation("All players are on the board");
        }

        internal void sendStartGameInfoToAll()
        {
            _logger.LogInformation("Sending StartGame messages to players...");
            Penalties penalties = getPenalties();
            foreach(var el in bluePlayers.Concat(redPlayers))
            {
                sendStartGameInfoToPlayer(el.Key, el.Value.Team, penalties);
            }
            _logger.LogInformation("StartGame messages sent to all players");
        }

        internal void sendStartGameInfoToPlayer(int playerId, TeamColor team, Penalties penalties)
        {
            Message m = new Message()
            {
                agentID = playerId,
                payload = prepareStartGamePayload(playerId, team, penalties)
            };
            _communicator.SendMessage(m);
        }

        internal StartGamePayload prepareStartGamePayload(int playerId, TeamColor team, Penalties penalties)
        {
            int[] allies = null, enemies = null;
            int leader = 0;
            Vector2D position = null;
            if (team == TeamColor.Blue)
            {
                allies = bluePlayers.Keys.Where(x => x != playerId).ToArray();
                enemies = redPlayers.Keys.ToArray();
                position = bluePlayers[playerId].Position.ToVector2D();
                leader = blueLeaderId;
            }
            else
            {
                allies = redPlayers.Keys.Where(x => x != playerId).ToArray();
                enemies = bluePlayers.Keys.ToArray();
                position = redPlayers[playerId].Position.ToVector2D();
                leader = redLeaderId;
            }

            StartGamePayload payload = new StartGamePayload()
            {
                agentID = playerId,
                alliesIDs = allies,
                leaderID = blueLeaderId,
                enemiesIDs = enemies,
                teamId = team,
                boardSize = new Vector2D() { x = _config.X, y = _config.Y },
                goalAreaSize = _config.GoalAreaHeight,
                numberOfPlayers = new NumberOfPlayers() { allies = _config.NumberOfPlayers - 1, enemies = _config.NumberOfPlayers},
                numberOfPieces = _config.NumberOfPieces,
                numberOfGoals = _config.NumberOfGoals,
                penalties = penalties,
                shamPieceProbability = _config.ShamPieceProbability,
                position = position
            };
            return payload;
        }

        internal Penalties getPenalties()
        {
            return new Penalties()
            {
                move = _config.MovePenalty,
                checkForSham = _config.CheckPenalty,
                discovery = _config.DiscoveryPenalty,
                destroyPiece = _config.DestroyPenalty,
                putPiece = _config.PutPenalty,
                response = _config.InformationExchangePenalty,
                pickup = _config.PickUpPenalty,
                ask = _config.RequestInformationExchangePenalty
            };
        }

        internal void generatePieces()
        {
            _logger.LogInformation("Generating pieces...");
            while(pieces.Count < _config.NumberOfPieces)
            {
                generatePiece();
            }
            _logger.LogInformation("Pieces generated");
        }

        internal void generatePiece()
        {
            Random r = new Random(); // seed będzie potrzebny do testowania
            int x = r.Next(_config.X);
            int y = r.Next(_config.GoalAreaHeight, _config.Y - _config.GoalAreaHeight);
            if(r.NextDouble() < _config.ShamPieceProbability)
            {
                AbstractPiece piece = new ShamPiece();
                map[y, x].PutPickablePiece(piece);
                pieces = pieces.Add(piece);
            }
            else
            {
                AbstractPiece piece = new NormalPiece();
                map[y, x].PutPickablePiece(piece);
                pieces = pieces.Add(piece);

            }
        }

        internal void generateBoard()
        {
            _logger.LogInformation("Generating the board...");
            map = new AbstractField[_config.Y, _config.X];
            for(int i = 0; i < _config.GoalAreaHeight; i++)
            {
                for(int j = 0; j < _config.X; j++)
                {
                    map[i, j] = new NonGoalField(j, i);
                    map[_config.Y - i - 1, j] = new NonGoalField(j, _config.Y - i - 1);
                }
            }
            for(int i = _config.GoalAreaHeight; i < _config.Y - _config.GoalAreaHeight; i++)
            {
                for(int j = 0; j < _config.X; j++)
                {
                    map[i, j] = new TaskField(j, i);
                }
            }
            Random r = new Random(); //seed pewnie będzie potrzebny do testowania
            for(int i = 0; i < _config.NumberOfGoals;)
            {
                int y = r.Next(_config.GoalAreaHeight);
                int x = r.Next(_config.X);
		// jeśli trafiliśmy na stworzony wcześniej goal, losuj współrzędne ponownie
		if(map[y,x] is GoalField)
			continue;
                map[y, x] = new GoalField(x, y) { Team = TeamColor.Blue };
                map[_config.Y - 1 - y, x] = new GoalField(x, _config.Y - y - 1) { Team = TeamColor.Red };
		i++;
            }
            _logger.LogInformation("The board generated");
        }

        /// <summary>
        /// Wypelnia obiekt payload odleglosciami do najblizszego kawalka z sasiadnich pol. Null jesli pole wykracza poza plansze lub nie ma kawalka.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <param name="payload"></param>
        internal void discover(int y, int x, out DiscoverResponsePayload payload)
        {
            payload = new DiscoverResponsePayload()
            {
                distanceFromCurrent = map[y, x].DistanceToClosestPiece(pieces)
            };
            //S
            if(y > 0)
            {
                payload.distanceS = map[y - 1, x].DistanceToClosestPiece(pieces);
            }

            //N
            if(y < _config.Y - 1)
            {
                payload.distanceN = map[y + 1, x].DistanceToClosestPiece(pieces);
            }

            //W
            if(x > 0)
            {
                payload.distanceW = map[y, x - 1].DistanceToClosestPiece(pieces);
            }

            //E
            if(x < _config.X - 1)
            {
                payload.distanceE = map[y, x + 1].DistanceToClosestPiece(pieces);
            }

            //SW
            if(y > 0 && x > 0)
            {
                payload.distanceSW = map[y - 1, x - 1].DistanceToClosestPiece(pieces);
            }

            //SE
            if(y > 0 && x < _config.X - 1)
            {
                payload.distanceSE = map[y - 1, x + 1].DistanceToClosestPiece(pieces);
            }

            //NE
            if(y < _config.Y - 1 && x < _config.X - 1)
            {
                payload.distanceNE = map[y + 1, x + 1].DistanceToClosestPiece(pieces);
            }

            //NW
            if(y < _config.Y - 1 && x > 0)
            {
                payload.distanceNW = map[y + 1, x - 1].DistanceToClosestPiece(pieces);
            }

        }

        internal Player findPlayer(int id)
        {
            if(bluePlayers.ContainsKey(id))
                return bluePlayers[id];
            else if(redPlayers.ContainsKey(id))
                return redPlayers[id];
            return null;
        }

        public BoardView GetBoardView()
        {
            return new BoardView(map, bluePlayers, redPlayers, pieces, _config.GoalAreaHeight, 
                RedTeamPoints, BlueTeamPoints, _config);
        }
    }
}
