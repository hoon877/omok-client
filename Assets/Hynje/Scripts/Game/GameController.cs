using UnityEngine;
using UnityEngine.Serialization;

public class GameController
{
    private TurnManager _turnManager;
    private BoardController _boardController;
    private BoardClickHandler _boardClickHandler;

    private Constants.MarkerType[,] _board;
    
    public GameController(Constants.GameType gameType)
    {
        _board = new Constants.MarkerType[Constants.BoardSize, Constants.BoardSize];
        InitBoard();
        
        _boardController = Object.FindObjectOfType<BoardController>();
        _boardClickHandler = Object.FindObjectOfType<BoardClickHandler>();
        _turnManager = new TurnManager(gameType, this);
        
    }

    private void InitBoard()
    {
        
        for (int i = 0; i < Constants.BoardSize; i++)
        {
            for (int j = 0; j < Constants.BoardSize; j++)
            {
                _board[i, j] = Constants.MarkerType.None;
            }
        }
    }
    
    public void ExecuteCurrentTurn()
    {
        _turnManager.ExecuteCurrentTurn();
    }
    
    public bool TryPlaceMarker(bool isBlackPlayer)
    {
        var (gridPos, markerPos) = _boardClickHandler.GetSelectedPosition();
        
        if (!IsValidPosition(gridPos)) return false;
        
        var marker = isBlackPlayer ? Constants.MarkerType.Black : Constants.MarkerType.White;
        
        // 보드에 마커 저장 및 표시
        _board[gridPos.x, gridPos.y] = marker;
        _boardController.SetMarker(marker, markerPos);
        
        // todo : 승리 조건 확인 등의 게임 로직 처리
        //CheckGameResult(gridPos, marker);
        
        return true;
    }

    private bool IsValidPosition(Vector2Int gridPos)
    {
        // 기본 유효성 검사
        if (gridPos.x < 0 || gridPos.x >= Constants.BoardSize || 
            gridPos.y < 0 || gridPos.y >= Constants.BoardSize)
            return false;
            
        // 이미 마커가 있는지 확인
        if (_board[gridPos.x, gridPos.y] != Constants.MarkerType.None)
            return false;
            
        // todo : 흑돌 특수 규칙 검사 (금수 등)
        if (_turnManager.IsBlackPlayerTurn())
        {
            // 금수 검사 로직
        }
        
        return true;
    }
    
    private void CheckGameResult(Vector2Int position, Constants.MarkerType marker)
    {
        // 승리 조건 확인 로직
    }
}
