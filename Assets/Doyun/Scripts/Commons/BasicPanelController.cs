using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class BasicPanelController : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;

/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    public void ShowPanel()
    {
        // 표시 애니메이션
        rectTransform.localScale = Vector3.zero;
        rectTransform.DOScale(1, 0.3f).SetEase(Ease.OutBack);
    }

    public void HidePanel()
    {
        // 숨김 애니메이션
        rectTransform.localScale = Vector3.one;
        rectTransform.DOScale(0, 0.3f).SetEase(Ease.InBack).OnComplete( () => { Destroy(gameObject); });
    }
}
