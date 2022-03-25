using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameMaster;
using Microsoft.Extensions.Logging;
using GameMaster.Fields;

namespace GameMaster.Test.PlayerTests
{
    [TestClass]
    public class MoveTests
    {
        private Player _player;

        [TestInitialize]
        public void Setup()
        {
            _player = new Player(CommunicationUtils.Structures.TeamColor.Blue, false);
        }

        [TestMethod]
        public void TestMoveOnTaskFieldValid()
        {
            //Given
            TaskField field = new TaskField(0, 0);
            TaskField previousField = new TaskField(1, 0) { Player = _player };
            _player.Position = previousField;

            //When
            bool res = _player.Move(field);

            //Then
            Assert.IsTrue(res);
            Assert.IsTrue(_player.Position == field);
            Assert.IsTrue(field.HasPlayer());
            Assert.IsTrue(field.Player == _player);
            Assert.IsTrue(!previousField.HasPlayer());
            Assert.IsTrue(previousField.Player == null);
        }

        [TestMethod]
        public void TestMoveOnGoalFieldValid()
        {
            //Given
            GoalField field = new GoalField(0, 0);
            TaskField previousField = new TaskField(1, 0) { Player = _player };
            _player.Position = previousField;

            //When
            bool res = _player.Move(field);

            //Then
            Assert.IsTrue(res);
            Assert.IsTrue(_player.Position == field);
            Assert.IsTrue(field.HasPlayer());
            Assert.IsTrue(field.Player == _player);
            Assert.IsTrue(!previousField.HasPlayer());
            Assert.IsTrue(previousField.Player == null);

        }

        [TestMethod]
        public void TestMoveOnNonGoalFieldValid()
        {
            //Given
            NonGoalField field = new NonGoalField(0, 0);
            TaskField previousField = new TaskField(1, 0) { Player = _player };
            _player.Position = previousField;

            //When
            bool res = _player.Move(field);

            //Then
            Assert.IsTrue(res);
            Assert.IsTrue(_player.Position == field);
            Assert.IsTrue(field.HasPlayer());
            Assert.IsTrue(field.Player == _player);
            Assert.IsTrue(!previousField.HasPlayer());
            Assert.IsTrue(previousField.Player == null);

        }

        [TestMethod]
        public void TestMoveOnTaskFieldWithPlayer()
        {
            //Given
            Player player2 = new Player(CommunicationUtils.Structures.TeamColor.Red, false);
            TaskField field = new TaskField(0, 0) { Player = player2 };
            player2.Position = field;
            TaskField previousField = new TaskField(1, 0) { Player = _player };
            _player.Position = previousField;

            //When
            bool res = _player.Move(field);

            //Then
            Assert.IsTrue(!res);
            Assert.IsTrue(_player.Position == previousField);
            Assert.IsTrue(field.HasPlayer());
            Assert.IsTrue(field.Player == player2);
            Assert.IsTrue(previousField.HasPlayer());
            Assert.IsTrue(previousField.Player == _player);
        }

        [TestMethod]
        public void TestMoveOnGoalFieldWithPlayer()
        {
            //Given
            Player player2 = new Player(CommunicationUtils.Structures.TeamColor.Red, false);
            GoalField field = new GoalField(0, 0) { Player = player2 };
            player2.Position = field;
            TaskField previousField = new TaskField(1, 0) { Player = _player };
            _player.Position = previousField;

            //When
            bool res = _player.Move(field);

            //Then
            Assert.IsTrue(!res);
            Assert.IsTrue(_player.Position == previousField);
            Assert.IsTrue(field.HasPlayer());
            Assert.IsTrue(field.Player == player2);
            Assert.IsTrue(previousField.HasPlayer());
            Assert.IsTrue(previousField.Player == _player);
        }

        [TestMethod]
        public void TestMoveOnNonGoalFieldWithPlayer()
        {
            //Given
            Player player2 = new Player(CommunicationUtils.Structures.TeamColor.Red, false);
            NonGoalField field = new NonGoalField(0, 0) { Player = player2 };
            player2.Position = field;
            TaskField previousField = new TaskField(1, 0) { Player = _player };
            _player.Position = previousField;

            //When
            bool res = _player.Move(field);

            //Then
            Assert.IsTrue(!res);
            Assert.IsTrue(_player.Position == previousField);
            Assert.IsTrue(field.HasPlayer());
            Assert.IsTrue(field.Player == player2);
            Assert.IsTrue(previousField.HasPlayer());
            Assert.IsTrue(previousField.Player == _player);
        }

    }
}
