using GameMaster.Fields;
using GameMaster.Pieces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMaster.Test.Pieces
{
    [TestClass]
    public class NormalPieceUnitTests
    {
        [TestMethod]
        public void PlaceNormalOnNonGoalFieldTest()
        {
            //Given
            AbstractField field = new NonGoalField(0, 0);
            AbstractPiece piece = new NormalPiece()
            {
                Position = null,
                Pickable = false,
            };

            //When
            var result = piece.Place(field);

            //Then
            Assert.AreEqual(result, CommunicationUtils.Structures.PlacePieceInfo.NormalOnNonGoalField);
        }

        [TestMethod]
        public void PlaceNormalOnNewGoalFieldTest()
        {
            //Given
            AbstractField field = new GoalField(0, 0);
            AbstractPiece piece = new NormalPiece()
            {
                Position = null,
                Pickable = false,
            };

            //When
            var result = piece.Place(field);

            //Then
            Assert.AreEqual(result, CommunicationUtils.Structures.PlacePieceInfo.NormalOnGoalField);
        }

        [TestMethod]
        public void PlaceNormalOnOldGoalFieldTest()
        {
            //Given
            AbstractField field = new GoalField(0, 0);
            AbstractPiece piece = new NormalPiece()
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
            Assert.AreEqual(result, CommunicationUtils.Structures.PlacePieceInfo.NormalOnNonGoalField);
        }

        [TestMethod]
        public void PlaceNormalOnTaskFieldTest()
        {
            //Given
            AbstractField field = new TaskField(0, 0);
            AbstractPiece piece = new NormalPiece()
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

