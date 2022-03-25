using Player.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Player.Test.Mocks
{
    public class EmptyStrategyMock : IStrategy
    {
        public void MakeDecision()
        {
            throw new System.NotImplementedException();
        }
        public void Init(Player player)
        {
            return;
        }
    }
}
