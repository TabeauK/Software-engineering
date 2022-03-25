using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Structures
{
    [JsonConverter(typeof(PickupPieceErrorTypeJsonConverter))]
    public enum PickupPieceErrorType { NoPieceToPickUp, AlreadyCarriesPiece };
    public class PickupPieceErrorTypeJsonConverter : JsonConverter
    {
        private static Type type = typeof(PickupPieceErrorType);
        public override bool CanConvert(Type objectType)
        {
            return objectType == type;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (((string)(reader.Value)).ToLower())
            {
                case "nothingthere":
                    return PickupPieceErrorType.NoPieceToPickUp;
                case "other":
                    return PickupPieceErrorType.AlreadyCarriesPiece;
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() == type)
            {
                switch ((PickupPieceErrorType)value)
                {
                    case PickupPieceErrorType.AlreadyCarriesPiece:
                        writer.WriteValue("Other");
                        break;
                    case PickupPieceErrorType.NoPieceToPickUp:
                        writer.WriteValue("NothingThere");
                        break;
                }
            }
        }
    }
}
