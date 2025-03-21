using UnityEngine;
using UnityEngine.UI;

public class HYGameManager : MonoBehaviour
{
    private GameController _gameController;
    private void Awake()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        _gameController = new GameController(HYConstants.GameType.DualPlay);
    }

    public void OnClickSetStoneButton()
    {
        _gameController.ExecuteCurrentTurn();
    }
}