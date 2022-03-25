using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Payloads
{
    public class PlayerDisconnectPayload: IPayload
    {
        public int agentID { get; set; }

        public override string ToString()
        {
            return $"agentID {agentID}";
        }
    }
}
