using System;
using System.Collections.Generic;
using System.Text;
using CommunicationUtils.Structures;
using GameMaster.Fields;

namespace GameMaster.Pieces
{
    public class ShamPiece : AbstractPiece
    {
        public ShamPiece()
        {
            IsSham = true;
        }

        public override PlacePieceInfo Place(AbstractField field)
        {
            return field.Place(this);
        }
    }
}
