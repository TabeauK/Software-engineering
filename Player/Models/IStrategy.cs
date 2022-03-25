using System;
using System.Collections.Generic;
using System.Text;

namespace Player.Models
{
    public interface IStrategy
    {
        void MakeDecision();
        void Init(Player player);
    }
}
