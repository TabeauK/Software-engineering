using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Structures
{
    public class Vector2D
    {
        public int x { get; set; }
        public int y { get; set; }

        public override string ToString()
        {
            return $"[x {x}, y {y}]";
        }
    }
}
