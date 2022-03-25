
using GameMaster.Fields;
using GameMaster.Pieces;
using CommunicationUtils.Structures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMaster.Test.FieldsTest
{
    [TestClass]
    public class GoalFielddTest
    {
        GoalField field;

        [TestInitialize]
        public void TestInitialize()
        {
            field = new GoalField(0, 0);
        }

        [TestMethod]
        public void TestPlaceNormalPiece_StillAGoalField()
        {
            // Given
            var piece = new NormalPiece();

            // When
            var result = field.Place(piece);

            // Then
            Assert.AreEqual(result, PlacePieceInfo.NormalOnGoalField);
        }

        [TestMethod]
        public void TestPlaceNormalPiece_NotAGoalField()
        {
            // Given
            var piece = new NormalPiece();
            field.IsStillAGoalField = false;

            // When
            var result = field.Place(piece);

            // Then
            Assert.AreEqual(result, PlacePieceInfo.NormalOnNonGoalField);
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
        }

    }


}