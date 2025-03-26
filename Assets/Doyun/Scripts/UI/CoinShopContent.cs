using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CoinShopContent : MonoBehaviour
{
    [SerializeField] private int amount;
    [SerializeField] private int price;

    [SerializeField] private TMP_Text firstText;
    [SerializeField] private TMP_Text secondText;

/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    private void Start()
    {
        firstText.text = $"코인 {amount}개";
        secondText.text = "₩" + price.ToString();
    }
/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    public void OnClickedButton()
    {
        PaymentPanel pay = CoinUIManager.Instance.PaymentPanel;
        pay.Subscribe(() => { CoinManager.Instance.AddCoin(amount); });
        pay.InitPanel(amount, price);
        pay.ShowPanel();
    }
}