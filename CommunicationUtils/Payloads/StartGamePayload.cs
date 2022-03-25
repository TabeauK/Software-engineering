using CommunicationUtils.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils.Payloads
{
    public class StartGamePayload: IPayload
    {
        public int agentID { get; set; }
        public int[] alliesIDs { get; set; }
        public int leaderID { get; set; }
        public int[] enemiesIDs { get; set; }
        [JsonProperty("teamID")]
        public TeamColor teamId { get; set; }
        public Vector2D boardSize { get; set; }
        public int goalAreaSize { get; set; }
        public NumberOfPlayers numberOfPlayers { get; set; }
        public int numberOfPieces { get; set; }
        public int numberOfGoals { get; set; }
        public Penalties penalties { get; set; }
        public double shamPieceProbability { get; set; }
        public Vector2D position { get; set; }

        public override string ToString()
        {
            return $"agentID {agentID}, alliesIDs {alliesIDs.ToString()}, leaderID {leaderID}, enemiesIDs {enemiesIDs.ToString()}, " +
                $"teamId {teamId.GetName()}, boardSize {boardSize.ToString()}, goalAreaSize {goalAreaSize}, " +
                $"numberOfPlayers {numberOfPlayers.ToString()}, numberOfPieces {numberOfPieces}, numberOfGoals {numberOfGoals}, " +
                $"penalties {penalties.ToString()}, shamPieceProbability {shamPieceProbability}, position {position.ToString()}";
        }

    }
    public class NumberOfPlayers
    {
        public int allies { get; set; }
        public int enemies { get; set; }

        public override string ToString()
        {
            return $"[allies {allies}, enemies {enemies}]";
        }
    }

    public class Penalties
    {
        public int move { get; set; }
        public int checkForSham { get; set; }
        public int discovery { get; set; }
        public int destroyPiece { get; set; }
        public int putPiece { get; set; }
        public int response { get; set; }
        public int pickup { get; set; }
        public int ask { get; set; }
        public int prematureRequest { get; set; }

        public override string ToString()
        {
            return $"[move {move}, checkForSham {checkForSham}, discovery {discovery}, destroyPiece {destroyPiece}, " +
                $"putPiece {putPiece}, response {response}, " +
                $"pickup {pickup}, ask {ask}, prematureRequest {prematureRequest}]";
        }
    }
}
