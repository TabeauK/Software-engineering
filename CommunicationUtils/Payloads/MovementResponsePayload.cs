using CommunicationUtils.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Payloads
{
    public class MovementResponsePayload: IPayload
    {
        public bool madeMove { get; set; }
        public Vector2D currentPosition { get; set; }
        public int? closestPiece { get; set; }

        public override string ToString()
        {
            return $"madeMove {madeMove}, currentPosition {currentPosition.ToString()}, closestPiece {closestPiece}";
        }
    }
}
