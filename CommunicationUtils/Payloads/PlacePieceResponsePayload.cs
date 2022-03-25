using System;
using System.Collections.Generic;
using System.Text;
using CommunicationUtils.Structures;
using Newtonsoft.Json;

namespace CommunicationUtils.Payloads
{
    public class PlacePieceResponsePayload: IPayload
    {
        [JsonProperty("PlacePieceInfo")]
        public PlacePieceInfo Info { get; set; }

        public override string ToString()
        {
            return $"PlacePieceInfo {Info.GetName()}";
        }
    }
    
}
