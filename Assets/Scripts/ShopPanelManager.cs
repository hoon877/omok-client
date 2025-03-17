using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelManager : MonoBehaviour
{
    private bool isShopPanelOpened;
    private float shopFadeTime = 1.0f;
    private float shopFadeTimer;
    private float shopFadeAmount;

    private GameObject _shopPanel;
    private Image _shopPanelImage;

    void Start()
    {
        if (ShopPanel.Instance != null)
        {
            _shopPanel = ShopPanel.Instance.shopPanel;
            _shopPanelImage = ShopPanel.Instance.shopPanelImage;
        }

        isShopPanelOpened = false;

        if (_shopPanel != null)
        {
            _shopPanel.SetActive(false);
        }
    }

    public void ShopOpenClose()
    {
        if (_shopPanel == null || _shopPanelImage == null)
        {
            Debug.LogWarning("ShopPanel이 초기화되지 않음.");
            return;
        }

        StartCoroutine(ShopPanelEnumerator());
    }

    IEnumerator ShopPanelEnumerator()
    {
        if (!isShopPanelOpened) // 패널 열기
        {
            _shopPanel.SetActive(true);
        }

        shopFadeAmount = 0;
        shopFadeTimer = 0;
        Color panelColor = _shopPanelImage.color;

        while (shopFadeAmount <= 1.0f)
        {
            shopFadeTimer += Time.deltaTime;
            shopFadeAmount = shopFadeTimer / shopFadeTime;

            // 알파 값 조절
            panelColor.a = isShopPanelOpened ? (1 - shopFadeAmount) : shopFadeAmount;
            _shopPanelImage.color = panelColor; // **여기서 적용해야 함**

            yield return null;
        }

        isShopPanelOpened = !isShopPanelOpened;

        if (!isShopPanelOpened)
        {
            _shopPanel.SetActive(false); // 페이드아웃 완료 후 비활성화
        }
    }
}