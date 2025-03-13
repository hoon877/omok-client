using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicPanelController : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    private RectTransform rectTransform;

    private Vector2 hiddenPosition;
    private Vector2 showPosition = Vector2.zero;
    private bool isShow;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        hiddenPosition = rectTransform.anchoredPosition;
    }

    public void ShowPanel()
    {
        // 화면에 표시되지 않을때
        if (!isShow)
            rectTransform.anchoredPosition = showPosition;
    }

    public void HidePanel()
    {
        // 화면에 표시되고 있을때
        if (isShow)
            rectTransform.anchoredPosition = hiddenPosition;
    }
}
