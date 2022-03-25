using CommunicationUtils;
using CommunicationUtils.Payloads;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationServer
{
    public partial class Server
    {
        private void HandleAgentMessage(object sender, MessageReceivedEventArgs e)
        {
            ICommunicator communicator = (ICommunicator)sender;
            int agentid = GetAgentID(communicator);
            if (agentid == -1)
            {
                Log.Fatal("Received a message from an untracked Agent communicator. Stopping the server.");
                Stop();
                return;
            }
            while (communicator.HasMessage())
            {
                Message m = communicator.GetNextMessage();
                m.agentID = agentid;
                if(m.payload==null)
                {
                    Log.Warning(string.Format("Received invalid message from Agent id {0}, message type id {1}. Ignoring.", m.agentID, m.messageID) +
                        (Verbose ? $"\nMessage content: {m.ToString()}" : string.Empty));
                }
                else if(gameMasterCommunicator!=null)
                {
                    gameMasterCommunicator.TrySendMessage(m);
                    Log.Information($"Message: Agent {m.agentID} [{m.payload.GetType().Name}] -> GameMaster" +
                        (Verbose ? $"\nMessage content: {m.ToString()}":string.Empty));
                }
                else
                {
                    Log.Warning(string.Format("Message from Agent id {0}, payload type {1}, will not be forwarded for there is no GameMaster connected", m.agentID, m.payload.GetType().Name) +
                        (Verbose ? $"\nMessage content: {m.ToString()}" : string.Empty));
                }
            }
        }

        private void HandleGameMasterMessage(object sender, MessageReceivedEventArgs e)
        {
            ICommunicator communicator = (ICommunicator)sender;
            while (communicator.HasMessage())
            {
                Message m = communicator.GetNextMessage();
                if (m.payload == null)
                {
                    Log.Warning(string.Format("Received invalid message from GameMaster, message type id {0}. Ignoring.", m.messageID) +
                        (Verbose ? $"\nMessage content: {m.ToString()}" : string.Empty));
                    continue;
                }
                if (m.payload is StartGamePayload && state == ServerState.PreGame)
                {
                    state = ServerState.InGame;
                    Log.Information("Game state changed to InGame");
                    StopListeners();
                }
                else if (m.payload is EndGamePayload)
                {
                    state = ServerState.PostGame;
                    Log.Information("Game state changed to PostGame");
                }
                //Czekamy z zamykaniem serwera do rozłączenia GM dopiero. Tu nic nie robimy.
                if(m.agentID==null)
                {
                    Log.Warning("Received invalid message from GameMaster. No agent id specified. Ignoring." +
                        (Verbose ? $"\nMessage content: {m.ToString()}" : string.Empty));
                    continue;
                }
                ICommunicator agentCommunicator = GetAgentCommunicator(m.agentID.Value);
                if(agentCommunicator==null)
                {
                    Log.Warning(string.Format("Received invalid message from GameMaster. No Agent with id {0} exists. Ignoring.",m.agentID.Value) +
                        (Verbose ? $"\nMessage content: {m.ToString()}" : string.Empty));
                    continue;
                }
                agentCommunicator.TrySendMessage(m);
                Log.Information($"Message: GameMaster [{m.payload.GetType().Name}] -> Agent {m.agentID}" +
                        (Verbose ? $"\nMessage content: {m.ToString()}" : string.Empty));

                if (m.payload is JoinGameResponsePayload)
                {
                    JoinGameResponsePayload p = (JoinGameResponsePayload)m.payload;
                    if (p.accepted == false)
                    {
                        agentCommunicator.Dispose();
                        return;
                    }
                    else
                        agentsYetToBeAccepted.Remove(agentCommunicator);
                }
            }
        }
    }
}
