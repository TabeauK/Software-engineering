using CommunicationUtils.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace Player.Models
{
    public class PlayerConfiguration
    {
        public string CSIP { get; set; }
        public int CSPort { get; set; }
        public TeamColor TeamID { get; set; }
        public bool Verbose { get; set; }
    }
}
