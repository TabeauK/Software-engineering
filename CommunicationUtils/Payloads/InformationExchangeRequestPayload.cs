using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Payloads
{
    /// <summary>
    /// Agent -> GM, zapytanie innego Agenta o info
    /// </summary>
    public class InformationExchangeRequestPayload: IPayload
    {
        public int askedAgentID { get; set; }

        public override string ToString()
        {
            return $"askedAgentID {askedAgentID}";
        }
    }
}
