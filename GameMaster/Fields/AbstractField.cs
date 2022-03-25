using CommunicationUtils.Structures;
using System;
using System.Collections.Generic;
using System.Text;
using GameMaster.Pieces;
using System.Linq;

namespace GameMaster.Fields
{
    public interface IGameMasterField
    {
        int Distance(AbstractField field2);
        void PutPickablePiece(AbstractPiece piece);
        Vector2D ToVector2D();
        bool MoveHere(Player player);
        int? DistanceToClosestPiece(IEnumerable<AbstractPiece> pieces);
        //bool HasPlayer();
    }


    public abstract class AbstractField : IGameMasterField
    {
        protected readonly int x;
        protected readonly int y;
        public Player Player { get; internal set; }

        public virtual PickupPieceErrorType? PickUp() => PickupPieceErrorType.NoPieceToPickUp;

        public abstract PlacePieceInfo Place(NormalPiece piece);

        public abstract PlacePieceInfo Place(ShamPiece piece);

        public virtual bool ContainsPieces() => false;

        public AbstractField(int X, int Y)
        {
            x = X;
            y = Y;
        }

        /// <summary>
        /// Ustawia wlasciwosc Player na null
        /// </summary>
        /// <param name="player"></param>
        public void Leave()
        {
            Player = null;
        }

        /// <summary>
        /// Jesli na polu nie stoi inny gracz, ustawia wlasciwosc Player na player i zwraca true, w przeciwnym wypadku zwraca false.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool MoveHere(Player player)
        {
            if(Player == null)
            {
                Player = player;
                player.Position = this;
                return true;
            }
            return false;
        }

        public Vector2D ToVector2D()
        {
            return new Vector2D
            {
                x = x,
                y = y
            };
        }
        
        /// <summary>
        /// Dodaje piece do listy kawalkow pola
        /// </summary>
        /// <param name="piece"></param>
        public virtual void PutPickablePiece(AbstractPiece piece)
        {
            
        }
        
        /// <summary>
        /// Zwraca dystans miedzy dwoma polami w metryce miejskiej
        /// </summary>
        /// <param name="field2"></param>
        /// <returns></returns>
        public int Distance(AbstractField field2)
        {
            return Math.Abs(field2.x - this.x) + Math.Abs(field2.y - this.y);
        }

        public int? DistanceToClosestPiece(IEnumerable<AbstractPiece> pieces)
        {
            int? min = null;
            foreach (var piece in pieces.Where(p => p.Pickable == true))
            {
                int dis = this.Distance(piece.Position);
                if (!min.HasValue || dis < min.Value)
                {
                    min = dis;
                }
            }
            return min;
        }

        public bool HasPlayer()
        {
            return Player != null;
        }
    }
}
