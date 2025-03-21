using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private GameController _gameController;
    private void Awake()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        _gameController = new GameController(Constants.GameType.DualPlay);
    }

    public void OnClickSetStoneButton()
    {
        _gameController.ExecuteCurrentTurn();
    }
}