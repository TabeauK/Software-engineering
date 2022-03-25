using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Structures
{
    [JsonConverter(typeof(PlacePieceErrorTypeJsonConverter))]
    public enum PlacePieceErrorType { NoPieceHeld, CannotPlaceThere, Other};
    public class PlacePieceErrorTypeJsonConverter : JsonConverter
    {
        private static Type type = typeof(PlacePieceErrorType);
        public override bool CanConvert(Type objectType)
        {
            return objectType == type;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (((string)(reader.Value)).ToLower())
            {
                case "agentnotholding":
                    return PlacePieceErrorType.NoPieceHeld;
                case "cannotputthere":
                    return PlacePieceErrorType.CannotPlaceThere;
                case "other":
                    return PlacePieceErrorType.Other;
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() == type)
            {
                switch ((PlacePieceErrorType)value)
                {
                    case PlacePieceErrorType.NoPieceHeld:
                        writer.WriteValue("AgentNotHolding");
                        break;
                    case PlacePieceErrorType.CannotPlaceThere:
                        writer.WriteValue("CannotPutThere");
                        break;
                    case PlacePieceErrorType.Other:
                        writer.WriteValue("Other");
                        break;
                }
            }
        }
    }
}
