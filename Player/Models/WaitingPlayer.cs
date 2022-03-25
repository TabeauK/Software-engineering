using System;
using System.Collections.Generic;
using System.Text;

namespace Player.Models
{
    public class WaitingPlayer
    {
        public int Id { get; set; }
        public bool IsLeader { get; set; }
        public WaitingPlayer(int id, bool isLeader)
        {
            Id = id;
            IsLeader = isLeader;
        }
    }
}
