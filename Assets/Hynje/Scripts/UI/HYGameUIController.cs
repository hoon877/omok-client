using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HYGameUIController : MonoBehaviour
{
    [SerializeField] private Image blackTurnFillImage;
    [SerializeField] private Image whiteTurnFillImage;

    private GameController _gameController;
    
    public Action<bool> OnTurnChanged;
    
    public void InitGameUIController(GameController gameController)
    {
        _gameController = gameController;
        
        OnTurnChanged += ChangeTurnUI;
    }

    public void OnClickGiveUpButton()
    {
        HGameManager.Instance.OpenConfirmPanel("GiveUp", () =>
        {
            _gameController.GameOverOnGiveUp();
            HGameManager.Instance.EndGame();
        });
    }
    
    public void OnClickSetStoneButton()
    {
        _gameController.ExecuteCurrentTurn();
    }

    private void ChangeTurnUI(bool isBlackTurn)
    {
        float duration = 0.5f;
        
        if (isBlackTurn)
        {
            blackTurnFillImage.DOFillAmount(1, duration).SetEase(Ease.OutBack);
            whiteTurnFillImage.DOFillAmount(0, duration).SetEase(Ease.OutBack);
        }
        else
        {
            blackTurnFillImage.DOFillAmount(0, duration).SetEase(Ease.OutBack);
            whiteTurnFillImage.DOFillAmount(1, duration).SetEase(Ease.OutBack);
        }
    }

    public void ShowGameOverUI(string winner)
    {
        // todo : 급수 점수 적용, 한판 더 할지 메인화면으로 돌아갈지, 필요하다면 패널 제작 
        HGameManager.Instance.OpenConfirmPanel(winner, () =>
        {
            OnTurnChanged = null;
            _gameController.Dispose();
        });
    }
}