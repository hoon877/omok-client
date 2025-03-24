using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPanelController : MonoBehaviour
{
    [SerializeField] private GameObject shopPanelPrefab;
    private Transform canvasTranform;

    private void Start()
    {
        canvasTranform = GetComponentInParent<Canvas>().transform;
    }

    public void OnClickShopButton()
    {
        if (shopPanelPrefab != null)
        {
            GameObject go = Instantiate(shopPanelPrefab, canvasTranform);
            go.GetComponent<CoinShopPanel>().ShowPanel();
        }
    }
}
