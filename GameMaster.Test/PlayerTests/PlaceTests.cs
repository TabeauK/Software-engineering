using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameMaster;
using Microsoft.Extensions.Logging;
using GameMaster.Fields;
using GameMaster.Pieces;
using CommunicationUtils.Structures;

namespace GameMaster.Test.PlayerTests
{
    [TestClass]
    public class PlaceTests
    {
        private Player _player;

        [TestInitialize]
        public void Setup()
        {
            _player = new Player(CommunicationUtils.Structures.TeamColor.Blue, false);
        }

        [TestMethod]
        public void TestPlaceNormalOnTaskValid()
        {
            //Given
            NormalPiece piece = new NormalPiece();
            _player.Holding = piece;
            piece.Pickable = false;
            piece.Position = null;
            TaskField taskField = new TaskField(0, 0) { Player = _player };
            _player.Position = taskField;

            //When
            PlacePieceInfo? res = _player.Place();

            //then
            Assert.IsTrue(res.HasValue);
            Assert.IsTrue(res.Value == PlacePieceInfo.TaskField);
            Assert.IsTrue(_player.Holding == null);
            Assert.IsTrue(piece.Pickable == true);
            Assert.IsTrue(piece.Position == taskField);
            Assert.IsTrue(_player.Position == taskField);
            Assert.IsTrue(taskField.ContainsPieces() == true);
            Assert.IsTrue(taskField.pieces[0] == piece);
        }

        [TestMethod]
        public void TestPlaceShamOnTaskValid()
        {
            //Given
            ShamPiece piece = new ShamPiece();
            _player.Holding = piece;
            piece.Pickable = false;
            piece.Position = null;
            TaskField taskField = new TaskField(0, 0) { Player = _player };
            _player.Position = taskField;

            //When
            PlacePieceInfo? res = _player.Place();

            //then
            Assert.IsTrue(res.HasValue);
            Assert.IsTrue(res.Value == PlacePieceInfo.TaskField);
            Assert.IsTrue(_player.Holding == null);
            Assert.IsTrue(piece.Pickable == true);
            Assert.IsTrue(piece.Position == taskField);
            Assert.IsTrue(_player.Position == taskField);
            Assert.IsTrue(taskField.ContainsPieces() == true);
            Assert.IsTrue(taskField.pieces[0] == piece);
        }

        [TestMethod]
        public void TestPlaceNormalOnGoalValid()
        {
            //Given
            NormalPiece piece = new NormalPiece();
            _player.Holding = piece;
            piece.Pickable = false;
            piece.Position = null;
            GoalField field = new GoalField(0, 0) { Player = _player };
            _player.Position = field;

            //When
            PlacePieceInfo? res = _player.Place();

            //then
            Assert.IsTrue(res.HasValue);
            Assert.IsTrue(res.Value == PlacePieceInfo.NormalOnGoalField);
            Assert.IsTrue(_player.Holding == null);
            Assert.IsTrue(_player.Position == field);
            Assert.IsTrue(field.IsStillAGoalField == false);
        }

        [TestMethod]
        public void TestPlaceShamOnGoalValid()
        {
            //Given
            ShamPiece piece = new ShamPiece();
            _player.Holding = piece;
            piece.Pickable = false;
            piece.Position = null;
            GoalField field = new GoalField(0, 0) { Player = _player };
            _player.Position = field;

            //When
            PlacePieceInfo? res = _player.Place();

            //then
            Assert.IsTrue(res.HasValue);
            Assert.IsTrue(res.Value == PlacePieceInfo.ShamOnGoalArea);
            Assert.IsTrue(_player.Holding == null);
            Assert.IsTrue(_player.Position == field);
            Assert.IsTrue(field.IsStillAGoalField == true);
        }

        [TestMethod]
        public void TestPlaceNormalOnNonGoalValid()
        {
            //Given
            NormalPiece piece = new NormalPiece();
            _player.Holding = piece;
            piece.Pickable = false;
            piece.Position = null;
            NonGoalField field = new NonGoalField(0, 0) { Player = _player };
            _player.Position = field;

            //When
            PlacePieceInfo? res = _player.Place();

            //then
            Assert.IsTrue(res.HasValue);
            Assert.IsTrue(res.Value == PlacePieceInfo.NormalOnNonGoalField);
            Assert.IsTrue(_player.Holding == null);
            Assert.IsTrue(_player.Position == field);
        }

        [TestMethod]
        public void TestPlaceShamOnNonGoalValid()
        {
            //Given
            ShamPiece piece = new ShamPiece();
            _player.Holding = piece;
            piece.Pickable = false;
            piece.Position = null;
            NonGoalField field = new NonGoalField(0, 0) { Player = _player };
            _player.Position = field;

            //When
            PlacePieceInfo? res = _player.Place();

            //then
            Assert.IsTrue(res.HasValue);
            Assert.IsTrue(res.Value == PlacePieceInfo.ShamOnGoalArea);
            Assert.IsTrue(_player.Holding == null);
            Assert.IsTrue(_player.Position == field);
        }

        [TestMethod]
        public void TestPlaceNoPieceOnTaskValid()
        {
            //Given
            TaskField field = new TaskField(0, 0) { Player = _player };
            _player.Position = field;

            //When
            PlacePieceInfo? res = _player.Place();

            //then
            Assert.IsTrue(res.HasValue == false);
            Assert.IsTrue(_player.Holding == null);
            Assert.IsTrue(_player.Position == field);
        }

        [TestMethod]
        public void TestPlaceNoPieceOnGoalValid()
        {
            //Given
            GoalField field = new GoalField(0, 0) { Player = _player };
            _player.Position = field;

            //When
            PlacePieceInfo? res = _player.Place();

            //then
            Assert.IsTrue(res.HasValue == false);
            Assert.IsTrue(_player.Holding == null);
            Assert.IsTrue(_player.Position == field);
        }

        [TestMethod]
        public void TestPlaceNoPieceOnNonGoalValid()
        {
            //Given
            NonGoalField field = new NonGoalField(0, 0) { Player = _player };
            _player.Position = field;

            //When
            PlacePieceInfo? res = _player.Place();

            //then
            Assert.IsTrue(res.HasValue == false);
            Assert.IsTrue(_player.Holding == null);
            Assert.IsTrue(_player.Position == field);
        }
    }
}
