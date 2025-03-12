using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private GameController _gameController;
    private void Awake()
    {
        StartGame();
    }

    private void StartGame()
    {
        _gameController = new GameController(Constants.GameType.DualPlay);
    }

    public void OnClickSetStoneButton()
    {
        _gameController.SetMarkerOnBoard();
    }
}