using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class TTTGameMode : MonoBehaviour
{
    public delegate void OnGameStateChangedDelegate(GameStage now);

    public event OnGameStateChangedDelegate OnGameStateChanged;

    public delegate void OnGameStartDelegate(GameRules gameRules, TTTPlayer[] players);

    public event OnGameStartDelegate OnGameStart;

    //winner==null：平局
    public delegate void OnGameEndDelegate(TTTPlayer winner);

    public event OnGameEndDelegate OnGameEnd;

    public delegate void OnNewTurnDelegate(TTTPlayer ofWhichPlayer);

    public event OnNewTurnDelegate OnNewTurn;

    public delegate void OnNewRoundDelegate(int round);

    public event OnNewRoundDelegate OnNewRound;

    Camera _mainCamera;

    public enum GameStage
    {
        MainMenu,
        WaitForCheckerBoard,
        GameOver
    }

    public enum PlayerSeat
    {
        NotAllowed,
        Human,
        Ai,
        None
    }


    //游戏规则
    public struct GameRules
    {
        private int _gridsXY; //几个格子？（3-16）
        private int _chessCountToWin; //连成多长能赢？（3~5，不能小于格子数）
        private List<PlayerSeat> _playerSeats; //玩家座位，有四个座位。分别可以坐人、坐Ai、不坐人、坐不了人

        public int GridsXY
        {
            get => _gridsXY;
            set
            {
                _gridsXY = value;
                DoLegalCheck();
            }
        }

        public int ChessCountToWin
        {
            get => _chessCountToWin;
            set
            {
                _chessCountToWin = value;
                DoLegalCheck();
            }
        }

        public List<PlayerSeat> PlayerSeats
        {
            get => _playerSeats;
            set
            {
                _playerSeats = value;
                DoLegalCheck();
            }
        }

        public bool IsValidSeat(PlayerSeat seat)
        {
            return seat is PlayerSeat.Ai or PlayerSeat.Human;
        }

        public bool IsValidSeat(int seat)
        {
            if (_playerSeats.Count <= seat)
            {
                return false;
            }

            return IsValidSeat(_playerSeats[seat]);
        }

        public int GetValidSeatCount()
        {
            int count = 0;
            for (int i = 0; i < _playerSeats.Count; i++)
            {
                if (IsValidSeat(_playerSeats[i]))
                {
                    count++;
                }
            }

            return count;
        }

        //检查规则合法性
        private void DoLegalCheck()
        {
            //格子数限制
            _gridsXY = Mathf.Clamp(_gridsXY, 3, 16);
            //连线子数不能大于格子数
            _chessCountToWin = Mathf.Clamp(_chessCountToWin, 3, Mathf.Min(5, _gridsXY));
            //限制游戏人数
            if (_playerSeats.Count != 4)
            {
                _playerSeats = new List<PlayerSeat> { PlayerSeat.Human, PlayerSeat.Ai, PlayerSeat.NotAllowed, PlayerSeat.NotAllowed };
            }

            for (int i = 0; i < _playerSeats.Count; i++)
            {
                if (_playerSeats[i] == PlayerSeat.NotAllowed)
                {
                    _playerSeats[i] = PlayerSeat.None;
                }
            }

            int seatLimit = 2;
            if (_gridsXY > 10)
            {
                seatLimit = 4;
            }
            else if (_gridsXY > 6)
            {
                seatLimit = 3;
            }

            for (int i = seatLimit; i < 4; i++)
            {
                _playerSeats[i] = PlayerSeat.NotAllowed;
            }
        }

        public GameRules(int gridsXY = 3, int chessCountToWin = 3)
        {
            _gridsXY = gridsXY;
            _chessCountToWin = chessCountToWin;
            _playerSeats = new List<PlayerSeat> { PlayerSeat.Human, PlayerSeat.Ai, PlayerSeat.NotAllowed, PlayerSeat.NotAllowed };
            DoLegalCheck();
        }
    }

    //Singleton
    public static TTTGameMode Instance;

    //Default values
    [SerializeField] public Sprite[] playerSymbols;
    public GameRules Rules;

    //游戏状态
    private GameStage _currentStage = GameStage.MainMenu;

    public GameStage CurrentStage
    {
        get { return _currentStage; }
    }

    public TTTPlayer[] players;
    public TTTPlayer activePlayer;
    public int currentRound = 0;

    //游戏整体状态机
    void SetStage(GameStage stage)
    {
        switch (_currentStage)
        {
            case GameStage.MainMenu:
                if (stage == GameStage.WaitForCheckerBoard)
                {
                    ManipulateCamera(stage);
                }

                break;
            case GameStage.WaitForCheckerBoard:
                if (stage == GameStage.GameOver)
                {
                }

                if (stage == GameStage.MainMenu)
                {
                    ManipulateCamera(stage);
                }

                break;
            case GameStage.GameOver:
                if (stage == GameStage.MainMenu)
                {
                    ManipulateCamera(stage);
                }

                break;
        }

        OnGameStateChanged?.Invoke(stage);
        _currentStage = stage;
    }


    void Awake()
    {
        Instance = this;
        _mainCamera = Camera.main;
        Rules = new GameRules(3, 3);
    }

    //开始游戏的地方
    public void StartGame()
    {
        players = new TTTPlayer[Rules.GetValidSeatCount()];
        int valid_i = 0;
        for (int i = 0; i < Rules.PlayerSeats.Count; i++)
        {
            if (Rules.IsValidSeat(i))
            {
                var player = transform.gameObject.AddComponent<TTTPlayer>();
                player.name = "Player" + valid_i;
                player.index = valid_i;
                player.symbol = playerSymbols[i];
                players[valid_i] = player;
                player.ai = Rules.PlayerSeats[i] == PlayerSeat.Ai;
                valid_i++;
            }
        }

        SetStage(GameStage.WaitForCheckerBoard);
        OnGameStart?.Invoke(Rules, players);
        Invoke(nameof(ProceedNextTurn), 1.0f);
    }

    //下一回合
    void ProceedNextTurn()
    {
        bool newRound = false;
        if (_currentStage != GameStage.WaitForCheckerBoard)
        {
            return;
        }

        if (activePlayer == null)
        {
            activePlayer = players[0];
            newRound = true;
        }
        else
        {
            if (activePlayer.index == players.Length)
            {
                newRound = true;
                currentRound++;
            }
            var newPlayerIndex = (activePlayer.index + players.Length + 1) % players.Length;
            activePlayer = players[newPlayerIndex];
        }
        OnNewTurn?.Invoke(activePlayer);
        if (newRound)
        {
            OnNewRound?.Invoke(currentRound);
        }
        
    }

    //处理落子后的结果
    public void DealWithPutChessResult(TTTCheckerBoard.GameResult result)
    {
        if (_currentStage != GameStage.WaitForCheckerBoard)
        {
            return;
        }
        switch (result)
        {
            case TTTCheckerBoard.GameResult.ActivePlayerWins:
                SetStage(GameStage.GameOver);
                OnNewTurn?.Invoke(null);
                OnGameEnd?.Invoke(activePlayer);
                break;
            case TTTCheckerBoard.GameResult.Draw:
                SetStage(GameStage.GameOver);
                OnNewTurn?.Invoke(null);
                OnGameEnd?.Invoke(null);
                break;
            case TTTCheckerBoard.GameResult.Continue:
                ProceedNextTurn();
                break;
        }
    }

    //手动中止游戏。游戏自动中止后点击屏幕也会调用
    public void TerminateGame()
    {
        if (_currentStage == GameStage.WaitForCheckerBoard)
        {
            activePlayer = null;
            OnNewTurn?.Invoke(activePlayer);
            OnGameEnd?.Invoke(null);
        }

        foreach (var player in players)
        {
            Destroy(player);
        }

        players = null;

        SetStage(GameStage.MainMenu);
    }

    //移动相机，在主菜单和游戏页面切换
    void ManipulateCamera(GameStage toStage)
    {
        if (toStage == GameStage.MainMenu)
        {
            StopCoroutine(nameof(AnimateCameraTo));
            StartCoroutine(nameof(AnimateCameraTo), 0.8f);
        }
        else
        {
            StopCoroutine(nameof(AnimateCameraTo));
            StartCoroutine(nameof(AnimateCameraTo), 2.56f);
        }
    }

    IEnumerator AnimateCameraTo(float size)
    {
        yield return new WaitForSeconds(0.3f);
        while (Mathf.Abs(_mainCamera.orthographicSize - size) > 0.01f)
        {
            _mainCamera.orthographicSize = Mathf.Lerp(_mainCamera.orthographicSize, size, Time.deltaTime * 10f);
            yield return null;
        }
    }
}