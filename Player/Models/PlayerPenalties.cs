using CommunicationUtils.Payloads;
using System;
using System.Collections.Generic;
using System.Text;

namespace Player.Models
{
    public class PlayerPenalties
    {
        public TimeSpan MovePenalty { get; set; }
        public TimeSpan InformationExchangePenalty { get; set; }
        public TimeSpan DiscoveryPenalty { get; set; }
        public TimeSpan PutPenalty { get; set; }
        public TimeSpan CheckPenalty { get; set; }
        public TimeSpan DestroyPenalty { get; set; }
        public TimeSpan PickPenalty { get; set; }
        public PlayerPenalties(int movePen, int askPen, int discPen, int putPen, int checkPen, int destroyPen, int pickPen)
        {
            int move, ask, disc, put, check, destroy, pick;

            move = movePen;
            ask = askPen;
            disc = discPen;
            put = putPen;
            check = checkPen;
            destroy = destroyPen;
            pick = pickPen;

            MovePenalty = new TimeSpan(0, 0, 0, 0, move);
            InformationExchangePenalty = new TimeSpan(0, 0, 0, 0, ask);
            DiscoveryPenalty = new TimeSpan(0, 0, 0, 0, disc);
            PutPenalty = new TimeSpan(0, 0, 0, 0, put);
            CheckPenalty = new TimeSpan(0, 0, 0, 0, check);
            DestroyPenalty = new TimeSpan(0, 0, 0, 0, destroy);
            PickPenalty = new TimeSpan(0, 0, 0, 0, pick);
        }
        public PlayerPenalties(Penalties penalties) : this(penalties.move, penalties.ask, penalties.discovery, penalties.putPiece, penalties.checkForSham, penalties.destroyPiece, penalties.pickup)
        { }
    }
}
