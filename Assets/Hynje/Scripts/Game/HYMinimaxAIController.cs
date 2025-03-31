using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class HYMinimaxAIController
{
    // 최대 탐색 깊이 제한
    private int _maxDepth = 4;

    // 이전 평가값을 저장하는 transposition table
    private Dictionary<string, (float score, int depth)> _transpositionTable;

    // 생성자
    public HYMinimaxAIController(int maxDepth = 4)
    {
        _maxDepth = maxDepth;
        _transpositionTable = new Dictionary<string, (float, int)>();
    }

    // AI 계산을 위한 메인 메서드: 최적의 수를 계산하여 반환
    public (int row, int col)? GetBestMove(HYConstants.MarkerType[,] board)
    {
        // 새로운 탐색 시작시 이전 평가값 캐시 초기화
        _transpositionTable.Clear();
        
        float bestScore = float.MinValue;
        (int row, int col)? bestMove = null;
    
        var moves = GenerateMoves(board);
        // 이동 순서 정렬
        moves.Sort((move1, move2) =>
        {
            int score1 = GetAdjacentScore(board, move1.row, move1.col);
            int score2 = GetAdjacentScore(board, move2.row, move2.col);
            return score2.CompareTo(score1);
        });
    
        foreach (var move in moves)
        {
            int row = move.row, col = move.col;
            // 백돌 (AI)
            board[row, col] = HYConstants.MarkerType.White;
            
            var score = DoMinimax(board, 0, false, float.MinValue, float.MaxValue);
            board[row, col] = HYConstants.MarkerType.None; // 원래 상태로 복원
        
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = (row, col);
            }
        }
    
        return bestMove;
    }

    // 빈 셀 중에서 기존 돌 주변에 있는 위치만 후보로 선정
    private List<(int row, int col)> GenerateMoves(HYConstants.MarkerType[,] board)
    {
        List<(int row, int col)> moves = new List<(int row, int col)>();
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);

        // 보드가 비어있는지 체크
        bool boardEmpty = true;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (board[i, j] != HYConstants.MarkerType.None)
                {
                    boardEmpty = false;
                    break;
                }
            }
            if (!boardEmpty)
                break;
        }
        
        // 보드가 비어있으면 중앙 위치만 반환
        if (boardEmpty)
        {
            moves.Add((rows / 2, cols / 2));
            return moves;
        }

        // 기존 돌 주변(1칸)에 있는 빈 셀만 후보로 추가
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (board[i, j] == HYConstants.MarkerType.None)
                {
                    bool adjacent = false;
                    // 8방향 주변 셀 체크
                    for (int di = -1; di <= 1 && !adjacent; di++)
                    {
                        for (int dj = -1; dj <= 1 && !adjacent; dj++)
                        {
                            if (di == 0 && dj == 0) continue;
                            
                            int ni = i + di, nj = j + dj;
                            if (ni >= 0 && ni < rows && nj >= 0 && nj < cols && 
                                board[ni, nj] != HYConstants.MarkerType.None)
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

    // 주변 돌 개수로 우선순위 평가
    private int GetAdjacentScore(HYConstants.MarkerType[,] board, int row, int col)
    {
        int score = 0;
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);
        
        for (int dr = -1; dr <= 1; dr++)
        {
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                
                int nr = row + dr, nc = col + dc;
                if (nr >= 0 && nr < rows && nc >= 0 && nc < cols && 
                    board[nr, nc] != HYConstants.MarkerType.None)
                {
                    score++;
                }
            }
        }
        return score;
    }

    // 보드 상태 해시를 위한 문자열 변환
    private string BoardToString(HYConstants.MarkerType[,] board)
    {
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);
        StringBuilder sb = new StringBuilder(rows * cols);
        
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                sb.Append((int)board[i, j]);
            }
        }
        return sb.ToString();
    }

    // Minimax 알고리즘 구현 (알파-베타 가지치기 포함)
    private float DoMinimax(HYConstants.MarkerType[,] board, int depth, bool isMaximizing, float alpha, float beta)
    {
        int remainingDepth = _maxDepth - depth;
        
        // 트랜스포지션 테이블에서 이미 계산된 값 확인
        string boardHash = BoardToString(board);
        if (_transpositionTable.TryGetValue(boardHash, out var entry))
        {
            if (entry.depth >= remainingDepth)
            {
                return entry.score;
            }
        }

        // 종료 조건: 최대 깊이 도달
        if (depth >= _maxDepth)
            return EvaluateBoard(board);

        // 종료 조건: 승리 체크 (5목 완성)
        if (HasFiveInARow(board, HYConstants.MarkerType.Black))
            return -10 + depth; // 흑돌(사용자) 승리
        if (HasFiveInARow(board, HYConstants.MarkerType.White))
            return 10 - depth; // 백돌(AI) 승리

        // 종료 조건: 보드 가득 참
        if (IsBoardFull(board))
            return 0;
    
        var moves = GenerateMoves(board);
        // 이동 순서 정렬
        moves.Sort((move1, move2) =>
        {
            int score1 = GetAdjacentScore(board, move1.row, move1.col);
            int score2 = GetAdjacentScore(board, move2.row, move2.col);
            return score2.CompareTo(score1);
        });
    
        float bestScore;
        if (isMaximizing) // AI 차례 (백돌)
        {
            bestScore = float.MinValue;
            foreach (var move in moves)
            {
                int row = move.row, col = move.col;
                board[row, col] = HYConstants.MarkerType.White;
                float score = DoMinimax(board, depth + 1, false, alpha, beta);
                board[row, col] = HYConstants.MarkerType.None;
                
                bestScore = Math.Max(bestScore, score);
                alpha = Math.Max(alpha, bestScore);
                if (beta <= alpha)
                    break; // 가지치기
            }
        }
        else // 사용자 차례 (흑돌)
        {
            bestScore = float.MaxValue;
            foreach (var move in moves)
            {
                int row = move.row, col = move.col;
                board[row, col] = HYConstants.MarkerType.Black;
                float score = DoMinimax(board, depth + 1, true, alpha, beta);
                board[row, col] = HYConstants.MarkerType.None;
                
                bestScore = Math.Min(bestScore, score);
                beta = Math.Min(beta, bestScore);
                if (beta <= alpha)
                    break; // 가지치기
            }
        }
    
        // 계산된 값을 캐시에 저장
        _transpositionTable[boardHash] = (bestScore, remainingDepth);
        return bestScore;
    }
    
    // 간단한 평가 함수 (향상된 휴리스틱 구현 가능)
    private float EvaluateBoard(HYConstants.MarkerType[,] board)
    {
        // 기본 구현은 0 반환, 필요하면 향상된 평가 함수 구현
        float score = 0;
        score += EvaluateLines(board, HYConstants.MarkerType.White); // AI (백돌) 점수
        score -= EvaluateLines(board, HYConstants.MarkerType.Black); // 플레이어 (흑돌) 점수
        return score;
    }
    
    // 가로, 세로, 대각선 라인에서 연속된 돌 평가
    private float EvaluateLines(HYConstants.MarkerType[,] board, HYConstants.MarkerType markerType)
    {
        float score = 0;
        int size = board.GetLength(0);
        
        // 가로, 세로, 대각선 방향 평가
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };
        
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int dir = 0; dir < 4; dir++)
                {
                    int count = 0;
                    int openEnds = 0;
                    
                    // 이전 위치 체크 (오픈된 끝인지)
                    int prevX = i - dx[dir];
                    int prevY = j - dy[dir];
                    if (prevX >= 0 && prevX < size && prevY >= 0 && prevY < size && 
                        board[prevX, prevY] == HYConstants.MarkerType.None)
                    {
                        openEnds++;
                    }
                    
                    // 연속된 같은 마커 세기
                    int x = i;
                    int y = j;
                    while (x >= 0 && x < size && y >= 0 && y < size && count < 5 && 
                           board[x, y] == markerType)
                    {
                        count++;
                        x += dx[dir];
                        y += dy[dir];
                    }
                    
                    // 다음 위치 체크 (오픈된 끝인지)
                    if (x >= 0 && x < size && y >= 0 && y < size && 
                        board[x, y] == HYConstants.MarkerType.None)
                    {
                        openEnds++;
                    }
                    
                    // 점수 계산
                    if (count > 0)
                    {
                        // 연속된 돌과 열린 끝 개수에 따라 점수 할당
                        // 열린 끝이 많을수록 더 높은 점수
                        switch (count)
                        {
                            case 1: score += 0.1f * openEnds; break;
                            case 2: score += 1f * openEnds; break;
                            case 3: score += 10f * openEnds; break;
                            case 4: score += 100f * openEnds; break;
                            case 5: score += 10000f; break; // 5개 연속은 승리
                        }
                    }
                }
            }
        }
        
        return score;
    }
    
    // 보드가 가득 찼는지 체크
    private bool IsBoardFull(HYConstants.MarkerType[,] board)
    {
        int size = board.GetLength(0);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (board[i, j] == HYConstants.MarkerType.None)
                    return false;
            }
        }
        return true;
    }
    
    // 5목 확인 (단순 체크용)
    private bool HasFiveInARow(HYConstants.MarkerType[,] board, HYConstants.MarkerType markerType)
    {
        int size = board.GetLength(0);
        
        // 가로, 세로, 대각선 방향 체크
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };
        
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int dir = 0; dir < 4; dir++)
                {
                    int count = 0;
                    int x = i;
                    int y = j;
                    
                    while (x >= 0 && x < size && y >= 0 && y < size && count < 5)
                    {
                        if (board[x, y] != markerType)
                            break;
                            
                        count++;
                        x += dx[dir];
                        y += dy[dir];
                    }
                    
                    if (count == 5)
                        return true;
                }
            }
        }
        
        return false;
    }
    
    // 최대 깊이 설정 메서드 (난이도 조정용)
    public void SetMaxDepth(int depth)
    {
        _maxDepth = Math.Max(1, depth);
    }
}