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

    [SerializeField] private GameObject paymentPanelPrefab;

    private Transform canvasTranform;

/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    private void Start()
    {
        canvasTranform = GetComponentInParent<Canvas>().transform;
        firstText.text = $"코인 {amount}개";
        secondText.text = "₩" + price.ToString();
    }
/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    public void OnClickedButton()
    {
        if (paymentPanelPrefab == null)
            return;

        GameObject go = Instantiate(paymentPanelPrefab, canvasTranform);
        PaymentPanel paymentPanel = go.GetComponent<PaymentPanel>();

        paymentPanel.Subscribe(() => { CoinManager.Instance.AddCoin(amount); });
        paymentPanel.InitPanel(amount, price);
        paymentPanel.ShowPanel();
    }
}