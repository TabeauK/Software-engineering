using System;
using System.Collections.Generic;
using System.Text;
using CommunicationUtils.Structures;
using Player.Models;

namespace Player.Models
{
    //public enum GoalInfo
    //{
    //    IDK, // gracz nic nie wie o polu
    //    DiscoveredNotGoal,
    //    DiscoveredGoal
    //}
    public class Field
    {
        public GoalAreaTileInformation GoalInfo { get; set; }  
        public bool PlayerInfo { get; set; } // czy inny gracz stoi na tym polu?
        public int? DistToPiece { get; set; }
    }
}
