
using System;
using System.Collections.Generic;
using UnityEngine;

public interface ITurnState
{
    void OnEnter(GameController gameController);
    void OnExecute(GameController gameController);
    void OnExit(GameController gameController);
}

public class PlayerState : ITurnState
{
    private HYConstants.PlayerType _playerType;
    private TurnManager _turnManager;
    private MultiPlayManager _multiPlayManager;
    private bool _isBlackPlayer;
    private string _roomId;
    private bool _isMultiPlay;
    private (int, string) _currentData;

    public PlayerState(bool isBlackPlayer, TurnManager turnManager)
    {
        _isBlackPlayer = isBlackPlayer;
       _playerType = isBlackPlayer ? HYConstants.PlayerType.BlackPlayer : HYConstants.PlayerType.WhitePlayer;
       _turnManager = turnManager;
       _isMultiPlay = false;
    }

    public PlayerState(bool isBlackPlayer, TurnManager turnManager, MultiPlayManager multiPlayManager, string roomId)
        : this(isBlackPlayer, turnManager)
    {
        _multiPlayManager = multiPlayManager;
        _roomId = roomId;
        _isMultiPlay = true;
    }
    
    public void OnEnter(GameController gameController)
    {
        Debug.Log($"{_playerType} 턴 시작");
        // UI 요소 활성화 등의 작업
        if (_isMultiPlay)
        {
            UnityThread.executeInUpdate(gameController.HandleTimer);
        }
        else gameController.HandleTimer();
    }

    public void OnExecute(GameController gameController)
    {
        // 플레이어의 입력에 따라 마커 배치
        if (gameController.TryPlaceMarker(_isBlackPlayer))
        {
            _currentData = gameController.GetCurrentData();
            
            // 마커 배치 성공시 턴 변경
            _turnManager.AdvanceToNextTurn();
        }
    }

    public void OnExit(GameController gameController)
    {
        Debug.Log($"{_playerType} 턴 종료");
        if (_isMultiPlay)
        {
            _multiPlayManager.SendPlayerMove(_roomId, _currentData.Item1, _currentData.Item2);
        }
    }
}

public class AIState : ITurnState
{
    private TurnManager _turnManager;
    private HYMinimaxAIController _aiController;
    
    public AIState(TurnManager turnManager, int aiDifficulty = 4)
    {
        _turnManager = turnManager;
        _aiController = new HYMinimaxAIController(aiDifficulty);
    }
    
    public void OnEnter(GameController gameController)
    {
        Debug.Log("AI 턴 시작 (백돌)");
        gameController.HandleTimer();
        
        // AI 턴이 시작되면 즉시 OnExecute 호출
        OnExecute(gameController);    
    }

    public void OnExecute(GameController gameController)
    {
        // AI 계산 로직 - 백돌만 사용
        HYConstants.MarkerType[,] board = gameController.GetBoard();
        
        // AI의 최적 수 계산
        var bestMove = _aiController.GetBestMove(board);
        
        if (bestMove.HasValue)
        {
            // 계산된 위치를 게임 좌표로 변환
            Vector2Int gridPos = new Vector2Int(bestMove.Value.col, bestMove.Value.row);
            
            // BoardClickHandler에 선택된 위치 설정
            gameController.SetAISelectedPosition(gridPos);
            
            // 마커 배치 시도 (백돌은 false)
            if (gameController.TryPlaceMarker(false))
            {
                _turnManager.AdvanceToNextTurn();
            }
            else
            {
                Debug.LogWarning("AI가 선택한 위치에 마커를 배치할 수 없습니다");
            }
        }
        else
        {
            Debug.LogWarning("AI가 유효한 이동을 찾지 못했습니다");
        }
    }

    public void OnExit(GameController gameController)
    {
        Debug.Log("AI 턴 종료 (백돌)");
    }
}

