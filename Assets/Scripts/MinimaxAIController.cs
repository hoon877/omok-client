using System;
using Unity.Mathematics;
using UnityEngine;

public static class MinimaxAIController
{
    // 최대 탐색 깊이 제한 (예시: 3)
    public static int MaxDepth = 3;

    public static (int row, int col)? GetBestMove(GameManager.PlayerType[,] board)
    {
        float bestScore = -1000;
        (int row, int col)? bestMove = null;
        
        for (var row = 0; row < board.GetLength(0); row++)
        {
            for (var col = 0; col < board.GetLength(1); col++)
            {
                if (board[row, col] == GameManager.PlayerType.None)
                {
                    board[row, col] = GameManager.PlayerType.PlayerB;
                    var score = DoMinimax(board, 0, false);
                    board[row, col] = GameManager.PlayerType.None;
                    
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = (row, col);
                    }
                }
            }
        }
        
        return bestMove;
    }

    private static float DoMinimax(GameManager.PlayerType[,] board, int depth, bool isMaximizing)
    {
        // 탐색 깊이 제한에 도달했으면 평가 함수 호출
        if (depth >= MaxDepth)
        {
            return EvaluateBoard(board);
        }

        if (CheckGameWin(GameManager.PlayerType.PlayerA, board))
            return -10 + depth;
        if (CheckGameWin(GameManager.PlayerType.PlayerB, board))
            return 10 - depth;
        if (IsAllBlocksPlaced(board))
            return 0;

        if (isMaximizing)
        {
            var bestScore = float.MinValue;
            for (var row = 0; row < board.GetLength(0); row++)
            {
                for (var col = 0; col < board.GetLength(1); col++)
                {
                    if (board[row, col] == GameManager.PlayerType.None)
                    {
                        board[row, col] = GameManager.PlayerType.PlayerB;
                        var score = DoMinimax(board, depth + 1, false);
                        board[row, col] = GameManager.PlayerType.None;
                        bestScore = Math.Max(bestScore, score);
                    }
                }
            }
            return bestScore;
        }
        else
        {
            var bestScore = float.MaxValue;
            for (var row = 0; row < board.GetLength(0); row++)
            {
                for (var col = 0; col < board.GetLength(1); col++)
                {
                    if (board[row, col] == GameManager.PlayerType.None)
                    {
                        board[row, col] = GameManager.PlayerType.PlayerA;
                        var score = DoMinimax(board, depth + 1, true);
                        board[row, col] = GameManager.PlayerType.None;
                        bestScore = Math.Min(bestScore, score);
                    }
                }
            }
            return bestScore;
        }
    }
    
    // 평가 함수: 최대 깊이에 도달했을 때 보드 상태를 평가합니다.
    // 현재는 단순히 0을 반환하도록 구현했으나, 실제 상황에 맞게 보드의 유리/불리 정도를 계산해야 합니다.
    private static float EvaluateBoard(GameManager.PlayerType[,] board)
    {
        return 0;
    }
    
    /// <summary>
    /// 모든 마커가 보드에 배치 되었는지 확인하는 함수
    /// </summary>
    /// <returns>True: 모두 배치</returns>
    public static bool IsAllBlocksPlaced(GameManager.PlayerType[,] board)
    {
        for (var row = 0; row < board.GetLength(0); row++)
        {
            for (var col = 0; col < board.GetLength(1); col++)
            {
                if (board[row, col] == GameManager.PlayerType.None)
                    return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// 게임의 승패를 판단하는 함수
    /// </summary>
    /// <param name="playerType"></param>
    /// <param name="board"></param>
    /// <returns></returns>
    private static bool CheckGameWin(GameManager.PlayerType playerType, GameManager.PlayerType[,] board)
    {
        // 가로로 마커가 일치하는지 확인
        for (var row = 0; row < board.GetLength(0); row++)
            if (board[row, 0] == playerType && board[row, 1] == playerType && board[row, 2] == playerType)
                return true;
        
        // 세로로 마커가 일치하는지 확인
        for (var col = 0; col < board.GetLength(1); col++)
            if (board[0, col] == playerType && board[1, col] == playerType && board[2, col] == playerType)
                return true;
        
        // 대각선 마커 일치 확인
        if (board[0, 0] == playerType && board[1, 1] == playerType && board[2, 2] == playerType)
            return true;
        if (board[0, 2] == playerType && board[1, 1] == playerType && board[2, 0] == playerType)
            return true;
        
        return false;
    }
}
