using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class CheckerGrid : MonoBehaviour
{
    public CheckerGrid[,] neighbors;
    
    public delegate void OnClickDelegate(int x, int y);
    public event OnClickDelegate OnClick;
    public Vector2Int index;
    private float _desiredScale;
    private float _desiredSize;
    [SerializeField] private Color EvenColor;
    [SerializeField] private Color OddColor;
    private TTTPlayer _posessedBy;
    public TTTPlayer PosessedBy
    {
        get => _posessedBy;
        set
        {
            _posessedBy = value;
            PlayPutChessEffect();
        }
    }
    
    private void OnMouseDown()
    {
        if (!TTTGameMode.Instance.activePlayer)
        {
            return;
        }

        if (TTTGameMode.Instance.activePlayer.ai)
        {
            return;
        }
        if (OnClick != null)
        {
            OnClick(index.x, index.y);
        }
    }

    private void OnMouseEnter()
    {
        if (TTTGameMode.Instance.activePlayer == null)
        {
            return;
        }
        if (!TTTGameMode.Instance.activePlayer.ai)
        {
            PlayMouseHoverEffect();
        }
    }

    private void OnMouseExit()
    {
        if (TTTGameMode.Instance.activePlayer == null)
        {
            return;
        }
        if (!TTTGameMode.Instance.activePlayer.ai)
        {
            PlayMouseExitEffect();
        }
        
    }

    void PlayMouseHoverEffect()
    {
        //TODO:音效，Fx
    }
    void PlayMouseExitEffect()
    {
        //TODO:音效，Fx
    }

    void PlayPutChessEffect()
    {
        var comp = GetComponent<SpriteRenderer>();
        if (_posessedBy == null)
        {
            comp.sprite = null;
        }
        else
        {
            comp.sprite = _posessedBy.symbol;
        }
    }

    //初始化格子
    public void Init(int indexX, int indexY, float desiredSize)
    {
        var initialSize = GetComponent<SpriteRenderer>().bounds.size.x;
        _desiredSize = desiredSize;
        _desiredScale = _desiredSize/initialSize * transform.localScale.x;
        index = new Vector2Int(indexX, indexY);
        //缩放动画
        transform.localScale = Vector3.zero;
        StopCoroutine(nameof(PlayResizeAnimation));
        StartCoroutine(nameof(PlayResizeAnimation),10.0f);
        //设置格子颜色
        SetupEvenOddColor();
    }

    //播放清除动画，然后销毁
    public void PlayDestroy()
    {
        _desiredScale = 0;
        StopCoroutine(nameof(PlayResizeAnimation));
        StartCoroutine(nameof(PlayDestroyAnimation),20.0f);
    }

    private IEnumerator PlayResizeAnimation(float speed = 5.0f)
    {
        while(Mathf.Abs(transform.localScale.x-_desiredScale)>0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one*_desiredScale, speed*Time.deltaTime);
            yield return null;
        }
    }
    private IEnumerator PlayDestroyAnimation(float speed = 10.0f)
    {
        while(Mathf.Abs(transform.localScale.x-_desiredScale)>0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one*_desiredScale, speed*Time.deltaTime);
            yield return null;
        }
        Destroy(transform.gameObject);
    }

    private void SetupEvenOddColor()
    {
        var id = index.x + index.y;
        GetComponent<SpriteRenderer>().material.SetColor("_BackgroundColor", id % 2 == 0 ? EvenColor : OddColor);
    }
    
}
