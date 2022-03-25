using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Structures
{
    [JsonConverter(typeof(DirectionJsonConverter))]
    public enum Direction { North, South, West, East};
    public class DirectionJsonConverter : JsonConverter
    {
        private static Type type = typeof(Direction);
        public override bool CanConvert(Type objectType)
        {
            return objectType == type;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (((string)(reader.Value)).ToLower())
            {
                case "n":
                    return Direction.North;
                case "e":
                    return Direction.East;
                case "w":
                    return Direction.West;
                case "s":
                    return Direction.South;
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() == type)
            {
                switch ((Direction)value)
                {
                    case Direction.North:
                        writer.WriteValue("N");
                        break;
                    case Direction.East:
                        writer.WriteValue("E");
                        break;
                    case Direction.West:
                        writer.WriteValue("W");
                        break;
                    case Direction.South:
                        writer.WriteValue("S");
                        break;
                }
            }
        }
    }
}
