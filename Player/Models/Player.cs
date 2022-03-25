using Microsoft.Extensions.Logging;
using Player.Models;
using Player.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using CommunicationUtils;
using CommunicationUtils.Payloads;
using System.Threading;
using System.Threading.Tasks;
using CommunicationUtils.Structures;
using System.Runtime.CompilerServices;
using CommunicationServer;

[assembly: InternalsVisibleTo("Player.Test")]
namespace Player
{

    public enum PlayerState
    {
        AwaitingConfig,
        AwaitingCSConnection,
        AwaitingGMAcceptanceForConnect,
        AwaitingGameStart,
        AwaitingGMAcceptanceForMove,
        Ready,
        Timeout,
        Paused,
        Finished,
        ReadyToFinish
    }

    public enum Action
    {
        Move,
        PlacePiece,
        TestPiece,
        BegForInfo,
        SendInfo,
        Discover,
        DestroyPiece,
        PickupPiece
    }

    public enum PieceState
    {
        NoPiece,
        HasUnknown,
        NotSham,
        Sham
    }
    public partial class Player
    {
        internal int _id;
        internal IStrategy _strategy;
        internal ILogger _logger;
        internal ConfigurationLoader _configLoader;
        internal PlayerConfiguration _config;
        internal ICommunicator _communicator;
        internal PlayerState _state = PlayerState.AwaitingConfig;
        // obiekt wykorzystywany do synchronizacji. Po wysłaniu wiadomości z żądaniem ruchu,
        // wątek gracza przechodzi w stan oczekiwania. Dopiero w momencie gdy wywoła się handler
        // odpowiedniej wiadomości i wysłany zostanie sygnał (tym obiektem), to wątek gracza się
        // zbudzi i przejdzie do wysyłania kolejnych żądań.
        internal ManualResetEventSlim _mre = new ManualResetEventSlim(false);
        internal Penalties _penalties;
        internal Action _lastAction;
        internal bool _shouldRespondToLeader = false;
        internal int _maxTimeToWaitForCS = 300000;
        internal int _maxTimeToWaitForGM = 300000;
        internal bool _couldMove = true;
        internal Direction _lastMoveDir;
        internal bool _couldPlace = true;
        internal PlacePieceErrorType _placeError;
        internal int _goalAreaSize;
        internal bool _couldAct = true;
        internal Message _lastMsg;
        internal bool _couldPickup;
        internal PickupPieceErrorType _pickupError;
        internal bool _verbose = false;

        public int PenaltyTime { get; set; }
        public TeamColor Team { get; set; }
        public bool IsLeader { get; set; }
        public PieceState PieceInfo { get; set; }
        public Field[,] Board { get; set; }
        public Vector2D Position { get; set; } = new Vector2D();
        public List<WaitingPlayer> WaitingPlayers { get; set; } = new List<WaitingPlayer>();
        public int? LeaderID { get; set; } = null;
        public int[] Teammates { get; set; }

