using CommunicationUtils.Structures;
using GameMaster.Pieces;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaster.Fields
{
    public class TaskField : AbstractField
    {
        //Kolejka? Stos?
        internal List<AbstractPiece> pieces;

        public TaskField(int X, int Y) : base(X, Y)
        {
            pieces = new List<AbstractPiece>();
        }

        public override bool ContainsPieces() => pieces.Count > 0;

        public override PickupPieceErrorType? PickUp()
        {
            if (pieces.Count == 0)
                return PickupPieceErrorType.NoPieceToPickUp;
            AbstractPiece piece = pieces[0];
            pieces.Remove(piece);
            Player.Holding = piece;
            piece.Position = null;
            piece.Pickable = false;
            return null;
        }

        public override void PutPickablePiece(AbstractPiece piece)
        {
            pieces.Add(piece);
            piece.Pickable = true;
            piece.Position = this;
        }

        public override PlacePieceInfo Place(NormalPiece piece)
        {
            PutPickablePiece(piece);
            return PlacePieceInfo.TaskField;
        }

        public override PlacePieceInfo Place(ShamPiece piece)
        {
            PutPickablePiece(piece);
            return PlacePieceInfo.TaskField;
        }
    }
}
