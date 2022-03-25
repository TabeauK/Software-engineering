using CommunicationUtils.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Payloads
{
    public class JoinGameRequestPayload: IPayload
    {
        [JsonProperty("teamID")]
        public TeamColor teamId { get; set; }

        public override string ToString()
        {
            return $"teamId {teamId.GetName()}";
        }
    }
}
