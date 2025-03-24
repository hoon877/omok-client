using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoinUIManager : HSingleton<CoinUIManager>, ISubject, IObserver
{
    private List<IObserver> observers;

    [SerializeField] private GameObject coinShopPanelPrefab;
    [SerializeField] private GameObject paymentPanelPrefab;
    private Transform canvasTransform;

    private CoinShopPanel shopPanel;
    private PaymentPanel paymentPanel;

    public CoinShopPanel ShopPanel
    {
        get
        {
            CheckCanvas();
            if (shopPanel == null)
            {
                GameObject go = Instantiate(coinShopPanelPrefab, canvasTransform);
                shopPanel = go.GetComponent<CoinShopPanel>();
            }
            return shopPanel;
        }
        set { shopPanel = value; }
    }

    public PaymentPanel PaymentPanel
    {
        get
        {
            CheckCanvas();
            if (paymentPanel == null)
            {
                GameObject go = Instantiate(paymentPanelPrefab, canvasTransform);
                paymentPanel = go.GetComponent<PaymentPanel>();
            }
            return paymentPanel;
        }
        set { paymentPanel = value; }
    }

/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    private void CheckCanvas()
    {
        if(canvasTransform == null)
        {
            canvasTransform = FindAnyObjectByType<Canvas>().transform;
        }
    }

    private void ResetValue()
    {
        (observers ??= new List<IObserver>()).Clear();

        canvasTransform = null;
        ShopPanel = null;
        PaymentPanel = null;
    }

/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    public void AddObserver(IObserver observer)
    {
        observers.Add(observer);
    }

    public void RemoveObserver(IObserver observer)
    {
        observers.Remove(observer);
    }

    public void OnNotify()
    {
        NotifyToObserver();
    }

    public void NotifyToObserver()
    {
        if (observers.Count > 0)
        {
            foreach (IObserver observer in observers)
            {
                observer.OnNotify();
            }
        }
    }

/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("씬이 전환됨");
        ResetValue();
    }
}
