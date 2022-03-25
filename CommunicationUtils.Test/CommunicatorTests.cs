using CommunicationUtils.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace CommunicationUtils.Test
{
    [TestClass]
    public class CommunicatorTests
    {
        [TestMethod]
        public void TestCommunicatorThrowsExceptionWhenUninitialized()
        {
            //given
            Communicator communicator = new Communicator();
            //when
            Message m = new Message();
            //then
            Assert.ThrowsException<InvalidOperationException>(()=>communicator.SendMessage(m));
        }

        [TestMethod]
        public void TestCommunicatorThrowsExceptionWhenIPNull()
        {
            //given
            Communicator communicator = new Communicator();
            //when
            string ip = null;
            int port = 1;
            //then
            Assert.ThrowsException<ArgumentNullException>(() => communicator.Connect(ip,port));
        }

        [TestMethod]
        public void TestCommunicatorThrowsExceptionWhenIPEmpty()
        {
            //given
            Communicator communicator = new Communicator();
            //when
            string ip = "";
            int port = 1;
            //then
            Assert.ThrowsException<ArgumentException>(() => communicator.Connect(ip, port));
        }

        [TestMethod]
        public void TestCommunicatorThrowsExceptionWhenInvalidPort()
        {
            //given
            Communicator communicator = new Communicator();
            //when
            string ip = "localhost";
            //then
            Assert.ThrowsException<ArgumentException>(() => communicator.Connect(ip, -1));
            Assert.ThrowsException<ArgumentException>(() => communicator.Connect(ip, int.MinValue));
            Assert.ThrowsException<ArgumentException>(() => communicator.Connect(ip, int.MaxValue));
            Assert.ThrowsException<ArgumentException>(() => communicator.Connect(ip, 65536));
        }

        [TestMethod]
        public void TestCommunicatorThrowsExceptionWhenClientNull()
        {
            //given
            Communicator communicator = new Communicator();
            //when
            TcpClient client = null;
            //then
            Assert.ThrowsException<ArgumentNullException>(() => communicator.Connect(client));
        }

        [TestMethod]
        public void TestCommunicatorThrowsExceptionWhenClientNotConnected()
        {
            //given
            Communicator communicator = new Communicator();
            //when
            TcpClient client = new TcpClient();
            //then
            Assert.ThrowsException<ArgumentException>(() => communicator.Connect(client));
        }

        [TestMethod]
        public void TestCommunicator_IgnoresWhenUninitialized()
        {
            //given
            Communicator communicator = new Communicator();
            //when
            Message m = new Message();
            //then
            communicator.TrySendMessage(m);
        }
    }
}
