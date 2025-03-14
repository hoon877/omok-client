using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PaymentPanel : BasicPanelController, ICallBack
{
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button denyButton;

    [SerializeField] private GameObject payingImage;
    [SerializeField] private TMP_Text amountTxt;
    [SerializeField] private TMP_Text priceTxt;

    private string defaultAmountTxt = "갯수가 설정되지 않음" ;
    private string defaultPriceTxt = "가격이 설정되지 않음";

    [SerializeField] private int payingTime;

    private int amount;
    private int price;
    private bool isPaying;

    public Action TriggerAction { get ; set ; }

/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    #region 판넬 설정
    public void InitPanel(int amount, int price)
    {
        this.amount = amount;
        this.price = price;

        amountTxt.text = $"코인 {amount.ToString()}개";
        priceTxt.text = $"₩{price.ToString()}";
    }

    private void ResetPanel()
    {
        amount = 0; price = 0;

        amountTxt.text = defaultAmountTxt;
        priceTxt.text = defaultPriceTxt;
    }
    #endregion
/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    #region 버튼함수
    /// <summary>
    /// 결제수락 버튼
    /// </summary>
    public void OnClickedAcceptButton()
    {
        if (isPaying)
            return;

        StartCoroutine(Pay());
    }

    /// <summary>
    /// 결제거절 버튼
    /// </summary>
    public void OnClickedDenyButton()
    {
        if (isPaying)
            return;

        Unsubscribe();
        HidePanel();
    }
    #endregion
/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    #region 결제 시스템, 나중에 서버로 변경
    IEnumerator Pay()
    {
        Debug.Log("결제중");
        isPaying = true;
        payingImage.SetActive(isPaying);

        yield return new WaitForSeconds(payingTime);

        TriggerEvent();
        Unsubscribe();
        HidePanel();
        ResetPanel();

        Debug.Log("결제 완료");
        isPaying = false;
        payingImage.SetActive(isPaying);
    }
    #endregion
/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    #region 구독
    public void Subscribe(Action action)
    {
        TriggerAction = action;
    }

    public void Unsubscribe()
    {
        TriggerAction = null;
    }

    public void TriggerEvent()
    {
        TriggerAction?.Invoke();
    }
    #endregion
}