public class MultiPlayerState :  ITurnState
{
    private HYConstants.PlayerType _playerType;
    private GameController _gameController;
    private TurnManager _turnManager;
    private MultiPlayManager _multiPlayManager;
    private bool _isBlackPlayer;
    public MultiPlayerState(bool isBlackPlayer, TurnManager turnManager, MultiPlayManager multiPlayManager)
    {
        _isBlackPlayer = isBlackPlayer;
        _playerType = _isBlackPlayer ? HYConstants.PlayerType.BlackPlayer :  HYConstants.PlayerType.WhitePlayer;
        _turnManager = turnManager;
        _multiPlayManager = multiPlayManager;
        
        // 상대방 돌 놓기 이벤트 등록
        _multiPlayManager.OnOpponentMove += HandleOpponentMove;
    }
    
    public void OnEnter(GameController gameController)
    {
        _gameController = gameController;
        Debug.Log($"{_playerType} 턴 시작 (상대방 턴)");
    }

    public void OnExecute(GameController gameController)
    {
        // 이 상태는 상대방의 턴을 나타내므로, 여기서는 아무 것도 하지 않음
        // 상대방의 입력은 HandleOpponentMove에서 처리됨
    }

    public void OnExit(GameController gameController)
    {
        Debug.Log($"{_playerType} 턴 종료 (상대방 턴)");
    }
    
    // 상대방 돌 놓기 이벤트 처리
    private void HandleOpponentMove(MoveData moveData)
    {
        // 상대방이 놓은 위치에 돌 놓기
        int position = moveData.position;
        int x = position % HYConstants.BoardSize;
        int y = position / HYConstants.BoardSize;
        
        // 네트워크에서 받은 위치에 마커를 배치함
        UnityThread.executeInUpdate(()=>
        {
            if (_gameController.TryPlaceMarkerFromNetwork(position, _isBlackPlayer))
            {
                _turnManager.AdvanceToNextTurn();
            }
        });

    }
    
    // 리소스 정리
    public void Dispose()
    {
        _multiPlayManager.OnOpponentMove -= HandleOpponentMove;
    }
}

public class TurnManager : IDisposable
{
    public event Action OnTurnChanged;
    private Action<string, string, string> _gameOverCallback;
    
    private ITurnState _currentState;
    private Dictionary<HYConstants.PlayerType, ITurnState> _states = new();
    private GameController _gameController;
    private MultiPlayManager _multiPlayManager;
    private string _roomId;
    private HYConstants.PlayerType _myPlayerType;

    private bool _isGameStarted = false;

    public TurnManager(HYConstants.GameType gameType, GameController gameController)
    {
        _gameController = gameController;
        InitializeStates(gameType);
    }

    private void InitializeStates(HYConstants.GameType gameType)
    {
        switch (gameType)
        {
            case HYConstants.GameType.SinglePlay:
                // 사용자는 흑돌, AI는 백돌
                _states[HYConstants.PlayerType.BlackPlayer] = new PlayerState(true, this);
                _states[HYConstants.PlayerType.WhitePlayer] = new AIState(this, 4); // 난이도 4로 설정
                _isGameStarted = true;
                break;
            case HYConstants.GameType.DualPlay:
                _states[HYConstants.PlayerType.BlackPlayer] = new PlayerState(true, this);
                _states[HYConstants.PlayerType.WhitePlayer] = new PlayerState(false, this);
                _isGameStarted = true;
                break;
            case HYConstants.GameType.MultiPlay:
                _multiPlayManager = new MultiPlayManager((state, roomId) =>
                {
                    switch (state)
                    {
                        case HConstants.MultiplayManagerState.CreateRoom:
                            Debug.Log("## Create Room");
                            break;
                        case HConstants.MultiplayManagerState.JoinRoom:
                            Debug.Log("## Join Room");
                            _myPlayerType = HYConstants.PlayerType.WhitePlayer;
                            _multiPlayManager.SetMyPlayerType(_myPlayerType); // 내 플레이어 타입 설정
                            _roomId = roomId;
                            _states[HYConstants.PlayerType.BlackPlayer] = new MultiPlayerState(true,this, _multiPlayManager);
                            _states[HYConstants.PlayerType.WhitePlayer] = new PlayerState(false, this, _multiPlayManager, roomId);
                            _isGameStarted = true;
                            SetInitialTurn();
                            break;
                        case HConstants.MultiplayManagerState.StartGame:
                            Debug.Log("## Start Game");
                            _myPlayerType = HYConstants.PlayerType.BlackPlayer;
                            _multiPlayManager.SetMyPlayerType(_myPlayerType); // 내 플레이어 타입 설정
                            _roomId = roomId;
                            _states[HYConstants.PlayerType.BlackPlayer] = new PlayerState(true, this, _multiPlayManager, roomId);
                            _states[HYConstants.PlayerType.WhitePlayer] = new MultiPlayerState(false, this, _multiPlayManager);
                            _isGameStarted = true;
                            SetInitialTurn();
                            break;
                        case HConstants.MultiplayManagerState.ExitRoom:
                            Debug.Log("## Exit Room");
                            _isGameStarted = false;
                            break;
                        case HConstants.MultiplayManagerState.EndGame:
                            Debug.Log("## End Game");
                            _isGameStarted = false;
                            break;
                    }
                });
                // 상대방 종료 시 게임 오버 처리 이벤트 등록
                _multiPlayManager.OnPlayerDisconnected += HandleOpponentDisconnected; // 강제 종료만 처리
                _multiPlayManager.OnGameEnded += HandleGameEnded; // 정상 게임 종료 처리
                _gameOverCallback += _multiPlayManager.SendGameOver;
                break;
        }

        if (_isGameStarted && gameType != HYConstants.GameType.MultiPlay)
        {
            SetInitialTurn();
        }
    }

