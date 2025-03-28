using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class CoinCounter : MonoBehaviour, IObserver
{
    [SerializeField] private TMP_Text coinText;
    [SerializeField] float animateTime;
    private int lastCoin;

    private void Start()
    {
        CoinUIManager.Instance.AddObserver(this);
        OnNotify();
    }

    public void OnNotify()
    {
        AnimateCoinCount(lastCoin, CoinManager.Instance.CoinsCount, animateTime);    
        lastCoin = CoinManager.Instance.CoinsCount;
    }

    void AnimateCoinCount(int startValue, int endValue, float time)
    {
        // (값을 가져오는 함수, 값을 설정하는 함수, 목표값, 애니메이션 시간)
        DOTween.To(() => startValue, x => startValue = x, endValue, time)
            .OnUpdate(() => coinText.text = startValue.ToString("N0"))      // startCoin의 값이 변경될때마다 호출
            .SetEase(Ease.Linear);                                          // 애니메이션의 ease 타입 설정 (선택 가능)
    }
}
