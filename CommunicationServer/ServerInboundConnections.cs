using CommunicationUtils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CommunicationServer
{
    public partial class Server
    {
        public void AddAgent(TcpClient client)
        {
            if (client == null)
                throw new ArgumentNullException();
            if(!client.Connected)
            {
                Log.Warning("Ignoring disconnected TcpClient agent");
                return;
            }
            Log.Information("New Agent listener connection from " + ((IPEndPoint)(client.Client.RemoteEndPoint)).Address);
            Communicator communicator = new Communicator();
            communicator.Connect(client);
            HandleNewAgent(communicator);
        }

        public void AddAgent(SynchronousCommunicator agent)
        {
            if(agent == null)
                throw new ArgumentNullException();
            if (agent.GetState() != CommunicatorState.Unitialized)
            {
                Log.Error("Invalid state agent tried to connect to server. Ignoring.");
                return;
            }
            Log.Information("New Agent connected via sync communicator");
            SynchronousCommunicator communicator = new SynchronousCommunicator();
            agent.Connect(communicator);
            HandleNewAgent(communicator);
        }

        private void HandleNewAgent(ICommunicator communicator)
        {
            if(state!=ServerState.PreGame)
            {
                Log.Information("Rejecting an Agent that tried to connect during active game");
                communicator.Dispose();
            }
            if (gameMasterCommunicator == null)
            {
                Log.Information("Rejecting agent because no GameMaster is connected");
                communicator.Dispose();
            }

            int newID;
            lock (newClientIdLock)
            {
                newID = ++lastClientId;
            }
            Log.Information("New agent id: " + newID);

            agentCommunicators.Add(communicator, newID);
            agentsYetToBeAccepted.Add(communicator);
            agentIDMap.Add(newID, communicator);
            communicator.MessageReceived += agentMessageReceivedHandler;
            communicator.StateChanged += agentStateChangedHandler;
        }

        public void AddGameMaster(TcpClient client)
        {
            if (client == null)
                throw new ArgumentNullException();
            if (!client.Connected)
            {
                Log.Warning("Ignoring disconnected TcpClient Game Master");
                return;
            }
            Log.Information("New GameMaster listener connection from " + ((IPEndPoint)(client.Client.RemoteEndPoint)).Address);
            Communicator communicator = new Communicator();
            communicator.Connect(client);
            HandleNewGameMaster(communicator);
        }

        public void AddGameMaster(SynchronousCommunicator gm)
        {
            if (gm == null)
                throw new ArgumentNullException();
            if (gm.GetState() != CommunicatorState.Unitialized)
            {
                Log.Error("Invalid state Game Master tried to connect to server. Ignoring.");
                return;
            }
            Log.Information("New GameMaster connected via sync communicator");
            SynchronousCommunicator communicator = new SynchronousCommunicator();
            gm.Connect(communicator);
            HandleNewGameMaster(communicator);
        }

        public void HandleNewGameMaster(ICommunicator gameMaster)
        {
            if (state == ServerState.PreGame)
            {
                if (gameMasterCommunicator == null)
                {
                    gameMasterCommunicator = gameMaster;
                    gameMaster.MessageReceived += gameMasterMessageReceivedHandler;
                    gameMaster.StateChanged += gameMasterStateChangedHandler;
                }
                else
                {
                    Log.Information("A second Game Master was rejected");
                    gameMaster.Dispose();
                }
            }
            else
            {
                Log.Information("A Game Master tried to connect during active game");
                gameMaster.Dispose();
            }
        }
    }
}
