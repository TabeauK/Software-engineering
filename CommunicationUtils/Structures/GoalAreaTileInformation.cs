using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Structures
{
    [JsonConverter(typeof(GoalAreaTileInformationJsonConverter))]
    public enum GoalAreaTileInformation { Unknown, Goal, NotGoal };
    public class GoalAreaTileInformationJsonConverter : JsonConverter
    {
        private static Type type = typeof(GoalAreaTileInformation);
        public override bool CanConvert(Type objectType)
        {
            return objectType == type;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch(((string)(reader.Value)).ToLower())
            {
                case "idk":
                    return GoalAreaTileInformation.Unknown;
                case "n":
                    return GoalAreaTileInformation.NotGoal;
                case "g":
                    return GoalAreaTileInformation.Goal;
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() == type)
            {
                switch((GoalAreaTileInformation)value)
                {
                    case GoalAreaTileInformation.Unknown:
                        writer.WriteValue("IDK");
                        break;
                    case GoalAreaTileInformation.NotGoal:
                        writer.WriteValue("N");
                        break;
                    case GoalAreaTileInformation.Goal:
                        writer.WriteValue("G");
                        break;
                }
            }  
        }
    }
}
