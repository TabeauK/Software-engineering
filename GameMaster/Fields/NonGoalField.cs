using CommunicationUtils.Structures;
using GameMaster.Pieces;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaster.Fields
{
    public class NonGoalField : AbstractField
    {
        public NonGoalField(int X, int Y) : base(X, Y)
        {
        }

        public override bool ContainsPieces()
        {
            return false;
        }

        public override PlacePieceInfo Place(NormalPiece piece)
        {
            return PlacePieceInfo.NormalOnNonGoalField;
        }

        public override PlacePieceInfo Place(ShamPiece piece)
        {
            return PlacePieceInfo.ShamOnGoalArea;
        }
    }
}
