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
    
    public bool isRecordMode;
    public string currentRoomId;
    private RecordController _recordController;
    public bool isLoggedIn = false;
    
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
        isRecordMode = false;
        _gameType = gameType;
        SceneManager.LoadScene("Game");
        
    }

    public void EndGame()
    {
        // Login 씬으로 이동 
        SceneManager.LoadScene(0);
    }
    
    private void InitializeGame(HYConstants.GameType gameType)
    {
        _gameController = new GameController(gameType);
    }
    
    public void ViewGameRecord(string roomId)
    {
        isRecordMode = true;
        currentRoomId = roomId;
        SceneManager.LoadScene("Game");
    }

    private void InitializeRecordMode(string roomId)
    {
        _recordController = new RecordController(roomId);
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            if (isRecordMode)
            {
                InitializeRecordMode(currentRoomId);
            }
            else
            {
                InitializeGame(_gameType);
            }
        }
        _canvas = GameObject.FindObjectOfType<Canvas>();
    }

    private void OnApplicationQuit()
    {
        _gameController?.Dispose();
        _gameController = null;
    }
}
