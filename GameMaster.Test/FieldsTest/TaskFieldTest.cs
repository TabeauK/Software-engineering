
using GameMaster.Fields;
using GameMaster.Pieces;
using CommunicationUtils.Structures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GameMaster.Test.FieldsTest
{
    [TestClass]
    public class TaskFieldTest
    {
        TaskField field;

        [TestInitialize]
        public void TestInitialize()
        {
            field = new TaskField(0, 0);
        }

        [TestMethod]
        public void TestPlaceNormalPiece()
        {
            // Given
            var piece = new NormalPiece();

            // When
            var result = field.Place(piece);

            // Then
            Assert.AreEqual(piece.Position, field);
            Assert.AreEqual(result, PlacePieceInfo.TaskField);
            Assert.IsTrue(piece.Pickable);
        }

        [TestMethod]
        public void TestPlaceShamPiece()
        {
            // Given
            var piece = new ShamPiece();

            // When
            var result = field.Place(piece);

            // Then
            Assert.AreEqual(piece.Position, field);
            Assert.AreEqual(result, PlacePieceInfo.TaskField);
            Assert.IsTrue(piece.Pickable);
        }

        [TestMethod]
        public void TestPickUpNothing()
        {
            // Given
            var player = new Player(TeamColor.Blue, false);
            field.MoveHere(player);

            // When
            var result = field.PickUp();

            // Then
            Assert.AreEqual(result, PickupPieceErrorType.NoPieceToPickUp);
        }

        [TestMethod]
        public void TestPickUpSomething()
        {
            // Given
            var piece = new NormalPiece();
            var player = new Player(TeamColor.Blue, false);
            field.MoveHere(player);
            field.Place(piece);

            // When
            var result = field.PickUp();

            // Then
            Assert.IsNull(result);
            Assert.AreEqual(player.Holding, piece);
            Assert.IsFalse(piece.Pickable);
            Assert.IsNull(piece.Position);
        }

        [TestMethod]
        public void TestPutPickablePiece()
        {
            // Given
            var piece = new NormalPiece();

            // When
            field.PutPickablePiece(piece);

            // Then
            Assert.IsTrue(piece.Pickable);
            Assert.AreEqual(piece.Position, field);
        }
    }
    
}