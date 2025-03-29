using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecordUIPanelController : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;

    private RecordController _recordController;
    
    public Action onClickQuitButton;
    

    public void InitRecordUIController(RecordController recordController)
    {
        _recordController = recordController;
        rectTransform.anchoredPosition = Vector2.zero;
    }
    
    public void OnClickQuitButton()
    {
        HGameManager.Instance.OpenConfirmPanel("Quit", () =>
        {
            onClickQuitButton?.Invoke();
        });
    }

    public void OnClickFirstButton()
    {
        _recordController.ResetRecord();
    }

    public void OnClickPreviousButton()
    {
        _recordController.PreviousStep();
    }

    public void OnClickNextButton()
    {
        _recordController.NextStep();
    }

    public void OnClickEndButton()
    {
        _recordController.EndRecord();
    }
}
