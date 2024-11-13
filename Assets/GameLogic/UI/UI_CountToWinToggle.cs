using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_CountToWinToggle : MonoBehaviour
{
    [SerializeField] Sprite threeSprite;
    [SerializeField] Sprite fourSprite;
    [SerializeField] Sprite fiveSprite;

    private int thisCount = 3;

    public delegate void OnToggleDelegate(int count);

    public event OnToggleDelegate OnToggle;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(ToggleCountToWin);
    }

    void ToggleCountToWin()
    {
        thisCount += 1;
        thisCount = (thisCount + 3) % 3 + 3;
        OnToggle?.Invoke(thisCount);
    }

    public void Refresh(int count)
    {
        thisCount = count;
        Sprite sprite = threeSprite;
        switch (thisCount)
        {
            case 3:
                sprite = threeSprite;
                break;
            case 4:
                sprite = fourSprite;
                break;
            case 5:
                sprite = fiveSprite;
                break;
        }
        GetComponent<Image>().sprite = sprite;
    }
}