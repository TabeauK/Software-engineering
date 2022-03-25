using CommunicationUtils.Structures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Test
{
    [TestClass]
    public class EnumJsonConverterTests
    {
        public class SampleEntity
        {
            public TeamColor? teamId { get; set; }
            public GoalAreaTileInformation? tileInfo { get; set; }
            public Direction? direction { get; set; }
            public PickupPieceErrorType? pickupError { get; set; }
            public PlacePieceErrorType? placeError { get; set; }
            public GoalAreaTileInformation[] tileInfoArray { get; set; }
            public PlacePieceInfo? placePieceInfo { get; set; }

        }
        [TestMethod]
        public void Test_EnumJsonConverter_DeserializeTeamColor()
        {
            string json = "{ \"teamId\" : \"blue\" }";
            SampleEntity deserialized = JsonConvert.DeserializeObject<SampleEntity>(json);
            Assert.AreEqual(TeamColor.Blue, deserialized.teamId);

            string json2 = "{ \"teamId\" : \"rEd\" }";
            SampleEntity deserialized2 = JsonConvert.DeserializeObject<SampleEntity>(json2);
            Assert.AreEqual(TeamColor.Red, deserialized2.teamId);
        }

        [TestMethod]
        public void Test_EnumJsonConverter_SerializeTeamColor()
        {
            SampleEntity entity = new SampleEntity { teamId = TeamColor.Blue };
            string json = JsonConvert.SerializeObject(entity);
            JObject jobject = JObject.Parse(json);
            Assert.AreEqual("blue", jobject["teamId"]);

            entity = new SampleEntity { teamId = TeamColor.Red };
            json = JsonConvert.SerializeObject(entity);
            jobject = JObject.Parse(json);
            Assert.AreEqual("red", jobject["teamId"]);
        }

        [TestMethod]
        public void Test_EnumJsonConverter_DeserializeGoalAreaTileInformation()
        {
            string json = "{ \"tileInfo\" : \"idK\" }";
            SampleEntity deserialized = JsonConvert.DeserializeObject<SampleEntity>(json);
            Assert.AreEqual(GoalAreaTileInformation.Unknown, deserialized.tileInfo);

            string json2 = "{ \"tileInfo\" : \"g\" }";
            SampleEntity deserialized2 = JsonConvert.DeserializeObject<SampleEntity>(json2);
            Assert.AreEqual(GoalAreaTileInformation.Goal, deserialized2.tileInfo);

            string json3 = "{ \"tileInfo\" : \"N\" }";
            SampleEntity deserialized3 = JsonConvert.DeserializeObject<SampleEntity>(json3);
            Assert.AreEqual(GoalAreaTileInformation.NotGoal, deserialized3.tileInfo);
        }

        [TestMethod]
        public void Test_EnumJsonConverter_SerializeGoalAreaTileInformation()
        {
            SampleEntity entity = new SampleEntity { tileInfo = GoalAreaTileInformation.Unknown };
            string json = JsonConvert.SerializeObject(entity);
            JObject jobject = JObject.Parse(json);
            Assert.AreEqual("IDK", jobject["tileInfo"]);

            entity = new SampleEntity { tileInfo = GoalAreaTileInformation.Goal };
            json = JsonConvert.SerializeObject(entity);
            jobject = JObject.Parse(json);
            Assert.AreEqual("G", jobject["tileInfo"]);

            entity = new SampleEntity { tileInfo = GoalAreaTileInformation.NotGoal };
            json = JsonConvert.SerializeObject(entity);
            jobject = JObject.Parse(json);
            Assert.AreEqual("N", jobject["tileInfo"]);
        }

        [TestMethod]
        public void Test_EnumJsonConverter_DeserializeDirection()
        {
            string json = "{ \"direction\" : \"N\" }";
            SampleEntity deserialized = JsonConvert.DeserializeObject<SampleEntity>(json);
            Assert.AreEqual(Direction.North, deserialized.direction);

            json = "{ \"direction\" : \"W\" }";
            deserialized = JsonConvert.DeserializeObject<SampleEntity>(json);
            Assert.AreEqual(Direction.West, deserialized.direction);

            json = "{ \"direction\" : \"s\" }";
            deserialized = JsonConvert.DeserializeObject<SampleEntity>(json);
            Assert.AreEqual(Direction.South, deserialized.direction);

            json = "{ \"direction\" : \"e\" }";
            deserialized = JsonConvert.DeserializeObject<SampleEntity>(json);
            Assert.AreEqual(Direction.East, deserialized.direction);
        }

        [TestMethod]
        public void Test_EnumJsonConverter_SerializeDirection()
        {
            SampleEntity entity = new SampleEntity { direction = Direction.North };
            string json = JsonConvert.SerializeObject(entity);
            JObject jobject = JObject.Parse(json);
            Assert.AreEqual("N", jobject["direction"]);

            entity = new SampleEntity { direction = Direction.East };
            json = JsonConvert.SerializeObject(entity);
            jobject = JObject.Parse(json);
            Assert.AreEqual("E", jobject["direction"]);

            entity = new SampleEntity { direction = Direction.West };
            json = JsonConvert.SerializeObject(entity);
            jobject = JObject.Parse(json);
            Assert.AreEqual("W", jobject["direction"]);

            entity = new SampleEntity { direction = Direction.South };
            json = JsonConvert.SerializeObject(entity);
            jobject = JObject.Parse(json);
            Assert.AreEqual("S", jobject["direction"]);
        }

        [TestMethod]
        public void Test_EnumJsonConverter_DeserializePickupPieceErrorType()
        {
            string json = "{ \"pickupError\" : \"oTHER\" }";
            SampleEntity deserialized = JsonConvert.DeserializeObject<SampleEntity>(json);
            Assert.AreEqual(PickupPieceErrorType.AlreadyCarriesPiece, deserialized.pickupError);

            json = "{ \"pickupError\" : \"NothingThere\" }";
            deserialized = JsonConvert.DeserializeObject<SampleEntity>(json);
            Assert.AreEqual(PickupPieceErrorType.NoPieceToPickUp, deserialized.pickupError);
        }

        [TestMethod]
        public void Test_EnumJsonConverter_SerializePickupPieceErrorType()
        {
            SampleEntity entity = new SampleEntity { pickupError = PickupPieceErrorType.NoPieceToPickUp };
            string json = JsonConvert.SerializeObject(entity);
            JObject jobject = JObject.Parse(json);
            Assert.AreEqual("NothingThere", jobject["pickupError"]);

            entity = new SampleEntity { pickupError = PickupPieceErrorType.AlreadyCarriesPiece };
            json = JsonConvert.SerializeObject(entity);
            jobject = JObject.Parse(json);
            Assert.AreEqual("Other", jobject["pickupError"]);
        }

        [TestMethod]
        public void Test_EnumJsonConverter_DeserializePlacePieceErrorType()
        {
            string json = "{ \"placeError\" : \"cannotPuTtHere\" }";
            SampleEntity deserialized = JsonConvert.DeserializeObject<SampleEntity>(json);
            Assert.AreEqual(PlacePieceErrorType.CannotPlaceThere, deserialized.placeError);

            json = "{ \"placeError\" : \"agENTnoThOldING\" }";
            deserialized = JsonConvert.DeserializeObject<SampleEntity>(json);
            Assert.AreEqual(PlacePieceErrorType.NoPieceHeld, deserialized.placeError);

            json = "{ \"placeError\" : \"Other\" }";
            deserialized = JsonConvert.DeserializeObject<SampleEntity>(json);
            Assert.AreEqual(PlacePieceErrorType.Other, deserialized.placeError);
        }

        [TestMethod]
        public void Test_EnumJsonConverter_SerializePlacePieceErrorType()
        {
            SampleEntity entity = new SampleEntity { placeError = PlacePieceErrorType.CannotPlaceThere };
            string json = JsonConvert.SerializeObject(entity);
            JObject jobject = JObject.Parse(json);
            Assert.AreEqual("CannotPutThere", jobject["placeError"]);

            entity = new SampleEntity { placeError = PlacePieceErrorType.NoPieceHeld };
            json = JsonConvert.SerializeObject(entity);
            jobject = JObject.Parse(json);
            Assert.AreEqual("AgentNotHolding", jobject["placeError"]);

            entity = new SampleEntity { placeError = PlacePieceErrorType.Other};
            json = JsonConvert.SerializeObject(entity);
            jobject = JObject.Parse(json);
            Assert.AreEqual("Other", jobject["placeError"]);
        }

        [TestMethod]
        public void Test_EnumJsonConverter_DeserializeGoalAreaTileInformationArray()
        {
            string json = "{ \"tileInfoArray\" : [\"idK\",\"n\",\"G\"] }";
            SampleEntity deserialized = JsonConvert.DeserializeObject<SampleEntity>(json);
            Assert.AreEqual(3, deserialized.tileInfoArray.Length);
            Assert.AreEqual(GoalAreaTileInformation.Unknown, deserialized.tileInfoArray[0]);
            Assert.AreEqual(GoalAreaTileInformation.NotGoal, deserialized.tileInfoArray[1]);
            Assert.AreEqual(GoalAreaTileInformation.Goal, deserialized.tileInfoArray[2]);
        }

        [TestMethod]
        public void Test_EnumJsonConverter_SerializeGoalAreaTileInformationArray()
        {
            SampleEntity entity = new SampleEntity { tileInfoArray = new GoalAreaTileInformation[]
            {
                GoalAreaTileInformation.Goal,
                GoalAreaTileInformation.NotGoal,
                GoalAreaTileInformation.Unknown
            }};
            string json = JsonConvert.SerializeObject(entity);
            JObject jobject = JObject.Parse(json);
            Assert.AreEqual("IDK", jobject["tileInfoArray"][2]);
            Assert.AreEqual("G", jobject["tileInfoArray"][0]);
            Assert.AreEqual("N", jobject["tileInfoArray"][1]);
        }

        [TestMethod]
        public void Test_EnumJsonConverter_DeserializePlacePieceInfo()
        {
            string json = "{ \"placePieceInfo\" : \"normalongoalfield\" }";
            SampleEntity deserialized = JsonConvert.DeserializeObject<SampleEntity>(json);
            Assert.AreEqual(PlacePieceInfo.NormalOnGoalField, deserialized.placePieceInfo);

            json = "{ \"placePieceInfo\" : \"NormalOnNonGOALField\" }";
            deserialized = JsonConvert.DeserializeObject<SampleEntity>(json);
            Assert.AreEqual(PlacePieceInfo.NormalOnNonGoalField, deserialized.placePieceInfo);

            json = "{ \"placePieceInfo\" : \"SHamOnGoalArEa\" }";
            deserialized = JsonConvert.DeserializeObject<SampleEntity>(json);
            Assert.AreEqual(PlacePieceInfo.ShamOnGoalArea, deserialized.placePieceInfo);

            json = "{ \"placePieceInfo\" : \"TASKFIELD\" }";
            deserialized = JsonConvert.DeserializeObject<SampleEntity>(json);
            Assert.AreEqual(PlacePieceInfo.TaskField, deserialized.placePieceInfo);
        }

        [TestMethod]
        public void Test_EnumJsonConverter_SerializePlacePieceInfo()
        {
            SampleEntity entity = new SampleEntity { placePieceInfo = PlacePieceInfo.NormalOnGoalField };
            string json = JsonConvert.SerializeObject(entity);
            JObject jobject = JObject.Parse(json);
            Assert.AreEqual("NormalOnGoalField", jobject["placePieceInfo"]);

            entity = new SampleEntity { placePieceInfo = PlacePieceInfo.NormalOnNonGoalField };
            json = JsonConvert.SerializeObject(entity);
            jobject = JObject.Parse(json);
            Assert.AreEqual("NormalOnNonGoalField", jobject["placePieceInfo"]);

            entity = new SampleEntity { placePieceInfo = PlacePieceInfo.ShamOnGoalArea };
            json = JsonConvert.SerializeObject(entity);
            jobject = JObject.Parse(json);
            Assert.AreEqual("ShamOnGoalArea", jobject["placePieceInfo"]);

            entity = new SampleEntity { placePieceInfo = PlacePieceInfo.TaskField };
            json = JsonConvert.SerializeObject(entity);
            jobject = JObject.Parse(json);
            Assert.AreEqual("TaskField", jobject["placePieceInfo"]);
        }
    }
}
