using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
public class Block : MonoBehaviour
{
    [SerializeField] private Sprite blackSprite;
    [SerializeField] private Sprite whiteSprite;
    [SerializeField] private Sprite forbiddenSprite;
    [SerializeField] private SpriteRenderer markerSpriteRenderer;

    public enum MarkerType { None, Black, White, Forbidden }
    
    public delegate void OnBlockClicked(int index);
    private OnBlockClicked _onBlockClicked;
    private int _blockIndex;
    private SpriteRenderer _spriteRenderer;
    private Color _defaultColor;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultColor = _spriteRenderer.color;
    }

    /// <summary>
    /// 블럭의 색상을 변경하는 함수
    /// </summary>
    /// <param name="color">색상</param>
    public void SetColor(Color color)
    {
        _spriteRenderer.color = color;
    }

    /// <summary>
    /// Block 초기화 함수
    /// </summary>
    /// <param name="blockIndex">Block 인덱스</param>
    /// <param name="onBlockClicked">Block 터치 이벤트</param>
    public void InitMarker(int blockIndex, OnBlockClicked onBlockClicked)
    {
        _blockIndex = blockIndex;
        SetMarker(MarkerType.None);
        this._onBlockClicked = onBlockClicked;
        SetColor(_defaultColor);
    }
    
    /// <summary>
    /// 어떤 마커를 표시할지 전달하는 함수
    /// </summary>
    /// <param name="markerType">마커 타입</param>
    public void SetMarker(MarkerType markerType)
    {
        switch (markerType)
        {
            case MarkerType.Black:
                markerSpriteRenderer.sprite = blackSprite;
                break;
            case MarkerType.White:
                markerSpriteRenderer.sprite = whiteSprite;
                break;
            case MarkerType.Forbidden:
                markerSpriteRenderer.sprite = forbiddenSprite;
                break;
            case MarkerType.None:
                markerSpriteRenderer.sprite = null;
                break;
        }
    }

    private void OnMouseUpAsButton()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("OnMouseUpAsButton");
            return;
        }
        Debug.Log($"OnMouseUpAsButton called for block {_blockIndex}. Delegate assigned: {_onBlockClicked != null}");
        _onBlockClicked?.Invoke(_blockIndex);
    }
}
