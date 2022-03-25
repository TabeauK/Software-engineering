using CommunicationUtils.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace CommunicationUtils.Test
{
    [TestClass]
    public class MessageParserTestClass
    {
        [TestMethod]
        public void TestMessageParserThrowsNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() => MessageParser.ParseJson(null));
        }

        [TestMethod]
        public void TestMessageParserNoMessageIdZero()
        {
            //given
            string json = "{messageID:0}";
            //when
            Message m = MessageParser.ParseJson(json);
            //then
            Assert.IsNull(m.payload);
        }

        [TestMethod]
        public void TestMessageParserAttachesPayload()
        {
            //given
            string json = "{messageID:5, payload:{askedAgentID: 2}}";
            //when
            Message m = MessageParser.ParseJson(json);
            //then
            Assert.IsInstanceOfType(m.payload, typeof(InformationExchangeRequestPayload));
            InformationExchangeRequestPayload payload = (InformationExchangeRequestPayload)m.payload;
            Assert.AreEqual(payload.askedAgentID, 2);
        }

        [TestMethod]
        public void TestMessageParserAttachesMessageID()
        {
            //given
            Message m = new Message();
            m.payload = new TestPieceRequestPayload();
            //when
            String json = MessageParser.ParseMessage(m);
            //then
            Assert.IsTrue(m.messageID == 1);
        }

        [TestMethod]
        public void TestMessageParserFillsBasicFields()
        {

            //given
            string string1 = "2020-03-19T13:16:54.097966+01:00";
            string string2 = "2020-03-19T13:18:54.097966+01:00";
            DateTime date1 = DateTime.Parse(string1);
            DateTime date2 = DateTime.Parse(string2);
            string json = "{messageID:2,timeSent:\""+string1+"\",waitUntil:\""+string2+"\",agentID:5}";
            //when
            Message m = MessageParser.ParseJson(json);
            //then
            Assert.AreEqual(m.agentID, 5);
            Assert.AreEqual(m.messageID, 2);
        }
    }
}
