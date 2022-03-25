using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Payloads
{
    public class JoinGameResponsePayload: IPayload
    {
        public bool accepted { get; set; }
        public int agentID { get; set; }

        public override string ToString()
        {
            return $"accepted {accepted}, agentID {agentID}";
        }
    }
}
