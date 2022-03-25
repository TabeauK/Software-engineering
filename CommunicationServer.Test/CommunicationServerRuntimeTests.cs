using CommunicationUtils;
using CommunicationUtils.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CommunicationServer.Test
{
    [TestClass]
    public class CommunicationServerRuntimeTests
    {
        [TestMethod]
        public void TestCommunicationServer_RejectsAgentsBeforeGameMasterJoined()
        {
            //given
            Server server = new Server();
            server.Launch();
            SynchronousCommunicator agent = new SynchronousCommunicator();
            //when
            server.AddAgent(agent);
            //then
            Assert.IsTrue(agent.GetState() == CommunicatorState.Disconnected);
        }

        [TestMethod]
        public void TestCommunicationServer_AcceptsFirstRejectsSecondGameMaster()
        {
            //given
            Server server = new Server();
            server.Launch();
            SynchronousCommunicator gm1 = new SynchronousCommunicator();
            SynchronousCommunicator gm2 = new SynchronousCommunicator();

            //when
            server.AddGameMaster(gm1);
            server.AddGameMaster(gm2);

            //then
            Assert.IsTrue(gm2.GetState() == CommunicatorState.Disconnected);
            Assert.IsTrue(gm1.GetState() == CommunicatorState.Connected);
        }

        [TestMethod]
        public void TestCommunicationServer_DetectsGameStateChanges()
        {
            //given
            Server server = new Server();
            server.Launch();
            SynchronousCommunicator gm1 = new SynchronousCommunicator();
            server.AddGameMaster(gm1);

            //when
            gm1.SendMessage(new Message() { agentID=1, payload = new StartGamePayload() });
            //then
            Assert.IsTrue(server.GetState() == ServerState.InGame);

            //when
            gm1.SendMessage(new Message() { payload = new EndGamePayload()});
            //then
            Assert.IsTrue(server.GetState() == ServerState.PostGame);
        }

        [TestMethod]
        public void TestCommunicationServer_ForwardsMessagesFromGMToAgent()
        {
            //given
            Server server = new Server();
            server.Launch();
            SynchronousCommunicator gm1 = new SynchronousCommunicator();
            SynchronousCommunicator agent1 = new SynchronousCommunicator();
            server.AddGameMaster(gm1);
            server.AddAgent(agent1);
            //when
            gm1.SendMessage(new Message() { agentID = 1, payload = new UndefinedErrorPayload()});
            //then
            Assert.IsTrue(agent1.HasMessage());
            Message m = agent1.GetNextMessage();
            Assert.AreEqual(m.agentID, 1);
            Assert.AreEqual(m.payload.GetType(), typeof(UndefinedErrorPayload));
        }
        [TestMethod]
        public void TestCommunicationServer_ForwardsMessagesFromAgentToGM()
        {
            //given
            Server server = new Server();
            server.Launch();
            SynchronousCommunicator gm1 = new SynchronousCommunicator();
            SynchronousCommunicator agent1 = new SynchronousCommunicator();

            //when
            server.AddGameMaster(gm1);
            server.AddAgent(agent1);
            agent1.SendMessage(new Message() { agentID = 2, payload = new InformationExchangeRequestPayload() });

            //then
            Assert.IsTrue(gm1.HasMessage());
            Message m = gm1.GetNextMessage();
            Assert.AreEqual(m.agentID, 1);
            Assert.AreEqual(m.payload.GetType(), typeof(InformationExchangeRequestPayload));
        }
        [TestMethod]
        public void TestCommunicationServer_DisconnectsAgentsOnGameMasterLeave()
        {
            //given
            Server server = new Server();
            server.Launch();
            SynchronousCommunicator gm1 = new SynchronousCommunicator();
            SynchronousCommunicator agent1 = new SynchronousCommunicator();
            SynchronousCommunicator agent2 = new SynchronousCommunicator();
            SynchronousCommunicator agent3 = new SynchronousCommunicator();
            server.AddGameMaster(gm1);
            server.AddAgent(agent1);
            server.AddAgent(agent2);
            server.AddAgent(agent3);
            //when
            gm1.Dispose();
            //then
            Assert.AreEqual(CommunicatorState.Disconnected, agent1.GetState());
            Assert.AreEqual(CommunicatorState.Disconnected, agent2.GetState());
            Assert.AreEqual(CommunicatorState.Disconnected, agent3.GetState());
        }
        [TestMethod]
        public void TestCommunicationServer_IsStoppedOnCreation()
        {
            //given
            Server server = new Server();
            //then
            Assert.AreEqual(ServerState.Stopped, server.GetState());
            Assert.AreEqual(0, server.GetNumberOfConnectedPlayers());
        }
        [TestMethod]
        public void TestCommunicationServer_IsInPreGameStateAfterLoading()
        {
            //given
            Server server = new Server();
            //when
            server.Launch();
            //then
            Assert.AreEqual(ServerState.PreGame, server.GetState());
        }
        [TestMethod]
        public void TestCommunicationServer_StopsAfterStopCommand()
        {
            //given
            Server server = new Server();
            server.Launch();
            //when
            server.Stop();
            //then
            Assert.AreEqual(ServerState.Stopped, server.GetState());
        }
        [TestMethod]
        public void TestCommunicationServer_AcceptsAgentsAfterGameMasterJoined()
        {
            //given
            Server server = new Server();
            server.Launch();
            SynchronousCommunicator gm1 = new SynchronousCommunicator();
            SynchronousCommunicator agent1 = new SynchronousCommunicator();
            SynchronousCommunicator agent2 = new SynchronousCommunicator();
            server.AddGameMaster(gm1);
            //when
            server.AddAgent(agent1);
            server.AddAgent(agent2);
            //then
            Assert.AreEqual(CommunicatorState.Connected, agent1.GetState());
            Assert.AreEqual(CommunicatorState.Connected, agent2.GetState());
        }
        [TestMethod]
        public void TestCommunicationServer_AssignsAgentIDsInAscendingOrder()
        {
            //given
            Server server = new Server();
            server.Launch();
            SynchronousCommunicator gm1 = new SynchronousCommunicator();
            SynchronousCommunicator agent1 = new SynchronousCommunicator();
            SynchronousCommunicator agent2 = new SynchronousCommunicator();
            SynchronousCommunicator agent3 = new SynchronousCommunicator();
            server.AddGameMaster(gm1);
            //when
            server.AddAgent(agent1);
            int agent1ID = server.GetConnectedPlayerIDs()[0];
            server.AddAgent(agent2);
            int agent2ID = server.GetConnectedPlayerIDs().Find(c => c != agent1ID);
            server.AddAgent(agent3);
            int agent3ID = server.GetConnectedPlayerIDs().Find(c => c != agent1ID && c!=agent2ID);
            //then
            Assert.AreEqual(1, agent1ID);
            Assert.IsTrue(agent2ID > agent1ID);
            Assert.IsTrue(agent3ID > agent2ID);
        }
        [TestMethod]
        public void TestCommunicationServer_RejectsInvalidAgentCommunicator()
        {
            //given
            Server server = new Server();
            server.Launch();
            SynchronousCommunicator gm1 = new SynchronousCommunicator();
            SynchronousCommunicator agent1 = new SynchronousCommunicator();
            SynchronousCommunicator partner = new SynchronousCommunicator();
            server.AddGameMaster(gm1);
            agent1.Connect(partner);
            //when
            server.AddAgent(agent1);
            //then
            Assert.AreEqual(0, server.GetNumberOfConnectedPlayers());
        }
        [TestMethod]
        public void TestCommunicationServer_ThrowsExceptionOnNullAgentTcpClient()
        {
            //given
            Server server = new Server();
            server.Launch();
            SynchronousCommunicator gm1 = new SynchronousCommunicator();
            server.AddGameMaster(gm1);
            //when
            TcpClient nullClient = null;
            //then
            Assert.ThrowsException<ArgumentNullException>(() => server.AddAgent(nullClient));
        }
        [TestMethod]
        public void TestCommunicationServer_ThrowsExceptionOnNullAgentCommunicator()
        {
            //given
            Server server = new Server();
            server.Launch();
            SynchronousCommunicator gm1 = new SynchronousCommunicator();
            server.AddGameMaster(gm1);
            //when
            SynchronousCommunicator nullAgent = null;
            //then
            Assert.ThrowsException<ArgumentNullException>(() => server.AddAgent(nullAgent));
        }
        [TestMethod]
        public void TestCommunicationServer_RejectsDisconnectedAgentClient()
        {
            //given
            Server server = new Server();
            server.Launch();
            TcpClient client = new TcpClient();
            //when
            server.AddAgent(client);
            //then
            Assert.AreEqual(0, server.GetNumberOfConnectedPlayers());
        }
        [TestMethod]
        public void TestCommunicationServer_RejectsInvalidGameMasterCommunicator()
        {
            //given
            Server server = new Server();
            server.Launch();
            SynchronousCommunicator gm1 = new SynchronousCommunicator();
            SynchronousCommunicator partner = new SynchronousCommunicator();
            gm1.Connect(partner);
            //when
            server.AddGameMaster(gm1);
            //then
            Assert.IsFalse(server.IsGameMasterConnected());
        }
        [TestMethod]
        public void TestCommunicationServer_ThrowsExceptionOnNullGameMasterTcpClient()
        {
            //given
            Server server = new Server();
            server.Launch();
            //when
            TcpClient nullClient = null;
            //then
            Assert.ThrowsException<ArgumentNullException>(() => server.AddGameMaster(nullClient));
        }
        [TestMethod]
        public void TestCommunicationServer_ThrowsExceptionOnNullGameMasterCommunicator()
        {
            //given
            Server server = new Server();
            server.Launch();
            //when
            SynchronousCommunicator nullGM = null;
            //then
            Assert.ThrowsException<ArgumentNullException>(() => server.AddGameMaster(nullGM));
        }
        [TestMethod]
        public void TestCommunicationServer_RejectsDisconnectedGameMasterClient()
        {
            //given
            Server server = new Server();
            server.Launch();
            TcpClient client = new TcpClient();
            //when
            server.AddGameMaster(client);
            //then
            Assert.IsFalse(server.IsGameMasterConnected());
        }
        [TestMethod]
        public void TestCommunicationServer_IgnoresInvalidAgentMessages()
        {
            //given
            Server server = new Server();
            server.Launch();
            SynchronousCommunicator gm1 = new SynchronousCommunicator();
            SynchronousCommunicator agent1 = new SynchronousCommunicator();
            server.AddGameMaster(gm1);
            server.AddAgent(agent1);
            //when
            agent1.SendMessage(new Message() { payload = null });
            //then
            Assert.IsFalse(gm1.HasMessage());
        }
        [TestMethod]
        public void TestCommunicationServer_IgnoresInvalidGameMasterMessages()
        {
            //given
            Server server = new Server();
            server.Launch();
            SynchronousCommunicator gm1 = new SynchronousCommunicator();
            SynchronousCommunicator agent1 = new SynchronousCommunicator();
            server.AddGameMaster(gm1);
            server.AddAgent(agent1);
            //when
            gm1.SendMessage(new Message() { messageID = -1, agentID=server.GetConnectedPlayerIDs()[0] });
            //then
            Assert.IsFalse(agent1.HasMessage());
        }
        [TestMethod]
        public void TestCommunicationServer_StopsOnIngameGameMasterLeave()
        {
            //given
            Server server = new Server();
            server.Launch();
            SynchronousCommunicator gm1 = new SynchronousCommunicator();
            SynchronousCommunicator agent1 = new SynchronousCommunicator();
            server.AddGameMaster(gm1);
            server.AddAgent(agent1);
            gm1.SendMessage(new Message() { agentID = 1, payload = new StartGamePayload() });
            //when
            gm1.Dispose();
            //then
            Assert.AreEqual(ServerState.Stopped, server.GetState());
        }
        [TestMethod]
        public void TestCommunicationServer_IgnoresInvalidPlayerDestinationID()
        {
            //given
            Server server = new Server();
            server.Launch();
            SynchronousCommunicator gm1 = new SynchronousCommunicator();
            SynchronousCommunicator agent1 = new SynchronousCommunicator();
            server.AddGameMaster(gm1);
            server.AddAgent(agent1);

            //when
            gm1.SendMessage(new Message() { agentID = 2, payload = new JoinGameResponsePayload() });
            //then
            Assert.IsFalse(agent1.HasMessage());
        }
        [TestMethod]
        public void TestCommunicationServer_IgnoresNoRecipientGameMasterMessage()
        {
            //given
            Server server = new Server();
            server.Launch();
            SynchronousCommunicator gm1 = new SynchronousCommunicator();
            SynchronousCommunicator agent1 = new SynchronousCommunicator();
            server.AddGameMaster(gm1);
            server.AddAgent(agent1);
            //when
            gm1.SendMessage(new Message() { payload = new JoinGameResponsePayload() });
            //then
            Assert.AreEqual(ServerState.PreGame, server.GetState());
            Assert.IsFalse(agent1.HasMessage());
        }

        [TestMethod]
        public void TestCommunicationServer_SendsPlayerDisconnectionPayload()
        {
            //given
            Server server = new Server();
            server.Launch();
            SynchronousCommunicator gm1 = new SynchronousCommunicator();
            SynchronousCommunicator agent1 = new SynchronousCommunicator();
            server.AddGameMaster(gm1);
            server.AddAgent(agent1);
            int agent1ID = server.GetConnectedPlayerIDs()[0];

            //when
            gm1.SendMessage(new Message()
            {
                agentID = agent1ID,
                payload = new JoinGameResponsePayload()
                {
                    accepted = true
                }
            });
            agent1.Dispose();
            //then
            Assert.IsTrue(gm1.HasMessage());

            Message m = gm1.GetNextMessage();
            Assert.IsTrue(m.payload is PlayerDisconnectPayload);

            PlayerDisconnectPayload dcPayload = (PlayerDisconnectPayload)(m.payload);
            Assert.AreEqual(agent1ID, dcPayload.agentID);
        }
    }
}
