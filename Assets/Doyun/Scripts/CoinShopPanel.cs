using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class CoinShopPanel : BasicPanelController
{
    private void Start()
    {
        if (CoinManager.Instance.isShopShow)
        {
            Debug.Log("이미 상점이 존재함");
            Destroy(gameObject);
        }

        CoinManager.Instance.isShopShow = true;
    }
/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    public void OnClickCloseButton()
    {
        CoinManager.Instance.isShopShow = false;
        HidePanel();
    }
}
