using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinCounter : MonoBehaviour, IObserver
{
    private TMP_Text coinText;

    private void Start()
    {
        coinText = GetComponent<TMP_Text>();
        CoinManager.Instance.AddObserver(this);
        OnNotify();
    }

    public void OnNotify()
    {
        coinText.text = $"코인 { CoinManager.Instance.CoinsCount.ToString() }개";
    }
}
