
using GameMaster.Fields;
using GameMaster.Pieces;
using CommunicationUtils.Structures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GameMaster.Test.FieldsTest
{
    /// <summary>
    /// Tylko metody niewritualne
    /// </summary>
    [TestClass]
    public class AbstractFieldTest
    {
        AbstractField field;

        [TestInitialize]
        public void TestInitialize()
        {
            field = new TaskField(0, 0);
        }

        [TestMethod]
        public void TestLeave()
        {
            // Given
            var player = new Player(TeamColor.Blue, false)
            { Position = field };

            // When
            field.Leave();

            // Then
            Assert.IsNull(field.Player);
        }

        [TestMethod]
        public void TestMoveHere_Empty()
        {
            // Given
            var oldPos = new TaskField(1, 1);
            var player = new Player(TeamColor.Blue, false)
            { Position = oldPos };

            // When
            var result = field.MoveHere(player);

            // Then
            Assert.IsTrue(result);
            Assert.AreEqual(field.Player, player);
            Assert.AreEqual(player.Position, field);
            Assert.IsNull(oldPos.Player);
        }

        [TestMethod]
        public void TestMoveHere_Occupied()
        {
            // Given
            var oldPos = new TaskField(0, 1);
            var player = new Player(TeamColor.Blue, false)
            { Position = oldPos };

            var oldOtherPos = new TaskField(1, 0);
            var otherPlayer = new Player(TeamColor.Red, true)
            { Position = oldOtherPos };

            // When
            field.MoveHere(otherPlayer);
            var result = field.MoveHere(player);

            // Then
            Assert.IsFalse(result);
        }


        [DataTestMethod]
        [DataRow(0, 0)]
        [DataRow(5, 5)]
        [DataRow(1, 4)]
        [DataRow(4, 1)]
        public void TestGetPosition(int x, int y)
        {
            // Given
            AbstractField field = new TaskField(x, y);

            // When
            var pos = field.ToVector2D();

            // Then
            Assert.AreEqual((x, y), (pos.x, pos.y));
        }


        [DataTestMethod]
        [DataRow(0, 0, 0, 0)]
        [DataRow(1, 2, 3, 4)]
        [DataRow(4, 3, 2, 1)]
        public void TestDistance(int x1, int y1, int x2, int y2)
        {
            // Given
            AbstractField field1 = new TaskField(x1, y1);
            AbstractField field2 = new NonGoalField(x2, y2);

            // When
            int dist1 = field1.Distance(field2);
            int dist2 = field2.Distance(field1);
            int dist0 = field1.Distance(field1);

            // Then
            Assert.AreEqual(dist1, dist2);
            Assert.AreEqual(dist0, 0);

        }

        [DataTestMethod]
        [DataRow(0, 0, 0)]
        [DataRow(1, 2, 3)]
        [DataRow(4, 3, 7)]
        public void TestDistance(int x, int y, int result)
        {
            // Given
            AbstractField field = new TaskField(x, y);
            AbstractField field0 = new GoalField(0, 0);

            // When
            int dist = field0.Distance(field);

            // Then
            Assert.AreEqual(result, dist);
        }

        [TestMethod]
        public void TestDistanceToClosestPiece_Pieces()
        {
            // Given
            var fields = new List<TaskField>()
            {
                new TaskField(1,1),
                new TaskField(3,0),
                new TaskField(0,3)
            };
            var pieces = new List<AbstractPiece>()
            {
                new NormalPiece() {Position = fields[0], Pickable = true},
                new ShamPiece() {Position = fields[1], Pickable = true},
                new NormalPiece() {Position = fields[2], Pickable = true},
                new ShamPiece() {Position = fields[0], Pickable = true},
            };

            // When
            var result = field.DistanceToClosestPiece(pieces);

            // Then
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void TestDistanceToClosestPiece_NoPieces()
        {
            // Given
            var pieces = new List<AbstractPiece>();

            // When
            var result = field.DistanceToClosestPiece(pieces);

            // Then
            Assert.IsNull(result);
        }
    }

}