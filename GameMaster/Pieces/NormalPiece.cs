using System;
using System.Collections.Generic;
using System.Text;
using CommunicationUtils.Structures;
using GameMaster.Fields;

namespace GameMaster.Pieces
{
    public class NormalPiece : AbstractPiece    
    {
        public NormalPiece()
        {
            IsSham = false;
        }

        public override PlacePieceInfo Place(AbstractField field)
        {
            return field.Place(this);
        }
    }
}
