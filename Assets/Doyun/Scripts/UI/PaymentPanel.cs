using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PaymentPanel : BasicPanelController, ICallBack
{
    [Space]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button denyButton;

    [Space]
    [SerializeField] private GameObject payingImage;
    [SerializeField] private TMP_Text amountTxt;
    [SerializeField] private TMP_Text priceTxt;

    [Space]
    // 임시로 결제 시간을 설정, 추후 서버로 결제 시스템이 될시 변경
    [SerializeField] private int payingTime;

    private int amount;
    private int price;
    private bool isPaying;

    public Action TriggerAction { get ; set ; }

/*---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

    #region 판넬 설정
    public void InitPanel(int coinAmount, int coinPrice)
    {
        amount = coinAmount;
        price = coinPrice;

        amountTxt.text = $"코인 {amount.ToString()}개";
        priceTxt.text = $"₩{price.ToString()}";

        SetButton();
    }

    private void SetButton()
    {
        acceptButton.onClick.AddListener(OnClickedAcceptButton);
        denyButton.onClick.AddListener(OnClickedDenyButton);
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

        // 구독된 함수 실행후 제거
        TriggerEvent();
        Unsubscribe();

        Debug.Log($"코인{amount}개 {price}원 결제 완료");

        isPaying = false;
        payingImage.SetActive(isPaying);

        HidePanel();
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
