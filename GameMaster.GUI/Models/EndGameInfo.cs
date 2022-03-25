using CommunicationUtils.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaster.GUI.Models
{
    public class EndGameInfo
    {
        public TeamColor winningTeam;
        public int redScore, blueScore;
        public TimeSpan time;

        public EndGameInfo(TeamColor winner, int redScore, int blueScore, TimeSpan time)
        {
            this.redScore = redScore;
            this.blueScore = blueScore;
            this.winningTeam = winner;
            this.time = time;
        }
    }
}
