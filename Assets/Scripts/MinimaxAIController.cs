using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class MinimaxAIController
{
    // 최대 탐색 깊이 제한 (예시: 3)
    public static int MaxDepth = 3;
    
    private static List<(int row, int col)> GenerateMoves(Constants.PlayerType[,] board)
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
                if (board[i, j] != Constants.PlayerType.None)
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
                if (board[i, j] == Constants.PlayerType.None)
                {
                    bool adjacent = false;
                    // 8방향(또는 원하는 범위)으로 인접한 셀 확인
                    for (int di = -1; di <= 1 && !adjacent; di++)
                    {
                        for (int dj = -1; dj <= 1 && !adjacent; dj++)
                        {
                            int ni = i + di, nj = j + dj;
                            if (ni >= 0 && ni < rows && nj >= 0 && nj < cols && board[ni, nj] != Constants.PlayerType.None)
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

    public static (int row, int col)? GetBestMove(Constants.PlayerType[,] board)
    {
        float bestScore = -1000;
        (int row, int col)? bestMove = null;
    
        var moves = GenerateMoves(board);
        foreach (var move in moves)
        {
            int row = move.row, col = move.col;
            board[row, col] = Constants.PlayerType.PlayerB;
            var score = DoMinimax(board, 0, false);
            board[row, col] = Constants.PlayerType.None;
        
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = (row, col);
            }
        }
    
        return bestMove;
    }

    private static float DoMinimax(Constants.PlayerType[,] board, int depth, bool isMaximizing)
    {
        if (depth >= MaxDepth)
            return EvaluateBoard(board);

        if (CheckGameWin(Constants.PlayerType.PlayerA, board))
            return -10 + depth;
        if (CheckGameWin(Constants.PlayerType.PlayerB, board))
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
                board[row, col] = Constants.PlayerType.PlayerB;
                var score = DoMinimax(board, depth + 1, false);
                board[row, col] = Constants.PlayerType.None;
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
                board[row, col] = Constants.PlayerType.PlayerA;
                var score = DoMinimax(board, depth + 1, true);
                board[row, col] = Constants.PlayerType.None;
                bestScore = Math.Min(bestScore, score);
            }
            return bestScore;
        }
    }
    
    // 평가 함수: 최대 깊이에 도달했을 때 보드 상태를 평가합니다.
    // 현재는 단순히 0을 반환하도록 구현했으나, 실제 상황에 맞게 보드의 유리/불리 정도를 계산해야 합니다.
    private static float EvaluateBoard(Constants.PlayerType[,] board)
    {
        return 0;
    }
    
    /// <summary>
    /// 모든 마커가 보드에 배치 되었는지 확인하는 함수
    /// </summary>
    /// <returns>True: 모두 배치</returns>
    public static bool IsAllBlocksPlaced(Constants.PlayerType[,] board)
    {
        for (var row = 0; row < board.GetLength(0); row++)
        {
            for (var col = 0; col < board.GetLength(1); col++)
            {
                if (board[row, col] == Constants.PlayerType.None)
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
    private static bool CheckGameWin(Constants.PlayerType playerType, Constants.PlayerType[,] board)
    {
        int numRows = board.GetLength(0);
        int numCols = board.GetLength(1);
        int winCount = 5;

        // 보드의 모든 셀에 대해 검사
        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numCols; col++)
            {
                // 현재 셀이 지정된 플레이어의 마커가 아니라면 넘어감
                if (board[row, col] != playerType)
                    continue;

                // 가로 방향 (오른쪽)
                if (col <= numCols - winCount)
                {
                    bool win = true;
                    for (int i = 0; i < winCount; i++)
                    {
                        if (board[row, col + i] != playerType)
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win)
                        return true;
                }

                // 세로 방향 (아래쪽)
                if (row <= numRows - winCount)
                {
                    bool win = true;
                    for (int i = 0; i < winCount; i++)
                    {
                        if (board[row + i, col] != playerType)
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win)
                        return true;
                }

                // 대각선 방향 (아래쪽 오른쪽)
                if (row <= numRows - winCount && col <= numCols - winCount)
                {
                    bool win = true;
                    for (int i = 0; i < winCount; i++)
                    {
                        if (board[row + i, col + i] != playerType)
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win)
                        return true;
                }

                // 역대각선 방향 (아래쪽 왼쪽)
                if (row <= numRows - winCount && col >= winCount - 1)
                {
                    bool win = true;
                    for (int i = 0; i < winCount; i++)
                    {
                        if (board[row + i, col - i] != playerType)
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win)
                        return true;
                }
            }
        }

        return false;
    }

}