        public Player(ILogger<Player> logger,
            IStrategy strategy,
            ConfigurationLoader configLoader) 
        {
            _strategy = strategy;
            _logger = logger;
            _configLoader = configLoader;
            //_communicator = communicator;
        }
        public void Start(string[] args)
        {
            _logger.LogInformation("Started player. Loading configuration...");
            if(!_configLoader.TryLoadFromArgs(args, out _config))
            {
                if (!_configLoader.TryLoadFromFile("config.json", out _config))
                {
                    _config = _configLoader.LoadDefaultConfig();
                }
            }
            Team = _config.TeamID;
            _verbose = _config.Verbose;
            _logger.LogInformation("Succesfully loaded configuration");
            _state = PlayerState.AwaitingCSConnection;

            Communicator tcpCommunicator = new Communicator();
            tcpCommunicator.MessageReceived += MessageReceivedHandler;
            tcpCommunicator.StateChanged += CommunicatorStateChangedHandler;
            _logger.LogInformation("Trying to connect with Communication Server...");
            tcpCommunicator.Connect(_config.CSIP, _config.CSPort);
            _communicator = tcpCommunicator;

            try
            {
                AwaitConnect();
                _mre.Reset();
            }
            catch(Exception e)
            {
                _logger.LogWarning(e.Message);
                Stop();
                return;
            }
            if (_state == PlayerState.Finished) return;

            _logger.LogInformation("Successfully connected with CS. Trying to connect with GM.");
            _state = PlayerState.AwaitingGMAcceptanceForConnect;

            try
            {
                Join();
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex.Message);
                Stop();
                return;
            }
            if (_state == PlayerState.Finished) return;
            StartGame();
            if (_state != PlayerState.Finished) _strategy.Init(this);
            while (_state != PlayerState.Finished)
            {
                _mre.Reset();
                MakeDecisionFromStrategy();
            }
        }
        public SynchronousCommunicator StartSyncCommunicator(string[] args, Server server)
        {
            _logger.LogInformation("Started player. Loading configuration...");
            if (!_configLoader.TryLoadFromArgs(args, out _config))
            {
                if (!_configLoader.TryLoadFromFile("config.json", out _config))
                {
                    _config = _configLoader.LoadDefaultConfig();
                }
            }
            Team = _config.TeamID;
            _logger.LogInformation("Succesfully loaded configuration");
            _state = PlayerState.AwaitingCSConnection;

            SynchronousCommunicator syncCommunicator = new SynchronousCommunicator();
            syncCommunicator.MessageReceived += MessageReceivedHandler;
            syncCommunicator.StateChanged += CommunicatorStateChangedHandler;
            _logger.LogInformation("Trying to connect with Communication Server...");
            _communicator = syncCommunicator;
            server.AddAgent(syncCommunicator);
            return syncCommunicator;
        }
        public bool InitSyncAgent()
        {
            try
            {
                AwaitConnect();
                _mre.Reset();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e.Message);
                Stop();
                return false;
            }
            if (_state == PlayerState.Finished) return false;

            _logger.LogInformation("Successfully connected with CS. Trying to connect with GM.");
            _state = PlayerState.AwaitingGMAcceptanceForConnect;

