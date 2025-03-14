using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicPanelController : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    private RectTransform rectTransform;

    private Vector2 showPosition = Vector2.zero;
    [SerializeField] private Vector2 hiddenPosition;
    [SerializeField] private bool isShow;
/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    private void Awake()
    {
        isShow = false;
        rectTransform = GetComponent<RectTransform>();
        hiddenPosition = rectTransform.anchoredPosition;
    }
/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    public void ShowPanel()
    {
        // 화면에 표시되지 않을때
        if (!isShow)
        {
            rectTransform.anchoredPosition = showPosition;
            isShow = true;
        }
    }

    public void HidePanel()
    {
        // 화면에 표시되고 있을때
        if (isShow)
        {
            rectTransform.anchoredPosition = hiddenPosition;
            isShow = false;
        }
    }
}
