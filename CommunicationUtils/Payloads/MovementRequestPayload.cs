using CommunicationUtils.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Payloads
{
    public class MovementRequestPayload: IPayload
    {
        public Direction direction { get; set; }
        public override string ToString()
        {
            return $"direction {direction.GetName()}";
        }
    }
}
