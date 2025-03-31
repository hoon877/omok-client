using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductPrefab : MonoBehaviour
{
    public TMP_Text productAmountText;
    public TMP_Text productPriceText;
    public Image productImage;
    public Button buyButton;

    public void Setup(Product product, System.Action onBuy)
    {
        // UI에 상품 데이터 반영
        productAmountText.text = $"{(product.coinAmount/1000).ToString()}K";
        productPriceText.text = $"{product.coinPrice.ToString("N0")}krw";
        productImage.sprite = product.coinSprite;

        // 구매 버튼에 이벤트 등록
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => onBuy?.Invoke());
    }
}