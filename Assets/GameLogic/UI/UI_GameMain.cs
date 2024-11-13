using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

public class UI_GameMain : MonoBehaviour
{
    UI_GridCountToggle gridCountToggle;
    UI_CountToWinToggle countToWinToggle;
    List<UI_SeatToggle> seatToggles = new();

    [SerializeField] GameObject playerListUI;

    void Start()
    {
        TTTGameMode.Instance.OnGameStart += StartGame;
        TTTGameMode.Instance.OnGameEnd += EndGame;
        TTTGameMode.Instance.OnGameStateChanged += ResponseStateChange;
        SetupMainScreenDelegates();
    }

    void SetupMainScreenDelegates()
    {
        transform.Find("MainScreenOver/StartButton").gameObject.GetComponent<Button>().onClick.AddListener(OnClickStart);
        gridCountToggle = transform.Find("MainScreenOver/Btn_NumGrids").gameObject.GetComponent<UI_GridCountToggle>();
        gridCountToggle.OnToggle += OnToggleGrids;
        countToWinToggle = transform.Find("MainScreenOver/Btn_NumChessToWin").gameObject.GetComponent<UI_CountToWinToggle>();
        countToWinToggle.OnToggle += OnToggleChessToWin;
        foreach (Transform child in transform.Find("MainScreenOver/PlayerSeats"))
        {
            seatToggles.Add(child.gameObject.GetComponent<UI_SeatToggle>());
            child.gameObject.GetComponent<UI_SeatToggle>().OnToggle += OnToggleSeat;
        }

        RefreshMainScreenGameRules();
    }


    void StartGame(TTTGameMode.GameRules gameRules, TTTPlayer[] players)
    {
        transform.Find("Btn_Return").gameObject.GetComponent<Button>().onClick.AddListener(OnClickReturn);
        //Generate player list
        var list = transform.Find("PlayerList");
        foreach (var player in players)
        {
            var wgt = Instantiate(playerListUI, list, false);
            //wgt.transform.parent = list;
            wgt.GetComponent<UI_PlayerWidget>().Init(player);
        }

        //Hide my title
        StopCoroutine(nameof(PlayTitleScreenFade));
        StartCoroutine(nameof(PlayTitleScreenFade), false);
    }

    void ResponseStateChange(TTTGameMode.GameStage gameStage)
    {
        if (gameStage == TTTGameMode.GameStage.MainMenu)
        {
            transform.Find("Btn_Return").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            foreach (Transform child in transform.Find("PlayerList"))
            {
                Destroy(child.gameObject);
            }

            transform.Find("Sprite_UI_Draw").gameObject.SetActive(false);
            transform.Find("Sprite_UI_Wins").gameObject.SetActive(false);
            //Return to title
            StopCoroutine(nameof(PlayTitleScreenFade));
            StartCoroutine(nameof(PlayTitleScreenFade), true);
        }
    }

    void EndGame(TTTPlayer winner)
    {
        if (winner == null)
        {
            var drawUI = transform.Find("Sprite_UI_Draw").gameObject;
            drawUI.SetActive(true);
        }
        else
        {
            var winUI = transform.Find("Sprite_UI_Wins").gameObject;
            winUI.SetActive(true);
            winUI.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = winner.symbol;
        }
    }

    IEnumerator PlayTitleScreenFade(bool inverse = false)
    {
        yield return new WaitForSeconds(inverse ? 0.4f : 0f);
        transform.Find("MainScreenOver").gameObject.SetActive(true);
        Vector3 targetScale = inverse ? new Vector3(1f, 1f, 1f) : new Vector3(0f, 0f, 0f);
        var titleScreen = transform.Find("MainScreenOver").gameObject.GetComponent<RectTransform>();
        while (Mathf.Abs(titleScreen.localScale.x - targetScale.x) > 0.05f)
        {
            titleScreen.localScale = Vector3.Lerp(titleScreen.localScale, targetScale, Time.deltaTime * 12f);
            yield return null;
        }

        transform.Find("MainScreenOver").gameObject.SetActive(inverse);
    }


    void OnClickReturn()
    {
        TTTGameMode.Instance.TerminateGame();
    }

    void OnClickStart()
    {
        if (TTTGameMode.Instance.Rules.GetValidSeatCount() >= 2)
        {
            TTTGameMode.Instance.StartGame();
        }
    }

    void OnToggleGrids(int grids)
    {
        TTTGameMode.Instance.Rules.GridsXY = grids;
        RefreshMainScreenGameRules();
    }

    void OnToggleChessToWin(int win)
    {
        TTTGameMode.Instance.Rules.ChessCountToWin = win;
        RefreshMainScreenGameRules();
    }

    private void OnToggleSeat(UI_SeatToggle seat, TTTGameMode.PlayerSeat thisseat)
    {
        TTTGameMode.Instance.Rules.PlayerSeats[seat.index] = thisseat;
        RefreshMainScreenGameRules();
    }

    void RefreshMainScreenGameRules()
    {
        gridCountToggle.Refresh(TTTGameMode.Instance.Rules.GridsXY);
        countToWinToggle.Refresh(TTTGameMode.Instance.Rules.ChessCountToWin);
        foreach (var seat in seatToggles)
        {
            seat.Refresh(TTTGameMode.Instance.Rules.PlayerSeats[seat.index]);
        }
    }

    private void OnDestroy()
    {
        TTTGameMode.Instance.OnGameStart -= StartGame;
        TTTGameMode.Instance.OnGameEnd -= EndGame;
    }
}