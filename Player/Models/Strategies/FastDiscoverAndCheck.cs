using CommunicationUtils.Structures;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Player.Models.Strategies
{
    public enum CurrentPlan
    {
        SearchPiece,
        CheckPiece,
        GoToGoalsArea,
        PlacePiece
    }

    public enum LastDecision
    {
        None,
        Move,
        Discover,
        Check,
        Place,
        PickUp
    }
    public enum Dir
    {
        NW,
        N,
        NE,
        W,
        C,
        E,
        SW,
        S,
        SE,
        NONE
    }
    public class FastDiscoverAndCheck : IStrategy
    {
        internal ILogger _logger;
        internal Player _player;
        internal int _boardHeight;
        internal int _boardLength;
        internal int _relativeId;
        internal int _playerCount;
        // Tablica list kawałków przypisanych do każdego gracza, według jego relativeId
        internal List<Vector2D>[] _fieldsToConsider;
        internal int _fieldsIdx;
        internal CurrentPlan _currentPlan = CurrentPlan.SearchPiece;
        internal LastDecision _lastDecision = LastDecision.None;
        internal Vector2D _currentGoal;
        internal Random _rand = new Random();

        public FastDiscoverAndCheck(ILogger<FastDiscoverAndCheck> logger)
        {
            _logger = logger;
        }
        public void Init(Player player)
        {
            _player = player;
            _boardHeight = _player.Board.GetLength(0);
            _boardLength = _player.Board.GetLength(1);

            if (player.Teammates != null)
            {
                _relativeId = FindRelativeId(player._id, player.Teammates);
                _playerCount = player.Teammates.Length + 1;
            }
            else
            {
                _relativeId = 0;
                _playerCount = 1;
            }

            _fieldsIdx = _relativeId;
            _fieldsToConsider = new List<Vector2D>[_playerCount];
            for(int i = 0; i < _fieldsToConsider.Length; i++)
                _fieldsToConsider[i] = new List<Vector2D>();

            int assignedToID(int y, int x)
            {
                return (y * _player._goalAreaSize + x) % (_playerCount);
            }

            if(_player.Team == TeamColor.Blue)
            {
                for(int i = 0; i < _player._goalAreaSize; i++)
                {
                    for(int j = 0; j < _boardLength; j++)
                    {
                        _fieldsToConsider[assignedToID(i, j)].Add(new Vector2D() { y = i, x = j });
                    }
                }
            }
            else if(_player.Team == TeamColor.Red)
            {
                for(int i = (_boardHeight - _player._goalAreaSize); i < _boardHeight; i++)
                {
                    for(int j = 0; j < _boardLength; j++)
                    {
                        _fieldsToConsider[assignedToID(i, j)].Add(new Vector2D() { y = i, x = j });
                    }
                }
            }

            int FindRelativeId(int myId, int[] otherIds)
            {
                int[] allIds = new int[otherIds.Length + 1];
                Array.Copy(otherIds, allIds, otherIds.Length);
                allIds[allIds.Length - 1] = myId;
                Array.Sort(allIds);
                return Array.IndexOf(allIds, myId);
            }
        }


        public void MakeDecision()
        {
            if(_lastDecision == LastDecision.PickUp)
            {
                _currentPlan = CurrentPlan.CheckPiece;
                CheckPiece();
                return;
            }
            if(_currentPlan == CurrentPlan.SearchPiece && _lastDecision == LastDecision.None)
            {
                Discover();
                return;
            }
            if(_currentPlan == CurrentPlan.SearchPiece && _lastDecision == LastDecision.Discover)
            {
                SearchPiece();
                return;
            }
            if(_currentPlan == CurrentPlan.SearchPiece && _lastDecision == LastDecision.Move)
            {
                Discover();
                return;
            }
            if(_currentPlan == CurrentPlan.SearchPiece && _lastDecision == LastDecision.Place)
            {
                Discover();
                return;
            }
            if(_currentPlan == CurrentPlan.GoToGoalsArea)
            {
                GoToGoal();
                return;
            }
            if(_currentPlan == CurrentPlan.CheckPiece && _lastDecision == LastDecision.Check)
            {
                ProcessCheck();
                return;
            }
        }
        internal void SearchPiece()
        {
            if(_player.PieceInfo != PieceState.NoPiece)
            {
                _currentPlan = CurrentPlan.CheckPiece;
                CheckPiece();
                return;
            }
            var distance = FindClosest();
            if(distance.Item1 == Dir.NONE)
            {
                Discover();
                return;
            }
            if(distance.Item1 == Dir.C)
            {
                PickupPiece();
                return;
            }
            _lastDecision = LastDecision.Move;

            /* Losowanie w którą stronę pójdziemy jeśli do kawałka najbliżej jest na skos umożliwia
             * wyjście z niektórych blokad, na przykład:
             * P - player, B - blokada, K - kawałek, | - ściana, _ - puste pole
             * | P_
             * | B_
             * | K_
             * Jeśli P po niemożliwym ruchu w dół odbije w prawo i użyje Discover, zobaczy że najlepiej wybrać
             * kierunek SW. Gdyby SW zawsze przechodziło na W, to wracałby do stanu początkowego.
             * 
             */
            if (_rand.Next(2) == 0)
                switch (distance.Item1) // skosy przechodzą zgodnie z ruchem wskazówek zegara
                {
                    case Dir.NW:
                    case Dir.N:
                        Move(Direction.North);
                        return;
                    case Dir.NE:
                    case Dir.E:
                        Move(Direction.East);
                        return;
                    case Dir.SE:
                    case Dir.S:
                        Move(Direction.South);
                        return;
                    case Dir.SW:
                    case Dir.W:
                        Move(Direction.West);
                        return;
                }
            else
                switch (distance.Item1) // skosy przechodzą przeciwnie do ruchu wskazówek zegara
                {
                    case Dir.NE:
                    case Dir.N:
                        Move(Direction.North);
                        return;
                    case Dir.SE:
                    case Dir.E:
                        Move(Direction.East);
                        return;
                    case Dir.SW:
                    case Dir.S:
                        Move(Direction.South);
                        return;
                    case Dir.NW:
                    case Dir.W:
                        Move(Direction.West);
                        return;
                }
        }

        internal void Discover()
        {
            _lastDecision = LastDecision.Discover;
            _player.Discovery();
        }

        internal void CheckPiece()
        {
            _lastDecision = LastDecision.Check;
            _player.CheckPiece();
        }
        internal void GoToGoal()
        {
            _lastDecision = LastDecision.Move;
            Dir directionToMove = DetermineDir();

            switch (directionToMove)
            {
                case Dir.C:
                    _currentPlan = CurrentPlan.PlacePiece;
                    PlacePiece();
                    return;
                case Dir.N:
                    Move(Direction.North);
                    return;
                case Dir.E:
                    Move(Direction.East);
                    return;
                case Dir.W:
                    Move(Direction.West);
                    return;
                case Dir.S:
                    Move(Direction.South);
                    return;
                case Dir.NONE:
                    _logger.LogWarning("STRATEGY ERROR: No way to move for goal???");
                    _player.Stop();
                    return;
            }
        }

        internal void ProcessCheck()
        {
            switch (_player.PieceInfo)
            {
                case PieceState.NoPiece:
                    _currentPlan = CurrentPlan.SearchPiece;
                    Discover();
                    return;
                case PieceState.NotSham:
                    _currentPlan = CurrentPlan.GoToGoalsArea;
                    FindTarget();
                    GoToGoal();
                    return;
                case PieceState.HasUnknown:
                    _logger.LogWarning("STRATEGY ERROR: Unknown piece state after checking???");
                    _player.Stop();
                    return;
            }
        }
        internal void PickupPiece()
        {
            _lastDecision = LastDecision.PickUp;
            _player.PickupPiece();
            return;
        }
        internal void PlacePiece()
        {
            _lastDecision = LastDecision.Place;
            _currentPlan = CurrentPlan.SearchPiece;
            _fieldsToConsider[_fieldsIdx].RemoveAt(_fieldsToConsider[_fieldsIdx].Count - 1);
            _player.PlacePiece();
            return;
        }
        internal void Move(Direction dir)
        {
            if(_player._couldMove == false)
            {
                MoveInRandomDir();
                return;
            }
            else
            {
                _player.Move(dir);
            }
        }
        internal void MoveInRandomDir()
        {
            List<Direction> possibleDirs = new List<Direction>();
            if (_player.Position.y + 1 < _boardHeight) possibleDirs.Add(Direction.North);
            if (_player.Position.x - 1 >= 0) possibleDirs.Add(Direction.West);
            if (_player.Position.y - 1 >= 0) possibleDirs.Add(Direction.South);
            if (_player.Position.x < _boardLength) possibleDirs.Add(Direction.East);

            int index = _rand.Next(possibleDirs.Count);
            _player.Move(possibleDirs[index]);

        }
        internal (Dir, int?) FindClosest()
        {
            int?[,] tempArray = new int?[3, 3];
            for (int i = _player.Position.y + 1, tempi = 0; tempi < 3; i--, tempi++)
            {
                for (int j = _player.Position.x - 1, tempj = 0; tempj < 3; j++, tempj++)
                {
                    if (i >= 0 && i < _player.Board.GetLength(0) && j >= 0 && j < _player.Board.GetLength(1))
                    {
                        tempArray[tempi, tempj] = _player.Board[i, j].DistToPiece;
                    }
                    else
                    {
                        tempArray[tempi, tempj] = null;
                    }
                }
            }
            int indexi = -1;
            int indexj = -1;
            int? min = null;
            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    if(tempArray[i,j].HasValue && min.HasValue)
                    {
                        if (tempArray[i, j] < min)
                        {
                            min = tempArray[i, j];
                            indexi = i;
                            indexj = j;
                        }
                    }
                    else if (tempArray[i, j].HasValue)
                    {
                        min = tempArray[i, j];
                        indexi = i;
                        indexj = j;
                    }
                }
            }
            Dir dirRetVal = Dir.NONE;
            var test = (indexi, indexj);
            switch (test)
            {
                case var t when t == (-1, -1):
                    dirRetVal = Dir.NONE;
                    break;
                case var t when t == (0,0):
                    dirRetVal = Dir.NW;
                    break;
                case var t when t == (0, 1):
                    dirRetVal = Dir.N;
                    break;
                case var t when t == (0, 2):
                    dirRetVal = Dir.NE;
                    break;
                case var t when t == (1, 0):
                    dirRetVal = Dir.W;
                    break;
                case var t when t == (1, 1):
                    dirRetVal = Dir.C;
                    break;
                case var t when t == (1, 2):
                    dirRetVal = Dir.E;
                    break;
                case var t when t == (2, 0):
                    dirRetVal = Dir.SW;
                    break;
                case var t when t == (2, 1):
                    dirRetVal = Dir.S;
                    break;
                case var t when t == (2, 2):
                    dirRetVal = Dir.SE;
                    break;
            }
            return (dirRetVal, min);
        }
        internal void FindTarget()
        {
            for (int i = 0; i < _playerCount; i++)
            {
                if (_fieldsToConsider[_fieldsIdx].Count == 0) // pola przypisane do danego id zostały pokryte
                    _fieldsIdx = (_fieldsIdx + 1) % _playerCount;
                else break;
            }

            if (_fieldsToConsider[_fieldsIdx].Count == 0) // wszystkie pola pokryte
            {
                _logger.LogWarning("STRATEGY ERROR: No targets???");
                _player.Stop();
            }
            else
            {
                var currentFields = _fieldsToConsider[_fieldsIdx];
                _currentGoal = currentFields[currentFields.Count - 1];
            }
        }
        internal Dir DetermineDir()
        {
            if ((_player.Position.y == _currentGoal.y) && (_player.Position.x == _currentGoal.x)) return Dir.C;
            else if ((_player.Position.y == _currentGoal.y) && (_player.Position.x < _currentGoal.x)) return Dir.E;
            else if ((_player.Position.y == _currentGoal.y) && (_player.Position.x > _currentGoal.x)) return Dir.W;
            else if ((_player.Position.y < _currentGoal.y)) return Dir.N;
            else if ((_player.Position.y > _currentGoal.y)) return Dir.S;
            return Dir.NONE;
        }
    }
}
