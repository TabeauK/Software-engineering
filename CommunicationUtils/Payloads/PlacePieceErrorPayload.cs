using CommunicationUtils.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Payloads
{ 
    //Błędne położenie kawałka
    public class PlacePieceErrorPayload: IPayload
    {
        public PlacePieceErrorType errorSubtype { get; set; }

        public override string ToString()
        {
            return $"errorSubtype {errorSubtype.GetName()}";
        }
    }
}
