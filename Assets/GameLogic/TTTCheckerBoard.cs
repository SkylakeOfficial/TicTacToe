using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class TTTCheckerBoard : MonoBehaviour
{
    //极大极小树节点
    class GameNode
    {
        private int depth;
        public static int ChessToWin = 3;
        private int[,] _board;
        private List<Vector2Int> _sparedGrids;
        public Vector2Int _thisgrid;
        public GameNode parent;
        public List<GameNode> children = new();
        public int score;
        private TTTPlayer _player;
        private TTTPlayer _nextPlayer;
        private TTTPlayer[] _players;
        private int _playerid;
        private int[] _totalPlayerIds;
        private int _iterLeft;

        public GameNode(GameNode parent, Vector2Int thisgrid, TTTPlayer player, TTTPlayer[] players, int iterLeft, List<Vector2Int> sparedGrids, int[,] board, int depth = 0)
        {
            this.parent = parent;
            _thisgrid = thisgrid;
            _player = player;
            _playerid = _player.index;
            _players = players;
            _totalPlayerIds = new int[players.Length];
            this.depth = depth + 1;
            for (int i = 0; i < players.Length; i++)
            {
                _totalPlayerIds[i] = players[i].index;
            }

            _board = board;
            _iterLeft = iterLeft;
            _sparedGrids = sparedGrids;
            BuildChildren();
        }


        void BuildChildren()
        {
            if (_player == _players.Last())
            {
                _nextPlayer = _players.First();
                _iterLeft--;
            }
            else
            {
                for (int i = 0; i < _players.Length; i++)
                {
                    if (_players[i] == _player)
                    {
                        _nextPlayer = _players[i + 1];
                        break;
                    }
                }
            }

            _sparedGrids.Remove(_thisgrid);
            _board[_thisgrid.x, _thisgrid.y] = _playerid;

            bool end = false;
            switch (ChessToWin)
            {
                case 3:
                    end = Score3();
                    break;
                case 4:
                    end = Score4();
                    break;
                case 5:
                    end = Score5();
                    break;
            }

            if (_sparedGrids.Count == 0)
            {
                if (!end)
                {
                    score = 0;
                    return;
                }

                return;
            }

            //如果是末端节点
            if ((_iterLeft == 0 && _player == TTTGameMode.Instance.activePlayer) || end)
            {
                return;
            }

            bool first = true;
            for (int i = 0; i < _sparedGrids.Count; i++)
            {
                var child = new GameNode(this, _sparedGrids[i], _nextPlayer, _players, _iterLeft, _sparedGrids, _board, depth);
                //不是末端节点的话，直接查找child里的最优分数，省得后头还做DFS
                if (first)
                {
                    score = child.score;
                    first = false;
                    continue;
                }

                //是min还是max？选手是自己则max
                if (_player == TTTGameMode.Instance.activePlayer)
                {
                    score = child.score > score ? child.score : score;
                }
                else
                {
                    score = child.score < score ? child.score : score;
                }
            }
        }

        int GetGridSafe(int offsetX, int offsetY)
        {
            var targetX = _thisgrid.x + offsetX;
            var targetY = _thisgrid.y + offsetY;
            if (targetX < 0 || targetY < 0 || targetX >= _board.GetLength(0) || targetY >= _board.GetLength(1))
            {
                return -2;
            }

            return _board[targetX, targetY];
        }

        bool Score3()
        {
            score = 0;
            int[] scoreVSCount = { 0, 1, 10, 100 };
            foreach (var grid in _board)
            {
                int[][] lines =
                {
                    new[] { grid, GetGridSafe(1, 1), GetGridSafe(2, 2) },
                    new[] { grid, GetGridSafe(-1, 1), GetGridSafe(-2, 2) },
                    new[] { grid, GetGridSafe(1, 0), GetGridSafe(2, 0) },
                    new[] { grid, GetGridSafe(0, 1), GetGridSafe(0, 2) },
                };
                foreach (var line in lines)
                {
                    var selfCount = 0;
                    var opCount = 0;
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (line[i] == TTTGameMode.Instance.activePlayer.index)
                        {
                            selfCount++;
                        }
                        else if (line[i] == -2)
                        {
                            break;
                        }
                        else
                        {
                            opCount++;
                        }
                    }

                    if (opCount == 3)
                    {
                        score = -100 + depth;
                        return true;
                    }

                    if (selfCount == 3)
                    {
                        score = 100 - depth;
                        return true;
                    }
                }
            }

            return false;
        }

        bool Score4()
        {
            int[] scoreVSCount = { 0, 10, 100, 1000, 10000 };
            foreach (var grid in _board)
            {
                int[][] lines =
                {
                    new[] { grid, GetGridSafe(1, 1), GetGridSafe(2, 2), GetGridSafe(3, 3) },
                    new[] { grid, GetGridSafe(-1, 1), GetGridSafe(-2, 2), GetGridSafe(-3, 3) },
                    new[] { grid, GetGridSafe(1, 0), GetGridSafe(2, 0), GetGridSafe(3, 0) },
                    new[] { grid, GetGridSafe(0, 1), GetGridSafe(0, 2), GetGridSafe(0, 3) }
                };
                score = 0;
                foreach (var line in lines)
                {
                    var selfCount = 0;
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (line[i] == TTTGameMode.Instance.activePlayer.index)
                        {
                            selfCount++;
                        }
                        else if (line[i] == -2)
                        {
                        }
                    }
                    score += scoreVSCount[selfCount];
                }
            }

            return false;
        }

        bool Score5()
        {
            int[] scoreVSCount = { 0, 10, 100, 1000, 10000, 100000 };
            int self = _playerid;
            int[][] lines =
            {
                new[] { GetGridSafe(-2, -2), GetGridSafe(-1, -1), self, GetGridSafe(1, 1), GetGridSafe(2, 2) },
                new[] { GetGridSafe(2, -2), GetGridSafe(1, -1), self, GetGridSafe(-1, 1), GetGridSafe(-2, 2) },
                new[] { GetGridSafe(-2, 0), GetGridSafe(-1, 0), self, GetGridSafe(1, 0), GetGridSafe(2, 0) },
                new[] { GetGridSafe(0, -2), GetGridSafe(0, -1), self, GetGridSafe(0, 1), GetGridSafe(0, 2) }
            };
            score = 0;
            foreach (var line in lines)
            {
                foreach (var player in _players)
                {
                    int scoreOfPlayer = 0;
                    var selfCount = 0;
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (line[i] == self)
                        {
                            selfCount++;
                        }
                        else if (line[i] == -2)
                        {
                            scoreOfPlayer -= 100;
                        }
                    }

                    scoreOfPlayer += scoreVSCount[selfCount];
                    score += player == _player ? scoreOfPlayer : -scoreOfPlayer;
                }
            }

            return false;
        }
    }

    public enum GameResult
    {
        Continue,
        ActivePlayerWins,
        Draw
    }

    //棋盘格子
    private CheckerGrid[,] _grids;
    List<Vector2Int> _spared = new(); //空闲的
    List<CheckerGrid> _filled = new(); //下了棋子的
    private int _gridCount;

    //格子间距
    [SerializeField] private float edgePadding = 0.1f;
    public float EdgePadding => Unity.Mathematics.math.remap(6f, 15f, 1f, 0.3f, _gridCount) * edgePadding;
    [SerializeField] GameObject gridPrefab;


    void Start()
    {
        TTTGameMode.Instance.OnGameStart += GameStart;
        TTTGameMode.Instance.OnGameStateChanged += ResponseGameStateChange;
        TTTGameMode.Instance.OnNewTurn += OnNewPlayerTurn;
    }

    private void OnNewPlayerTurn(TTTPlayer ofwhichplayer)
    {
        if (ofwhichplayer == null)
        {
            return;
        }

        if (ofwhichplayer.ai)
        {
            StartCoroutine(nameof(MakeAiDecisions));
        }
    }

    private void ResponseGameStateChange(TTTGameMode.GameStage now)
    {
        if (now == TTTGameMode.GameStage.MainMenu)
        {
            StopCoroutine(nameof(AsyncMakeGrids));
            EmptyGrids();
        }
    }

    private void GameStart(TTTGameMode.GameRules gamerules, TTTPlayer[] players)
    {
        _gridCount = gamerules.GridsXY;
        MakeGrids();
    }


    //创建棋盘格
    private void MakeGrids()
    {
        GameNode.ChessToWin = TTTGameMode.Instance.Rules.ChessCountToWin;
        var spriteRenderer = GetComponent<SpriteRenderer>();
        var boardBounds = spriteRenderer.bounds;
        boardBounds.Expand(-EdgePadding);
        StartCoroutine(nameof(AsyncMakeGrids), boardBounds);
    }

    //异步生成格子，可以播一下动画
    private IEnumerator AsyncMakeGrids(Bounds bounds)
    {
        _grids = new CheckerGrid[_gridCount, _gridCount];

        for (int i = 0; i < _gridCount; i++)
        {
            for (int j = 0; j < _gridCount; j++)
            {
                var waitFor = 1.0f / (_gridCount * _gridCount);
                yield return new WaitForSeconds(waitFor);
                Vector3 spawnPosition = bounds.size / _gridCount;
                spawnPosition.x *= j + 0.5f;
                spawnPosition.y *= i + 0.5f;
                spawnPosition += bounds.center - bounds.extents;
                GameObject grid = Instantiate(gridPrefab, spawnPosition, transform.rotation);
                var gridScript = grid.GetComponent<CheckerGrid>();
                if (gridScript)
                {
                    gridScript.Init(j, i, bounds.size.x / _gridCount - EdgePadding);
                    gridScript.OnClick += OnPutChessAttempt;
                    _grids[j, i] = gridScript;
                }
            }
        }

        //格子的相邻格
        for (int i = 0; i < _gridCount; i++)
        {
            for (int j = 0; j < _gridCount; j++)
            {
                _grids[j, i].neighbors = new[,]
                {
                    { GetNeighborSafe(j - 1, i - 1), GetNeighborSafe(j, i - 1), GetNeighborSafe(j + 1, i - 1) },
                    { GetNeighborSafe(j - 1, i), null, GetNeighborSafe(j + 1, i) },
                    { GetNeighborSafe(j - 1, i + 1), GetNeighborSafe(j, i + 1), GetNeighborSafe(j + 1, i + 1) }
                };
            }
        }

        foreach (var grid in _grids)
        {
            _spared.Add(grid.index);
        }
    }

    CheckerGrid GetNeighborSafe(int x, int y)
    {
        if (x < 0 || x >= _gridCount)
        {
            return null;
        }
        else if (y < 0 || y >= _gridCount)
        {
            return null;
        }

        return _grids[x, y];
    }

    //清空棋盘格，为下一局做准备
    void EmptyGrids()
    {
        foreach (var grid in _grids)
        {
            if (grid != null)
            {
                grid.PlayDestroy();
            }
        }

        _grids = null;
    }

    //试图下棋子
    void OnPutChessAttempt(int x, int y)
    {
        if (CanPutChessAt(x, y))
        {
            PutChess(x, y);
            var gameResult = CheckGameResult();
            Debug.Log(gameResult);
            TTTGameMode.Instance.DealWithPutChessResult(gameResult);
        }
    }

    //可以在这里放棋子吗？
    private bool CanPutChessAt(int x, int y)
    {
        bool result = true;
        result &= TTTGameMode.Instance.CurrentStage == TTTGameMode.GameStage.WaitForCheckerBoard;
        result &= TTTGameMode.Instance.activePlayer != null;
        result &= _grids[x, y].PosessedBy == null;
        return result;
    }

    //在这里下棋子
    private void PutChess(int x, int y)
    {
        _grids[x, y].PosessedBy = TTTGameMode.Instance.activePlayer;
    }

    //去掉这里的棋子 不一定用得到
    private void RemoveChess(int x, int y)
    {
        _grids[x, y].PosessedBy = null;
    }

    //检查棋局结果：继续？胜负已分？平局？
    private GameResult CheckGameResult()
    {
        _spared.Clear();
        _filled.Clear();
        //先看看有没有玩家赢
        foreach (var grid in _grids)
        {
            if (grid.PosessedBy == null)
            {
                _spared.Add(grid.index);
            }
            else
            {
                _filled.Add(grid);
            }
        }

        foreach (var grid in _filled)
        {
            int id = grid.PosessedBy.index;

            //这颗棋如果不是active player下的，要是能赢前面早赢了
            if (id != TTTGameMode.Instance.activePlayer.index)
            {
                continue;
            }

            //朝四个方向搜索连线
            Vector2Int[] searchDirs = { new(1, 0), new(1, 1), new(0, 1), new(-1, 1) };

            foreach (var dir in searchDirs)
            {
                int count = 1;
                for (int i = 1; i < TTTGameMode.Instance.Rules.ChessCountToWin; i++)
                {
                    var searchid = Id(Expand(grid, dir.x * i, dir.y * i));
                    if (searchid == id)
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (count >= TTTGameMode.Instance.Rules.ChessCountToWin)
                {
                    return GameResult.ActivePlayerWins;
                }
            }
        }

        //没赢家，也没地儿下了
        if (_spared.Count == 0)
        {
            return GameResult.Draw;
        }

        return GameResult.Continue;
    }

    //将该格转化为格子id。空或无格子：-1，否则：对应玩家的id
    public static int Id(CheckerGrid grid)
    {
        if (grid == null)
        {
            return -1;
        }

        if (grid.PosessedBy == null)
        {
            return -1;
        }

        return grid.PosessedBy.index;
    }

    //返回格子方向上的邻居，没有则返回null
    public static CheckerGrid Expand(CheckerGrid grid, int dirX, int dirY)
    {
        CheckerGrid result = grid;
        while (dirX != 0 || dirY != 0)
        {
            var xOffset = dirX == 0 ? 0 : (dirX > 0 ? 1 : -1);
            var yOffset = dirY == 0 ? 0 : (dirY > 0 ? 1 : -1);
            result = result.neighbors[1 + xOffset, 1 + yOffset];
            dirX -= xOffset;
            dirY -= yOffset;
            if (result == null)
            {
                return null;
            }
        }

        return result;
    }

    //Ai决策
    IEnumerator MakeAiDecisions()
    {
        //先顿一下，下太快了看不清
        yield return new WaitForSeconds(0.5f);
        int iterations = 5;
        var playercount = TTTGameMode.Instance.Rules.GetValidSeatCount();
        //人多我们就少算几步
        if (playercount > 2)
        {
            iterations--;
        }

        if (playercount > 3)
        {
            iterations--;
        }

        //把棋盘数组换成玩家id的数组。-1就是空格。
        int[,] board = new int[_gridCount, _gridCount];
        for (int i = 0; i < _gridCount; i++)
        {
            for (int j = 0; j < _gridCount; j++)
            {
                if (_grids[i, j].PosessedBy != null)
                {
                    board[i, j] = _grids[i, j].PosessedBy.index;
                }
                else
                {
                    board[i, j] = -1;
                }
            }
        }

        GameNode bestNode = null;
        for (int i = 0; i < _spared.Count; i++)
        {
            GameNode node = new GameNode(null, _spared[i], TTTGameMode.Instance.activePlayer, TTTGameMode.Instance.players.ToArray(), iterations, _spared.ToList(), board);
            if (bestNode == null)
            {
                bestNode = node;
            }
            else
            {
                bestNode = node.score > bestNode.score ? node : bestNode;
            }

            Debug.Log(node._thisgrid.ToString() + node.score);
        }

        System.Diagnostics.Debug.Assert(bestNode != null, nameof(bestNode) + " != null");
        OnPutChessAttempt(bestNode._thisgrid.x, bestNode._thisgrid.y);
    }
}