using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Payloads
{
    public class TestPieceResponsePayload: IPayload
    {
        public bool sham { get; set; }

        public override string ToString()
        {
            return $"sham {sham}";
        }
    }
}
