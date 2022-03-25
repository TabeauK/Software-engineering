using GameMaster.Pieces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaster.Test.PlayerTests
{
    [TestClass]
    public class DestroyTests
    {
        private Player _player;

        [TestInitialize]
        public void Setup()
        {
            _player = new Player(CommunicationUtils.Structures.TeamColor.Blue, false);
        }

        [TestMethod]
        public void TestDestroyNormal()
        {
            //Given
            NormalPiece piece = new NormalPiece();
            _player.Holding = piece;
            piece.Pickable = false;
            piece.Position = null;

            //When
            bool res = _player.Destroy();

            //Then
            Assert.IsTrue(res == true);
            Assert.IsTrue(_player.Holding == null);
        }

        [TestMethod]
        public void TestDestroySham()
        {
            //Given
            ShamPiece piece = new ShamPiece();
            _player.Holding = piece;
            piece.Pickable = false;
            piece.Position = null;

            //When
            bool res = _player.Destroy();

            //Then
            Assert.IsTrue(res == true);
            Assert.IsTrue(_player.Holding == null);
        }

        [TestMethod]
        public void TestDestroyNoPiece()
        {
            //Given
            _player.Holding = null;

            //When
            bool res = _player.Destroy();

            //Then
            Assert.IsTrue(res == false);
            Assert.IsTrue(_player.Holding == null);
        }
    }
}
