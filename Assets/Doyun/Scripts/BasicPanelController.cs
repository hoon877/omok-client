using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicPanelController : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    private RectTransform rectTransform;

    private Vector2 hiddenPosition;
    private Vector2 showPosition = Vector2.zero;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        hiddenPosition = rectTransform.anchoredPosition;
    }

    public void ShowPanel()
    {
        rectTransform.anchoredPosition = showPosition;
    }

    public void HidePanel()
    {
        rectTransform.anchoredPosition = hiddenPosition;
    }
}
