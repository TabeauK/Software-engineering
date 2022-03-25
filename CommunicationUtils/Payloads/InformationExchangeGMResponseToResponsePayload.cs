using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Payloads
{
    public class InformationExchangeGMResponseToResponsePayload : IPayload
    {
        public bool succeeded { get; set; }
        public override string ToString()
        {
            return $"[succeeded {succeeded}]";
        }
    }
}
