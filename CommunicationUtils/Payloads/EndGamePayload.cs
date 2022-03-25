using CommunicationUtils.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Payloads
{
    public class EndGamePayload:IPayload
    {
        public TeamColor winner { get; set; }

        public override string ToString()
        {
            return $"winner {winner.GetName()}";
        }
    }
}
