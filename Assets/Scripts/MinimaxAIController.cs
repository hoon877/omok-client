using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class MinimaxAIController
{
    // 최대 탐색 깊이 제한 (예시: 3)
    public static int MaxDepth = 3;
    
    private static List<(int row, int col)> GenerateMoves(GameManager.PlayerType[,] board)
    {
        List<(int row, int col)> moves = new List<(int row, int col)>();
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);

        // 보드에 하나도 마커가 없는 경우, 중앙만 후보로 추가
        bool boardEmpty = true;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (board[i, j] != GameManager.PlayerType.None)
                {
                    boardEmpty = false;
                    break;
                }
            }
            if (!boardEmpty)
                break;
        }
        if (boardEmpty)
        {
            moves.Add((rows / 2, cols / 2));
            return moves;
        }

        // 이미 놓인 마커 주변(예: 1칸 이내)에 빈 셀만 후보로 추가
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (board[i, j] == GameManager.PlayerType.None)
                {
                    bool adjacent = false;
                    // 8방향(또는 원하는 범위)으로 인접한 셀 확인
                    for (int di = -1; di <= 1 && !adjacent; di++)
                    {
                        for (int dj = -1; dj <= 1 && !adjacent; dj++)
                        {
                            int ni = i + di, nj = j + dj;
                            if (ni >= 0 && ni < rows && nj >= 0 && nj < cols && board[ni, nj] != GameManager.PlayerType.None)
                            {
                                adjacent = true;
                            }
                        }
                    }
                    if (adjacent)
                        moves.Add((i, j));
                }
            }
        }
        return moves;
    }

    public static (int row, int col)? GetBestMove(GameManager.PlayerType[,] board)
    {
        float bestScore = -1000;
        (int row, int col)? bestMove = null;
    
        var moves = GenerateMoves(board);
        foreach (var move in moves)
        {
            int row = move.row, col = move.col;
            board[row, col] = GameManager.PlayerType.PlayerB;
            var score = DoMinimax(board, 0, false);
            board[row, col] = GameManager.PlayerType.None;
        
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = (row, col);
            }
        }
    
        return bestMove;
    }

    private static float DoMinimax(GameManager.PlayerType[,] board, int depth, bool isMaximizing)
    {
        if (depth >= MaxDepth)
            return EvaluateBoard(board);

        if (CheckGameWin(GameManager.PlayerType.PlayerA, board))
            return -10 + depth;
        if (CheckGameWin(GameManager.PlayerType.PlayerB, board))
            return 10 - depth;
        if (IsAllBlocksPlaced(board))
            return 0;
    
        var moves = GenerateMoves(board);
    
        if (isMaximizing)
        {
            var bestScore = float.MinValue;
            foreach (var move in moves)
            {
                int row = move.row, col = move.col;
                board[row, col] = GameManager.PlayerType.PlayerB;
                var score = DoMinimax(board, depth + 1, false);
                board[row, col] = GameManager.PlayerType.None;
                bestScore = Math.Max(bestScore, score);
            }
            return bestScore;
        }
        else
        {
            var bestScore = float.MaxValue;
            foreach (var move in moves)
            {
                int row = move.row, col = move.col;
                board[row, col] = GameManager.PlayerType.PlayerA;
                var score = DoMinimax(board, depth + 1, true);
                board[row, col] = GameManager.PlayerType.None;
                bestScore = Math.Min(bestScore, score);
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
