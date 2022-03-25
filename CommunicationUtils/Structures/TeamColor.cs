using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace CommunicationUtils.Structures
{
    [JsonConverter(typeof(TeamColorJsonConverter))]
    public enum TeamColor { Blue, Red };
    public class TeamColorJsonConverter : JsonConverter
    {
        private static Type type = typeof(TeamColor);
        public override bool CanConvert(Type objectType)
        {
            return objectType == type;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Enum.Parse(type, (string)reader.Value, true);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is TeamColor)
                writer.WriteValue(Enum.GetName(type, value).ToLower());
        }
    }
}
