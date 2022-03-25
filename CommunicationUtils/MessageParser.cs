using CommunicationUtils.Payloads;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils
{
    public static class MessageParser
    {
        //wiele wiadomości naraz mogło powodować że dwa razy się mapa przestawała być nullem ale nie była zainicjowana
        private static readonly object threadSync = new object();
        public static Message ParseJson(string json)
        {
            if (json == null)
                throw new ArgumentNullException("Json string cannot be null");
            MessageStub ms = JsonConvert.DeserializeObject<MessageStub>(json);
            Type type = GetPayloadType(ms.messageID);
            Message m = ms.toMessage();
            IPayload payload = null;
            if(type!=null)
            {
                JObject jObject = JObject.Parse(json);
                JToken jPayload = jObject["payload"];
                if (jPayload != null)
                    payload = (IPayload)jPayload.ToObject(type);
                else
                    payload = (IPayload)Activator.CreateInstance(type);
            }
            m.payload = payload;
            return m;
        }

        public static string ParseMessage(Message message)
        {
            if (message.payload != null)
                message.messageID = GetPayloadID(message.payload);
            return JsonConvert.SerializeObject(message);
        }

        private static Type GetPayloadType(int id)
        {
            lock(threadSync)
            {
                if (MessageIDToTypeMap == null)
                    InitializeMaps();
            }
            
            Type t = null;
            if (!MessageIDToTypeMap.TryGetValue(id, out t))
                return null;
            return t;
        }

        private static int GetPayloadID(IPayload payload)
        {
            lock (threadSync)
            {
                if (MessageTypeToIDMap == null)
                    InitializeMaps();
            }
            int i;
            if(!MessageTypeToIDMap.TryGetValue(payload.GetType(), out i))
            {
                throw new InvalidOperationException("Payload of type " + payload.GetType().ToString() + " has no message ID defined");
            }
            return i;
        }

        private static Dictionary<int, Type> MessageIDToTypeMap;
        private static Dictionary<Type, int> MessageTypeToIDMap;

        private static void InitializeMaps()
        {
            MessageIDToTypeMap = new Dictionary<int, Type>();
            MessageIDToTypeMap.Add(1, typeof(TestPieceRequestPayload));
            MessageIDToTypeMap.Add(2, typeof(DestroyPieceRequestPayload));
            MessageIDToTypeMap.Add(3, typeof(DiscoverRequestPayload));
            MessageIDToTypeMap.Add(4, typeof(InformationExchangeResponsePayload));
            MessageIDToTypeMap.Add(5, typeof(InformationExchangeRequestPayload));
            MessageIDToTypeMap.Add(6, typeof(JoinGameRequestPayload));
            MessageIDToTypeMap.Add(7, typeof(MovementRequestPayload));
            MessageIDToTypeMap.Add(8, typeof(PickupPieceRequestPayload));
            MessageIDToTypeMap.Add(9, typeof(PlacePieceRequestPayload));

            MessageIDToTypeMap.Add(101, typeof(TestPieceResponsePayload));
            MessageIDToTypeMap.Add(102, typeof(DestroyPieceResponsePayload));
            MessageIDToTypeMap.Add(103, typeof(DiscoverResponsePayload));
            MessageIDToTypeMap.Add(104, typeof(EndGamePayload));
            MessageIDToTypeMap.Add(105, typeof(StartGamePayload));
            MessageIDToTypeMap.Add(106, typeof(InformationExchangePassRequestPayload));
            MessageIDToTypeMap.Add(107, typeof(JoinGameResponsePayload));
            MessageIDToTypeMap.Add(108, typeof(MovementResponsePayload));
            MessageIDToTypeMap.Add(109, typeof(PickupPieceResponsePayload));
            MessageIDToTypeMap.Add(110, typeof(PlacePieceResponsePayload));
            MessageIDToTypeMap.Add(111, typeof(InformationExchangePassResponsePayload));
            MessageIDToTypeMap.Add(112, typeof(InformationExchangeGMResponseToResponsePayload));
            MessageIDToTypeMap.Add(113, typeof(InformationExchangeGMResponseToRequestPayload));

            MessageIDToTypeMap.Add(901, typeof(MovementErrorPayload));
            MessageIDToTypeMap.Add(902, typeof(PickupPieceErrorPayload));
            MessageIDToTypeMap.Add(903, typeof(PlacePieceErrorPayload));
            MessageIDToTypeMap.Add(904, typeof(PenaltyNotServedErrorPayload));
            MessageIDToTypeMap.Add(905, typeof(UndefinedErrorPayload));
            MessageIDToTypeMap.Add(906, typeof(PlayerDisconnectPayload));

            MessageTypeToIDMap = new Dictionary<Type, int>();
            foreach (var v in MessageIDToTypeMap)
            {
                MessageTypeToIDMap.Add(v.Value, v.Key);
            }
        }
    }
}
