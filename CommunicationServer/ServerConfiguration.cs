using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationServer
{
    internal class ServerConfiguration
    {
        [JsonProperty("portAgentow")]
        public int? AgentListenerPort { get; set; }
        [JsonProperty("portGM")]
        public int? GameMasterListenerPort { get; set; }
        [JsonProperty("verbose")]
        public bool Verbose { get; set; }
    }
}
