using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Structures
{
    [JsonConverter(typeof(PlacePieceInfoJsonConverter))]
    public enum PlacePieceInfo
    {
        NormalOnGoalField,
        NormalOnNonGoalField,
        TaskField,
        ShamOnGoalArea
    }
    public class PlacePieceInfoJsonConverter : JsonConverter
    {
        private static Type type = typeof(PlacePieceInfo);
        public override bool CanConvert(Type objectType)
        {
            return objectType == type;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (((string)(reader.Value)).ToLower())
            {
                case "normalongoalfield":
                    return PlacePieceInfo.NormalOnGoalField;
                case "normalonnongoalfield":
                    return PlacePieceInfo.NormalOnNonGoalField;
                case "taskfield":
                    return PlacePieceInfo.TaskField;
                case "shamongoalarea":
                    return PlacePieceInfo.ShamOnGoalArea;
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() == type)
            {
                switch ((PlacePieceInfo)value)
                {
                    case PlacePieceInfo.NormalOnGoalField:
                        writer.WriteValue("NormalOnGoalField");
                        break;
                    case PlacePieceInfo.NormalOnNonGoalField:
                        writer.WriteValue("NormalOnNonGoalField");
                        break;
                    case PlacePieceInfo.TaskField:
                        writer.WriteValue("TaskField");
                        break;
                    case PlacePieceInfo.ShamOnGoalArea:
                        writer.WriteValue("ShamOnGoalArea");
                        break;
                }
            }
        }
    }
    
}
