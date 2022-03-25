using CommunicationUtils;
using CommunicationUtils.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CommunicationServer.Test
{
    [TestClass]
    public class CommunicationServerConfigTests
    {
        [TestMethod]
        public void TestCommunicationServerCLI3Parameters()
        {
            //given
            Server server = new Server();
            //when
            string[] parameters = new string[] { "5000", "5001", "true" };
            //then
            Assert.IsTrue(server.Launch(parameters));
        }

        [TestMethod]
        public void TestCommunicationServerCLI4Parameters()
        {
            //given
            Server server = new Server();
            //when
            string[] parameters = new string[] { "something", "5002", "5003", "true" };
            //then
            Assert.IsTrue(server.Launch(parameters));
        }

        [TestMethod]
        public void TestCommunicationServerFileConfiguration()
        {
            //given
            Server server = new Server();
            string json = "{\"portAgentow\": 5005,\"portGM\": 5004, \"verbose\": true}";
            string filepath = "config.json";
            //when
            using (FileStream fs = File.OpenWrite(filepath))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(json);
                }
            }
            //then
            Assert.IsTrue(server.Launch(filepath));
            File.Delete(filepath);
        }

        [TestMethod]
        public void TestCommunicationServerEqualPorts()
        {
            //given
            Server server = new Server();
            //when
            string[] parameters = new string[] { "something", "5006", "5006", "false" };
            //then
            Assert.IsFalse(server.Launch(parameters));
        }

        [TestMethod]
        public void TestCommunicationServerOutOfRangePorts()
        {
            //given
            Server server = new Server();
            //when
            string[] parameters = new string[] { "something", "-1", "5007", "false" };
            //then
            Assert.IsFalse(server.Launch(parameters));
        }

        [TestMethod]
        public void TestCommunicationServerOutOfRangePorts2()
        {
            //given
            Server server = new Server();
            //when
            string[] parameters = new string[] { "something", "5008", "65536", "false" };
            //then
            Assert.IsFalse(server.Launch(parameters));
        }

        [TestMethod]
        public void TestCommunicationServerInvalidPortString()
        {
            //given
            Server server = new Server();
            //when
            string[] parameters = new string[] { "something", "s", "5009", "false" };
            //then
            Assert.IsFalse(server.Launch(parameters));
        }

        [TestMethod]
        public void TestCommunicationServerNullPortString()
        {
            //given
            Server server = new Server();
            //when
            string[] parameters = new string[] { "5010", null, "false" };
            //then
            Assert.IsFalse(server.Launch(parameters));
        }

        [TestMethod]
        public void TestCommunicationServerInvalidJson()
        {
            //given
            Server server = new Server();
            string json = "{\"portAgentowERROR\": 5011,\"portGM\": 5012}";
            string filepath = "config2.json";
            //when
            using (FileStream fs = File.OpenWrite(filepath))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(json);
                }
            }
            //then
            Assert.IsFalse(server.Launch(filepath));
            File.Delete(filepath);
        }

        [TestMethod]
        public void TestCommunicationServerCliInvalidVerbosity()
        {
            //given
            Server server = new Server();
            //when
            string[] parameters = new string[] { "something", "5006", "5006", "yes" };
            //then
            Assert.IsFalse(server.Launch(parameters));
        }

        [TestMethod]
        public void TestCommunicationServerCliNullVerbosity()
        {
            //given
            Server server = new Server();
            //when
            string[] parameters = new string[] { "something", "5006", "5006", null };
            //then
            Assert.IsFalse(server.Launch(parameters));
        }

        [TestMethod]
        public void TestCommunicationServerFileConfigurationInvalidVerbosity()
        {
            //given
            Server server = new Server();
            string json = "{\"portAgentow\": 5005,\"portGM\": 5004, \"verbose\": indeed}";
            string filepath = "config3.json";
            //when
            using (FileStream fs = File.OpenWrite(filepath))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(json);
                }
            }
            //then
            Assert.IsFalse(server.Launch(filepath));
            File.Delete(filepath);
        }
    }
}