    private void SetInitialTurn()
    {
        SetState(_states[HYConstants.PlayerType.BlackPlayer]);
    }

    public void ExecuteCurrentTurn()
    {
        _currentState?.OnExecute(_gameController);
    }

    public void AdvanceToNextTurn()
    {
        HYConstants.PlayerType nextPlayerType = _currentState == _states[HYConstants.PlayerType.BlackPlayer] 
            ? HYConstants.PlayerType.WhitePlayer 
            : HYConstants.PlayerType.BlackPlayer;
            
        OnTurnChanged?.Invoke();
        SetState(_states[nextPlayerType]);
    }

    private void SetState(ITurnState newState)
    {
        _currentState?.OnExit(_gameController);
        _currentState = newState;
        _currentState.OnEnter(_gameController);
    }
    
    public ITurnState GetCurrentState()
    {
        return _currentState;
    }

    public bool IsBlackPlayerTurn()
    {
        return _currentState == _states[HYConstants.PlayerType.BlackPlayer];
    }
    
    private void HandleOpponentDisconnected(string winner)
    {
        // UI 쓰레드에서 실행하기 위해 UnityThread 사용
        UnityThread.executeInUpdate(() => {
            if (_isGameStarted)
            {
                Debug.Log($"상대방이 게임을 비정상적으로 종료했습니다. 승자: {winner}");
    
                // 승리 처리 (X는 흑돌, O는 백돌)
                HYConstants.GameResult gameResult = winner == "BlackWin" ? 
                    HYConstants.GameResult.BlackWin : HYConstants.GameResult.WhiteWin;
        
                // 게임 컨트롤러의 GameOver 메서드 호출
                _gameController.GameOverByOpponentDisconnect(gameResult);
            }
        });
    }
    
    private void HandleGameEnded(string winner)
    {
        // UI 쓰레드에서 실행하기 위해 UnityThread 사용
        UnityThread.executeInUpdate(() => {
            if (_isGameStarted)
            {
                Debug.Log($"게임이 정상적으로 종료되었습니다. 승자: {winner}");
            }
        });
    }


    public bool IsGameStarted()
    {
        return _isGameStarted;
    }

    public void GameOver(string winner)
    {
        string myPlayerType = _myPlayerType.ToString();
        _gameOverCallback?.Invoke(_roomId, winner, myPlayerType);
    }

    public void Dispose()
    {
        foreach (var state in _states.Values)
        {
            if (state is MultiPlayerState multiPlayerState)
            {
                multiPlayerState.Dispose();
            }
        }
        _gameOverCallback = null;
        
        if (_multiPlayManager != null)
        {
            _multiPlayManager.OnGameEnded -= HandleOpponentDisconnected;
            _multiPlayManager.LeaveRoom(_roomId);
            _multiPlayManager.Dispose();
        }
    }
}
