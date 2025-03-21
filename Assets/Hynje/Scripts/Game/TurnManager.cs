
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
    private bool _isBlackPlayer;

    public PlayerState(bool isBlackPlayer, TurnManager turnManager)
    {
        _isBlackPlayer = isBlackPlayer;
       _playerType = isBlackPlayer ? HYConstants.PlayerType.BlackPlayer : HYConstants.PlayerType.WhitePlayer;
       _turnManager = turnManager;
    }
    public void OnEnter(GameController gameController)
    {
        Debug.Log($"{_playerType} 턴 시작");
        // UI 요소 활성화 등의 작업
    }

    public void OnExecute(GameController gameController)
    {
        // 플레이어의 입력에 따라 마커 배치
        if (gameController.TryPlaceMarker(_isBlackPlayer))
        {
            // 마커 배치 성공시 턴 변경
            _turnManager.AdvanceToNextTurn();
        }
    }

    public void OnExit(GameController gameController)
    {
        Debug.Log($"{_playerType} 턴 종료");
        // UI 요소 비활성화 등의 작업
    }
}

public class AIState : ITurnState
{
    public void OnEnter(GameController gameController)
    {
        throw new System.NotImplementedException();
    }

    public void OnExecute(GameController gameController)
    {
        throw new System.NotImplementedException();
    }

    public void OnExit(GameController gameController)
    {
        throw new System.NotImplementedException();
    }
}

public class TurnManager
{
    public event Action OnTurnChanged;
    
    private ITurnState _currentState;
    private Dictionary<HYConstants.PlayerType, ITurnState> _states = new();
    private GameController _gameController;

    public TurnManager(HYConstants.GameType gameType, GameController gameController)
    {
        _gameController = gameController;
        InitializeStates(gameType);
        SetInitialTurn();
    }

    private void InitializeStates(HYConstants.GameType gameType)
    {
        switch (gameType)
        {
            case HYConstants.GameType.SinglePlay:
                // todo: AI 기능 추가 
                break;
            case HYConstants.GameType.DualPlay:
                _states[HYConstants.PlayerType.BlackPlayer] = new PlayerState(true, this);
                _states[HYConstants.PlayerType.WhitePlayer] = new PlayerState(false, this);
                break;
            case HYConstants.GameType.MultiPlay:
                // todo: 멀티 기능 추가 
                break;
            
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

    public bool IsBlackPlayerTurn()
    {
        return _currentState == _states[HYConstants.PlayerType.BlackPlayer];
    }
}
