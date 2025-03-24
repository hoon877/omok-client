using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LGameManager : LSingleton<LGameManager>
{
    //[SerializeField] private GameObject settingsPanel;
    //[SerializeField] private GameObject confirmPanel;
    //[SerializeField] private GameObject signinPanel;
    //[SerializeField] private GameObject signupPanel;
    
    private BlockController _blockController;
    private GameUIController _gameUIController;
    private GameLogic _gameLogic;
    private LRenjuRuleChecker _renjuRuleChecker;
    private Canvas _canvas;
    
    private LConstants.PlayerType[,] _board;
    
    private enum TurnType { PlayerA, PlayerB }
    
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
        _board = new LConstants.PlayerType[15, 15];
        
        //GameLogic 초기화
        _gameLogic = GameObject.FindObjectOfType<GameLogic>();
        _gameLogic.SetBoard(_board);
        
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
    private void EndGame(LConstants.GameResult gameResult)
    {
        _gameUIController.SetGameUIMode(GameUIController.GameUIMode.GameOver);
        _blockController.OnBlockClickedDelegate = null;
        
        switch (gameResult)
        {
            case LConstants.GameResult.Win:
                Debug.Log("Player A win");
                break;
            case LConstants.GameResult.Lose:
                Debug.Log("Player B win");
                break;
            case LConstants.GameResult.Draw:
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
    private bool SetNewBoardValue(LConstants.PlayerType playerType, int row, int col)
    {
        if (_board[row, col] != LConstants.PlayerType.None) return false;
        
        if (playerType == LConstants.PlayerType.PlayerA)
        {
            _board[row, col] = playerType;
            _blockController.PlaceMarker(Block.MarkerType.Black, row, col);
            return true;
        }
        else if (playerType == LConstants.PlayerType.PlayerB)
        {
            _board[row, col] = playerType;
            _blockController.PlaceMarker(Block.MarkerType.White, row, col);
            return true;
        }
        return false;
    }

    private void SetTurn(TurnType turnType)
    {
        ForbiddenMarker(LConstants.PlayerType.PlayerA);
        switch (turnType)
        {
            case TurnType.PlayerA:
                //_gameUIController.SetGameUIMode(GameUIController.GameUIMode.TurnA);
                _blockController.OnBlockClickedDelegate = (row, col) =>
                {
                    // 금수 판정을 먼저 수행합니다.
                    if (_renjuRuleChecker.IsMoveForbidden(_board, row, col, LConstants.PlayerType.PlayerA))
                    {
                        return; // 금수이면 이동 무효화
                    }
                    if (SetNewBoardValue(LConstants.PlayerType.PlayerA, row, col))
                    {
                        var gameResult = _gameLogic.CheckGameResult();
                        if (gameResult == LConstants.GameResult.None)
                        {
                            SetTurn(TurnType.PlayerB);
                        }
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
                        if (SetNewBoardValue(LConstants.PlayerType.PlayerB, result.Value.row, result.Value.col))
                        {
                            var gameResult = _gameLogic.CheckGameResult();
                            if (gameResult == LConstants.GameResult.None)
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
                        EndGame(LConstants.GameResult.Win);
                    }
                    break;
                }
                else if (_gameType == GameType.DualPlayer)
                {
                    _blockController.OnBlockClickedDelegate = (row, col) =>
                    {
                        if (SetNewBoardValue(LConstants.PlayerType.PlayerB, row, col))
                        {
                            var gameResult = _gameLogic.CheckGameResult();
                            if (gameResult == LConstants.GameResult.None)
                            {
                                SetTurn(TurnType.PlayerA);
                            }
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
    
    private void ForbiddenMarker(LConstants.PlayerType player)
    {
        
        int rows = _board.GetLength(0);
        int cols = _board.GetLength(1);
    
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                // 해당 위치가 빈 칸인 경우에만 체크
                if (_board[row, col] == LConstants.PlayerType.None)
                {
                    bool isForbidden = _renjuRuleChecker.IsMoveForbidden(_board, row, col, player);
                    if (isForbidden)
                    {
                        // BlockController에서 금수 표시를 위한 메서드를 호출
                        _blockController.PlaceMarker(Block.MarkerType.Forbidden, row, col);
                    }
                }
            }
        }
    }

    
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // "Game" 씬이 아닌 경우에는 초기화하지 않음
        if (scene.name != "Game")
            return;
        
        _blockController = GameObject.FindObjectOfType<BlockController>();
        _gameUIController = GameObject.FindObjectOfType<GameUIController>();
        _renjuRuleChecker = GameObject.FindObjectOfType<LRenjuRuleChecker>();
        _canvas = GameObject.FindObjectOfType<Canvas>();
        
        // 컴포넌트가 모두 준비되었으면 게임 시작
        StartGame();
    }
}
