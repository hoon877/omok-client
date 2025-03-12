
using System;

public interface ITurnState
{
    void Enter();
    void Execute();
    void Exit();
}

public class PlayerState : ITurnState
{
    private Constants.PlayerType _playerType;
    private bool _isBlackPlayer;

    public PlayerState(bool isBlackPlayer)
    {
       _playerType = isBlackPlayer ? Constants.PlayerType.BlackPlayer : Constants.PlayerType.WhitePlayer;
    }
    public void Enter()
    {
        
    }

    public void Execute()
    {
        
    }

    public void Exit()
    {
        
    }
}

public class AIState : ITurnState
{
    public void Enter()
    {
        throw new System.NotImplementedException();
    }

    public void Execute()
    {
        throw new System.NotImplementedException();
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }
}

public class TurnManager
{
    private ITurnState _currentPlayerState;
    private ITurnState _blackPlayerState;
    private ITurnState _whitePlayerState;

    public TurnManager(Constants.GameType gameType)
    {
        switch (gameType)
        {
            case Constants.GameType.SinglePlay:
                // todo: AI 기능 추가 
                break;
            case Constants.GameType.DualPlay:
                _blackPlayerState = new PlayerState(true);
                _whitePlayerState = new PlayerState(false);
                break;
            case Constants.GameType.MultiPlay:
                // todo: 멀티 기능 추가 
                break;
            
        }
        _currentPlayerState = _blackPlayerState;
        _currentPlayerState?.Enter();
    }

    public void ChangeTurn()
    {
        _currentPlayerState.Exit();
        _currentPlayerState = (_currentPlayerState == _blackPlayerState) ? _whitePlayerState : _blackPlayerState;
        _currentPlayerState.Enter();
    }

    public ITurnState GetCurrentTurn()
    {
        return _currentPlayerState;
    }

    public bool IsBlackPlayerTurn()
    {
        return _currentPlayerState == _blackPlayerState;
    }
}
