using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SeatToggle : MonoBehaviour
{

    TTTGameMode.PlayerSeat thisSeat;
    [SerializeField]
    public int index;
    [SerializeField]
    Sprite seatSprite;
    [SerializeField]
    Sprite playerSprite;
    [SerializeField]
    Sprite aiSprite;
    [SerializeField]
    Sprite emptySprite;
    

    public delegate void OnToggleDelegate(UI_SeatToggle seat, TTTGameMode.PlayerSeat thisSeat);
    public event OnToggleDelegate OnToggle;
    
    void Start()
    {
        gameObject.GetComponent<Image>().sprite = seatSprite;
    }

    public void Refresh(TTTGameMode.PlayerSeat seat)
    {
        if (seat == TTTGameMode.PlayerSeat.NotAllowed)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
        }

        var seatImg = gameObject.GetComponent<Image>();
        var playerImg = transform.Find("PlayerBotMark").gameObject.GetComponent<Image>();
        
        switch (seat)
        {
            case TTTGameMode.PlayerSeat.None:
                seatImg.color = new Color(1, 1, 1, 0.1f);
                playerImg.sprite = emptySprite;
                break;
            case TTTGameMode.PlayerSeat.Human:
                seatImg.color = Color.white;
                playerImg.sprite = playerSprite;
                break;
            case TTTGameMode.PlayerSeat.Ai:
                seatImg.color = Color.white;
                playerImg.sprite = aiSprite;
                break;
        }
        thisSeat = seat;
    }

    private void OnMouseDown()
    {
        switch (thisSeat)
        {
            case TTTGameMode.PlayerSeat.NotAllowed:
                break;
            case TTTGameMode.PlayerSeat.None:
                thisSeat = TTTGameMode.PlayerSeat.Human;
                break;
            case TTTGameMode.PlayerSeat.Human:
                thisSeat = TTTGameMode.PlayerSeat.Ai;
                break;
            case TTTGameMode.PlayerSeat.Ai:
                thisSeat = TTTGameMode.PlayerSeat.None;
                break;
        }
        OnToggle?.Invoke(this, thisSeat);
    }
}
