using System;
using System.Collections.Generic;
using UnityEngine;

public class RenjuRuleChecker
{
    private Constants.MarkerType[,] _board;
    private TurnManager _turnManager;
    
    // 금수 위치를 저장할 배열
    private bool[,] _forbiddenPositions;
    
    public RenjuRuleChecker(Constants.MarkerType[,] board, TurnManager turnManager)
    {
        _board = board;
        _turnManager = turnManager;
        _forbiddenPositions = new bool[_board.GetLength(0), _board.GetLength(1)];
    }
    
    // 백돌 차례가 끝난 후 호출하여 모든 금수 위치 계산
    public void CalculateForbiddenPositions()
    {
        Debug.Log("Calculating forbidden positions");
        // 금수 위치 배열 초기화
        for (int x = 0; x < _board.GetLength(0); x++)
        {
            for (int y = 0; y < _board.GetLength(1); y++)
            {
                _forbiddenPositions[x, y] = false;
            }
        }
    
        // 흑돌 차례일 때만 금수 계산 (백돌에게는 금수가 없음)
        if (_turnManager.IsBlackPlayerTurn())
        {
            // 모든 빈 위치에 대해 금수 여부 계산
            for (int x = 0; x < _board.GetLength(0); x++)
            {
                for (int y = 0; y < _board.GetLength(1); y++)
                {
                    if (_board[x, y] == Constants.MarkerType.None)
                    {
                        Vector2Int pos = new Vector2Int(x, y);
                        // 통합된 금수 체크 메서드 사용
                        _forbiddenPositions[x, y] = CheckAllForbiddenRules(pos);
                    }
                }
            }
        }
    }
    
    private bool CheckAllForbiddenRules(Vector2Int position)
    {
        // 이미 돌이 놓인 위치면 금수가 아님
        if (_board[position.x, position.y] != Constants.MarkerType.None)
            return false;
        
        // 임시로 보드에 흑돌을 놓아보고 금수 여부 확인
        Constants.MarkerType originalValue = _board[position.x, position.y];
        _board[position.x, position.y] = Constants.MarkerType.Black;
    
        bool isForbidden = false;
    
        // 1. 삼삼 금지 체크 (3-3)
        if (CheckDoubleThree(position))
        {
            isForbidden = true;
        }
        // 2. 사사 금지 체크 (4-4)
        else if (CheckDoubleFour(position))
        {
            isForbidden = true;
        }
        // 3. 장목 금지 체크 (오목 초과)
        else if (CheckOverline(position))
        {
            isForbidden = true;
        }
    
        // 임시로 놓은 돌 제거
        _board[position.x, position.y] = originalValue;
    
        return isForbidden;
    }
    
    // 특정 위치가 금수인지 확인
    public bool IsForbiddenPosition(Vector2Int position)
    {
        // 흑돌 차례가 아니면 금수 규칙 적용 안함
        if (!_turnManager.IsBlackPlayerTurn())
            return false;
            
        return _forbiddenPositions[position.x, position.y];
    }
    
    // 금수 위치 배열 반환 (UI 표시용)
    public bool[,] GetForbiddenPositions()
    {
        return _forbiddenPositions;
    }
    
