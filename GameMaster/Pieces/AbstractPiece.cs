using System;
using System.Collections.Generic;
using System.Text;
using CommunicationUtils.Structures;
using GameMaster.Fields;

namespace GameMaster.Pieces
{
    public abstract class AbstractPiece
    {
        public AbstractField Position { get; set; }
        public bool Pickable { get; set; }

        public bool IsSham { get; protected set; }

        //visitor pattern
        public abstract PlacePieceInfo Place(AbstractField field);
    }
}
