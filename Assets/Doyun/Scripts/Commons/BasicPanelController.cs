using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class BasicPanelController : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;

    private Vector2 hiddenPosition = new Vector2(9999, 9999);
    private Vector2 visiblePosition = Vector2.zero;

/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    public void ShowPanel()
    {
        rectTransform.anchoredPosition = Vector2.zero;

        // 표시 애니메이션
        rectTransform.localScale = visiblePosition;
        rectTransform.DOScale(1, 0.3f).SetEase(Ease.OutBack);
    }

    public void HidePanel()
    {

        // 숨김 애니메이션
        rectTransform.localScale = Vector3.one;
        rectTransform.DOScale(0, 0.3f).SetEase(Ease.InBack).OnComplete( () => { rectTransform.anchoredPosition = hiddenPosition; });
    }
}
