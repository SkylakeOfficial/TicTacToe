using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerWidget : MonoBehaviour
{
    [SerializeField]
    Sprite playerSprite;
    [SerializeField]
    Sprite aiPlayerSprite;
    
    TTTPlayer _player;
    // Start is called before the first frame update
    void Start()
    {
        SetPlayerActive(false);
    }
    
    void SetPlayerActive(bool active)
    {
        //箭头
        transform.Find("Arrow").gameObject.SetActive(active);
    }

    public void Init(TTTPlayer player)
    {
        _player = player;
        gameObject.GetComponent<Image>().sprite = _player.symbol;
        transform.Find("PlayerBotMark").gameObject.GetComponent<Image>().sprite = _player.ai ? aiPlayerSprite : playerSprite;
        TTTGameMode.Instance.OnNewTurn += OnTurnChanged;
    }

    private void OnTurnChanged(TTTPlayer player)
    {
        if (player == _player)
        {
            SetPlayerActive(true);
        }
        else
        {
            SetPlayerActive(false);
        }
    }

    private void OnDestroy()
    {
        TTTGameMode.Instance.OnNewTurn -= OnTurnChanged;
    }
    
}
