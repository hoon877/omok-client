using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public class GameController
{
    private TurnManager _turnManager;
    private BoardController _boardController;
    private BoardClickHandler _boardClickHandler;
    private HYRenjuRuleChecker _renjuRuleChecker;
    private HYGameUIController _gameUIController;
    private Timer _timer;
    
    private HYConstants.MarkerType[,] _board;

    public GameController(HYConstants.GameType gameType)
    {
        _timer = Object.FindObjectOfType<Timer>();
        _timer.OnTimeout += GameOverOnTimeOut;
        
        _board = new HYConstants.MarkerType[HYConstants.BoardSize, HYConstants.BoardSize];
        InitBoard();

        _boardController = Object.FindObjectOfType<BoardController>();
        _boardClickHandler = Object.FindObjectOfType<BoardClickHandler>();
        _turnManager = new TurnManager(gameType, this);
        _renjuRuleChecker = new HYRenjuRuleChecker(_board, _turnManager);
        _gameUIController = Object.FindObjectOfType<HYGameUIController>();
        _gameUIController.InitGameUIController(this);

        _turnManager.OnTurnChanged += HandleTurnChanged;
        
        _timer.StartTImer(); 
    }

    private void InitBoard()
    {
        for (int i = 0; i < HYConstants.BoardSize; i++)
        {
            for (int j = 0; j < HYConstants.BoardSize; j++)
            {
                _board[i, j] = HYConstants.MarkerType.None;
            }
        }
    }

    public void HandleTimer()
    {
        _timer.InitTimer();
        _timer.StartTImer();
    }

    public void ExecuteCurrentTurn()
    {
        _turnManager.ExecuteCurrentTurn();
    }
    
    // 착수 성공시 실행 
    private void HandleTurnChanged()
    {
        var isBlackTurn = _turnManager.IsBlackPlayerTurn();
        if (isBlackTurn)
        {
            // 흑돌을 놓으면 마커 숨기기 
            _boardController.HideForbiddenMarkers();
        }
        else
        {
            // 백돌을 놓으면 금수를 계산하여 표시 
            _renjuRuleChecker.CalculateForbiddenPositions();
            UpdateForbiddenMarkers();
        }
        // 다음 턴의 UI로 변경 
        _gameUIController.OnTurnChanged?.Invoke(!isBlackTurn);
    }

    private void UpdateForbiddenMarkers()
    {
        var forbiddenPositions = _renjuRuleChecker.GetForbiddenPositions();
        for (int x = 0; x < forbiddenPositions.GetLength(0); x++)
        {
            for (int y = 0; y < forbiddenPositions.GetLength(1); y++)
            {
                if (!forbiddenPositions[x, y]) continue;
                var markerPos = _boardClickHandler.GetWorldPositionFromGrid(new Vector2Int(x, y));
                _boardController.SetForbiddenMarker(markerPos);
            }
        }
    }

    public bool TryPlaceMarker(bool isBlackPlayer)
    {
        var (gridPos, markerPos) = _boardClickHandler.GetSelectedPosition();

        if (!IsValidPosition(gridPos)) return false;

        var marker = isBlackPlayer ? HYConstants.MarkerType.Black : HYConstants.MarkerType.White;

        // 보드에 마커 저장 및 표시
        _board[gridPos.x, gridPos.y] = marker;
        _boardController.SetMarker(marker, markerPos);

        // 승리 조건 확인
        CheckGameResult(gridPos, marker);

        return true;
    }

    private bool IsInBoardRange(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < HYConstants.BoardSize && pos.y >= 0 && pos.y < HYConstants.BoardSize;
    }

    private bool IsValidPosition(Vector2Int gridPos)
    {
        // 보드 안쪽인지 확인 
        if (IsInBoardRange(gridPos) == false)
            return false;

        // 이미 마커가 있는지 확인
        if (_board[gridPos.x, gridPos.y] != HYConstants.MarkerType.None)
            return false;
        
        // 흑돌일 때 금수인지 확인
        if (_renjuRuleChecker.IsForbiddenPosition(gridPos))
            return false;

        return true;
    }
    
    
    
    #region CHECK_GAME_RESULT
    // 오목 완성 확인 
    private bool CheckGameResult(Vector2Int position, HYConstants.MarkerType marker)
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
                HYConstants.GameResult gameResult = 
                    marker == HYConstants.MarkerType.Black ? HYConstants.GameResult.BlackWin : HYConstants.GameResult.WhiteWin;
                GameOver(gameResult);
            }
            
        }

        return false;
    }

    // 연속된 마커 검사 
    private int CountConsecutiveMarkers(Vector2Int startPos, Vector2Int direction, HYConstants.MarkerType marker)
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
    
    private void GameOverOnTimeOut()
    {
        HYConstants.GameResult gameResult = 
            _turnManager.IsBlackPlayerTurn() ? HYConstants.GameResult.WhiteWin : HYConstants.GameResult.BlackWin;
        GameOver(gameResult);
    }
    private void GameOver(HYConstants.GameResult gameResult)
    {
        _gameUIController.ShowGameOverUI();
        _timer.InitTimer();
        string winner = gameResult.ToString();
        Debug.Log(winner);
    }
    #endregion
}
