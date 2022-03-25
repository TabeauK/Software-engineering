using CommunicationUtils.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Payloads
{
    /// <summary>
    /// GM -> Agent, przekazuje zapytanie od innego Agenta
    /// </summary>
    public class InformationExchangePassRequestPayload: IPayload
    {
        public int askingID { get; set; }
        public bool leader { get; set; }
        [JsonProperty("teamID")]
        public TeamColor teamId { get; set; }

        public override string ToString()
        {
            return $"askingID {askingID}, leader {leader}, teamId {teamId.GetName()}";
        }
    }
}
