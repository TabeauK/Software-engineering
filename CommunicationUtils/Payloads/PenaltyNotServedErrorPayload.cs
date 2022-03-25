using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Payloads
{
    public class PenaltyNotServedErrorPayload: IPayload
    {
        public int waitFor { get; set; }

        public override string ToString()
        {
            return $"waitFor {waitFor}";
        }
    }
}
