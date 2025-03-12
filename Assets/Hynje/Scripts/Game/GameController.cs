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
        _turnManager = new TurnManager(gameType);
        _boardController = Object.FindObjectOfType<BoardController>();
        _boardClickHandler = Object.FindObjectOfType<BoardClickHandler>();
        
        InitBoard();
    }

    private void InitBoard()
    {
        _board = new Constants.MarkerType[Constants.BoardSize, Constants.BoardSize];
        
        for (int i = 0; i < Constants.BoardSize; i++)
        {
            for (int j = 0; j < Constants.BoardSize; j++)
            {
                _board[i, j] = Constants.MarkerType.None;
            }
        }
    }
    
    public void SetMarkerOnBoard()
    {
        var (girdPos,markerPos) = _boardClickHandler.GetSelectedPosition();
        var marker = _turnManager.IsBlackPlayerTurn() ? Constants.MarkerType.Black : Constants.MarkerType.White;

        if (!IsValidPosition(girdPos)) return;
        
        // _board에 마커 타입 저장
        _board[girdPos.x, girdPos.y] = marker;
        
        // 보드에 마커 표시 
        _boardController.SetMarker(marker, markerPos);
        
        // todo : 결과 체크
        
        _turnManager.ChangeTurn();
    }

    private bool IsValidPosition(Vector2Int gridPos)
    {
        // todo : black 턴일 때 금수 체크 
        
        return _board[gridPos.x, gridPos.y] == Constants.MarkerType.None;
    }
}
