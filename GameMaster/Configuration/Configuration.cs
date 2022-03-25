using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaster
{
    public class Configuration
    {
        public string CSIP { get; set; }
        public int CSPort { get; set; }
        public int MovePenalty { get; set; }
        public int DestroyPenalty { get; set; }
        public int DiscoveryPenalty { get; set; }
        public int PutPenalty { get; set; }
        public int CheckPenalty { get; set; }
        public int InformationExchangePenalty { get; set; } //response
        public int PickUpPenalty { get; set; }
        public int RequestInformationExchangePenalty { get; set; } //ask
        
        public bool Verbose { get; set; }

        /// <summary>
        /// Szerokosc planszy
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Wysokosc planszy
        /// </summary>
        public int Y { get; set; }

        public int NumberOfGoals { get; set;  }
        public int NumberOfPieces { get; set; }
        public int NumberOfPlayers { get; set; }
        public int GoalAreaHeight { get; set; }
        public float ShamPieceProbability { get; set; }
    }
}
