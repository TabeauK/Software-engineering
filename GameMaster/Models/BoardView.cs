using CommunicationUtils.Structures;
using System.Collections.Generic;
using GameMaster.Pieces;
using GameMaster.Fields;
using System;

namespace GameMaster
{
    public class GameEndArgs : EventArgs
    {
        public TeamColor winner;
        public GameEndArgs(TeamColor winner)
        {
            this.winner = winner;
        }
    }

    public class BoardView
    {
        public readonly int Width, Height, GoalAreaHeight;
        public readonly int RedScore, BlueScore, RedPlayerCount, BluePlayerCount, GamePlayerCount;

        public enum Field
        {
            Empty,
            RedPlayer,
            BluePlayer,
            RedPlayerWithPiece,
            RedPlayerWithSham,
            BluePlayerWithPiece,
            BluePlayerWithSham,
            RedGoal,
            BlueGoal,
            NonGoal,
            CoveredGoal,
            Piece,
            Sham
        }

        public readonly Field[,] Fields;
	    public readonly IReadOnlyList<(int x, int y, int id)> PlayerIds;
        public readonly (int x, int y)? RedLeader, BlueLeader;

        public BoardView(IGameMasterField[,] map,
            IReadOnlyDictionary<int, Player> bluePlayers, IReadOnlyDictionary<int, Player> redPlayers,
            IReadOnlyList<AbstractPiece> pieces, int goalAreaHeight, int redScore, int blueScore, Configuration _config)
        {
            RedScore = redScore;
            BlueScore = blueScore;
            BluePlayerCount = bluePlayers == null ? 0 : bluePlayers.Count;
            RedPlayerCount = redPlayers == null ? 0 : redPlayers.Count;
            GamePlayerCount = _config.NumberOfPlayers;

            Width = map.GetLength(1);
            Height = map.GetLength(0);
            GoalAreaHeight = goalAreaHeight;
            Fields = new Field[Height, Width];
            // Rodzaj pól
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    var field = map[i, j];
                    switch (field)
                    {
                        case GoalField f:
                            if (f.IsStillAGoalField)
                                Fields[i, j] = f.Team == TeamColor.Blue ? Field.BlueGoal : Field.RedGoal;
                            else
                                Fields[i, j] = Field.CoveredGoal;
                            break;
                        case NonGoalField f:
                            Fields[i, j] = Field.NonGoal;
                            break;
                        case TaskField f:
                            Fields[i, j] = Field.Empty;
                            break;
                    }
                }

            // Pozycje kawałków
            // BoardView aktualnie nie zlicza, ile kawałków leży na polu
            if (pieces != null)
            {
                foreach (var piece in pieces)
                {
                    if (piece == null)
                        continue;
                    // Pokaż tylko kawałki które można podnieść i znajdują się na planszy
                    if (!piece.Pickable)
                        continue;
                    if (piece.Position == null)
                        continue;
                    var pos = piece.Position.ToVector2D();
                    Fields[pos.y, pos.x] = piece.IsSham ? Field.Sham : Field.Piece;
                }
            }

            var _playerIds = new List<(int, int, int)>();
            // Team Blue
            if (bluePlayers != null)
                foreach (var (id, player) in bluePlayers)
                {
                    if (player.Position == null)
                        continue;
                    var pos = player.Position.ToVector2D();
                    if (player.Holding != null)
                        Fields[pos.y, pos.x] = player.Holding.IsSham ? Field.BluePlayerWithSham : Field.BluePlayerWithPiece;
                    else
                        Fields[pos.y, pos.x] = Field.BluePlayer;
                    _playerIds.Add((pos.y, pos.x, id));
                    if (player.IsLeader)
                        BlueLeader = (pos.y, pos.x);
                }

            // Team Red
            if (redPlayers != null)
                foreach (var (id, player) in redPlayers)
                {
                    if (player.Position == null)
                        continue;
                    var pos = player.Position.ToVector2D();
                    if (player.Holding != null)
                        Fields[pos.y, pos.x] = player.Holding.IsSham ? Field.RedPlayerWithSham : Field.RedPlayerWithPiece;
                    else
                        Fields[pos.y, pos.x] = Field.RedPlayer;
                    _playerIds.Add((pos.y, pos.x, id));
                    if (player.IsLeader)
                        RedLeader = (pos.y, pos.x);
                }
            PlayerIds = _playerIds;

        }

    }
}
