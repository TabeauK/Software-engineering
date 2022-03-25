using CommunicationUtils;
using CommunicationUtils.Payloads;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CommunicationServer
{
    public partial class Server
    {
        private ServerConfiguration configuration { get; set; }
        private TcpListener gameMasterListener { get; set; }
        private TcpListener agentListener { get; set; }
        private Task gameMasterListenerTask;
        private Task agentListenerTask;
        private int lastClientId = 0;
        private Dictionary<ICommunicator, int> agentCommunicators = new Dictionary<ICommunicator, int>();
        private Dictionary<int, ICommunicator> agentIDMap = new Dictionary<int, ICommunicator>();
        private ICommunicator gameMasterCommunicator;
        private ServerState state = ServerState.PreGame;
        private HashSet<ICommunicator> agentsYetToBeAccepted = new HashSet<ICommunicator>();

        public bool Verbose { get; private set; }

        private MessageReceivedEventHandler agentMessageReceivedHandler;
        private MessageReceivedEventHandler gameMasterMessageReceivedHandler;

        private StateChangedEventHandler agentStateChangedHandler;
        private StateChangedEventHandler gameMasterStateChangedHandler;

        private readonly object newClientIdLock = new object(); 

        public Server()
        {
            agentMessageReceivedHandler = new MessageReceivedEventHandler(HandleAgentMessage);
            gameMasterMessageReceivedHandler = new MessageReceivedEventHandler(HandleGameMasterMessage);
            agentStateChangedHandler = new StateChangedEventHandler(HandleAgentStateChange);
            gameMasterStateChangedHandler = new StateChangedEventHandler(HandleGameMasterStateChange);
            state = ServerState.Stopped;
        }

        public bool Launch(string filepath)
        {
            Log.Information("Loading configuration from file");
            ServerConfiguration config = new ServerConfiguration();
            string json = null;
            try
            {
                using (FileStream fs = File.OpenRead("config.json"))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        json = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                Log.Fatal("An error occured while loading configuration from file. Make sure the file \"config.json\" is present.");
                return false;
            }
            config = JsonConvert.DeserializeObject<ServerConfiguration>(json);
            if(config.GameMasterListenerPort==null || config.AgentListenerPort==null)
            {
                Log.Fatal("An error occured while parsing configuration file. Make sure it has a proper structure");
                return false;
            }
            return Launch(config.GameMasterListenerPort.Value, config.AgentListenerPort.Value, config.Verbose);
            
        }
        public bool Launch(string[] args)
        {
            Log.Information("Loading configuration from CLI parameters");
            if(args==null)
            {
                Log.Fatal("Null args specified");
                return false;
            }
            if (args.Length != 4 && args.Length != 3)
            {
                Log.Fatal("Incorrect number of configuration parameters");
                return false;
            }
            int gmport = -1, agentport = -1;
            if (!int.TryParse(args[args.Length - 3], out gmport))
            {
                Log.Fatal("Invalid GM listener port parameter: " + args[args.Length - 3]);
                return false;
            }
            if (!int.TryParse(args[args.Length - 2], out agentport))
            {
                Log.Fatal("Invalid Agent port listener parameter: " + args[args.Length - 2]);
                return false;
            }
            bool verbose;
            if (!bool.TryParse(args[args.Length - 1], out verbose))
            {
                Log.Fatal("Invalid Agent port verbose parameter: " + args[args.Length - 1]);
                return false;
            }
            return Launch(gmport, agentport, verbose); 
        }

        public void Launch()
        {
            Log.Information(" >>> Launching NoListener version of CS <<<");
            Log.Information(" *** Starting Communication Server *** ");
            state = ServerState.PreGame;
            Log.Information("NoListener CS started");
        }

        private bool Launch(int gmport, int agentport, bool verbose)
        {
            Log.Information(" *** Starting Communication Server *** ");
            if (state != ServerState.Stopped)
            {
                Log.Error("The server was told to start for the second time. Aborting the action.");
                return false;
            }
            configuration = new ServerConfiguration();
            configuration.GameMasterListenerPort = gmport;
            configuration.AgentListenerPort = agentport;
            if (configuration.GameMasterListenerPort < IPEndPoint.MinPort ||
                configuration.GameMasterListenerPort > IPEndPoint.MaxPort)
            {
                Log.Fatal(string.Format("Game Master listener port was {0}, but must be between {1} and {2}",
                    configuration.GameMasterListenerPort, IPEndPoint.MinPort, IPEndPoint.MaxPort));
                return false;
            }
            if (configuration.AgentListenerPort < IPEndPoint.MinPort ||
                configuration.AgentListenerPort > IPEndPoint.MaxPort)
            {
                Log.Fatal(string.Format("Agent listener port was {0}, but must be between {1} and {2}",
                    configuration.AgentListenerPort, IPEndPoint.MinPort, IPEndPoint.MaxPort));
                return false;
            }
            if (configuration.GameMasterListenerPort == configuration.AgentListenerPort)
            {
                Log.Fatal("Game Master listener port cannot be the same as Agent listener port");
                return false;
            }
            Verbose = verbose;
            if (Verbose)
                Log.Information("Server is running in VERBOSE mode");
            state = ServerState.Loading;
            if (!StartListeners())
            {
                Stop();
                return false;
            }
            state = ServerState.PreGame;
            Log.Information("Server initiation complete");
            return true;
        }

        public void Stop()
        {
            if (state == ServerState.Stopped)
            {
                Log.Warning("Tried to stop a stopped server");
                return;
            }   
            state = ServerState.Stopping;
            Log.Information("Stopping procedure started");
            StopListeners();
            DisconnectAgents();
            DisconnectGameMaster();
            
            state = ServerState.Stopped;
            Thread.Sleep(1000);
            Log.Information("*** Communication Server has successfully stopped ***");
        }

        private void StopListeners()
        {
            Log.Information("Stopping listeners...");
            bool isLinux = System.Runtime.InteropServices.RuntimeInformation
                .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
            if (gameMasterListener != null)
            {
                if(isLinux)
                    gameMasterListener.Server.Shutdown(SocketShutdown.Both);
                gameMasterListener.Stop();
            }
            if (agentListener != null)
            {
                if(isLinux)
                    agentListener.Server.Shutdown(SocketShutdown.Both);
                agentListener.Stop();
            }
            Log.Information("Listeners stopped");
        }

        public ServerState GetState()
        {
            return state;
        }

        private void DisconnectAgents()
        {
            Log.Information("Disconnecting Agents...");
            List<ICommunicator> agents = new List<ICommunicator>();
            foreach (var v in agentCommunicators)
            {
                agents.Add(v.Key);
            }
            foreach (ICommunicator c in agents)
            {
                DisconnectAgent(c);
            }
        }
        private void DisconnectAgent(ICommunicator communicator)
        {
            int id = -1;
            lock (agentCommunicators)
            {
                if (agentCommunicators.TryGetValue(communicator, out id))
                    agentCommunicators.Remove(communicator);
                else //już obsłużony w innym wątku podczas zamykania
                    return;
            }
            communicator.MessageReceived -= agentMessageReceivedHandler;
            communicator.StateChanged -= agentStateChangedHandler;
            communicator.Dispose();
            if(!agentsYetToBeAccepted.Contains(communicator) && 
                gameMasterCommunicator != null && gameMasterCommunicator.GetState()==CommunicatorState.Connected)
            {
                PlayerDisconnectPayload payload = new PlayerDisconnectPayload() { agentID = id };
                Message m = new Message() { payload = payload };
                gameMasterCommunicator.TrySendMessage(m);
            }
            agentsYetToBeAccepted.Remove(communicator);
            agentIDMap.Remove(id);
            Log.Information(string.Format("Agent disconnection procedure finished for Agent id {0}", id));
        }

        private void DisconnectGameMaster()
        {
            if (gameMasterCommunicator != null)
            {
                Log.Information("Disconnecting Game Master...");
                gameMasterCommunicator.MessageReceived -= gameMasterMessageReceivedHandler;
                gameMasterCommunicator.StateChanged -= gameMasterStateChangedHandler;
                gameMasterCommunicator.Dispose();
                gameMasterCommunicator = null;
                Log.Information("Game Master succesfully disconnected");
                DisconnectAgents();
                if (state != ServerState.PreGame)
                {
                    Stop();
                }
                Log.Information(string.Format("GameMaster disconnection procedure finished"));
            }
        }



        private void HandleGameMasterStateChange(object sender, StateChangedEventArgs e)
        {
            ICommunicator ICommunicator = (ICommunicator)sender;
            if(gameMasterCommunicator!=ICommunicator)
            {
                Log.Fatal("Received GameMaster ICommunicator state change from non-GameMaster ICommunicator. Stopping the server.");
                Stop();
                return;
            }
            if (e.NewState == CommunicatorState.Disconnected)
            {
                Log.Information("GameMaster has disconnected");
                DisconnectGameMaster();
            }
        }



        private void HandleAgentStateChange(object sender, StateChangedEventArgs e)
        {
            ICommunicator ICommunicator = (ICommunicator)sender;
            int agentID = -1;
            if(!agentCommunicators.TryGetValue(ICommunicator, out agentID))
            {
                Log.Fatal("Received Agent ICommunicator state change from untracked ICommunicator. Stopping the server.");
                Stop();
                return;
            }
            if(e.NewState==CommunicatorState.Disconnected)
            {
                Log.Information(string.Format("Agent with id {0} has disconnected", agentID));
                DisconnectAgent(ICommunicator);
            }
        }

        private int GetAgentID(ICommunicator communicator)
        {
            int ret = -1;
            if (agentCommunicators.TryGetValue(communicator, out ret))
                return ret;
            return -1;
        }

        private ICommunicator GetAgentCommunicator(int id)
        {
            ICommunicator ret = null;
            if (agentIDMap.TryGetValue(id, out ret))
                return ret;
            return null;
        }

        private void GameMasterListenerLoop()
        {
            while (true)
            {
                TcpClient client;
                try
                {
                    client = gameMasterListener.AcceptTcpClient();
                }
                catch (SocketException) //zamknięcie listenera powoduje wyjątek, naturalna akcja przy wyłączaniu serwera
                {
                    Log.Information("GameMaster listener loop has been interrupted");
                    return;
                }
                //ten kod wykonuje się po zaakceptowaniu połączenia
                AddGameMaster(client);
            }
        }

        private void AgentListenerLoop()
        {
            while (true)
            {
                TcpClient client;
                try
                {
                     client = agentListener.AcceptTcpClient();
                }
                catch(SocketException) //zamknięcie listenera powoduje wyjątek, naturalna akcja przy wyłączaniu serwera
                {
                    Log.Information("Agent listener loop has been interrupted");
                    return;
                }
                AddAgent(client);
            }
        }

        private bool StartListeners()
        {
            try
            {
                // TcpListener server = new TcpListener(port);
                gameMasterListener = new TcpListener(IPAddress.Any, configuration.GameMasterListenerPort.Value);
                gameMasterListener.Start();
                Log.Information("Game Master listener started on port " + configuration.GameMasterListenerPort.Value);
            }
            catch (SocketException e)
            {
                Log.Fatal("Error while starting Game Master listener: " + e.Message);
                return false;
            }
            try
            {
                agentListener = new TcpListener(IPAddress.Any, configuration.AgentListenerPort.Value);
                agentListener.Start();
                Log.Information("Agent listener started on port " + configuration.AgentListenerPort.Value);
            }
            catch (SocketException e)
            {
                Log.Fatal("Error while starting Agent listener: " + e.Message);
                return false;
            }

            gameMasterListenerTask = Task.Run(() => GameMasterListenerLoop());
            agentListenerTask = Task.Run(() => AgentListenerLoop());
            Log.Information("Listeners started");
            return true;
        }

        public int GetNumberOfConnectedPlayers()
        {
            return agentCommunicators.Count;
        }

        public List<int> GetConnectedPlayerIDs()
        {
            List<int> ret = new List<int>();
            foreach(var v in agentCommunicators.Values)
            {
                ret.Add(v);
            }
            return ret;
        }

        public bool IsGameMasterConnected()
        {
            return gameMasterCommunicator != null;
        }

        public bool IsRunning()
        {
            return state!=ServerState.Stopped;
        }
    }

    public enum ServerState {Stopped, Loading, PreGame, InGame, PostGame, Stopping}
}
