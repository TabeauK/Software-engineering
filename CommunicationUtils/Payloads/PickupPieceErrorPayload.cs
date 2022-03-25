using CommunicationUtils.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Payloads
{
    public class PickupPieceErrorPayload: IPayload
    {
        public PickupPieceErrorType errorSubtype { get; set; }

        public override string ToString()
        {
            return $"errorSubtype {errorSubtype.GetName()}";
        }
    }
}
