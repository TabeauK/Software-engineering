using GameMaster.Fields;
using GameMaster.Pieces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMaster.Test.Pieces
{
    [TestClass]
    public class ShamPieceUnitTests
    {
        [TestMethod]
        public void PlaceShamOnNonGoalFieldTest()
        {
            //Given
            AbstractField field = new NonGoalField(0, 0);
            AbstractPiece piece = new ShamPiece()
            {
                Position = null,
                Pickable = false,
            };

            //When
            var result = piece.Place(field);

            //Then
            Assert.AreEqual(result, CommunicationUtils.Structures.PlacePieceInfo.ShamOnGoalArea);
        }

        [TestMethod]
        public void PlaceShamOnNewGoalFieldTest()
        {
            //Given
            AbstractField field = new GoalField(0, 0);
            AbstractPiece piece = new ShamPiece()
            {
                Position = null,
                Pickable = false,
            };

            //When
            var result = piece.Place(field);

            //Then
            Assert.AreEqual(result, CommunicationUtils.Structures.PlacePieceInfo.ShamOnGoalArea);
        }

        [TestMethod]
        public void PlaceShamOnOldGoalFieldTest()
        {
            //Given
            AbstractField field = new GoalField(0, 0);
            AbstractPiece piece = new ShamPiece()
            {
                Position = null,
                Pickable = false,
            };
            AbstractPiece piece1 = new NormalPiece()
            {
                Position = null,
                Pickable = false,
            };

            //When
            piece1.Place(field);
            var result = piece.Place(field);

            //Then
            Assert.AreEqual(result, CommunicationUtils.Structures.PlacePieceInfo.ShamOnGoalArea);
        }

        [TestMethod]
        public void PlaceShamOnTaskFieldTest()
        {
            //Given
            AbstractField field = new TaskField(0, 0);
            AbstractPiece piece = new ShamPiece()
            {
                Position = null,
                Pickable = false,
            };

            //When
            var result = piece.Place(field);

            //Then
            Assert.AreEqual(result, CommunicationUtils.Structures.PlacePieceInfo.TaskField);
            Assert.AreEqual(field, piece.Position);
            Assert.AreEqual(true, piece.Pickable);
        }
    }
}
