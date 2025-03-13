using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopCoinContent : MonoBehaviour
{
    [SerializeField] private int amount;
    [SerializeField] private int price;

    [SerializeField] private TMP_Text firstText;
    [SerializeField] private TMP_Text secondText;

    [SerializeField] private BasicPanelController paymentPanel;

    private void Start()
    {
        firstText.text = $"코인 {amount}개";
        secondText.text = "₩" + price.ToString();
    }

    public void OnClickedButton()
    {

    }
}