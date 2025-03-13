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
        
        // 승리 조건 확인
        CheckGameResult(gridPos, marker);
        
        return true;
    }

    private bool IsValidPosition(Vector2Int gridPos)
    {
        // 보드 안쪽인지 확인 
        if (gridPos.x < 0 || gridPos.x >= Constants.BoardSize || 
            gridPos.y < 0 || gridPos.y >= Constants.BoardSize)
            return false;
            
        // 이미 마커가 있는지 확인
        if (_board[gridPos.x, gridPos.y] != Constants.MarkerType.None)
            return false;
            
        // todo : 흑돌 금수 검사 
        if (_turnManager.IsBlackPlayerTurn())
        {
            // 금수 검사 로직
        }
        
        return true;
    }
    
    #region CheckGameResult
    // 오목 완성 확인 
    private bool CheckGameResult(Vector2Int position, Constants.MarkerType marker)
    {
        // 방향 벡터 : 가로, 세로, 대각선 
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),   // 오른쪽
            new Vector2Int(0, 1),   // 위쪽
            new Vector2Int(1, 1),   // 오른쪽 위 대각선
            new Vector2Int(1, -1)   // 오른쪽 아래 대각선 
        };
        
        // 양쪽 방향으로 검사 
        foreach (Vector2Int dir in directions)
        {
            // 반대 방향 벡터 
            Vector2Int oppositeDir = new Vector2Int(-dir.x, -dir.y);
            
            // 현재 위치에서 해당 방향과 반대 방향으로 연속된 마커 카운트
            int count = 1;  // 현재 위치 마커 포함
            
            // 정방향
            count += CountConsecutiveMarkers(position, dir, marker);
            
            // 역방향
            count += CountConsecutiveMarkers(position, oppositeDir, marker);

            // 오목 완성, 승리 
            if (count >= 5)
            {
                string winner = marker == Constants.MarkerType.Black ? "흑돌" : "백돌";
                Debug.Log($"{winner} 플레이어가 승리했습니다!");
                return true;
            }
            
        }

        return false;
    }

    private int CountConsecutiveMarkers(Vector2Int startPos, Vector2Int direction, Constants.MarkerType marker)
    {
        int count = 0;
        Vector2Int currentPos = startPos + direction;

        while (IsInBoardRange(currentPos) && _board[currentPos.x, currentPos.y] == marker)
        {
            count++;
            currentPos += direction;
        }
        return count;
    }
    
    private bool IsInBoardRange(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < Constants.BoardSize && pos.y >= 0 && pos.y < Constants.BoardSize;
    }

    #endregion
}
