using CommunicationUtils.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Payloads
{
    /// <summary>
    /// Agent -> GM -> Agent, GM tylko sprawdza czy Agent nie byl zablokowany
    /// </summary>
    public class InformationExchangeResponsePayload: IPayload
    {
        public int respondToID { get; set; }
        public int?[] distances { get; set; }
        public GoalAreaTileInformation[] redTeamGoalAreaInformations { get; set; }
        public GoalAreaTileInformation[] blueTeamGoalAreaInformations { get; set; }

        public override string ToString()
        {
            return $"respondToID {respondToID}, distances {distances}," +
                $" redTeamGoalAreaInformations {redTeamGoalAreaInformations.GetName()}," +
                $" blueTeamGoalAreaInformations {blueTeamGoalAreaInformations.GetName()}";
        }
    }
}