            try
            {
                Join();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                Stop();
                return false;
            }
            if (_state == PlayerState.Finished) return false;
            StartGame();
            return true;
        }

        public void Stop() 
        {
            _logger.LogInformation("Stopping Agent...");
            _state = PlayerState.Finished;
            _communicator.Dispose();
        }
        public void Discovery()
        {
            _logger.LogInformation($"Proceeding to make a discover action");
            Message discMsg = new Message() { payload = new DiscoverRequestPayload() };
            LogVerbose(discMsg);
            _lastMsg = discMsg;
            _lastAction = Action.Discover;
            _state = PlayerState.AwaitingGMAcceptanceForMove;
            _communicator.SendMessage(discMsg);
        }
        public void Move(Direction dir) 
        {
            _logger.LogInformation($"Proceeding to make a move in a {dir} direction");
            _lastMoveDir = dir;
            Message moveMsg = new Message() { payload = new MovementRequestPayload() { direction = dir } };
            LogVerbose(moveMsg);
            _lastMsg = moveMsg;
            _lastAction = Action.Move;
            _state = PlayerState.AwaitingGMAcceptanceForMove;
            _communicator.SendMessage(moveMsg);
        }
        public void BegForInfo(int id) 
        {
            _logger.LogInformation($"Proceeding to beg for info from player with id: {id}");
            Message begMsg = new Message() { payload = new InformationExchangeRequestPayload() { askedAgentID = id } };
            LogVerbose(begMsg);
            _lastMsg = begMsg;
            _lastAction = Action.BegForInfo;
            _state = PlayerState.AwaitingGMAcceptanceForMove;
            _communicator.SendMessage(begMsg);
        }
        public void GiveInfo(int id)
        {
            _logger.LogInformation($"Proceeding to send info to player with id: {id}");
            InformationExchangeResponsePayload responsePayload = PrepareResponse(id);
            Message infoMsg = new Message() { payload = responsePayload };
            LogVerbose(infoMsg);
            _lastMsg = infoMsg;
            _lastAction = Action.SendInfo;
            _state = PlayerState.AwaitingGMAcceptanceForMove;
            _communicator.SendMessage(infoMsg);
        }

        public void PlacePiece()
        {
            _logger.LogInformation($"Proceeding to place piece");
            if (PieceInfo == PieceState.NoPiece)
            {
                _logger.LogWarning("Agent tried to place piece while he has no piece");
                return;
            }
            Message placeMsg = new Message() { payload = new PlacePieceRequestPayload() };
            LogVerbose(placeMsg);
            _lastMsg = placeMsg;
            _lastAction = Action.PlacePiece;
            _state = PlayerState.AwaitingGMAcceptanceForMove;
            _communicator.SendMessage(placeMsg);
        }
        public void CheckPiece() 
        {
            _logger.LogInformation($"Proceeding to check piece");
            if (PieceInfo == PieceState.NoPiece)
            {
                _logger.LogWarning("Agent tried to check piece while he has no piece");
                return;
            }
            Message checkMsg = new Message() { payload = new TestPieceRequestPayload() };
            LogVerbose(checkMsg);
            _lastMsg = checkMsg;
            _lastAction = Action.TestPiece;
            _state = PlayerState.AwaitingGMAcceptanceForMove;
            _communicator.SendMessage(checkMsg);
        }
        public void PickupPiece()
        {
            _logger.LogInformation($"Proceeding to pick up piece");
            if (Board[Position.y, Position.x].DistToPiece != 0)
            {
                _logger.LogWarning("Agent tried to pick up piece while not on piece");
                return;
            }
            Message pickupMsg = new Message() { payload = new PickupPieceRequestPayload() };
            LogVerbose(pickupMsg);
            _lastMsg = pickupMsg;
            _lastAction = Action.PickupPiece;
            _state = PlayerState.AwaitingGMAcceptanceForMove;
            _communicator.SendMessage(pickupMsg);
        }
        public void DestroyPiece()
        {
            _logger.LogInformation($"Proceeding to destroy piece");
            if (PieceInfo == PieceState.NoPiece)
            {
                _logger.LogWarning("Agent tried to destroy piece while he has no piece");
                return;
            }
            Message destroyMsg = new Message() { payload = new DestroyPieceRequestPayload() };
            LogVerbose(destroyMsg);
            _lastMsg = destroyMsg;
            _lastAction = Action.DestroyPiece;
            _state = PlayerState.AwaitingGMAcceptanceForMove;
            _communicator.SendMessage(destroyMsg);
        }
        public void MakeDecisionFromStrategy() 
        {
            if (_couldAct)
            {
                _strategy.MakeDecision();
            }
            else
            {
                _logger.LogInformation("Resending last message...");
                _communicator.SendMessage(_lastMsg);
                _couldAct = true;
            }

            _mre.Wait();

            if(_state == PlayerState.Ready)
            {
                _logger.LogInformation("Action completed. Proceeding to next...");
            }
            else if(_state == PlayerState.ReadyToFinish)
            {
                Stop();
            }
            else
            {
                _logger.LogWarning("Bad action???");
            }
        }

        internal void Join()
        {
            Message joinRequestMsg = new Message();
            joinRequestMsg.payload = new JoinGameRequestPayload() { teamId = _config.TeamID };
            LogVerbose(joinRequestMsg);
            _communicator.SendMessage(joinRequestMsg);
            _logger.LogInformation("Request to join game sent. Waiting for confirmation...");

            _mre.Wait(_maxTimeToWaitForGM);
            if (!_mre.IsSet) throw new Exception("Exceeded maximum waiting time GM connection!");
            _mre.Reset();
            if(_state == PlayerState.AwaitingGameStart)
            {
                _logger.LogInformation("Player accepted. Waiting for game to start...");
            }
            else
            {
                _logger.LogWarning("Player not accepted. Ending this Agent instance...");
                Stop();
            }
        }

        internal void StartGame()
        {
            _mre.Wait();
            _mre.Reset();
            if(_state == PlayerState.Ready)
            {
                _logger.LogInformation("Game sucessfully started. Proceeding to make some moves...");
            }
            else
            {
                _logger.LogWarning("Unable to start the game. Ending this Agent instance...");
                Stop();
            }
        }

        internal void AwaitConnect()
        {
            // 1. Czekamy na wybudzenie przez event o zmianie stanu Communicatora
            _mre.Wait(_maxTimeToWaitForCS);
            if (!_mre.IsSet) throw new Exception("Exceeded maximum waiting time CS connection!");
        }

        internal void CommunicatorStateChangedHandler(object obj, StateChangedEventArgs e)
        {
            if(_state == PlayerState.AwaitingCSConnection && e.NewState == CommunicatorState.Connected)
            {
                _mre.Set();
                return;
            }
            if((_state != PlayerState.ReadyToFinish && _state != PlayerState.Finished) && (e.NewState == CommunicatorState.Disconnected))
            {
                _logger.LogWarning("Communication Server disconnected.");
                _state = PlayerState.ReadyToFinish;
                _mre.Set();
            }
        }
        internal void MessageReceivedHandler(object obj, MessageReceivedEventArgs e)
        {
            Message m;
            while ((m = _communicator.GetNextMessage()) != null)
            {
                if (_verbose) _logger.LogInformation($"Received message of type: {m.messageID}, with value: {m.payload}");
                switch (m.payload)
                {
                    case TestPieceResponsePayload p:
                        Task.Run(() => ProcessTestPieceResponse(p));
                        break;
                    case DestroyPieceResponsePayload p:
                        Task.Run(() => ProcessDestroyPieceResponse(p));
                        break;
                    case DiscoverResponsePayload p:
                        Task.Run(() => ProcessDiscoveryResponse(p));
                        break;
                    case EndGamePayload p:
                        Task.Run(() => ProcessEndGameResponse(p));
                        break;
                    case StartGamePayload p:
                        Task.Run(() => ProcessStartGameResponse(p));
                        break;
                    case InformationExchangePassRequestPayload p:
                        Task.Run(() => ProcessInfoRequest(p));
                        break;
                    case JoinGameResponsePayload p:
                        Task.Run(() => ProcessJoinGameResponse(p));
                        break;
                    case MovementResponsePayload p:
                        Task.Run(() => ProcessMoveResponse(p));
                        break;
                    case PickupPieceResponsePayload p:
                        Task.Run(() => ProcessPickupResponse(p));
                        break;
                    case PlacePieceResponsePayload p:
                        Task.Run(() => ProcessPlacePieceResponse(p));
                        break;
                    case MovementErrorPayload p:
                        Task.Run(() => ProcessMoveErrorResponse(p));
                        break;
                    case PlacePieceErrorPayload p:
                        Task.Run(() => ProcessPlaceErrorResponse(p));
                        break;
                    case PenaltyNotServedErrorPayload p:
                        Task.Run(() => ProcessPenaltyNotServedResponse(p));
                        break;
                    case PickupPieceErrorPayload p:
                        Task.Run(() => ProcessPickupErrorResponse(p));
                        break;
                    case InformationExchangeResponsePayload p:
                        Task.Run(() => ProcessInformationResponse(p));
                        break;
                    case InformationExchangeGMResponseToRequestPayload p:
                        Task.Run(() => ProcessGMResponseToIERequest(p));
                        break;
                    case InformationExchangeGMResponseToResponsePayload p:
                        Task.Run(() => ProcessGMResponseToIEResponse(p));
                        break;
                    case UndefinedErrorPayload p:
                        break;
                    default:
                        _logger.LogWarning($"Incorrect message received! Skipping...: {m.payload}, {m.messageID}");
                        break;

                }
            }
        }



        internal void ProcessJoinGameResponse(JoinGameResponsePayload msg)
        {
            _logger.LogInformation("Received JoinGame response. Proceeding to process it...");
            if (!msg.accepted)
            {
                _state = PlayerState.ReadyToFinish;
            }
            else
            {
                _state = PlayerState.AwaitingGameStart;
                _id = msg.agentID;
            }
            _mre.Set();
        }
        internal void ProcessStartGameResponse(StartGamePayload payload)
        {
            _logger.LogInformation("Received StartGame response. Proceeding to process it...");
            if (_state == PlayerState.AwaitingGameStart)
            {
                Teammates = payload.alliesIDs;
                IsLeader = _id == payload.leaderID;
                LeaderID = payload.leaderID;
                Board = new Field[payload.boardSize.y, payload.boardSize.x];
                _goalAreaSize = payload.goalAreaSize;
                for (int i = 0; i < payload.boardSize.y; i++)
                {
                    for (int j = 0; j < payload.boardSize.x; j++)
                    {
                        Board[i, j] = new Field() { DistToPiece = null, GoalInfo = GoalAreaTileInformation.Unknown, PlayerInfo = false };
                    }
                }

                for (int i = payload.goalAreaSize; i < (payload.boardSize.y - payload.goalAreaSize); i++)
                {
                    for (int j = 0; j < payload.boardSize.x; j++)
                    {
                        Board[i, j].GoalInfo = GoalAreaTileInformation.NotGoal;
                    }
                }

                Position = payload.position;
                _penalties = payload.penalties;
                _state = PlayerState.Ready;
                _mre.Set();
            }
            else
            {
                _state = PlayerState.ReadyToFinish;
                _logger.LogWarning("Received unexpected StartGame response. Stopping Agent...");
            }

        }
        internal void ProcessEndGameResponse(EndGamePayload payload)
        {
            _state = PlayerState.ReadyToFinish;
            _logger.LogInformation($"Game ended. {payload.winner} won!");
            _mre.Set();
        }
        internal void ProcessMoveResponse(MovementResponsePayload payload)
        {
            // 1. Sprawdzamy, czy takiej odpowiedzi się spodziewaliśmy: czy nasza
            // ostatnia akcja to ruch?
            // Sprawdzamy także, czy byliśmy w oczekiwaniu na odpowiedź GameMastera
            if ((_lastAction != Action.Move) || (_state != PlayerState.AwaitingGMAcceptanceForMove))
            {
                return;
            }

            // 2. Zaczynamy timeout
            _state = PlayerState.Timeout;

            // 3. Realizujemy logikę ruchu
            if (payload.madeMove)
            {
                _logger.LogInformation($"Made move to ({payload.currentPosition.y}, {payload.currentPosition.x})!");
                _couldMove = true;
                Board[Position.y, Position.x].PlayerInfo = false;
                Position = payload.currentPosition;
                Board[Position.y, Position.x].DistToPiece = payload.closestPiece;
                Board[Position.y, Position.x].PlayerInfo = true;
            }
            else
            {
                _logger.LogInformation($"Unable to make a move!");
                _couldMove = false;
                Board[payload.currentPosition.x, payload.currentPosition.y].PlayerInfo = true;
            }

            // 4. Odczekujemy timeout
            ServePenalty(_penalties.move);

            // 5. Odblokowujemy agenta
            _state = PlayerState.Ready;

            // 6. Aktywujemy główny wątek
            _mre.Set();

        }

        internal void ProcessMoveErrorResponse(MovementErrorPayload p)
        {
            if ((_lastAction != Action.Move) || (_state != PlayerState.AwaitingGMAcceptanceForMove))
            {
                return;
            }

            _logger.LogWarning("Couldn't make a move!");

            _state = PlayerState.Timeout;
            _couldMove = false;
            ServePenalty(_penalties.move);
            _state = PlayerState.Ready;
            _mre.Set();
        }



        internal void ProcessDiscoveryResponse(DiscoverResponsePayload payload)
        {
            if((_lastAction != Action.Discover) || (_state != PlayerState.AwaitingGMAcceptanceForMove))
            {
                return;
            }
            _logger.LogInformation("Received Discovery response. Proceeding to process it...");

            _state = PlayerState.Timeout;

            int?[,] tempArray =
            {
                { payload.distanceNW, payload.distanceN, payload.distanceNE },
                { payload.distanceW, payload.distanceFromCurrent, payload.distanceE },
                { payload.distanceSW, payload.distanceS, payload.distanceSE }
            };
            for (int i = Position.y + 1, tempi = 0; tempi < 3; i--, tempi++)
            {
                for(int j = Position.x - 1, tempj = 0; tempj < 3; j++, tempj++)
                {
                    if(i >= 0 && i < Board.GetLength(0) && j >= 0 && j < Board.GetLength(1))
                    {
                        Board[i, j].DistToPiece = tempArray[tempi, tempj];
                    }
                }
            }

            ServePenalty(_penalties.discovery);

            _state = PlayerState.Ready;

            _mre.Set();
        }
        internal void ProcessPickupResponse(PickupPieceResponsePayload payload)
        {
            if ((_lastAction != Action.PickupPiece) || (_state != PlayerState.AwaitingGMAcceptanceForMove))
            {
                return;
            }

            _logger.LogInformation("Received Pickup response. Proceeding to process it...");

            _state = PlayerState.Timeout;

            PieceInfo = PieceState.HasUnknown;

            ServePenalty(_penalties.pickup);

            _state = PlayerState.Ready;

            _mre.Set();
        }
        internal void ProcessTestPieceResponse(TestPieceResponsePayload payload)
        {
            if ((_lastAction != Action.TestPiece) || (_state != PlayerState.AwaitingGMAcceptanceForMove))
            {
                return;
            }

            _logger.LogInformation("Received TestPiece response. Proceeding to process it...");

            _state = PlayerState.Timeout;
            if (payload.sham && (PieceInfo != PieceState.NoPiece))
            {
                PieceInfo = PieceState.NoPiece;
            }
            else if(PieceInfo != PieceState.NoPiece)
            {
                PieceInfo = PieceState.NotSham;
            }

            ServePenalty(_penalties.checkForSham);

            _state = PlayerState.Ready;

            _mre.Set();
        }
        internal void ProcessDestroyPieceResponse(DestroyPieceResponsePayload payload)
        {
            if ((_lastAction != Action.DestroyPiece) || (_state != PlayerState.AwaitingGMAcceptanceForMove))
            {
                return;
            }

            _logger.LogInformation("Received DestroyPiece response. Proceeding to process it...");

            _state = PlayerState.Timeout;

            PieceInfo = PieceState.NoPiece;

            ServePenalty(_penalties.destroyPiece);

            _state = PlayerState.Ready;

            _mre.Set();

        }
        internal void ProcessInfoRequest(InformationExchangePassRequestPayload payload)
        {
            _logger.LogInformation("Received Info Request. Proceeding to process it...");
            WaitingPlayers.Add(new WaitingPlayer(payload.askingID, payload.leader));
            if (payload.leader) _shouldRespondToLeader = true;
        }
        internal void ProcessInformationResponse(InformationExchangeResponsePayload p)
        {
            _logger.LogInformation("Received information response. Proceeding to process it...");
            int boardHeight = Board.GetLength(0);
            int boardLength = Board.GetLength(1);
            GoalAreaTileInformation responseInfo;

            for (int i = _goalAreaSize - 1, ind = 0; i >= 0; i--, ind++)
            {
                for (int j = 0; j < boardLength; j++)
                {
                    responseInfo = p.blueTeamGoalAreaInformations[ind * boardLength + j];
                    if(Board[i,j].GoalInfo == GoalAreaTileInformation.Unknown)
                    {
                        Board[i, j].GoalInfo = responseInfo;
                    }
                }
            }

            for (int i = boardHeight - 1, ind = 0; i >= boardHeight - _goalAreaSize; i--, ind++)
            {
                for (int j = 0; j < boardLength; j++)
                {
                    responseInfo = p.redTeamGoalAreaInformations[ind * boardLength + j];
                    if(Board[i,j].GoalInfo == GoalAreaTileInformation.Unknown)
                    {
                        Board[i, j].GoalInfo = responseInfo;
                    }
                }
            }
        }

        internal void ProcessGMResponseToIERequest(InformationExchangeGMResponseToRequestPayload p)
        {
            if ((_lastAction != Action.BegForInfo) || (_state != PlayerState.AwaitingGMAcceptanceForMove))
            {
                return;
            }

            _logger.LogInformation("Received GMResponse to IERequest response. Proceeding to process it...");

            _state = PlayerState.Timeout;

            ServePenalty(_penalties.ask);

            _state = PlayerState.Ready;

            _mre.Set();
        }

        internal void ProcessGMResponseToIEResponse(InformationExchangeGMResponseToResponsePayload p)
        {
            if ((_lastAction != Action.SendInfo) || (_state != PlayerState.AwaitingGMAcceptanceForMove))
            {
                return;
            }

            _logger.LogInformation("Received GMResponse to IEResponse response. Proceeding to process it...");

            _state = PlayerState.Timeout;

            ServePenalty(_penalties.response);

            _state = PlayerState.Ready;

            _mre.Set();
        }

        internal void ProcessPlacePieceResponse(PlacePieceResponsePayload payload)
        {
            if ((_lastAction != Action.PlacePiece) || (_state != PlayerState.AwaitingGMAcceptanceForMove))
            {
                return;
            }

            _logger.LogInformation("Received PlacePiece response. Proceeding to process it...");

            _couldPlace = true;

            _state = PlayerState.Timeout;

            PlacePieceInfo info = payload.Info;

            if(info == PlacePieceInfo.NormalOnGoalField)
            {
                Board[Position.y, Position.x].GoalInfo = GoalAreaTileInformation.Goal;
                PieceInfo = PieceState.NoPiece;
                _logger.LogInformation("Successfully placed a piece on a Goal!");
            }
            else if(info == PlacePieceInfo.NormalOnNonGoalField)
            {
                Board[Position.y, Position.x].GoalInfo = GoalAreaTileInformation.NotGoal;
                PieceInfo = PieceState.NoPiece;
                _logger.LogInformation("Successfully placed a piece on Goals area!");
            }
            else if(info == PlacePieceInfo.ShamOnGoalArea)
            {
                PieceInfo = PieceState.NoPiece;
                _logger.LogInformation("Sham piece was placed on Goals area");
            }
            else if(info == PlacePieceInfo.TaskField)
            {
                PieceInfo = PieceState.NoPiece;
                Board[Position.y, Position.x].DistToPiece = 0;
                _logger.LogInformation("Successfully placed a piece on Task field");
            }

            ServePenalty(_penalties.putPiece);

            _state = PlayerState.Ready;

            _mre.Set();
        }

        internal void ProcessPlaceErrorResponse(PlacePieceErrorPayload p)
        {
            if ((_lastAction != Action.PlacePiece) || (_state != PlayerState.AwaitingGMAcceptanceForMove))
            {
                return;
            }

            _logger.LogWarning($"Couldn't place a piece! error type: {p.errorSubtype}");

            _state = PlayerState.Timeout;
            _couldPlace = false;
            _placeError = p.errorSubtype;
            ServePenalty(_penalties.putPiece);
            _state = PlayerState.Ready;
            _mre.Set();
        }

        internal void ProcessPickupErrorResponse(PickupPieceErrorPayload p)
        {
            if ((_lastAction != Action.PickupPiece) || (_state != PlayerState.AwaitingGMAcceptanceForMove))
            {
                return;
            }

            _logger.LogWarning($"Couldn't pick up a piece! error type: {p.errorSubtype}");

            _state = PlayerState.Timeout;
            _couldPickup = false;
            _pickupError = p.errorSubtype;

            _state = PlayerState.Ready;
            _mre.Set();
        }

        internal void ProcessPenaltyNotServedResponse(PenaltyNotServedErrorPayload p)
        {
            _logger.LogWarning("Penalty not served! Proceeding to sleep and repeat message...");
            _state = PlayerState.Timeout;
            _couldAct = false;
            int sleepLen = p.waitFor;
            Thread.Sleep(sleepLen);
            _state = PlayerState.Ready;
            _mre.Set();
        }

        internal void LogVerbose(Message message)
        {
            if (_verbose)
            {
                _logger.LogInformation($"Sending message with values: {message.payload}");
            }
        }
        internal InformationExchangeResponsePayload PrepareResponse(int id)
        {
            InformationExchangeResponsePayload response = new InformationExchangeResponsePayload();
            response.respondToID = id;
            int boardHeight = Board.GetLength(0);
            int boardLength = Board.GetLength(1);
            response.distances = new int?[boardHeight * boardLength];
            for (int i = 0; i < boardHeight; i++)
            {
                for(int j = 0; j < boardLength; j++)
                {
                    response.distances[i * boardLength + j] = Board[i, j].DistToPiece;
                }
            }

            response.blueTeamGoalAreaInformations = new GoalAreaTileInformation[_goalAreaSize * boardLength];
            for (int i = _goalAreaSize - 1, ind = 0; i >= 0; i--, ind++)
            {
                for (int j = 0; j < boardLength; j++)
                {
                    response.blueTeamGoalAreaInformations[ind * boardLength + j] = Board[i, j].GoalInfo;
                }
            }

            response.redTeamGoalAreaInformations = new GoalAreaTileInformation[_goalAreaSize * boardLength];
            for (int i = boardHeight - 1, ind = 0; i >= boardHeight - _goalAreaSize; i--, ind++)
            {
                for (int j = 0; j < boardLength; j++)
                {
                    response.blueTeamGoalAreaInformations[ind * boardLength + j] = Board[i, j].GoalInfo;
                }
            }

            return response;
        }

        internal void ServePenalty(int configPen) => Thread.Sleep(configPen);
    }
}
