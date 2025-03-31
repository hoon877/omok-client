using System;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

public static class MinimaxAIController
{
    // 최대 탐색 깊이 제한 (깊이 3이하는 게임이 안 됨)
    public static int MaxDepth = 4;

    // 이전 평가값을 저장하는 transposition table.
    // key: board 상태의 해시, value: (평가 점수, 남은 탐색 깊이)
    private static Dictionary<string, (float score, int depth)> transpositionTable = new Dictionary<string, (float, int)>();

    private static List<(int row, int col)> GenerateMoves(LConstants.PlayerType[,] board)
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
                if (board[i, j] != LConstants.PlayerType.None)
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
                if (board[i, j] == LConstants.PlayerType.None)
                {
                    bool adjacent = false;
                    // 8방향(또는 원하는 범위)으로 인접한 셀 확인
                    for (int di = -1; di <= 1 && !adjacent; di++)
                    {
                        for (int dj = -1; dj <= 1 && !adjacent; dj++)
                        {
                            int ni = i + di, nj = j + dj;
                            if (ni >= 0 && ni < rows && nj >= 0 && nj < cols && board[ni, nj] != LConstants.PlayerType.None)
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

    // 주변에 놓인 돌의 개수를 기반으로 후보 수의 우선순위를 평가하는 함수
    private static int GetAdjacentScore(LConstants.PlayerType[,] board, int row, int col)
    {
        int score = 0;
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);
        for (int dr = -1; dr <= 1; dr++)
        {
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0)
                    continue;
                int nr = row + dr, nc = col + dc;
                if (nr >= 0 && nr < rows && nc >= 0 && nc < cols && board[nr, nc] != LConstants.PlayerType.None)
                {
                    score++;
                }
            }
        }
        return score;
    }

    // 보드 상태를 문자열로 변환하여 해시로 사용합니다.
    private static string BoardToString(LConstants.PlayerType[,] board)
    {
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);
        StringBuilder sb = new StringBuilder(rows * cols);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                // LConstants.PlayerType은 열거형(enum)이라 가령 정수형 값으로 변환
                sb.Append((int)board[i, j]);
            }
        }
        return sb.ToString();
    }
    
    public static (int row, int col)? GetBestMove(LConstants.PlayerType[,] board)
    {
        // 새로운 탐색을 시작할 때 이전 평가값 캐시를 초기화합니다.
        transpositionTable.Clear();
        
        float bestScore = -100;
        (int row, int col)? bestMove = null;
    
        var moves = GenerateMoves(board);
        // 상위 레벨에서도 후보 순서를 정렬하여 알파-베타 가지치기를 보다 효과적으로 만듭니다.
        moves.Sort((move1, move2) =>
        {
            int score1 = GetAdjacentScore(board, move1.row, move1.col);
            int score2 = GetAdjacentScore(board, move2.row, move2.col);
            return score2.CompareTo(score1);
        });
    
        foreach (var move in moves)
        {
            int row = move.row, col = move.col;
            board[row, col] = LConstants.PlayerType.PlayerB;
            // 초기 alpha는 -∞, beta는 +∞로 설정
            var score = DoMinimax(board, 0, false, float.MinValue, float.MaxValue);
            board[row, col] = LConstants.PlayerType.None;
        
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = (row, col);
            }
        }
    
        return bestMove;
    }

    // alpha-beta pruning 및 이동 순서 정렬과 이전 평가값 저장(transposition table)을 적용한 minimax 함수
    private static float DoMinimax(LConstants.PlayerType[,] board, int depth, bool isMaximizing, float alpha, float beta)
    {
        // 남은 탐색 깊이
        int remainingDepth = MaxDepth - depth;
        
        // 보드 상태의 해시값을 생성합니다.
        string boardHash = BoardToString(board);
        // 캐시된 값이 존재하고, 저장된 남은 깊이가 현재 남은 깊이보다 크거나 같으면 재사용합니다.
        if (transpositionTable.TryGetValue(boardHash, out var entry))
        {
            if (entry.depth >= remainingDepth)
            {
                return entry.score;
            }
        }

        if (depth >= MaxDepth)
            return EvaluateBoard(board);

        if (CheckGameWin(LConstants.PlayerType.PlayerA, board))
            return -10 + depth;
        if (CheckGameWin(LConstants.PlayerType.PlayerB, board))
            return 10 - depth;
        if (IsAllBlocksPlaced(board))
            return 0;
    
        var moves = GenerateMoves(board);
        // 이동 순서 정렬: 주변에 돌이 많은 후보를 우선적으로 탐색
        moves.Sort((move1, move2) =>
        {
            int score1 = GetAdjacentScore(board, move1.row, move1.col);
            int score2 = GetAdjacentScore(board, move2.row, move2.col);
            return score2.CompareTo(score1);
        });
    
        float bestScore;
        if (isMaximizing)
        {
            bestScore = float.MinValue;
            foreach (var move in moves)
            {
                int row = move.row, col = move.col;
                board[row, col] = LConstants.PlayerType.PlayerB;
                float score = DoMinimax(board, depth + 1, false, alpha, beta);
                board[row, col] = LConstants.PlayerType.None;
                bestScore = Math.Max(bestScore, score);
                alpha = Math.Max(alpha, bestScore);
                if (beta <= alpha)
                    break; // 가지치기
            }
        }
        else
        {
            bestScore = float.MaxValue;
            foreach (var move in moves)
            {
                int row = move.row, col = move.col;
                board[row, col] = LConstants.PlayerType.PlayerA;
                float score = DoMinimax(board, depth + 1, true, alpha, beta);
                board[row, col] = LConstants.PlayerType.None;
                bestScore = Math.Min(bestScore, score);
                beta = Math.Min(beta, bestScore);
                if (beta <= alpha)
                    break; // 가지치기
            }
        }
    
        // 계산된 값을 캐시에 저장합니다.
        transpositionTable[boardHash] = (bestScore, remainingDepth);
        return bestScore;
    }
    
    // 평가 함수: 최대 깊이에 도달했을 때 보드 상태를 평가합니다.
    // 현재는 단순히 0을 반환하도록 구현했으나, 실제 상황에 맞게 보드의 유리/불리 정도를 계산해야 합니다.
    private static float EvaluateBoard(LConstants.PlayerType[,] board)
    {
        return 0;
    }
    
    /// <summary>
    /// 모든 마커가 보드에 배치 되었는지 확인하는 함수
    /// </summary>
    /// <returns>True: 모두 배치</returns>
    public static bool IsAllBlocksPlaced(LConstants.PlayerType[,] board)
    {
        for (var row = 0; row < board.GetLength(0); row++)
        {
            for (var col = 0; col < board.GetLength(1); col++)
            {
                if (board[row, col] == LConstants.PlayerType.None)
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
    private static bool CheckGameWin(LConstants.PlayerType playerType, LConstants.PlayerType[,] board)
    {
        int numRows = board.GetLength(0);
        int numCols = board.GetLength(1);
        int winCount = 5;

        // 보드의 모든 셀에 대해 검사
        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numCols; col++)
            {
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
