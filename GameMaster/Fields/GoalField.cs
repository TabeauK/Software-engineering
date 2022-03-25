using CommunicationUtils.Structures;
using GameMaster.Pieces;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaster.Fields
{
    public class GoalField : AbstractField
    {
        public bool IsStillAGoalField;

        public TeamColor Team { get; set; }

        public GoalField(int X, int Y) : base(X, Y)
        {
            IsStillAGoalField = true;
        }

        public override PlacePieceInfo Place(NormalPiece piece)
        {
            if(IsStillAGoalField)
            {
                IsStillAGoalField = false;
                return PlacePieceInfo.NormalOnGoalField;
            }
            return PlacePieceInfo.NormalOnNonGoalField;
        }

        public override PlacePieceInfo Place(ShamPiece piece)
        {
            return PlacePieceInfo.ShamOnGoalArea;
        }
        
    }
}
