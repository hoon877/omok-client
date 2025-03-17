using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class ShopPanelManager : MonoBehaviour
{
    ShopPanel shopPanel;
    public Image shopFadeImage;
    private bool isShopPanelOpened = false;
    private float shopFadeTime;
    private float shopFadeTimer;
    private float shopFadeAmount;

    public void ShopOpenClose()
    {
        StartCoroutine(ShopPanelEnumerator());
    }

    void ShopPanelSwitch()
    {
        shopFadeAmount = 0;
        shopFadeTimer = 0;

        if (isShopPanelOpened)
            isShopPanelOpened = false;
        else
            isShopPanelOpened = true;
    }

    IEnumerator ShopPanelEnumerator()
    {
        while (shopFadeAmount <= 1.5f)
        {
            shopFadeTimer += Time.deltaTime;
            shopFadeAmount = shopFadeTimer / shopFadeTime;

            if (isShopPanelOpened && shopFadeAmount >= 1f)
            {
                ShopPanelSwitch();
                break;
            }

            if (isShopPanelOpened)
            {
                shopPanel.GetComponent(Image.color) = new Color(shopFadeImage.color.r, shopFadeImage.color.g, shopFadeImage.color.b,
                    1 - shopFadeAmount);
            }

            if (!isShopPanelOpened && shopFadeAmount >= 1f)
            {
                ShopPanelSwitch();
                break;
            }

            if (!isShopPanelOpened)
            {
                shopFadeImage.color = new Color(shopFadeImage.color.r, shopFadeImage.color.g, shopFadeImage.color.b,
                    shopFadeAmount);
            }

            yield return null;
        }

    }
}
