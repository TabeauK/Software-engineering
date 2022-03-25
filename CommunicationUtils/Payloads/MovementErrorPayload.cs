using CommunicationUtils.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Payloads
{
    public class MovementErrorPayload: IPayload
    {
        public Vector2D position { get; set; }

        public override string ToString()
        {
            return $"position {position.ToString()}";
        }
    }
}
