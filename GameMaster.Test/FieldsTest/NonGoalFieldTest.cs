
using GameMaster.Fields;
using GameMaster.Pieces;
using CommunicationUtils.Structures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMaster.Test.FieldsTest
{
    [TestClass]
    public class NonGoalFielddTest
    {
        NonGoalField field;

        [TestInitialize]
        public void TestInitialize()
        {
            field = new NonGoalField(0, 0);
        }

        [TestMethod]
        public void TestPlaceNormalPiece()
        {
            // Given
            var piece = new NormalPiece();

            // When
            var result = field.Place(piece);

            // Then
            Assert.AreEqual(result, PlacePieceInfo.NormalOnNonGoalField);
            Assert.IsFalse(field.ContainsPieces());
        }

        [TestMethod]
        public void TestPlaceShamPiece()
        {
            // Given
            var piece = new ShamPiece();

            // When
            var result = field.Place(piece);

            // Then
            Assert.AreEqual(result, PlacePieceInfo.ShamOnGoalArea);
            Assert.IsFalse(field.ContainsPieces());
        }

    }


}