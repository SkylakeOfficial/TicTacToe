using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GridCountToggle : MonoBehaviour
{
    
    private int[] _options = { 3, 4, 6, 10, 15 };
        
    [SerializeField] Sprite threeSprite;
    [SerializeField] Sprite fourSprite;
    [SerializeField] Sprite sixSprite;
    [SerializeField] Sprite tenSprite;
    [SerializeField] Sprite fiftSprite;

    private int _thisIndex;

    public delegate void OnToggleDelegate(int count);

    public event OnToggleDelegate OnToggle;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(ToggleCountToWin);
    }

    void ToggleCountToWin()
    {
        _thisIndex += 1;
        _thisIndex = (_thisIndex + _options.Length) % _options.Length;
        OnToggle?.Invoke(_options[_thisIndex]);
    }

    public void Refresh(int count)
    {
        for (int i = 0; i < _options.Length; i++)
        {
            if (count == _options[i])
            {
                _thisIndex = i;
                break;
            }
        }
        
        Sprite sprite = threeSprite;
        switch (count)
        {
            case 3:
                sprite = threeSprite;
                break;
            case 4:
                sprite = fourSprite;
                break;
            case 6:
                sprite = sixSprite;
                break;
            case 10:
                sprite = tenSprite;
                break;
            case 15:
                sprite = fiftSprite;
                break;
        }
        transform.GetChild(0).GetComponent<Image>().sprite = sprite;
    }
}