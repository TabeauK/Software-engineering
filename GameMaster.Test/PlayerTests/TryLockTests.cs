using GameMaster.Pieces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaster.Test.PlayerTests
{
    [TestClass]
    public class TryLockTests
    {
        private Player _player;

        [TestInitialize]
        public void Setup()
        {
            _player = new Player(CommunicationUtils.Structures.TeamColor.Blue, false);
        }

        [TestMethod]
        public void TestTryLockNotBlocked()
        {
            //Given
            int timeSpan = 100;
            _player.lockedTill = null;

            //When
            bool res = _player.TryLock(timeSpan);

            //Then
            Assert.IsTrue(res == true);
            Assert.IsTrue(_player.lockedTill != null);
            Assert.IsTrue(_player.lockedTill.Value.CompareTo(DateTime.Now) >= 0);
        }

        [TestMethod]
        public void TestTryLockNotBlockedAnymore()
        {
            //Given
            int timeSpan = 100;
            _player.lockedTill = DateTime.Now - (new TimeSpan(100, 0, 0));

            //When
            bool res = _player.TryLock(timeSpan);

            //Then
            Assert.IsTrue(res == true);
            Assert.IsTrue(_player.lockedTill != null);
            Assert.IsTrue(_player.lockedTill.Value.CompareTo(DateTime.Now) >= 0);
        }

        [TestMethod]
        public void TestTryLockBlocked()
        {
            //Given
            int timeSpan = 1000;
            DateTime locked = DateTime.Now + (new TimeSpan(100, 0, 0));
            _player.lockedTill = locked;

            //When
            bool res = _player.TryLock(timeSpan);

            //Then
            Assert.IsTrue(res == false);
            Assert.IsTrue(_player.lockedTill == locked);
        }
    }
}
