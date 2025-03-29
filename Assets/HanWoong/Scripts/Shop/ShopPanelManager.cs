using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelManager : MonoBehaviour
{
    public GameObject _shopPanel;
    
    private bool isShopPanelOpened;
    private float shopFadeTime = 0.3f;
    private float shopFadeTimer;
    private float shopFadeAmount;
    
    void Start()
    {
        isShopPanelOpened = false;
    }

    public void ShopOpenClose()
    {
        StartCoroutine(ShopPanelEnumerator());
    }

    IEnumerator ShopPanelEnumerator()
    {
        isShopPanelOpened = !isShopPanelOpened;
        
        if (isShopPanelOpened)
        {
            _shopPanel.SetActive(true);
            
            shopFadeAmount = 0;
            shopFadeTimer = 0;
            Color panelColor = _shopPanel.GetComponent<Image>().color;

            while (shopFadeAmount <= 1.0f)
            {
                shopFadeTimer += Time.deltaTime;
                shopFadeAmount = shopFadeTimer / shopFadeTime;

                // 알파 값 조절
                panelColor.a = shopFadeAmount;
                _shopPanel.GetComponent<Image>().color = panelColor;

                yield return null;
            }
        }
        else
        {
            _shopPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // ESC키 입력 시 상점 닫기
        if (isShopPanelOpened && Input.GetKeyDown(KeyCode.Escape))
        {
            ShopOpenClose();
        }
    }
    
}