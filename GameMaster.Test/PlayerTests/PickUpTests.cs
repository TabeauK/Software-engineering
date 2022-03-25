using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameMaster;
using Microsoft.Extensions.Logging;
using GameMaster.Fields;
using GameMaster.Pieces;
using CommunicationUtils.Structures;
using System.Collections.Generic;

namespace GameMaster.Test.PlayerTests
{
    [TestClass]
    public class PickUpTests
    {
        private Player _player;

        [TestInitialize]
        public void Setup()
        {
            _player = new Player(CommunicationUtils.Structures.TeamColor.Blue, false);
        }

        [TestMethod]
        public void TestPickUpWithNoPieceOnTaskWithNormal()
        {
            //Given
            NormalPiece piece = new NormalPiece();
            piece.Pickable = true;
            piece.Position = null;

            TaskField field = new TaskField(0, 0);
            field.pieces = new List<AbstractPiece>() { piece };
            field.Player = _player;
            _player.Position = field;
            //When
            PickupPieceErrorType? res = _player.PickUp();

            //Then
            Assert.IsTrue(!res.HasValue);
            Assert.IsTrue(_player.Holding == piece);
            Assert.IsTrue(piece.Pickable == false);
            Assert.IsTrue(piece.Position == null);
            Assert.IsTrue(!field.pieces.Contains(piece));
            Assert.IsTrue(field.Player == _player);
        }

        [TestMethod]
        public void TestPickUpWithNoPieceOnTaskWithSham()
        {
            //Given
            ShamPiece piece = new ShamPiece();
            piece.Pickable = true;
            piece.Position = null;

            TaskField field = new TaskField(0, 0);
            field.pieces = new List<AbstractPiece>() { piece };
            field.Player = _player;
            _player.Position = field;
            //When
            PickupPieceErrorType? res = _player.PickUp();

            //Then
            Assert.IsTrue(!res.HasValue);
            Assert.IsTrue(_player.Holding == piece);
            Assert.IsTrue(piece.Pickable == false);
            Assert.IsTrue(piece.Position == null);
            Assert.IsTrue(!field.pieces.Contains(piece));
            Assert.IsTrue(field.Player == _player);
        }

        [TestMethod]
        public void TestPickUpWithNoPieceOnGoal()
        {
            //Given
            GoalField field = new GoalField(0, 0);
            field.Player = _player;
            _player.Position = field;

            //When
            PickupPieceErrorType? res = _player.PickUp();

            //Then
            Assert.IsTrue(res.HasValue);
            Assert.IsTrue(res == PickupPieceErrorType.NoPieceToPickUp);
            Assert.IsTrue(_player.Holding == null);
            Assert.IsTrue(field.Player == _player);
        }

        [TestMethod]
        public void TestPickUpWithNoPieceOnNonGoal()
        {
            //Given
            NonGoalField field = new NonGoalField(0, 0);
            field.Player = _player;
            _player.Position = field;

            //When
            PickupPieceErrorType? res = _player.PickUp();

            //Then
            Assert.IsTrue(res.HasValue);
            Assert.IsTrue(res == PickupPieceErrorType.NoPieceToPickUp);
            Assert.IsTrue(_player.Holding == null);
            Assert.IsTrue(field.Player == _player);
        }

        [TestMethod]
        public void TestPickUpWithPieceOnTaskWithNormal()
        {
            //Given
            NormalPiece piece = new NormalPiece();
            piece.Pickable = true;
            piece.Position = null;

            NormalPiece alreadyHoldingPiece = new NormalPiece();
            alreadyHoldingPiece.Pickable = false;
            alreadyHoldingPiece.Position = null;
            _player.Holding = alreadyHoldingPiece;

            TaskField field = new TaskField(0, 0);
            field.pieces = new List<AbstractPiece>() { piece };
            field.Player = _player;
            _player.Position = field;
            //When
            PickupPieceErrorType? res = _player.PickUp();

            //Then
            Assert.IsTrue(res.HasValue);
            Assert.IsTrue(res == PickupPieceErrorType.AlreadyCarriesPiece);
            Assert.IsTrue(_player.Holding == alreadyHoldingPiece);
            Assert.IsTrue(field.Player == _player);
            Assert.IsTrue(field.pieces.Contains(piece));
        }

        [TestMethod]
        public void TestPickUpWithPieceOnGoal()
        {
            //Given
            GoalField field = new GoalField(0, 0);
            field.Player = _player;
            _player.Position = field;

            NormalPiece alreadyHoldingPiece = new NormalPiece();
            alreadyHoldingPiece.Pickable = false;
            alreadyHoldingPiece.Position = null;
            _player.Holding = alreadyHoldingPiece;

            //When
            PickupPieceErrorType? res = _player.PickUp();

            //Then
            Assert.IsTrue(res.HasValue);
            Assert.IsTrue(res == PickupPieceErrorType.AlreadyCarriesPiece);
            Assert.IsTrue(_player.Holding == alreadyHoldingPiece);
            Assert.IsTrue(field.Player == _player);
        }

        [TestMethod]
        public void TestPickUpWithPieceOnNonGoal()
        {
            //Given
            NonGoalField field = new NonGoalField(0, 0);
            field.Player = _player;
            _player.Position = field;

            NormalPiece alreadyHoldingPiece = new NormalPiece();
            alreadyHoldingPiece.Pickable = false;
            alreadyHoldingPiece.Position = null;
            _player.Holding = alreadyHoldingPiece;

            //When
            PickupPieceErrorType? res = _player.PickUp();

            //Then
            Assert.IsTrue(res.HasValue);
            Assert.IsTrue(res == PickupPieceErrorType.AlreadyCarriesPiece);
            Assert.IsTrue(_player.Holding == alreadyHoldingPiece);
            Assert.IsTrue(field.Player == _player);
        }

    }
}
