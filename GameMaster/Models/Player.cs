using CommunicationUtils.Structures;
using System;
using System.Collections.Generic;
using System.Text;
using GameMaster.Fields;
using GameMaster.Pieces;

namespace GameMaster
{

    public class Player
    {
        public TeamColor Team { get; private set; }
        public bool IsLeader { get; set; }
        public AbstractPiece Holding { get; set; }
        public AbstractField Position { get; set; }
        public DateTime? lockedTill { get; set; } //DateTime?

        public Player(TeamColor team, bool isleader)
        {
            Team = team;
            IsLeader = isleader;
        }

        /// <summary>
        /// Sprawdza czy Agent byl zablokowany, jesli nie to blokuje na timespan czasu. 
        /// Zwraca True jesli nie byl zablokowany.
        /// </summary>
        /// <param name="miliseconds"></param>
        /// <returns>
        /// </returns>
        public bool TryLock(int miliseconds)
        {
            if (lockedTill.HasValue && DateTime.Compare(DateTime.Now, lockedTill.Value) < 1)
                return false;
            lockedTill = DateTime.Now.AddMilliseconds(miliseconds);
            return true;
        }

        /// <summary>
        /// Podnosi kawalek z pola na ktorym stoi Agent. Zwraca null jesli podniosl, w przeciwnym razie odpowiedni blad.
        /// </summary>
        /// <returns></returns>
        public PickupPieceErrorType? PickUp()
        {
            if(Holding != null)
            {
                return PickupPieceErrorType.AlreadyCarriesPiece;
            }
            return Position.PickUp();
        }

        /// <summary>
        /// Przenosi gracza na pole field. Zwraca true jesli ruch sie udal, w przeciwnym razie false.
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public bool Move(IGameMasterField field)
        {
            var pos = Position;
            if(field.MoveHere(this))
            {
                pos.Leave();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Niszczy kawalek gracza. Zwraca true jesli gracz mial kawalek, w przeciwnym razie false.
        /// </summary>
        /// <returns></returns>
        public bool Destroy()
        {
            if (Holding == null)
                return false;
            Holding = null;
            return true;
        }

        /// <summary>
        /// Stawia kawalek na polu, na ktorym stoi Agent. Zwraca odpowiednie PlacePieceInfo jesli gracz ma kawalek, w przeciwnym razie null.
        /// </summary>
        /// <returns></returns>
        public PlacePieceInfo? Place()
        {
            if (null != Holding)
            {
                var piece = Holding;
                Holding = null;
                return piece.Place(Position);
            }
            return null;
        }

        /// <summary>
        /// Sprawdza kawalek trzymany przez Agenta. Zwraca true jesli kawalek jest shamem, false jesli nie jest, null jesli Agent nie ma kawalka.
        /// </summary>
        /// <returns></returns>
        public bool? ShamTest()
        {
            bool? res = Holding?.IsSham;
            if(res.HasValue && res == true)
            {
                this.Holding = null;
            }
            return res;
        }

        
        
    }
}