    // 삼삼 금지 체크 (3-3)
    private bool CheckDoubleThree(Vector2Int position)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),   // 가로
            new Vector2Int(0, 1),   // 세로
            new Vector2Int(1, 1),   // 대각선 (↗)
            new Vector2Int(1, -1)   // 대각선 (↘)
        };

        int openThreeCount = 0;

        foreach (Vector2Int dir in directions)
        {
            if (HasOpenThree(position, dir))
            {
                openThreeCount++;
                if (openThreeCount >= 2)
                    return true; // 삼삼(33) 금수 발생
            }
        }

        return false;
    }
    
    // 열린 3 확인 (한 방향)
    private bool HasOpenThree(Vector2Int position, Vector2Int direction)
    {
        Vector2Int oppositeDir = new Vector2Int(-direction.x, -direction.y);

        // 양방향에서 패턴 가져오기
        List<Constants.MarkerType> pattern = GetLinePattern(position, direction, 4);
        pattern.Reverse();
        List<Constants.MarkerType> oppositePattern = GetLinePattern(position, oppositeDir, 4);
        oppositePattern.RemoveAt(0); // 중복 제거

        pattern.AddRange(oppositePattern);

        int centerIdx = pattern.Count / 2;

        return IsOpenThreePattern(pattern, centerIdx);
    }
    
    // 열린 3 패턴 체크 
    private bool IsOpenThreePattern(List<Constants.MarkerType> pattern, int centerIdx)
    {
        // B: 흑돌, 0: 빈 공간, W: 백돌, X: 보드 밖 또는 백돌 

        // 열린 3 패턴들 (0: 빈칸, B: 흑돌, W: 백돌)
        Constants.MarkerType[][] openThreePatterns = new Constants.MarkerType[][]
        {
            new[]
            {
                Constants.MarkerType.None, Constants.MarkerType.Black, Constants.MarkerType.Black,
                Constants.MarkerType.Black, Constants.MarkerType.None
            }, // 0BBB0
            new[]
            {
                Constants.MarkerType.None, Constants.MarkerType.Black, Constants.MarkerType.Black,
                Constants.MarkerType.None, Constants.MarkerType.Black, Constants.MarkerType.None
            }, // 0BB0B0
            new[]
            {
                Constants.MarkerType.None, Constants.MarkerType.Black, Constants.MarkerType.None,
                Constants.MarkerType.Black, Constants.MarkerType.Black, Constants.MarkerType.None
            }, // 0B0BB0
            new[]
            {
                Constants.MarkerType.None, Constants.MarkerType.Black, Constants.MarkerType.Black,
                Constants.MarkerType.None, Constants.MarkerType.None, Constants.MarkerType.Black,
                Constants.MarkerType.None
            }, // 0BB00B0
            new[]
            {
                Constants.MarkerType.None, Constants.MarkerType.Black, Constants.MarkerType.None,
                Constants.MarkerType.None, Constants.MarkerType.Black, Constants.MarkerType.Black,
                Constants.MarkerType.None
            } // 0B00BB0
        };

        foreach (var patternToCheck in openThreePatterns)
        {
            if (CheckPattern(pattern, centerIdx, patternToCheck))
                return true;
        }

        return false;
    }

    // 사사 금지 체크 (4-4)
    private bool CheckDoubleFour(Vector2Int position)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),   // 가로
            new Vector2Int(0, 1),   // 세로
            new Vector2Int(1, 1),   // 대각선 (↗)
            new Vector2Int(1, -1)   // 대각선 (↘)
        };

        int openFourCount = 0;

        foreach (Vector2Int dir in directions)
        {
            if (HasOpenFour(position, dir))
            {
                openFourCount++;
                // 사사가 되면 바로 true 반환
                if (openFourCount >= 2)
                    return true;
            }
        }

        return false;
    }
    
    private bool HasOpenFour(Vector2Int position, Vector2Int direction)
    {
        Vector2Int oppositeDir = new Vector2Int(-direction.x, -direction.y);

        // 양방향에서 패턴 가져오기
        List<Constants.MarkerType> pattern = GetLinePattern(position, direction, 5);
        pattern.Reverse();
        List<Constants.MarkerType> oppositePattern = GetLinePattern(position, oppositeDir, 5);
        oppositePattern.RemoveAt(0); // 중복 제거

        pattern.AddRange(oppositePattern);

        // 중심 인덱스 설정
        int centerIdx = pattern.Count / 2;

        return IsOpenFourPattern(pattern, centerIdx);
    }

    private bool IsOpenFourPattern(List<Constants.MarkerType> pattern, int centerIdx)
    {
        // 열린 4 패턴들 (0: 빈칸, B: 흑돌, W: 백돌)
        Constants.MarkerType[][] openFourPatterns = new Constants.MarkerType[][]
        {
            new[]
            {
                Constants.MarkerType.None, Constants.MarkerType.Black, Constants.MarkerType.Black,
                Constants.MarkerType.Black, Constants.MarkerType.Black, Constants.MarkerType.None
            }, // 0BBBB0
            new[]
            {
                Constants.MarkerType.None, Constants.MarkerType.Black, Constants.MarkerType.Black,
                Constants.MarkerType.None, Constants.MarkerType.Black, Constants.MarkerType.Black,
                Constants.MarkerType.None
            }, // 0BB0BB0
            new[]
            {
                Constants.MarkerType.None, Constants.MarkerType.Black, Constants.MarkerType.None,
                Constants.MarkerType.Black, Constants.MarkerType.Black, Constants.MarkerType.Black,
                Constants.MarkerType.None
            }, // 0B0BBB0
            new[]
            {
                Constants.MarkerType.None, Constants.MarkerType.Black, Constants.MarkerType.Black,
                Constants.MarkerType.Black, Constants.MarkerType.None, Constants.MarkerType.Black,
                Constants.MarkerType.None
            }, // 0BBB0B0
        };

        foreach (var patternToCheck in openFourPatterns)
        {
            if (CheckPattern(pattern, centerIdx, patternToCheck))
                return true;
        }

        return false;
    }

    private bool CheckPattern(List<Constants.MarkerType> linePattern, int centerIdx, Constants.MarkerType[] patternToCheck)
    {
        int patternHalfLength = patternToCheck.Length / 2;
        int startIdx = centerIdx - patternHalfLength;
    
        // 패턴이 라인 패턴 범위 내에 있는지 확인
        if (startIdx < 0 || startIdx + patternToCheck.Length > linePattern.Count)
            return false;
    
        // 패턴 매칭 확인
        for (int i = 0; i < patternToCheck.Length; i++)
        {
            if (patternToCheck[i] != linePattern[startIdx + i])
                return false;
        }
    
        return true;
    }

    // 장목 금지 체크 (오목 초과)
    private bool CheckOverline(Vector2Int position)
    {
        // 방향 벡터 (8방향)
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),   // 가로
            new Vector2Int(0, 1),   // 세로
            new Vector2Int(1, 1),   // 대각선 (↗)
            new Vector2Int(1, -1)   // 대각선 (↘)
        };
    
        foreach (Vector2Int dir in directions)
        {
            Vector2Int oppositeDir = new Vector2Int(-dir.x, -dir.y);
        
            // 해당 방향의 돌 개수 세기
            List<Constants.MarkerType> pattern = GetLinePattern(position, dir, 5);
            pattern.Reverse();
        
            List<Constants.MarkerType> oppositePattern = GetLinePattern(position, oppositeDir, 5);
            oppositePattern.RemoveAt(0); // 중복 위치 제거
        
            pattern.AddRange(oppositePattern);
        
            // 연속된 흑돌 세기
            int maxConsecutiveBlacks = 0;
            int currentConsecutiveBlacks = 0;
        
            foreach (var marker in pattern)
            {
                if (marker == Constants.MarkerType.Black)
                {
                    currentConsecutiveBlacks++;
                    maxConsecutiveBlacks = Math.Max(maxConsecutiveBlacks, currentConsecutiveBlacks);
                }
                else
                {
                    currentConsecutiveBlacks = 0;
                }
            }
        
            // 6목 이상이면 금지
            if (maxConsecutiveBlacks > 5)
                return true;
        }
    
        return false;
    }
    
    // 방향에 따른 패턴 가져오기
    private List<Constants.MarkerType> GetLinePattern(Vector2Int startPos, Vector2Int direction, int range)
    {
        List<Constants.MarkerType> pattern = new List<Constants.MarkerType>();
        
        // 시작 위치 추가
        pattern.Add(_board[startPos.x, startPos.y]);
        
        // 주어진 방향으로 패턴 수집
        for (int i = 1; i <= range; i++)
        {
            Vector2Int pos = startPos + (direction * i);
            
            if (IsInBoardRange(pos))
                pattern.Add(_board[pos.x, pos.y]);
            else
                break;
        }
        
        return pattern;
    }
    
    // 보드 범위 내에 있는지 확인
    private bool IsInBoardRange(Vector2Int position)
    {
        return position.x >= 0 && position.x < _board.GetLength(0) &&
               position.y >= 0 && position.y < _board.GetLength(1);
    }
}