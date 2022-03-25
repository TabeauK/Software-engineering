using CommunicationUtils;
using CommunicationUtils.Payloads;
using CommunicationUtils.Structures;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaster
{
    public partial class GameMaster
    {

        internal void messageReceivedHandler(object obj, MessageReceivedEventArgs e)
        {
            //Gdy przyjdzie wiadomosc watek wchodzi do tej funkcji. Jesli nikt nie przetwarza wiadomosci to zaczyna to robic on. W przeciwnym wypadku
            //konczy swoje dzialanie.
            lock (threadSync)
            {
                if (messageHandlerState == MessageHandlerState.Handling)
                    return;
                messageHandlerState = MessageHandlerState.Handling;
            }
            while (_communicator.HasMessage())
            {
                Message m = _communicator.GetNextMessage();
                if (gameMasterState == GameMasterState.PostGame)
                {
                    _logger.LogWarning("Received a message but the game has already ended. Ignoring");
                    break;
                }
                switch (m.payload)
                {
                    case JoinGameRequestPayload p:
                        joinPlayerHandler(m);
                        break;
                    case DestroyPieceRequestPayload p:
                        destroyPieceHandler(m);
                        break;
                    case DiscoverRequestPayload p:
                        discoverHandler(m);
                        break;
                    case InformationExchangeRequestPayload p:
                        informationExchangeRequestHandler(m);
                        break;
                    case InformationExchangeResponsePayload p:
                        informationExchangeResponseHandler(m);
                        break;
                    case MovementRequestPayload p:
                        movementHandler(m);
                        break;
                    case PickupPieceRequestPayload p:
                        pickupPieceHandler(m);
                        break;
                    case PlacePieceRequestPayload p:
                        placePieceHandler(m);
                        break;
                    case TestPieceRequestPayload p:
                        testPieceHandler(m);
                        break;
                    case PlayerDisconnectPayload p:
                        playerDisconnectedHandler(m);
                        break;
                    default:
                        _logger.LogWarning("Received unknown message. Ignoring");
                        if (_config.Verbose)
                            _logger.LogWarning($"Unknown message: {m.ToString()}");
                        break;
                }
            }
            lock (threadSync)
            {
                messageHandlerState = MessageHandlerState.Idle;
            }
        }

        internal void joinPlayerHandler(Message m)
        {
            if (_config.Verbose)
                _logger.LogInformation($"Received request to join the game. Message:\n {m.ToString()}");
            else
                _logger.LogInformation($"Received request to join the game.");
            bool response = false;
            int id = m.agentID.Value;
            var p = m.payload as JoinGameRequestPayload;
            TeamColor team = p.teamId;
            Player newPlayer = new Player(team, false);
            switch (team)
            {
                case TeamColor.Blue:
                    if (bluePlayers.Count >= _config.NumberOfPlayers)
                    {
                        if (_config.Verbose)
                            _logger.LogWarning($"Declined request from Agent #{m.agentID} to join the {team.ToString()} team: Team full.");
                        else
                            _logger.LogWarning("Join game request denied.");
                        response = false;
                        break;
                    }
                    response = true;
                    _logger.LogInformation($"Agent #{m.agentID} joined the {team.ToString()} team.");
                    bluePlayers = bluePlayers.Add(id, newPlayer);
                    _logger.LogInformation($"{team.ToString()} team: {bluePlayers.Count}/{_config.NumberOfPlayers}");
                    _waitAgentsMre.Set();
                    break;
                case TeamColor.Red:
                    if (redPlayers.Count >= _config.NumberOfPlayers)
                    {
                        if(_config.Verbose)
                            _logger.LogWarning($"Declined request from Agent #{m.agentID} to join the {team.ToString()} team: Team full.");
                        response = false;
                        break;
                    }
                    response = true;
                    _logger.LogInformation($"Agent #{m.agentID} joined the {team.ToString()} team.");
                    redPlayers = redPlayers.Add(id, newPlayer);
                    _logger.LogInformation($"{team.ToString()} team: {redPlayers.Count}/{_config.NumberOfPlayers}");
                    _waitAgentsMre.Set();
                    break;
            }

            Message responseMessage = prepareMessage(id, new JoinGameResponsePayload() { accepted = response, agentID = id });
            _communicator.SendMessage(responseMessage);
            if(_config.Verbose)
                _logger.LogInformation($"JoinGameResponse sent. Message:\n {responseMessage.ToString()}");
        }

        internal void testPieceHandler(Message m)
        {
            if(_config.Verbose)
                _logger.LogInformation($"Received request to test piece. Message:\n {m.ToString()}");
            var p = m.payload as TestPieceRequestPayload;
            Player player = findPlayer(m.agentID.Value);
            if(!player.TryLock(_config.CheckPenalty))
            {
                sendPenaltyNotServedMessage(m.agentID.Value);
                return;
            }
            if(player.Holding == null)
            {
                if (_config.Verbose)
                    _logger.LogWarning($"Agent #{m.agentID} tried to test piece while not holding any.");
                handleNotHoldingError(player, m.agentID.Value);
                return;
            }
            bool sham = player.Holding.IsSham;
            if(sham)
            {
                pieces = pieces.Remove(player.Holding);
                player.Holding = null;
                generatePiece();
            }
            var payload = new TestPieceResponsePayload()
            {
                sham = sham
            };
            Message responseMessage = prepareMessage(m.agentID, payload);
            _communicator.SendMessage(responseMessage);
            if(_config.Verbose)
                _logger.LogInformation($"TestPieceResponse sent. Message:\n {responseMessage.ToString()}");
        }

        internal void placePieceHandler(Message m)
        {
            if(_config.Verbose)
                _logger.LogInformation($"Received request to place piece. Message:\n {m.ToString()}");
            var p = m.payload as PlacePieceRequestPayload;
            Player player = findPlayer(m.agentID.Value);
            var piece = player.Holding;
            if(!player.TryLock(_config.PutPenalty))
            {
                sendPenaltyNotServedMessage(m.agentID.Value);
                return;
            }
            if (player.Holding == null)
            {
                if (_config.Verbose)
                    _logger.LogWarning($"Agent #{m.agentID} tried to place piece while not holding any.");
                handleNotHoldingError(player, m.agentID.Value);
                return;
            }
            IPayload payload = null;
            var result = player.Place();
            if (!result.HasValue)
            {
                payload = new PlacePieceErrorPayload()
                {
                    errorSubtype = PlacePieceErrorType.NoPieceHeld,
                };
            }
            else
            {
                payload = new PlacePieceResponsePayload()
                {
                    Info = result.Value
                };
                if(result.Value != PlacePieceInfo.TaskField)
                {
                    pieces = pieces.Remove(piece);
                    generatePiece();
                }
                if (result.Value == PlacePieceInfo.NormalOnGoalField)
                {
                    if (player.Team == TeamColor.Blue)
                        BlueTeamPoints++;
                    else
                        RedTeamPoints++;
                    _logger.LogInformation($"{player.Team.ToString()} team scored a point!");
                    _endGameMre.Set();
                }
            }
            Message responseMessage = prepareMessage(m.agentID, payload);
            _communicator.SendMessage(responseMessage);
            if(_config.Verbose)
                _logger.LogInformation($"PlacePieceResponse sent. Message:\n {responseMessage.ToString()}");
        }

        internal void pickupPieceHandler(Message m)
        {
            if(_config.Verbose)
                _logger.LogInformation($"Received request to pick up piece. Message:\n {m.ToString()}");
            var p = m.payload as PickupPieceRequestPayload;
            Player player = findPlayer(m.agentID.Value);
            if(!player.TryLock(_config.PickUpPenalty))
            {
                sendPenaltyNotServedMessage(m.agentID.Value);
                return;
            }
            IPayload payload = null;
            {
            };
            PickupPieceErrorType? result = player.PickUp();
            if (!result.HasValue)//udalo sie
            {
                payload = new PickupPieceResponsePayload();
            }
            else
            {
                payload = new PickupPieceErrorPayload()
                {
                    errorSubtype = result.Value
                };
            }
            Message responseMessage = prepareMessage(m.agentID, payload);
            _communicator.SendMessage(responseMessage);
            if(_config.Verbose)
                _logger.LogInformation($"PickupPieceResponse sent. Message:\n {responseMessage.ToString()}");
        }

        internal void movementHandler(Message m)
        {
            var p = m.payload as MovementRequestPayload;
            if(_config.Verbose)
                _logger.LogInformation($"Received request to move. Message:\n {m.ToString()}");
            Player player = findPlayer(m.agentID.Value);
            var position = player.Position.ToVector2D();
            if(!player.TryLock(_config.MovePenalty))
            {
                sendPenaltyNotServedMessage(m.agentID.Value);
                return;
            }
            IPayload payload;
            bool moved = false;
            switch (p.direction)
            {
                case Direction.North:
                    if (position.y < _config.Y - 1)
                        moved = player.Move(map[position.y + 1, position.x]);
                    break;
                case Direction.South:
                    if (position.y > 0)
                        moved = player.Move(map[position.y - 1, position.x]);
                    break;
                case Direction.West:
                    if (position.x > 0)
                        moved = player.Move(map[position.y, position.x - 1]);
                    break;
                case Direction.East:
                    if (position.x < _config.X - 1)
                        moved = player.Move(map[position.y, position.x + 1]);
                    break;
            }
            if(!moved)
            {
                payload = new MovementErrorPayload()
                {
                    position = position
                };
            }
            else
            {
                payload = new MovementResponsePayload()
                {
                    closestPiece = player.Position.DistanceToClosestPiece(pieces),
                    currentPosition = player.Position.ToVector2D(),
                    madeMove = true
                };
            }
            Message responseMessage = prepareMessage(m.agentID, payload);
            _communicator.SendMessage(responseMessage);
            if(_config.Verbose)
                _logger.LogInformation($"MovementResponse sent. Message:\n {responseMessage.ToString()}");
        }

        internal void informationExchangeRequestHandler(Message m)
        {
            InformationExchangeRequestPayload p = m.payload as InformationExchangeRequestPayload;
            if(_config.Verbose)
                _logger.LogInformation($"Received Ask request. Message:\n {m.ToString()}");
            Player askingPlayer = findPlayer(m.agentID.Value);
            if(!askingPlayer.TryLock(_config.RequestInformationExchangePenalty))
            {
                sendPenaltyNotServedMessage(m.agentID.Value);
                return;
            }
            bool succeeded = (findPlayer(p.askedAgentID) != null);
            var responsePayload = new InformationExchangeGMResponseToRequestPayload()
            {
                succeeded = succeeded
            };
            Message responseMessage = prepareMessage(m.agentID, responsePayload);
            _communicator.SendMessage(responseMessage);
            if (_config.Verbose)
                _logger.LogInformation($"Ask response sent. Message:\n {responseMessage.ToString()}");
            if (succeeded)
            {
                var passPayload = new InformationExchangePassRequestPayload()
                {
                    askingID = m.agentID.Value,
                    leader = askingPlayer.IsLeader,
                    teamId = askingPlayer.Team
                };
                Message passMessage = prepareMessage(p.askedAgentID, passPayload);
                _communicator.SendMessage(passMessage);
                if (_config.Verbose)
                    _logger.LogInformation($"Ask sent to Agent #{p.askedAgentID} from Agent #{m.agentID.Value}. Message:\n {passMessage.ToString()}");
            }
        }

        internal void informationExchangeResponseHandler(Message m)
        {
            var p = m.payload as InformationExchangeResponsePayload;
            if(_config.Verbose)
                _logger.LogInformation($"Received Response. Message:\n {m.ToString()}");
            Player respondToPlayer = findPlayer(p.respondToID);
            Player respondingPlayer = findPlayer(m.agentID.Value);
            var succeeded = false;
            if (!respondingPlayer.TryLock(_config.InformationExchangePenalty))
            {
                sendPenaltyNotServedMessage(m.agentID.Value);
                return;
            }
            if (respondToPlayer != null)
            {
                Message forwardMessage = prepareMessage(p.respondToID, m.payload);
                _communicator.SendMessage(forwardMessage);
                if (_config.Verbose)
                    _logger.LogInformation($"Info response sent to Agent #{p.respondToID} from Agent #{m.agentID}. " +
                        $"Message:\n{forwardMessage.ToString()}");
                succeeded = true;
            }
            var responePayload = new InformationExchangeGMResponseToResponsePayload() { succeeded = succeeded };
            Message responseMessage = prepareMessage(m.agentID, responePayload);
            _communicator.SendMessage(responseMessage);
            if(_config.Verbose)
                _logger.LogInformation($"Response response sent. Message:\n {responseMessage.ToString()}");
            return;
        }

        internal void destroyPieceHandler(Message m)
        {
            if(_config.Verbose)
                _logger.LogInformation($"Received request to destroy piece. Message:\n {m.ToString()}");
            var p = m.payload as DestroyPieceRequestPayload;
            Player player = findPlayer(m.agentID.Value);
            if (player.Holding == null)
            {
                if (_config.Verbose)
                    _logger.LogWarning($"Agent #{m.agentID} tried to destroy piece while not holding any.");
                handleNotHoldingError(player, m.agentID.Value);
                return;
            }
            IPayload payload = null;
            if(!player.TryLock(_config.DestroyPenalty))
            {
                sendPenaltyNotServedMessage(m.agentID.Value);
                return;
            }
            pieces = pieces.Remove(player.Holding);//tego nie usuwac
            if(player.Destroy())
            {
                payload = new DestroyPieceResponsePayload();
                generatePiece();
            }
            else
            {
                payload = new DestroyPieceResponsePayload();//TODO: zamienic na DestroyErrorPayload jak bedzie
            }
            Message responseMessage = prepareMessage(m.agentID, payload);
            _communicator.SendMessage(responseMessage);
            if(_config.Verbose)
                _logger.LogInformation($"DestroyResponse sent. Message:\n {responseMessage.ToString()}");
        }

        internal void discoverHandler(Message m)
        {
            if(_config.Verbose)
                _logger.LogInformation($"Received request to discover. Message:\n {m.ToString()}");
            var p = m.payload as DiscoverRequestPayload;
            Player player = findPlayer(m.agentID.Value);
            if(!player.TryLock(_config.DiscoveryPenalty))
            {
                sendPenaltyNotServedMessage(m.agentID.Value);
                return;
            }
            var pos = player.Position.ToVector2D();
            discover(pos.y, pos.x, out DiscoverResponsePayload payload);
            Message responseMessage = prepareMessage(m.agentID, payload);
            _communicator.SendMessage(responseMessage);
            if(_config.Verbose)
                _logger.LogInformation($"DiscoverResponse sent. Message:\n {responseMessage.ToString()}");
        }

        internal void playerDisconnectedHandler(Message m)
        {
            var p = m.payload as PlayerDisconnectPayload;
            Player player = findPlayer(p.agentID);
            if(player == null)
            {
                _logger.LogWarning($"Received PlayerDisconnected payload with invalid agent ID. Message:\n{m.ToString()}" );
                return;
            }
            _logger.LogWarning($"Agent #{p.agentID} has disconnected. Processing..." + (_config.Verbose ? $" Message:\n{m.ToString()}" : string.Empty));
            
            if(gameMasterState == GameMasterState.Playing) //w czasie gry, koniec
            {
                if (player.Team == TeamColor.Blue)
                {
                    endGame(TeamColor.Red);
                    return;
                }
                endGame(TeamColor.Blue);
                return;
            }
            else if(gameMasterState == GameMasterState.WaitingForPlayers)
            {
                if (player.Team == TeamColor.Blue)
                {
                    _logger.LogWarning($"Removing Agent #{p.agentID} from the Blue team.");
                    bluePlayers = bluePlayers.Remove(p.agentID);
                    return;
                }
                _logger.LogWarning($"Removing Agent #{p.agentID} from the Red team.");
                redPlayers = redPlayers.Remove(p.agentID);
                return;
            }
        }

        internal void sendPenaltyNotServedMessage(int agentId)
        {
            _logger.LogInformation($"Agent #{agentId} did not serve penalty. Sending error message back");
            var player = findPlayer(agentId);
            var errorPayload = new PenaltyNotServedErrorPayload()
            {
                waitFor = (int)(player.lockedTill.Value - DateTime.Now).TotalMilliseconds,
            };
            Message errorMessage = prepareMessage(agentId, errorPayload);
            _communicator.SendMessage(errorMessage);
            if(_config.Verbose)
                _logger.LogInformation($"PenaltyNotServed sent. Message:\n {errorMessage.ToString()}");
            return;
        }

        internal Message prepareMessage(int? agentID, IPayload payload)
        {
            return new Message()
            {
                agentID = agentID,
                payload = payload
            };

        }

        internal void handleNotHoldingError(Player player, int id)
        {
            var pay = new UndefinedErrorPayload()
            {
                holdingPiece = false,
                position = player.Position.ToVector2D()
            };
            var mes = prepareMessage(id, pay);
            _communicator.SendMessage(mes);
            if (_config.Verbose)
                _logger.LogWarning($"Sending undefined error message to Agent. Message:\n{mes.ToString()}");
            return;
        }
    }
}
