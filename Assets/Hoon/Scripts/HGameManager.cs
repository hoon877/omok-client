using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HGameManager : HSingleton<HGameManager>
{
    private Canvas _canvas;
    [SerializeField] private GameObject confirmPanel;
    
    private GameController _gameController;
    private HYConstants.GameType _gameType;
    
    public void OpenConfirmPanel(string message, ConfirmPanelController.OnConfirmButtonClick onConfirmButtonClick)
    {
        if (_canvas != null)
        {
            var confirmPanelObject = Instantiate(confirmPanel, _canvas.transform);
            confirmPanelObject.GetComponent<ConfirmPanelController>()
                .Show(message, onConfirmButtonClick);
        }
    }

    public void StartGame(HYConstants.GameType gameType)
    {
        _gameType = gameType;
        SceneManager.LoadScene("Game");
    }

    public void EndGame()
    {
        SceneManager.LoadScene(1);
    }
    
    private void InitializeGame(HYConstants.GameType gameType)
    {
        _gameController = new GameController(gameType);
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            InitializeGame(_gameType);
        }
        _canvas = GameObject.FindObjectOfType<Canvas>();
    }

    private void OnApplicationQuit()
    {
        _gameController.Dispose();
        _gameController = null;
    }
}
