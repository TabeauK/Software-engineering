using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CommunicationUtils
{
    /*
     * Klasa MessageStub wynika z faktu, że nie ma dobrego sposobu
     * na pominięcie deserializacji właściwości payload zachowując
     * jej serializację.
     * MessageStub służy do deserializacji wspólnych właściwości,
     * a końcowy obiekt typu Message otrzymuje payload w osobnej
     * operacji, wszystko dzieje się bez wiedzy świata zewnętrznego
     */
    internal class MessageStub
    {
        public int messageID { get; set; }
        public int? agentID { get; set; }

        internal Message toMessage()
        {
            Message m = new Message();
            m.messageID = messageID;
            m.agentID = agentID;
            return m;
        }
    }

    public class Message
    {
        public int messageID { get; set; }
        public int? agentID { get; set; }
        public IPayload payload { get; set; }

        public override string ToString()
        {
            return $"Id {messageID}, agentID {agentID}, payload [{payload?.ToString()}]";
        }
    }

    public interface IPayload
    {

    }
}
