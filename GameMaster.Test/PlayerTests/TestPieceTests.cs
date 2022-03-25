using GameMaster.Pieces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaster.Test.PlayerTests
{
    [TestClass]
    public class TestPieceTests
    {
        private Player _player;

        [TestInitialize]
        public void Setup()
        {
            _player = new Player(CommunicationUtils.Structures.TeamColor.Blue, false);
        }

        [TestMethod]
        public void TestTestNormal()
        {
            //Given
            NormalPiece piece = new NormalPiece();
            _player.Holding = piece;
            piece.Pickable = false;
            piece.Position = null;

            //When
            bool? res = _player.ShamTest();

            //Then
            Assert.IsTrue(res.HasValue);
            Assert.IsTrue(res.Value == false);
            Assert.IsTrue(_player.Holding == piece);
            Assert.IsTrue(piece.Pickable == false);
            Assert.IsTrue(piece.Position == null);
        }

        [TestMethod]
        public void TestTestSham()
        {
            //Given
            ShamPiece piece = new ShamPiece();
            _player.Holding = piece;
            piece.Pickable = false;
            piece.Position = null;

            //When
            bool? res = _player.ShamTest();

            //Then
            Assert.IsTrue(res.HasValue);
            Assert.IsTrue(res.Value == true);
            Assert.IsTrue(_player.Holding == null);
        }

        [TestMethod]
        public void TestTestNoPiece()
        {
            //Given
            _player.Holding = null;

            //When
            bool? res = _player.ShamTest();

            //Then
            Assert.IsTrue(!res.HasValue);
            Assert.IsTrue(res == null);
            Assert.IsTrue(_player.Holding == null);
        }
    }
}
