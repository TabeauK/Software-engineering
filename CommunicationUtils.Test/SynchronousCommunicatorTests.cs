using CommunicationUtils.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Test
{
    [TestClass]
    public class SynchronousCommunicatorTests
    {
        [TestMethod]
        public void TestSynchronousCommunicatorThrowsExceptionWhenUninitialized()
        {
            //given
            SynchronousCommunicator communicator = new SynchronousCommunicator();
            //when
            Message m = new Message();
            //then
            Assert.ThrowsException<InvalidOperationException>(() => communicator.SendMessage(m));
        }

        [TestMethod]
        public void TestSynchronousCommunicatorThrowsExceptionWhenPartnerNull()
        {
            //given
            SynchronousCommunicator communicator = new SynchronousCommunicator();
            //when
            SynchronousCommunicator partner = null;
            //then
            Assert.ThrowsException<ArgumentNullException>(() => communicator.Connect(partner));
        }

        [TestMethod]
        public void TestSynchronousCommunicatorDoesNotConnectWhenPartnerDisposed()
        {
            //given
            SynchronousCommunicator communicator = new SynchronousCommunicator();
            //when
            SynchronousCommunicator partner = new SynchronousCommunicator();
            partner.Dispose();
            communicator.Connect(partner);
            //then
            Assert.AreEqual(CommunicatorState.Disconnected, communicator.GetState());
        }

        [TestMethod]
        public void TestSynchronousCommunicatorThrowsExceptionWhenAlreadyConnected()
        {
            //given
            SynchronousCommunicator communicator = new SynchronousCommunicator();
            SynchronousCommunicator partner = new SynchronousCommunicator();
            //when
            communicator.Connect(partner);
            //then
            Assert.ThrowsException<InvalidOperationException>(() => communicator.Connect(partner));
            Assert.ThrowsException<InvalidOperationException>(() => partner.Connect(communicator));

        }

        [TestMethod]
        public void TestSynchronousCommunicatorThrowsExceptionWhenDisconnected()
        {
            //given
            SynchronousCommunicator communicator = new SynchronousCommunicator();
            SynchronousCommunicator partner = new SynchronousCommunicator();

            //when
            communicator.Connect(partner);
            partner.Dispose();

            //then
            Assert.ThrowsException<InvalidOperationException>(() => communicator.SendMessage(new Message()));
        }

        [TestMethod]
        public void TestSynchronousCommunicatorPassesMessageCorrectly()
        {
            SynchronousCommunicator communicator = new SynchronousCommunicator();
            SynchronousCommunicator partner = new SynchronousCommunicator();

            //when
            communicator.Connect(partner);
            communicator.SendMessage(new Message() { agentID = 2, payload = new PlacePieceRequestPayload() });
            Message m = partner.GetNextMessage();

            //then
            Assert.AreEqual(2, m.agentID);
            Assert.IsInstanceOfType(m.payload, typeof(PlacePieceRequestPayload));

        }

        [TestMethod]
        public void TestCommunicator_IgnoresWhenUninitialized()
        {
            //given
            SynchronousCommunicator communicator = new SynchronousCommunicator();
            //when
            Message m = new Message();
            //then
            communicator.TrySendMessage(m);
        }
    }
}
