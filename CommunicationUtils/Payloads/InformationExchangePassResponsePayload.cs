using CommunicationUtils.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Payloads
{

    public class InformationExchangePassResponsePayload : IPayload
    {
        public int respondingID { get; set; }
        public int[] distances { get; set; }
        public GoalAreaTileInformation[] redTeamGoalAreaInformations { get; set; }
        public GoalAreaTileInformation[] blueTeamGoalAreaInformations { get; set; }

        public override string ToString()
        {
            return $"respondingID {respondingID}, distances {distances}," +
                $" redTeamGoalAreaInformations {redTeamGoalAreaInformations.GetName()}," +
                $" blueTeamGoalAreaInformations {blueTeamGoalAreaInformations.GetName()}";
        }
    }
}
