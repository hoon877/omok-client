using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    //[SerializeField] private GameObject settingsPanel;
    //[SerializeField] private GameObject confirmPanel;
    //[SerializeField] private GameObject signinPanel;
    //[SerializeField] private GameObject signupPanel;
    
    private BlockController _blockController;
    private GameUIController _gameUIController;
    private Canvas _canvas;
    
    public enum PlayerType { None, PlayerA, PlayerB }
    private PlayerType[,] _board;
    
    private enum TurnType { PlayerA, PlayerB }

    private enum GameResult
    {
        None,   // 게임 진행 중
        Win,    // 플레이어 승
        Lose,   // 플레이어 패
        Draw    // 비김
    }
    
    public enum GameType { SinglePlayer, DualPlayer }
    private GameType _gameType;

    private void Start()
    {
        // 로그인 관련 처리
        //OpenSigninPanel();
        ChangeToGameScene(GameType.SinglePlayer);
    }

    public void ChangeToGameScene(GameType gameType)
    {
        _gameType = gameType;
        // 씬 전환이 필요한 경우 주석 해제
        SceneManager.LoadScene("Game");
    }

    public void ChangeToMainScene()
    {
        SceneManager.LoadScene("Main");
    }

    /// <summary>
    /// 게임 시작
    /// </summary>
    private void StartGame()
    {
        // _board 초기화
        _board = new PlayerType[15, 15];
        
        // 블록 초기화
        _blockController.InitBlocks();
        
        // Game UI 초기화
        _gameUIController.SetGameUIMode(GameUIController.GameUIMode.Init);
        
        // 턴 시작
        SetTurn(TurnType.PlayerA);
    }

    /// <summary>
    /// 게임 오버시 호출되는 함수
    /// </summary>
    /// <param name="gameResult">win, lose, draw</param>
    private void EndGame(GameResult gameResult)
    {
        _gameUIController.SetGameUIMode(GameUIController.GameUIMode.GameOver);
        _blockController.OnBlockClickedDelegate = null;
        
        switch (gameResult)
        {
            case GameResult.Win:
                Debug.Log("Player A win");
                break;
            case GameResult.Lose:
                Debug.Log("Player B win");
                break;
            case GameResult.Draw:
                Debug.Log("Draw");
                break;
        }
    }

    /// <summary>
    /// _board에 새로운 값을 할당하는 함수
    /// </summary>
    /// <param name="playerType">할당하고자 하는 플레이어 타입</param>
    /// <param name="row">Row</param>
    /// <param name="col">Col</param>
    /// <returns>False가 반환되면 할당할 수 없음, True는 할당이 완료됨</returns>
    private bool SetNewBoardValue(PlayerType playerType, int row, int col)
    {
        if (_board[row, col] != PlayerType.None) return false;
        
        if (playerType == PlayerType.PlayerA)
        {
            _board[row, col] = playerType;
            _blockController.PlaceMarker(Block.MarkerType.Black, row, col);
            return true;
        }
        else if (playerType == PlayerType.PlayerB)
        {
            _board[row, col] = playerType;
            _blockController.PlaceMarker(Block.MarkerType.White, row, col);
            return true;
        }
        return false;
    }

    private void SetTurn(TurnType turnType)
    {
        switch (turnType)
        {
            case TurnType.PlayerA:
                //_gameUIController.SetGameUIMode(GameUIController.GameUIMode.TurnA);
                _blockController.OnBlockClickedDelegate = (row, col) =>
                {
                    if (SetNewBoardValue(PlayerType.PlayerA, row, col))
                    {
                        var gameResult = CheckGameResult();
                        if (gameResult == GameResult.None)
                            SetTurn(TurnType.PlayerB);
                        else
                            EndGame(gameResult);
                    }
                    else
                    {
                        // 이미 있는 곳을 터치했을 때 처리
                    }
                };
                break;
            case TurnType.PlayerB:
                //_gameUIController.SetGameUIMode(GameUIController.GameUIMode.TurnB);

                if (_gameType == GameType.SinglePlayer)
                {
                    var result = MinimaxAIController.GetBestMove(_board);
                    if (result.HasValue)
                    {
                        if (SetNewBoardValue(PlayerType.PlayerB, result.Value.row, result.Value.col))
                        {
                            var gameResult = CheckGameResult();
                            if (gameResult == GameResult.None)
                                SetTurn(TurnType.PlayerA);
                            else
                                EndGame(gameResult);
                        }
                        else
                        {
                            // 이미 있는 곳을 터치했을 때 처리
                        }
                    }
                    else
                    {
                        EndGame(GameResult.Win);
                    }
                    break;
                }
                else if (_gameType == GameType.DualPlayer)
                {
                    _blockController.OnBlockClickedDelegate = (row, col) =>
                    {
                        if (SetNewBoardValue(PlayerType.PlayerB, row, col))
                        {
                            var gameResult = CheckGameResult();
                            if (gameResult == GameResult.None)
                                SetTurn(TurnType.PlayerA);
                            else
                                EndGame(gameResult);
                        }
                        else
                        {
                            // 이미 있는 곳을 터치했을 때 처리
                        }
                    };
                }
                break;
        }
    }

    /// <summary>
    /// 게임 결과 확인 함수
    /// </summary>
    /// <returns>플레이어 기준 게임 결과</returns>
    private GameResult CheckGameResult()
    {
        if (CheckGameWin(PlayerType.PlayerA)) { return GameResult.Win; }
        if (CheckGameWin(PlayerType.PlayerB)) { return GameResult.Lose; }
        if (MinimaxAIController.IsAllBlocksPlaced(_board)) { return GameResult.Draw; }
        
        return GameResult.None;
    }
    
    // 게임의 승패를 판단하는 함수
    private bool CheckGameWin(GameManager.PlayerType playerType)
{
    int winCount = 5;
    int numRows = _board.GetLength(0);
    int numCols = _board.GetLength(1);

    for (int row = 0; row < numRows; row++)
    {
        for (int col = 0; col < numCols; col++)
        {
            if (_board[row, col] != playerType)
                continue;

            // 가로 방향 (오른쪽)
            if (col <= numCols - winCount)
            {
                bool win = true;
                (int, int)[] blocks = new (int, int)[winCount];
                for (int i = 0; i < winCount; i++)
                {
                    blocks[i] = (row, col + i);
                    if (_board[row, col + i] != playerType)
                    {
                        win = false;
                        break;
                    }
                }
                if (win)
                {
                    return true;
                }
            }

            // 세로 방향 (아래쪽)
            if (row <= numRows - winCount)
            {
                bool win = true;
                (int, int)[] blocks = new (int, int)[winCount];
                for (int i = 0; i < winCount; i++)
                {
                    blocks[i] = (row + i, col);
                    if (_board[row + i, col] != playerType)
                    {
                        win = false;
                        break;
                    }
                }
                if (win)
                {
                    return true;
                }
            }

            // 대각선 방향 (오른쪽 아래)
            if (row <= numRows - winCount && col <= numCols - winCount)
            {
                bool win = true;
                (int, int)[] blocks = new (int, int)[winCount];
                for (int i = 0; i < winCount; i++)
                {
                    blocks[i] = (row + i, col + i);
                    if (_board[row + i, col + i] != playerType)
                    {
                        win = false;
                        break;
                    }
                }
                if (win)
                {
                    return true;
                }
            }

            // 역대각선 방향 (왼쪽 아래)
            if (row <= numRows - winCount && col >= winCount - 1)
            {
                bool win = true;
                (int, int)[] blocks = new (int, int)[winCount];
                for (int i = 0; i < winCount; i++)
                {
                    blocks[i] = (row + i, col - i);
                    if (_board[row + i, col - i] != playerType)
                    {
                        win = false;
                        break;
                    }
                }
                if (win)
                {
                    return true;
                }
            }
        }
    }
    return false;
}



    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // "Game" 씬이 아닌 경우에는 초기화하지 않음
        if (scene.name != "Game")
            return;
        
        _blockController = GameObject.FindObjectOfType<BlockController>();
        if (_blockController == null)
        {
            Debug.LogError("Scene에 BlockController 컴포넌트가 없습니다.");
            return;
        }

        _gameUIController = GameObject.FindObjectOfType<GameUIController>();
        if (_gameUIController == null)
        {
            Debug.LogError("Scene에 GameUIController 컴포넌트가 없습니다.");
            return;
        }
        
        _canvas = GameObject.FindObjectOfType<Canvas>();
        if (_canvas == null)
        {
            Debug.LogWarning("Scene에 Canvas 컴포넌트가 없습니다.");
        }
        
        // 컴포넌트가 모두 준비되었으면 게임 시작
        StartGame();
    }
}
